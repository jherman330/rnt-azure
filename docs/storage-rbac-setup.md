# Azure Blob Storage RBAC Setup Guide

## Quick Answer

**Role:** `Storage Blob Data Contributor`  
**Scope:** Storage Account level (sufficient for your use case)  
**Role Definition ID:** `ba92f5b4-2d11-453d-a403-e96b0029c9fe`

## Required Permissions

Your application needs the **Storage Blob Data Contributor** role, which provides:
- Read blobs (`Exists()`, `GetProperties()`, `DownloadContent()`)
- Write blobs (`Upload()`, `Delete()`)
- List blobs (`GetBlobs()`)
- Create containers (`CreateIfNotExists()`)

## Where to Apply the Role

### Storage Account Level (Recommended for Your Use Case)

**Why account-level is sufficient:**
- Your app only needs access to one container (`narrative-artifacts`)
- Container creation works (proves account-level permissions work)
- Simpler to manage - one role assignment covers all containers
- No need for container-level granularity in a single-tenant dev system

**When you'd need container-level:**
- Multi-tenant systems where different apps need different containers
- Strict isolation requirements between containers
- Compliance requirements for per-container access control

**Your infrastructure already does this correctly:**
```bicep
// infra/app/storage-rbac-extension.bicep
scope: storageAccount  // Account-level scope
roleDefinitionId: 'ba92f5b4-2d11-453d-a403-e96b0029c9fe'  // Storage Blob Data Contributor
```

## Setup by Environment

### 1. Azure App Service (Managed Identity)

**Already configured** in `infra/app/storage-rbac-extension.bicep`:

```bicep
resource apiStorageRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(apiPrincipalId, storageAccount.id, 'ba92f5b4-2d11-453d-a403-e96b0029c9fe')
  scope: storageAccount  // Storage account level
  properties: {
    roleDefinitionId: '/subscriptions/${subscription().subscriptionId}/providers/Microsoft.Authorization/roleDefinitions/ba92f5b4-2d11-453d-a403-e96b0029c9fe'
    principalId: apiPrincipalId  // App Service managed identity
    principalType: 'ServicePrincipal'
  }
}
```

**Verify it's assigned:**
```bash
az role assignment list \
  --scope "/subscriptions/<subscription-id>/resourceGroups/<rg-name>/providers/Microsoft.Storage/storageAccounts/<storage-account-name>" \
  --role "Storage Blob Data Contributor"
```

### 2. Local Development (DefaultAzureCredential)

**Problem:** Your local Azure CLI/Visual Studio identity needs the same role.

**Solution:** Assign the role to your user/service principal identity.

#### Option A: Azure CLI (Recommended)

1. **Get your user identity:**
   ```bash
   az account show --query user.name -o tsv
   ```

2. **Get storage account resource ID:**
   ```bash
   az storage account show \
     --name <storage-account-name> \
     --resource-group <resource-group-name> \
     --query id -o tsv
   ```

3. **Assign the role:**
   ```bash
   az role assignment create \
     --role "Storage Blob Data Contributor" \
     --assignee $(az account show --query user.name -o tsv) \
     --scope "/subscriptions/<subscription-id>/resourceGroups/<rg-name>/providers/Microsoft.Storage/storageAccounts/<storage-account-name>"
   ```

   Or using the storage account resource ID:
   ```bash
   STORAGE_ACCOUNT_ID=$(az storage account show \
     --name <storage-account-name> \
     --resource-group <resource-group-name> \
     --query id -o tsv)
   
   az role assignment create \
     --role "Storage Blob Data Contributor" \
     --assignee $(az account show --query user.name -o tsv) \
     --scope "$STORAGE_ACCOUNT_ID"
   ```

#### Option B: Azure Portal

1. Navigate to your Storage Account
2. Go to **Access control (IAM)**
3. Click **Add** → **Add role assignment**
4. Select role: **Storage Blob Data Contributor**
5. Select member: Your Azure AD user account
6. Click **Review + assign**

#### Option C: Service Principal (For CI/CD)

If using a service principal for local dev:

```bash
az role assignment create \
  --role "Storage Blob Data Contributor" \
  --assignee <service-principal-object-id> \
  --scope "/subscriptions/<subscription-id>/resourceGroups/<rg-name>/providers/Microsoft.Storage/storageAccounts/<storage-account-name>"
```

