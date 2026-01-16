# Work Order 9 Implementation Plan Review

## Summary
This document reviews the Work Order 9 implementation plan against the current codebase to identify necessary updates and corrections.

## Key Findings

### 1. Container Name Mismatch
**Issue**: The implementation plan specifies container name as `"artifacts"`, but the codebase uses `"narrative-artifacts"` as the default.

**Location**: 
- Plan: Specifies `AZURE_STORAGE_CONTAINER_NAME=artifacts`
- Codebase: `src/api/Repositories/BlobArtifactRepository.cs` line 15 uses `"narrative-artifacts"` as default

**Recommendation**: Update the plan to use `"narrative-artifacts"` as the container name, or update the code to use `"artifacts"` to match the plan. The plan should be updated to reflect the current codebase default.

### 2. Configuration Variable Names
**Issue**: The plan mentions connection strings, but the codebase uses endpoint URLs with DefaultAzureCredential.

**Current Codebase Configuration**:
- `AZURE_BLOB_STORAGE_ENDPOINT` - Storage account endpoint URL (required)
- `AZURE_BLOB_STORAGE_CONTAINER_NAME` - Container name (optional, defaults to "narrative-artifacts")

**Plan Mentions**:
- `AZURE_STORAGE_ACCOUNT_NAME`
- `AZURE_STORAGE_CONNECTION_STRING`
- `AZURE_STORAGE_CONTAINER_NAME=artifacts`

**Recommendation**: Update the plan to match the codebase:
- Use `AZURE_BLOB_STORAGE_ENDPOINT` instead of connection string
- Use `AZURE_BLOB_STORAGE_CONTAINER_NAME` (not `AZURE_STORAGE_CONTAINER_NAME`)
- The codebase uses `DefaultAzureCredential` for authentication (managed identity), not connection strings
- Storage account name can be derived from the endpoint URL

### 3. Module Directory Structure
**Issue**: The plan suggests creating modules in `infra/modules/`, but the current codebase uses `infra/app/` for infrastructure modules.

**Current Structure**:
- `infra/app/api-appservice-avm.bicep`
- `infra/app/db-avm.bicep`
- `infra/app/web-appservice-avm.bicep`
- `infra/app/cosmos-role-assignment.bicep`

**Plan Suggests**:
- `infra/modules/storage.bicep`
- `infra/modules/storage-container.bicep`
- `infra/modules/app-service.bicep` (already exists as `infra/app/api-appservice-avm.bicep`)

