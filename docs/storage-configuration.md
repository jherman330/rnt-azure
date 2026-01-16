# Storage Configuration Reference

This document provides a comprehensive reference for Azure Blob Storage configuration in the narrative artifact system.

## Environment Variables

The application uses the following environment variables for blob storage configuration:

### Required

- **`AZURE_BLOB_STORAGE_ENDPOINT`**: The blob storage account endpoint URL
  - Format: `https://<account-name>.blob.core.windows.net` (Azure)
  - Format: `http://127.0.0.1:10000/devstoreaccount1` (Azurite local)
  - Example: `https://stabc123def456.blob.core.windows.net`

### Optional

- **`AZURE_BLOB_STORAGE_CONTAINER_NAME`**: The container name for storing artifacts
  - Default: `narrative-artifacts`
  - Must be lowercase, alphanumeric, and hyphens only
  - Length: 3-63 characters

## Configuration by Environment

### Local Development

```bash
AZURE_BLOB_STORAGE_ENDPOINT=http://127.0.0.1:10000/devstoreaccount1
AZURE_BLOB_STORAGE_CONTAINER_NAME=narrative-artifacts
```

Uses Azurite emulator. See [Local Development Storage Setup](./local-development-storage.md) for details.

### Azure App Service (Production/Staging)

The infrastructure automatically configures these via App Service application settings:

```bicep
appSettings: {
  AZURE_BLOB_STORAGE_ENDPOINT: storage.outputs.endpoint
  AZURE_BLOB_STORAGE_CONTAINER_NAME: storage.outputs.containerName
}
```

The storage account endpoint is automatically provided by the infrastructure deployment.

## Authentication

### Managed Identity (Production)

In Azure environments, the application uses **Managed Identity** authentication:

1. The App Service has a system-assigned managed identity
2. The managed identity is granted the **Storage Blob Data Contributor** role on the storage account
3. The application uses `DefaultAzureCredential` which automatically uses the managed identity

**No connection strings or keys are required** in production environments.

### Local Development

For local development with Azurite:
- Uses `DefaultAzureCredential` which falls back to local Azure CLI credentials
- Azurite accepts any credentials (default: `devstoreaccount1`)

## Container Structure

The storage container follows a path-based hierarchy:

```
narrative-artifacts/
├── users/
│   └── {user_id}/
│       ├── story-root/
│       │   └── root/
│       │       ├── versions/
│       │       │   └── {version_id}.json
│       │       └── current.json
│       └── world-state/
│           └── world/
│               ├── versions/
│               │   └── {version_id}.json
│               └── current.json
```

### Path Patterns

- **Version blobs**: `users/{user_id}/{artifact_type}/{artifact_id}/versions/{version_id}.json`
- **Current pointers**: `users/{user_id}/{artifact_type}/{artifact_id}/current.json`

Where:
- `{user_id}`: User identifier from authentication context
- `{artifact_type}`: Either `story-root` or `world-state`
- `{artifact_id}`: `root` for story-root, `world` for world-state
- `{version_id}`: Unique version identifier (UUID)

## Storage Account Configuration

### Infrastructure Settings

The storage account is provisioned with the following settings:

- **Kind**: `StorageV2` (General Purpose v2)
- **SKU**: `Standard_LRS` (Standard Locally Redundant Storage)
- **Access Tier**: `Hot` (for active narrative artifacts)
- **HTTPS Only**: Enabled
- **Public Access**: Disabled (blobs are private)
- **Minimum TLS Version**: TLS 1.2

### Network Access

- **Default Action**: Allow (can be restricted in production)
- **Bypass**: AzureServices (allows managed identity access)

## Application Configuration Files

### appsettings.json

```json
{
  "Azure": {
    "BlobStorage": {
      "Endpoint": "",
      "ContainerName": "narrative-artifacts"
    }
  }
}
```

**Note**: Actual values come from environment variables in deployed environments. The appsettings files serve as documentation and placeholders.

### appsettings.Development.json

Same structure as `appsettings.json`. Override with local environment variables for Azurite.

### appsettings.Production.json

Same structure. Values are provided by Azure App Service configuration (from infrastructure).

## Code Configuration

The application reads configuration in `Program.cs`:

```csharp
var blobStorageEndpoint = builder.Configuration["AZURE_BLOB_STORAGE_ENDPOINT"];
if (!string.IsNullOrEmpty(blobStorageEndpoint))
{
    builder.Services.AddSingleton(_ => new BlobServiceClient(new Uri(blobStorageEndpoint), credential));
    builder.Services.AddSingleton<IBlobArtifactRepository, BlobArtifactRepository>();
    // ... other repositories
}
```

The `BlobArtifactRepository` reads the container name:

```csharp
_containerName = configuration["AZURE_BLOB_STORAGE_CONTAINER_NAME"] ?? "narrative-artifacts";
```

## Infrastructure Outputs

The infrastructure (`main.bicep`) outputs:

- `AZURE_BLOB_STORAGE_ENDPOINT`: Storage account blob endpoint
- `AZURE_BLOB_STORAGE_CONTAINER_NAME`: Container name (always `narrative-artifacts`)

These are automatically mapped to App Service application settings during deployment.

## Security Considerations

### Production

1. **Managed Identity**: No connection strings or keys stored
2. **Private Access**: Blobs are not publicly accessible
3. **HTTPS Only**: All traffic encrypted
4. **RBAC**: Least privilege access (Storage Blob Data Contributor role only)
5. **Network Rules**: Can be restricted to App Service subnet in production

### Local Development

1. **Azurite**: No real credentials required
2. **Local Only**: Azurite runs on localhost only
3. **No Production Data**: Never use production storage accounts for local development

## Troubleshooting

### Configuration Not Found

- Verify environment variables are set in App Service (Azure Portal → App Service → Configuration)
- Check that infrastructure deployment completed successfully
- Verify `AZURE_BLOB_STORAGE_ENDPOINT` is not empty

### Authentication Failures

- **Production**: Verify managed identity is enabled and role assignment exists
- **Local**: Ensure Azurite is running and endpoint URL is correct
- Check that `DefaultAzureCredential` can authenticate (Azure CLI logged in for local)

### Container Access Issues

- Container is created automatically on first access
- Verify container name matches: `narrative-artifacts`
- Check RBAC role assignment in Azure Portal

### Network Issues

- Verify storage account firewall rules (if enabled)
- Check that App Service can reach storage account endpoint
- For local: ensure Azurite is listening on correct port (10000)

## Related Documentation

- [Local Development Storage Setup](./local-development-storage.md)
- [Infrastructure Documentation](../infra/README.md) (if exists)
- [Azure Blob Storage Documentation](https://docs.microsoft.com/azure/storage/blobs/)
