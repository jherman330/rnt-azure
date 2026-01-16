#!/bin/bash
# Setup script for local Azure Blob Storage development using Azurite
# This script helps developers set up Azurite for local blob storage development

set -e

echo "Setting up local Azure Blob Storage development environment..."

# Check if Azurite is installed
if ! command -v azurite &> /dev/null; then
    echo "Azurite is not installed. Installing via npm..."
    npm install -g azurite
    if [ $? -ne 0 ]; then
        echo "Failed to install Azurite. Please install it manually: npm install -g azurite"
        exit 1
    fi
fi

# Check if Azurite is running
if pgrep -x "azurite" > /dev/null; then
    echo "Azurite is already running (PID: $(pgrep -x azurite))"
    echo "If you need to restart it, stop the process first."
else
    echo "Starting Azurite..."
    # Create .azurite directory if it doesn't exist
    mkdir -p "$(dirname "$0")/../.azurite"
    
    # Start Azurite in the background
    azurite --silent --location "$(dirname "$0")/../.azurite" --debug "$(dirname "$0")/../.azurite/debug.log" &
    AZURITE_PID=$!
    
    # Wait a moment for Azurite to start
    sleep 2
    
    # Check if Azurite is still running
    if ps -p $AZURITE_PID > /dev/null; then
        echo "Azurite started successfully! (PID: $AZURITE_PID)"
        echo "To stop Azurite, run: kill $AZURITE_PID"
    else
        echo "Failed to start Azurite. Please start it manually: azurite"
        exit 1
    fi
fi

# Set environment variables for local development
ENDPOINT="http://127.0.0.1:10000/devstoreaccount1"
CONTAINER_NAME="narrative-artifacts"

echo ""
echo "Local storage configuration:"
echo "  Endpoint: $ENDPOINT"
echo "  Container: $CONTAINER_NAME"
echo ""
echo "Set these environment variables in your development environment:"
echo "  export AZURE_BLOB_STORAGE_ENDPOINT=$ENDPOINT"
echo "  export AZURE_BLOB_STORAGE_CONTAINER_NAME=$CONTAINER_NAME"
echo ""
echo "For VS Code, add these to your .env file or launch configuration"
echo ""
echo "Note: The container will be created automatically when the application first accesses it."
