# Implementation Plan: Story Root and World State Data Models and Storage

## Codebase Analysis Summary

### Existing Patterns Identified

**Dependency Injection:**
- Uses `AddSingleton<T>()` pattern in `Program.cs`
- No interface-based DI currently (e.g., `ListsRepository` registered directly)
- Configuration accessed via `IConfiguration` with environment variables

**Serialization:**
- Will use `System.Text.Json` with camelCase naming policy for consistency
- Models are simple POCOs with public properties

**Storage (Observation - Not Applicable to This Work Order):**
- Existing codebase uses Azure Cosmos DB via `Microsoft.Azure.Cosmos` SDK (observation only)
- This work order implements Blob Storage as the sole persistence mechanism
- Repository pattern will be used with interfaces for testability

**Validation:**
- No existing validation framework or patterns
- Will introduce validation classes following standard .NET patterns

**Authentication/User Context:**
- Uses `DefaultAzureCredential` for Azure services
- No user context extraction currently visible
- Will need to create user context service (placeholder for now, to be integrated with auth later)

**Project Structure:**
- Flat structure in `src/api/` directory
- No `Models/`, `Repositories/`, `Validation/`, or `Utils/` folders yet
- All classes in `SimpleTodo.Api` namespace

---

## Implementation Plan

**Note:** This implementation uses Azure Blob Storage as the sole persistence mechanism. Any references to Cosmos DB in the codebase analysis are observations only and do not influence the design of this Blob Storage-based solution.

### Phase 1: Foundation - Models and Utilities

#### 1.1 Create Data Models
**Files to Create:**
- `src/api/Models/StoryRoot.cs`
- `src/api/Models/WorldState.cs`
- `src/api/Models/VersionMetadata.cs`

**Responsibilities:**
- Define POCO classes with properties matching work order specifications
- Use `System.Text.Json.Serialization` attributes for camelCase serialization
- Include JSON serialization support

**Order:** Create all three models first (foundation for everything else)

---

#### 1.2 Create Utility Classes
**Files to Create:**
- `src/api/Utils/ArtifactPathHelper.cs`
- `src/api/Services/IUserContextService.cs` (interface)
- `src/api/Services/UserContextService.cs` (implementation)

**Responsibilities:**
- `ArtifactPathHelper`: Static helper for blob path construction (`/users/{user_id}/story-root/root/versions/{version_id}.json`)
- `IUserContextService`: Interface for getting current user ID (placeholder implementation for now)
- `UserContextService`: Implementation that extracts user ID (initially returns placeholder, to be integrated with auth later)

**Order:** Create utilities before repositories (repositories depend on these)

---

### Phase 2: Storage Infrastructure

#### 2.1 Create Generic Blob Storage Repository
**Files to Create:**
- `src/api/Repositories/IBlobArtifactRepository.cs`
- `src/api/Repositories/BlobArtifactRepository.cs`

**Responsibilities:**
- `IBlobArtifactRepository`: Generic interface for blob operations (save, retrieve, list versions, manage current version pointers)
- `BlobArtifactRepository`: Implementation using Azure.Storage.Blobs SDK
- Handles JSON serialization/deserialization
- Manages current version pointers (separate blob files)
- User-scoped operations (all paths include user_id)

**Dependencies:** Requires Azure Blob Storage connection string/endpoint from configuration
**Order:** Create before artifact-specific repositories

---

#### 2.2 Create Artifact-Specific Repositories
**Files to Create:**
- `src/api/Repositories/IStoryRootRepository.cs`
- `src/api/Repositories/StoryRootRepository.cs`
- `src/api/Repositories/IWorldStateRepository.cs`
- `src/api/Repositories/WorldStateRepository.cs`

**Responsibilities:**
- `IStoryRootRepository` / `IWorldStateRepository`: Artifact-specific interfaces with methods:
  - `SaveNewVersionAsync(artifact, priorVersionId?, sourceRequestId?, environment?, llmAssisted?)`
  - `GetCurrentVersionAsync(userId)`
  - `GetVersionAsync(userId, versionId)`
  - `ListVersionsAsync(userId)`
- Implementations use `BlobArtifactRepository` for storage operations
- Handle automatic version ID generation (GUID)
- Populate `VersionMetadata` automatically
- Wrap artifacts with version metadata in storage

