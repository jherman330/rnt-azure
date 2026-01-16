# Azure Blob Storage Quick Start

Quick reference for setting up and using Azure Blob Storage in this project.

## What is This?

Azure Blob Storage is used to store narrative artifacts (story roots and world states) in a versioned, append-only format. The storage account is automatically provisioned when you run `azd up`.

## Quick Setup

### For Local Development

1. **Start Azurite** (choose one method):
   ```bash
   # Option 1: Using script
   ./scripts/setup-local-storage.sh  # Linux/macOS
   .\scripts\setup-local-storage.ps1 # Windows
   
   # Option 2: Using Docker
   docker-compose -f docker-compose.dev.yml up -d
   
   # Option 3: Manual
   npm install -g azurite
   azurite
   ```

2. **Set environment variables**:
   ```bash
   export AZURE_BLOB_STORAGE_ENDPOINT=http://127.0.0.1:10000/devstoreaccount1
   export AZURE_BLOB_STORAGE_CONTAINER_NAME=narrative-artifacts
   ```

3. **Run your application** - it will automatically connect to Azurite!

### For Azure Deployment

No setup needed! The infrastructure automatically:
- ✅ Creates the storage account
- ✅ Creates the `narrative-artifacts` container
- ✅ Configures App Service with connection details
- ✅ Sets up managed identity authentication

Just run:
```bash
azd up
```

## Configuration

| Environment Variable | Description | Default |
|---------------------|-------------|---------|
| `AZURE_BLOB_STORAGE_ENDPOINT` | Storage account endpoint URL | (required) |
| `AZURE_BLOB_STORAGE_CONTAINER_NAME` | Container name | `narrative-artifacts` |

## Storage Structure

```
narrative-artifacts/
└── users/
    └── {user_id}/
        ├── story-root/root/versions/{version_id}.json
        └── world-state/world/versions/{version_id}.json
```

## Authentication

- **Azure (Production)**: Uses Managed Identity (automatic, no keys needed)
- **Local (Azurite)**: Uses DefaultAzureCredential (works with Azure CLI login)

## Common Tasks

### Check if Storage is Working

1. Start your application
2. Create a story root or world state via the API
3. Check Azurite logs or Azure Portal to see the blob

### View Local Storage Data

Azurite stores data in `.azurite/` directory (if using `--location` flag) or in Docker volume.

### Reset Local Storage

```bash
# Stop Azurite
# Delete .azurite directory or Docker volume
# Restart Azurite
```

## Troubleshooting

**Problem**: "Container not found"  
**Solution**: Container is created automatically. Ensure Azurite is running.

**Problem**: "Authentication failed"  
**Solution**: 
- Local: Ensure Azurite is running on port 10000
- Azure: Verify managed identity role assignment exists

**Problem**: "Endpoint not configured"  
**Solution**: Set `AZURE_BLOB_STORAGE_ENDPOINT` environment variable

## More Information

- 📖 [Local Development Guide](./docs/local-development-storage.md)
- 📖 [Configuration Reference](./docs/storage-configuration.md)
- 🔧 [Infrastructure Code](./infra/app/storage-avm.bicep)

## Support

If you encounter issues:
1. Check the [troubleshooting section](./docs/local-development-storage.md#troubleshooting)
2. Verify environment variables are set correctly
3. Ensure Azurite/Azure storage account is accessible