### 3. Verify Assignment

**Check your current identity:**
```bash
az account show --query "{Subscription:name, User:user.name}"
```

**List role assignments on storage account:**
```bash
az role assignment list \
  --scope "/subscriptions/<subscription-id>/resourceGroups/<rg-name>/providers/Microsoft.Storage/storageAccounts/<storage-account-name>" \
  --output table
```

**Test authentication:**
```bash
az storage blob list \
  --account-name <storage-account-name> \
  --container-name narrative-artifacts \
  --auth-mode login
```

## Troubleshooting 403 Errors

### Symptom: `AuthorizationPermissionMismatch (403)` on `Exists()` or `GetProperties()`

**Causes:**
1. ❌ Role not assigned to your local identity
2. ❌ Role assigned at wrong scope (resource group instead of storage account)
3. ❌ Wrong role (e.g., `Storage Account Contributor` instead of `Storage Blob Data Contributor`)
4. ❌ Storage account firewall blocking access
5. ❌ Identity not authenticated (Azure CLI not logged in)

**Diagnosis:**

1. **Check if you're authenticated:**
   ```bash
   az account show
   ```

2. **Check role assignments:**
   ```bash
   az role assignment list \
     --scope "/subscriptions/<subscription-id>/resourceGroups/<rg-name>/providers/Microsoft.Storage/storageAccounts/<storage-account-name>" \
     --assignee $(az account show --query user.name -o tsv) \
     --output table
   ```

3. **Check storage account firewall:**
   ```bash
   az storage account show \
     --name <storage-account-name> \
     --resource-group <resource-group-name> \
     --query "{networkRuleSet:networkRuleSet.defaultAction, allowAzureServices:networkRuleSet.bypass}"
   ```
   
   If `defaultAction` is `Deny`, ensure `bypass` includes `AzureServices` or add your IP.

4. **Test with Azure CLI:**
   ```bash
   az storage blob list \
     --account-name <storage-account-name> \
     --container-name narrative-artifacts \
     --auth-mode login
   ```
   
   If this fails, the role assignment is the issue.

## Role Comparison

| Role | Read Blobs | Write Blobs | List Blobs | Create Containers | Manage Account |
|------|------------|-------------|------------|-------------------|----------------|
| **Storage Blob Data Contributor** | ✅ | ✅ | ✅ | ✅ | ❌ |
| Storage Blob Data Reader | ✅ | ❌ | ✅ | ❌ | ❌ |
| Storage Account Contributor | ❌ | ❌ | ❌ | ❌ | ✅ |

**Use `Storage Blob Data Contributor`** - it's the minimal role that provides all needed permissions.

## Container-Level Scope (Not Needed for Your Case)

If you needed container-level scope (you don't), you'd use:

```bash
az role assignment create \
  --role "Storage Blob Data Contributor" \
  --assignee <principal-id> \
  --scope "/subscriptions/<subscription-id>/resourceGroups/<rg-name>/providers/Microsoft.Storage/storageAccounts/<storage-account-name>/blobServices/default/containers/narrative-artifacts"
```

**Why you don't need this:**
- Your app only uses one container
- Container creation requires account-level permissions anyway
- Simpler to manage one assignment

## Quick Fix Script

For local development, run this PowerShell script:

```powershell
# Get storage account details
$storageAccountName = Read-Host "Enter storage account name"
$resourceGroupName = Read-Host "Enter resource group name"

# Get current user
$currentUser = (az account show --query user.name -o tsv)

# Get storage account ID
$storageAccountId = (az storage account show `
  --name $storageAccountName `
  --resource-group $resourceGroupName `
  --query id -o tsv)

# Assign role
az role assignment create `
  --role "Storage Blob Data Contributor" `
  --assignee $currentUser `
  --scope $storageAccountId

Write-Host "Role assigned to $currentUser on $storageAccountName"
```

## Summary

1. **Role:** `Storage Blob Data Contributor` ✅
2. **Scope:** Storage Account level ✅ (already correct in infrastructure)
3. **Azure (Managed Identity):** Already configured ✅
4. **Local Dev:** Assign the same role to your Azure CLI/VS identity ⚠️ (this is what you're missing)

The 403 error is because your local identity doesn't have the role assignment. Run the Azure CLI command above to fix it.