**Dependencies:** `BlobArtifactRepository`, `IUserContextService`
**Order:** Create interfaces first, then implementations

---

### Phase 3: Validation

#### 3.1 Create Validators
**Files to Create:**
- `src/api/Validation/StoryRootValidator.cs`
- `src/api/Validation/WorldStateValidator.cs`

**Responsibilities:**
- Validate required fields are present and non-empty
- Validate JSON structure (ensure serializable)
- Return validation results (success/failure with error messages)
- Simple validation pattern (can be extended later with FluentValidation if needed)

**Dependencies:** Models
**Order:** Create after models, before DI registration

---

### Phase 4: Dependency Injection and Configuration

#### 4.1 Update Program.cs
**File to Modify:**
- `src/api/Program.cs`

**Changes Required:**
1. Add Azure Blob Storage client registration:
   - Add `Azure.Storage.Blobs` package reference
   - Register `BlobServiceClient` using `DefaultAzureCredential` (consistent with existing authentication pattern)
   - Read blob storage endpoint from configuration (`AZURE_BLOB_STORAGE_ENDPOINT` or similar)

2. Register new services:
   - `AddSingleton<IUserContextService, UserContextService>()`
   - `AddSingleton<IBlobArtifactRepository, BlobArtifactRepository>()`
   - `AddSingleton<IStoryRootRepository, StoryRootRepository>()`
   - `AddSingleton<IWorldStateRepository, WorldStateRepository>()`

**Dependencies:** All repositories and services
**Order:** Last step - register everything after all classes are created

---

#### 4.2 Update Project File
**File to Modify:**
- `src/api/Todo.Api.csproj`

**Changes Required:**
- Add package reference: `Azure.Storage.Blobs` (latest stable version compatible with .NET 8.0)

**Order:** Can be done early, but must be before building

---

## Implementation Order Summary

1. **Update `Todo.Api.csproj`** - Add Azure.Storage.Blobs package
2. **Create Models** (3 files) - StoryRoot, WorldState, VersionMetadata
3. **Create Utilities** (3 files) - ArtifactPathHelper, IUserContextService, UserContextService
4. **Create Generic Blob Repository** (2 files) - IBlobArtifactRepository, BlobArtifactRepository
5. **Create Artifact Repositories** (4 files) - IStoryRootRepository, StoryRootRepository, IWorldStateRepository, WorldStateRepository
6. **Create Validators** (2 files) - StoryRootValidator, WorldStateValidator
7. **Update Program.cs** - Register all services and configure Blob Storage client

**Total Files:** 14 files (13 new, 2 modified)

---

## Key Design Decisions

1. **User Context Service**: Created as placeholder that returns a default user ID. Will need integration with authentication system later (likely from HttpContext claims).

2. **Version Storage Strategy**: 
   - Store each version as separate blob: `/users/{user_id}/story-root/root/versions/{version_id}.json`
   - Store current version pointer as separate blob: `/users/{user_id}/story-root/root/current.json` (contains version_id)
   - Artifact JSON includes embedded `VersionMetadata` for provenance

3. **Serialization**: Use `System.Text.Json` with `JsonSerializerOptions` configured for camelCase naming policy for consistency.

4. **Error Handling**: Return nullable types and throw appropriate exceptions for missing artifacts/versions.

5. **Interface-Based DI**: Introducing interfaces for testability and following SOLID principles, even though existing code doesn't use them yet.

6. **Validation Approach**: Simple validation classes with methods returning bool + error messages. Can be enhanced with FluentValidation later if needed.

---

## Configuration Requirements

**New Environment Variables Needed:**
- `AZURE_BLOB_STORAGE_ENDPOINT` - Blob storage account endpoint URL
- (Optional) `AZURE_BLOB_STORAGE_CONTAINER_NAME` - Container name (default: "narrative-artifacts")

**Note:** Will use `DefaultAzureCredential` for authentication (consistent with existing authentication pattern in the codebase).

---

## Testing Considerations

- All repositories should be testable via interfaces
- User context service can be mocked for unit tests
- Blob storage operations can be tested with Azurite (local emulator) or integration tests

---

## Out of Scope (Per Work Order)

- API endpoints (separate work order)
- LLM integration (separate work order)
- Frontend components (separate work order)
- Cross-artifact relationships or transactions
