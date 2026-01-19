# Work Order 17: Implementation Plan Changes Summary

## Key Corrections Made

### 1. Terminology: Story Group → Story Root
- **Original Plan**: Referenced "Story Group" as a new entity
- **Corrected**: All functionality applies to existing `StoryRoot` model
- **Impact**: No new domain entities, uses existing models and services

### 2. Versioning: Integer → GUID-based VersionId
- **Original Plan**: Used integer versions (1, 2, 3...) with increment logic
- **Corrected**: Uses GUID-based `VersionId` (string) from `VersionMetadata`
- **Impact**: 
  - No version increment logic needed
  - Conflict detection compares version IDs, not numbers
  - Aligns with existing `StoryRootRepository` implementation

### 3. Save Pattern: Direct Save → Enhanced Commit
- **Original Plan**: New separate save endpoint
- **Corrected**: Enhances existing `POST /api/story-root/commit` endpoint
- **Impact**: 
  - Backward compatible (optional parameter)
  - Follows existing propose → commit flow
  - No new endpoints required

### 4. File Path Corrections

| Original Plan Path | Corrected Path | Reason |
|-------------------|----------------|---------|
| `src/Api/Controllers/StoryGroupController.cs` | `src/api/StoryRootEndpointsExtensions.cs` (modify) | Uses extension methods, not controllers |
| `src/Services/StoryGroup/StoryGroupService.cs` | `src/api/Services/StoryRootService.cs` (modify) | Services in `src/api/Services/` |
| `src/Services/Exceptions/VersionConflictException.cs` | `src/api/Services/VersionConflictException.cs` | Exceptions in `src/api/Services/` |
| `src/Api/Models/StoryGroupSaveModels.cs` | `src/api/Models/Requests.cs` (modify) | Models consolidated in existing files |
| `src/Frontend/Services/StoryGroupApiClient.ts` | `src/web/src/services/storyRootService.ts` (modify) | Frontend in `src/web/src/` |
| `src/Frontend/Components/StoryGroupEditor.tsx` | `src/web/src/components/StoryRoot/StoryRootProposalEditor.tsx` (modify) | Components in `src/web/src/components/` |
| `tests/Unit/Services/StoryGroupServiceSaveTests.cs` | `tests/Todo.Api.Tests/Services/StoryRootServiceCommitVersionConflictTests.cs` | Tests in `tests/Todo.Api.Tests/` |

### 5. API Pattern: Controller → Extension Methods
- **Original Plan**: Suggested creating a controller class
- **Corrected**: Modifies existing extension method in `StoryRootEndpointsExtensions.cs`
- **Impact**: Follows established minimal API pattern

### 6. Storage Path: Custom → Existing Helper
- **Original Plan**: Custom blob path `story-groups/{id}/v{version}.json`
- **Corrected**: Uses existing `ArtifactPathHelper.GetStoryRootVersionPath()`
- **Impact**: No storage structure changes, uses existing infrastructure

## Implementation Approach

### Backend Changes (3 files)

1. **`src/api/Models/Requests.cs`**
   - Add optional `ExpectedVersionId` property to `CommitStoryRootRequest`
   - ~5 lines added

2. **`src/api/Services/VersionConflictException.cs`** (NEW)
   - Create exception class following existing patterns
   - ~20 lines

3. **`src/api/Services/StoryRootService.cs`**
   - Add optional `expectedVersionId` parameter to `CommitStoryRootVersionAsync`
   - Add version conflict detection logic
   - ~30 lines added/modified

4. **`src/api/StoryRootEndpointsExtensions.cs`**
   - Add catch block for `VersionConflictException` → 409 response
   - Pass `expectedVersionId` to service method
   - ~15 lines modified

### Frontend Changes (2 files)

1. **`src/web/src/services/storyRootService.ts`**
   - Add optional `expectedVersionId` parameter to `commitStoryRootVersion`
   - Handle 409 Conflict status code
   - ~15 lines modified

2. **`src/web/src/components/StoryRoot/StoryRootProposalEditor.tsx`** (or appropriate component)
   - Track `currentVersionId` state
   - Pass `expectedVersionId` when committing
   - Display conflict error on 409
   - ~40 lines added/modified

### Testing (1 file)

1. **`tests/Todo.Api.Tests/Services/StoryRootServiceCommitVersionConflictTests.cs`** (NEW)
   - Test version conflict scenarios
   - Test backward compatibility
   - ~120 lines

## Backward Compatibility

✅ **Fully Backward Compatible**
- `expectedVersionId` parameter is optional
- Existing commit calls without `expectedVersionId` work unchanged
- No breaking changes to API contracts
- No changes to storage structure

## Key Design Decisions

1. **Optional Parameter**: Makes feature opt-in, maintains backward compatibility
2. **Exception-Based**: Uses exception for conflict detection (consistent with existing patterns)
3. **Service Layer Logic**: All version checking in service, not repository
4. **GUID Comparison**: Simple string comparison of version IDs
5. **No Storage Changes**: Uses existing blob storage and path helpers

## Questions Resolved

✅ Story Group → Story Root: Use existing entity  
✅ Versioning: GUID-based VersionId, not integers  
✅ Save Pattern: Enhance commit, not separate save  
✅ File Paths: Aligned with actual codebase structure  
✅ API Pattern: Extension methods, not controllers  

## Next Steps

1. Review revised implementation plan
2. Confirm approach aligns with requirements
3. Proceed with implementation following revised plan