**Recommendation**: Update the plan to use `infra/app/` directory for consistency with existing structure:
- `infra/app/storage-avm.bicep` (following the naming pattern)
- `infra/app/storage-container-avm.bicep` (optional, see #4)

### 4. Container Creation
**Issue**: The plan includes a separate container creation template, but the codebase already handles container creation automatically.

**Current Codebase**: `BlobArtifactRepository.GetContainerClientAsync()` calls `CreateIfNotExistsAsync()` on the container client, so containers are created automatically when first accessed.

**Recommendation**: 
- Option A: Remove the separate container creation bicep template from the plan (containers created on-demand by application)
- Option B: Keep it for explicit provisioning during infrastructure deployment (better practice for production)
- If keeping, ensure it creates the container with the correct name: `"narrative-artifacts"`

### 5. Managed Identity Already Configured
**Issue**: The plan includes creating a managed identity module, but the app service already has system-assigned managed identity enabled.

**Current Codebase**: `infra/app/api-appservice-avm.bicep` line 34-36 already configures:
```bicep
managedIdentities: {
  systemAssigned: true
}
```

**Recommendation**: 
- Remove `infra/modules/managed-identity.bicep` from the plan
- Focus on RBAC role assignment only (similar to how Cosmos DB role assignment is handled)
- Use the existing pattern: `infra/app/storage-role-assignment.bicep` (similar to `cosmos-role-assignment.bicep`)

### 6. Storage Account Naming
**Issue**: The plan doesn't specify the abbreviation to use for storage account naming.

**Current Codebase**: Uses abbreviations from `infra/abbreviations.json`:
- `"storageStorageAccounts": "st"`

**Recommendation**: Update the plan to specify storage account naming:
- Use abbreviation: `st` (from abbreviations.json)
- Naming pattern: `${abbrs.storageStorageAccounts}${resourceToken}` or `${abbrs.storageStorageAccounts}${environmentName}${resourceToken}`
- Add `storageAccountName` parameter to `main.bicep` (similar to `cosmosAccountName`)

### 7. Path Hierarchy Implementation
**Status**: ✅ Already correctly implemented in codebase

**Current Codebase**: `src/api/Utils/ArtifactPathHelper.cs` implements:
- `users/{userId}/story-root/root/versions/{versionId}.json`
- `users/{userId}/world-state/world/versions/{versionId}.json`
- Current version pointers: `users/{userId}/{artifact_type}/{artifact_id}/current.json`

**Note**: The codebase uses `"root"` and `"world"` as artifact_id segments, which is slightly different from the plan's generic `{artifact_id}`. This is acceptable as it's specific to the current artifact types.

### 8. Application Configuration Files
**Issue**: The plan mentions creating/modifying appsettings files, but they currently don't exist or are minimal.

**Current State**:
- `src/api/appsettings.json` - Minimal (only logging)
- `src/api/appsettings.Development.json` - Minimal (only logging)
- No `appsettings.Production.json` file exists

**Recommendation**: 
- Add storage configuration sections to these files (even if empty/placeholder)
- Document that actual values come from environment variables in Azure App Service
- Local development can use Azurite endpoint

### 9. Storage Account Outputs
**Issue**: The plan should specify what outputs need to be added to `main.bicep` for app service configuration.

**Recommendation**: Add outputs similar to Cosmos DB:
```bicep
output AZURE_BLOB_STORAGE_ENDPOINT string = storage.outputs.endpoint
output AZURE_BLOB_STORAGE_CONTAINER_NAME string = 'narrative-artifacts'
```

### 10. App Service Configuration Integration
**Issue**: The plan mentions updating `infra/modules/app-service.bicep`, but this file is actually `infra/app/api-appservice-avm.bicep`.

**Current Codebase**: The API app service is configured in `infra/main.bicep` lines 62-85, passing appSettings directly.

**Recommendation**: 
- Update the plan to reference `infra/app/api-appservice-avm.bicep` (not `infra/modules/app-service.bicep`)
- Add storage configuration to the `appSettings` object in `main.bicep`:
  ```bicep
  appSettings: {
    // ... existing settings ...
    AZURE_BLOB_STORAGE_ENDPOINT: storage.outputs.endpoint
    AZURE_BLOB_STORAGE_CONTAINER_NAME: 'narrative-artifacts'
  }
  ```

### 11. RBAC Role Assignment Pattern
**Issue**: The plan should follow the existing pattern used for Cosmos DB.

**Current Pattern**: `infra/app/cosmos-role-assignment.bicep` is a separate module that assigns roles.

**Recommendation**: 
- Create `infra/app/storage-role-assignment.bicep` following the same pattern
- Assign `Storage Blob Data Contributor` role to the API service's managed identity
- Reference in `main.bicep` similar to `apiCosmosRoleAssignment`

### 12. Local Development Support
**Status**: Plan is appropriate, but needs container name update

**Recommendation**: 
- Update scripts to use container name `"narrative-artifacts"` (not `"artifacts"`)
- Ensure Azurite connection string format matches endpoint URL format expected by codebase
- Document that `AZURE_BLOB_STORAGE_ENDPOINT` should be set to Azurite endpoint (e.g., `http://127.0.0.1:10000/devstoreaccount1`)

### 13. Health Checks and Configuration Validation
**Status**: Plan is appropriate, but these are application code changes

**Note**: The work order states "Out of Scope: Application code changes to use the storage (handled in other work orders)", but health checks and configuration validation are infrastructure-related concerns. Clarify if these should be included or deferred.

### 14. CI/CD Pipeline Updates
**Issue**: The plan mentions updating `.github/workflows/azure-dev.yml`, but this file does not exist in the codebase.

**Current State**: No `.github/workflows/` directory exists.

**Recommendation**: 
- Remove CI/CD pipeline updates from the implementation plan, OR
- Note as optional/future work if CI/CD is not currently set up
- If using Azure Developer CLI (`azd`), deployment validation can be done via `azd` commands instead

## Required Plan Updates Summary

### High Priority
1. ✅ Update container name from `"artifacts"` to `"narrative-artifacts"`
2. ✅ Update configuration variable names to match codebase (`AZURE_BLOB_STORAGE_ENDPOINT`, `AZURE_BLOB_STORAGE_CONTAINER_NAME`)
3. ✅ Change module directory from `infra/modules/` to `infra/app/`
4. ✅ Remove managed identity module (already configured)
5. ✅ Update app service module reference to `infra/app/api-appservice-avm.bicep`
6. ✅ Add storage account name parameter and use abbreviation `"st"`
7. ✅ Follow existing RBAC pattern (create `storage-role-assignment.bicep` similar to `cosmos-role-assignment.bicep`)

### Medium Priority
8. ✅ Clarify container creation approach (explicit vs on-demand)
9. ✅ Add storage outputs to `main.bicep`
10. ✅ Update app service configuration in `main.bicep` to include storage settings
11. ✅ Update local development scripts with correct container name

### Low Priority / Clarification Needed
12. ⚠️ Clarify if health checks and configuration validation are in scope
13. ⚠️ CI/CD pipeline file does not exist - remove from plan or mark as optional
14. ⚠️ Document that connection strings are NOT used (endpoint + managed identity only)
15. ⚠️ Scripts directory may need to be created if it doesn't exist

## Files That Already Exist (Should NOT Be Created)
- ✅ `src/api/Repositories/IBlobArtifactRepository.cs` - Already exists
- ✅ `src/api/Repositories/BlobArtifactRepository.cs` - Already exists
- ✅ `src/api/Repositories/IStoryRootRepository.cs` - Already exists
- ✅ `src/api/Repositories/StoryRootRepository.cs` - Already exists
- ✅ `src/api/Repositories/IWorldStateRepository.cs` - Already exists
- ✅ `src/api/Repositories/WorldStateRepository.cs` - Already exists
- ✅ `src/api/Utils/ArtifactPathHelper.cs` - Already exists
- ✅ Application code in `Program.cs` - Already configured

## Files That Need to Be Created (Infrastructure Only)
- `infra/app/storage-avm.bicep` - Storage account module
- `infra/app/storage-role-assignment.bicep` - RBAC role assignment
- `infra/app/storage-container-avm.bicep` - Optional: explicit container creation
- Updates to `infra/main.bicep` - Add storage module and configuration
- Updates to `infra/main.parameters.json` - Add storage parameters (if needed)
- Updates to `azure.yaml` - Add storage service configuration
- Local development scripts and documentation

## Conclusion
The implementation plan is mostly sound but needs updates to align with:
1. Existing codebase patterns and naming conventions
2. Current directory structure (`infra/app/` not `infra/modules/`)
3. Actual configuration variable names used in the code
4. Existing managed identity setup
5. Container name used by the application

The application code for blob storage is already implemented, so this work order focuses solely on infrastructure provisioning and configuration.
