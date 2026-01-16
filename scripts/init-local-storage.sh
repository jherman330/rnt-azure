#!/bin/bash
# Initialize local Azurite storage with the narrative-artifacts container
# This script creates the container in Azurite for local development

set -e

ENDPOINT="${AZURE_BLOB_STORAGE_ENDPOINT:-http://127.0.0.1:10000/devstoreaccount1}"
CONTAINER_NAME="${AZURE_BLOB_STORAGE_CONTAINER_NAME:-narrative-artifacts}"

echo "Initializing local Azurite storage..."
echo "Endpoint: $ENDPOINT"
echo "Container: $CONTAINER_NAME"

# Check if Azure CLI is available (for az storage commands)
if command -v az &> /dev/null; then
    echo "Using Azure CLI to create container..."
    
    # Set connection string for Azurite
    export AZURE_STORAGE_CONNECTION_STRING="DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=$ENDPOINT;"
    
    # Create container if it doesn't exist
    az storage container create \
        --name "$CONTAINER_NAME" \
        --connection-string "$AZURE_STORAGE_CONNECTION_STRING" \
        --public-access off \
        || echo "Container may already exist or Azurite is not running"
    
    echo "Container '$CONTAINER_NAME' initialized successfully!"
else
    echo "Azure CLI not found. Skipping container creation."
    echo "The container will be created automatically when the application first accesses it."
    echo ""
    echo "Alternatively, install Azure CLI: https://docs.microsoft.com/en-us/cli/azure/install-azure-cli"
fi
