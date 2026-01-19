# Work Order 17: Story Root Save/Persistence with Optimistic Version Control

## Overview

Enhance the existing Story Root commit operation with optimistic concurrency control. This adds version conflict detection to prevent overwriting changes when the current version has been updated by another user or process.

## Key Changes from Original Plan

1. **Story Group → Story Root**: All functionality applies to existing `StoryRoot` model
2. **Versioning**: Uses GUID-based `VersionId` (string) from `VersionMetadata`, not integer versions
3. **Save Pattern**: Enhances existing `commit` endpoint, not a separate save operation
4. **File Paths**: Aligned with actual codebase structure
5. **API Pattern**: Uses minimal API extension methods, not controllers

## Acceptance Criteria

### Frontend Requirements

- Save/Commit button in story root editor that calls backend API
- Send `storyRoot`, `expectedVersionId` (current version ID) to backend
- Display loading state during commit operation
- Show success confirmation on successful commit
- Display conflict error (409) if version mismatch occurs
- Track current version ID from latest commit response

### Backend API Requirements

- Enhance existing `POST /api/story-root/commit` endpoint
- Accept optional `expected_version_id` in request
- Return 200 OK with new version ID on success
- Return 409 Conflict on version mismatch
- Maintain existing commit behavior when `expected_version_id` is not provided (backward compatible)

### Service Layer Requirements

- Enhance `StoryRootService.CommitStoryRootVersionAsync` method with version checking:
  1. If `expectedVersionId` provided, load current version from storage
  2. Compare current version ID with `expectedVersionId`
  3. If mismatch → throw `VersionConflictException`
  4. Else: proceed with existing commit logic (save new version, return version ID)
- Keep all commit logic inside service method
- No separate orchestrator, no workflow engine

### Storage Requirements

- Use existing Azure Blob Storage infrastructure
- Use existing `ArtifactPathHelper` for blob paths
- No changes to storage structure or naming conventions

### Version Management

- Optimistic concurrency control using GUID-based version IDs
- Conflict detection by comparing expected vs actual current version ID
- No history tracking changes, no branching, no rollback (uses existing versioning)

## Out of Scope

- Version history UI or diff services
- Merge workflows or approval flows
- Advanced conflict resolution beyond detection
- Retry logic or resilience frameworks
- New domain abstractions or workflow engines
- Branching, rollback, or advanced versioning features
- Changes to existing version storage structure

## Implementation Plan

## Required Files (5 files total)

### Backend (3 files)

**1. Enhanced Commit Request Model**

- **File**: `src/api/Models/Requests.cs` (modify existing)
- **Purpose**: Add optional `expected_version_id` field to `CommitStoryRootRequest`
- **Implementation**: 
  - Add `[JsonPropertyName("expected_version_id")]` property (nullable string)
- **Size**: ~5 lines added

**2. Version Conflict Exception**

- **File**: `src/api/Services/VersionConflictException.cs` (new file)
- **Purpose**: Specific exception for version mismatches during commit
- **Implementation**: Simple exception class following existing exception patterns
- **Size**: ~20 lines

**3. Enhanced Story Root Service**

- **File**: `src/api/Services/StoryRootService.cs` (modify existing)
- **Purpose**: Add version conflict detection to `CommitStoryRootVersionAsync`
- **Implementation**: 
  - Add optional `expectedVersionId` parameter
  - Load current version if `expectedVersionId` provided
  - Compare and throw `VersionConflictException` on mismatch
  - Otherwise proceed with existing commit logic
- **Size**: ~30 lines added/modified

### Frontend (1 file)

**4. Enhanced Story Root Service Client**

- **File**: `src/web/src/services/storyRootService.ts` (modify existing)
- **Purpose**: Add `expectedVersionId` parameter to `commitStoryRootVersion` method
- **Implementation**: 
  - Update method signature to accept optional `expectedVersionId`
  - Include in request body when provided
  - Handle 409 Conflict status code
- **Size**: ~15 lines modified

**5. Enhanced Story Root Editor with Version Tracking**

- **File**: `src/web/src/components/StoryRoot/StoryRootEditor.tsx` or `StoryRootProposalEditor.tsx` (modify existing)
- **Purpose**: Track current version ID and pass to commit operation
- **Implementation**: 
  - Track `currentVersionId` state from latest commit response
  - Pass `expectedVersionId` when committing
  - Display conflict error on 409 response
  - Show loading/success states
- **Size**: ~40 lines added/modified

### Testing (1 file)

**6. Story Root Commit Version Conflict Tests**

- **File**: `tests/Todo.Api.Tests/Services/StoryRootServiceCommitVersionConflictTests.cs` (new file)
- **Purpose**: Test version conflict detection in commit operation
- **Implementation**: 
  - Happy path with matching version
  - Conflict scenario with mismatched version
  - Backward compatibility (no expectedVersionId provided)
  - Storage error scenarios
