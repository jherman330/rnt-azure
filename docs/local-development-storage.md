# Local Development Storage Setup

This guide explains how to set up Azure Blob Storage for local development using Azurite (Azure Storage Emulator).

## Overview

The application uses Azure Blob Storage to store narrative artifacts (story roots and world states). For local development, you can use Azurite, which emulates Azure Blob Storage services locally.

## Prerequisites

- Node.js and npm (for installing Azurite)
- OR Docker (for using Docker Compose)

## Quick Start

### Option 1: Using Setup Scripts

**Windows (PowerShell):**
```powershell
.\scripts\setup-local-storage.ps1
```

**Linux/macOS:**
```bash
./scripts/setup-local-storage.sh
```

### Option 2: Using Docker Compose

```bash
docker-compose -f docker-compose.dev.yml up -d
```


This will start Azurite in a Docker container with the following ports:
- `10000`: Blob service
- `10001`: Queue service  
- `10002`: Table service

### Option 3: Manual Setup

1. Install Azurite globally:
   ```bash
   npm install -g azurite
   ```

2. Start Azurite:
   ```bash
   azurite --silent --location .azurite
   ```

## Configuration

After starting Azurite, configure your development environment with these environment variables:

```bash
AZURE_BLOB_STORAGE_ENDPOINT=http://127.0.0.1:10000/devstoreaccount1
AZURE_BLOB_STORAGE_CONTAINER_NAME=narrative-artifacts
```

### Visual Studio / VS Code

Add these to your `launchSettings.json` (for Visual Studio) or `.env` file (for VS Code):

**launchSettings.json:**
```json
{
  "environmentVariables": {
    "AZURE_BLOB_STORAGE_ENDPOINT": "http://127.0.0.1:10000/devstoreaccount1",
    "AZURE_BLOB_STORAGE_CONTAINER_NAME": "narrative-artifacts"
  }
}
```

**VS Code .env file:**
```
AZURE_BLOB_STORAGE_ENDPOINT=http://127.0.0.1:10000/devstoreaccount1
AZURE_BLOB_STORAGE_CONTAINER_NAME=narrative-artifacts
```

## Container Creation

The `narrative-artifacts` container will be created automatically when the application first accesses it. However, you can also create it manually using the initialization script:

```bash
./scripts/init-local-storage.sh
```

Or using Azure CLI:
```bash
az storage container create \
  --name narrative-artifacts \
  --connection-string "DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;"
```

## Azurite Default Credentials

When using Azurite locally, use these default credentials:

- **Account Name:** `devstoreaccount1`
- **Account Key:** `Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==`
- **Blob Endpoint:** `http://127.0.0.1:10000/devstoreaccount1`

## Verifying Setup

1. Start Azurite (using one of the methods above)
2. Run your application
3. The application should automatically create the container and be able to read/write blobs

You can verify Azurite is running by checking if the ports are listening:
```bash
# Windows
netstat -an | findstr "10000"

# Linux/macOS
netstat -an | grep 10000
```

## Troubleshooting

### Azurite Not Starting

- Ensure ports 10000, 10001, and 10002 are not in use by another application
- Check if Azurite is already running: `ps aux | grep azurite` (Linux/macOS) or check Task Manager (Windows)
- Try running Azurite with verbose logging: `azurite --debug debug.log`

### Connection Errors

- Verify Azurite is running on the correct port (10000 for blob service)
- Check that the endpoint URL is correct: `http://127.0.0.1:10000/devstoreaccount1`
- Ensure environment variables are set correctly in your development environment

### Container Not Found

- The container is created automatically on first access
- If you need to create it manually, use the initialization script or Azure CLI
- Verify the container name matches: `narrative-artifacts`

### Authentication Issues

- Azurite uses default credentials (see above)
- The application uses `DefaultAzureCredential` which should work with Azurite
- For local development, ensure you're using the endpoint URL format, not connection strings

## Data Persistence

By default, Azurite stores data in memory. To persist data between restarts:

1. Use the `--location` parameter to specify a directory:
   ```bash
   azurite --location .azurite
   ```

2. Or use Docker Compose (which uses a volume for persistence)

## Stopping Azurite

**If started via script:**
- The script will show the process ID
- Stop it using: `kill <PID>` (Linux/macOS) or `Stop-Process -Id <PID>` (Windows)

**If started via Docker Compose:**
```bash
docker-compose -f docker-compose.dev.yml down
```

**If started manually:**
- Press `Ctrl+C` in the terminal where Azurite is running
- Or find and kill the process

## Next Steps

Once Azurite is running and configured:

1. Start your application
2. The application will automatically connect to Azurite
3. Test blob storage operations (create, read, update story roots and world states)

For more information about storage configuration, see [Storage Configuration Reference](./storage-configuration.md).