- **Size**: ~120 lines

## Implementation Details

### Enhanced Commit API Endpoint

```csharp
// In StoryRootEndpointsExtensions.cs - modify existing CommitStoryRoot method
public static async Task<IResult> CommitStoryRoot(
    IStoryRootService storyRootService,
    CommitStoryRootRequest request,
    HttpContext context)
{
    if (request == null || request.StoryRoot == null)
    {
        var correlationId = CorrelationIdMiddleware.GetCorrelationId(context);
        return TypedResults.Json(
            new ErrorResponse
            {
                Error = "Request body is required and must include a 'story_root' field.",
                CorrelationId = correlationId
            },
            statusCode: 400);
    }

    try
    {
        var correlationId = CorrelationIdMiddleware.GetCorrelationId(context);
        var versionId = await storyRootService.CommitStoryRootVersionAsync(
            request.StoryRoot, 
            correlationId,
            request.ExpectedVersionId); // Add this parameter

        var response = new CommitResponse<StoryRoot>
        {
            VersionId = versionId,
            Artifact = request.StoryRoot
        };

        return TypedResults.Ok(response);
    }
    catch (VersionConflictException ex) // Add this catch block
    {
        var correlationId = CorrelationIdMiddleware.GetCorrelationId(context);
        return TypedResults.Json(
            new ErrorResponse
            {
                Error = $"Story Root has been updated by another user. {ex.Message}",
                CorrelationId = correlationId
            },
            statusCode: 409);
    }
    catch (Exception ex)
    {
        var correlationId = CorrelationIdMiddleware.GetCorrelationId(context);
        return TypedResults.Json(
            new ErrorResponse
            {
                Error = $"An error occurred while committing Story Root: {ex.Message}",
                CorrelationId = correlationId
            },
            statusCode: 500);
    }
}
```

### Enhanced Commit Request Model

```csharp
// In Models/Requests.cs - modify existing CommitStoryRootRequest
public class CommitStoryRootRequest
{
    [JsonPropertyName("story_root")]
    public StoryRoot StoryRoot { get; set; } = null!;

    [JsonPropertyName("expected_version_id")]
    public string? ExpectedVersionId { get; set; }
}
```

### Version Conflict Exception

```csharp
// New file: src/api/Services/VersionConflictException.cs
namespace SimpleTodo.Api.Services;

/// <summary>
/// Exception thrown when a version conflict is detected during commit operations.
/// Indicates that the current version has changed since the user last loaded it.
/// </summary>
public class VersionConflictException : Exception
{
    public string? ExpectedVersionId { get; }
    public string? CurrentVersionId { get; }

    public VersionConflictException(string message) : base(message)
    {
    }

    public VersionConflictException(string message, string? expectedVersionId, string? currentVersionId) 
        : base(message)
    {
        ExpectedVersionId = expectedVersionId;
        CurrentVersionId = currentVersionId;
    }

    public VersionConflictException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }
}
```

### Enhanced Service Logic

```csharp
// In StoryRootService.cs - modify existing CommitStoryRootVersionAsync method
public async Task<string> CommitStoryRootVersionAsync(
    Models.StoryRoot proposal, 
    string? identity = null,
    string? expectedVersionId = null) // Add this parameter
{
    if (proposal == null)
    {
        throw new ArgumentNullException(nameof(proposal));
    }

    // Validate the proposal before committing
    var (isValid, errorMessage) = _validator.Validate(proposal);
    if (!isValid)
    {
        throw new ArgumentException($"Proposal validation failed: {errorMessage}", nameof(proposal));
    }

    // Version conflict detection (if expectedVersionId provided)
    if (!string.IsNullOrEmpty(expectedVersionId))
    {
        var currentStoryRoot = await GetCurrentStoryRootAsync();
        var versions = await ListStoryRootVersionsAsync();
        var currentVersionId = versions.FirstOrDefault()?.VersionId;

        if (currentVersionId != expectedVersionId)
        {
            throw new VersionConflictException(
                $"Expected version {expectedVersionId}, but current version is {currentVersionId}",
                expectedVersionId,
                currentVersionId);
        }
    }

    // Get current version to establish prior version link
    var versions = await ListStoryRootVersionsAsync();
    var priorVersionId = versions.FirstOrDefault()?.VersionId;

    // Generate source request ID for provenance
    var sourceRequestId = identity ?? Guid.NewGuid().ToString("N");

    // Get environment (could be from configuration in future)
    var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

    // Save new version with llm_assisted flag set to true
    var versionId = await _repository.SaveNewVersionAsync(
        proposal,
        priorVersionId: priorVersionId,
        sourceRequestId: sourceRequestId,
        environment: environment,
        llmAssisted: true);

    return versionId;
}
```

### Frontend Integration

```typescript
// In storyRootService.ts - modify existing commitStoryRootVersion method
async commitStoryRootVersion(
    storyRoot: StoryRoot, 
    expectedVersionId?: string
): Promise<CommitResponse<StoryRoot>> {
    if (!storyRoot) {
        throw new Error('Story Root is required');
    }

    try {
        const requestBody: any = {
            story_root: storyRoot
        };
        
        if (expectedVersionId) {
            requestBody.expected_version_id = expectedVersionId;
        }

        const response = await this.client.post<CommitResponse<StoryRoot>>('/commit', requestBody);
        return response.data;
    } catch (error) {
        const axiosError = error as AxiosError<ErrorResponse>;
        const correlationId = this.extractCorrelationId(axiosError);
        
        if (axiosError.response?.status === 409) {
            const errorMessage = axiosError.response?.data?.error || 'Version conflict detected';
            throw new Error(correlationId ? `${errorMessage} (Correlation ID: ${correlationId})` : errorMessage);
        }
        
        const errorMessage = axiosError.response?.data?.error || axiosError.message || 'Failed to commit Story Root';
        throw new Error(correlationId ? `${errorMessage} (Correlation ID: ${correlationId})` : errorMessage);
    }
}
```

### Frontend Component Enhancement

```typescript
// In StoryRootProposalEditor.tsx or appropriate component
// Add state for tracking current version ID
const [currentVersionId, setCurrentVersionId] = useState<string | null>(null);
const [saveState, setSaveState] = useState<'idle' | 'saving' | 'success' | 'conflict' | 'error'>('idle');

// On commit/save
const handleCommit = async () => {
    setSaveState('saving');
    try {
        const result = await storyRootService.commitStoryRootVersion(
            editedStoryRoot,
            currentVersionId || undefined
        );
        setCurrentVersionId(result.version_id);
        setSaveState('success');
        // Show success message
    } catch (error) {
        if (error.message.includes('Version conflict') || error.message.includes('409')) {
            setSaveState('conflict');
            // Show conflict error message
        } else {
            setSaveState('error');
            // Show generic error message
        }
    }
};
```

## Architecture Principles

**Minimal Surface Area:**
- Single parameter addition to existing commit method
- Backward compatible (optional parameter)
- No new endpoints or controllers

**Clean Separation:**
- API layer: HTTP concerns only (409 status code handling)
- Service layer: All business logic (version comparison)
- Storage layer: No changes (uses existing repository)
- Frontend: UI state management only

**Existing Patterns:**
- Follows established extension method pattern
- Uses existing blob storage setup
- Leverages existing service DI patterns
- Matches existing frontend component structure
- Uses existing exception patterns (similar to `LlmServiceException`)

## Success Criteria

1. **Happy Path**: Commit button → API call with expectedVersionId → service checks version → commit succeeds → 200 OK response → UI success state → version ID tracked
2. **Version Conflict**: Expected version mismatch → `VersionConflictException` → 409 Conflict → UI conflict state
3. **Backward Compatibility**: Commit without expectedVersionId → works as before (no version check)
4. **Error Handling**: Storage failures → 500 error → UI error state
5. **Performance**: Commit operation completes in under 2 seconds for typical story roots
6. **Testability**: Core logic isolated in service method, easily unit tested

## Testing Strategy

### Unit Tests

- **Happy Path**: Commit with matching expectedVersionId succeeds
- **Version Conflict**: Commit with mismatched expectedVersionId throws VersionConflictException
- **Backward Compatibility**: Commit without expectedVersionId works as before
- **Null/Empty Handling**: Proper handling of null/empty expectedVersionId
- **Storage Errors**: Proper exception propagation on storage failures

### Integration Considerations

- Frontend should track version ID from commit responses
- Frontend should refresh current version after successful commit
- Error messages should be user-friendly

## Future Extensibility (Non-Implemented)

**If needed later, could add:**
- Version history retrieval endpoint (already exists)
- Diff calculation between versions
- Merge conflict resolution UI
- Advanced conflict resolution strategies
- Real-time collaborative features

**Extension points designed in:**
- Service interface can be extended with additional methods
- Exception hierarchy can be expanded
- Frontend state management can accommodate additional features

**Current implementation explicitly avoids:**
- Over-engineering version management
- Creating workflow abstractions
- Building features not immediately needed
- Adding complexity for theoretical future requirements

This implementation delivers exactly what's needed: optimistic concurrency control for Story Root commits, following existing architectural patterns with minimal complexity and full backward compatibility.
