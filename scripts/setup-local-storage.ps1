# Setup script for local Azure Blob Storage development using Azurite
# This script helps developers set up Azurite for local blob storage development

Write-Host "Setting up local Azure Blob Storage development environment..." -ForegroundColor Green

# Check if Azurite is installed
$azuriteInstalled = Get-Command azurite -ErrorAction SilentlyContinue

if (-not $azuriteInstalled) {
    Write-Host "Azurite is not installed. Installing via npm..." -ForegroundColor Yellow
    npm install -g azurite
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Failed to install Azurite. Please install it manually: npm install -g azurite" -ForegroundColor Red
        exit 1
    }
}

# Check if Azurite is running
$azuriteProcess = Get-Process -Name "azurite" -ErrorAction SilentlyContinue

if ($azuriteProcess) {
    Write-Host "Azurite is already running (PID: $($azuriteProcess.Id))" -ForegroundColor Yellow
    Write-Host "If you need to restart it, stop the process first." -ForegroundColor Yellow
} else {
    Write-Host "Starting Azurite..." -ForegroundColor Green
    Start-Process -NoNewWindow azurite -ArgumentList "--silent", "--location", "$PSScriptRoot/../.azurite", "--debug", "$PSScriptRoot/../.azurite/debug.log"
    Start-Sleep -Seconds 2
    
    if (Get-Process -Name "azurite" -ErrorAction SilentlyContinue) {
        Write-Host "Azurite started successfully!" -ForegroundColor Green
    } else {
        Write-Host "Failed to start Azurite. Please start it manually: azurite" -ForegroundColor Red
        exit 1
    }
}

# Set environment variables for local development
$endpoint = "http://127.0.0.1:10000/devstoreaccount1"
$containerName = "narrative-artifacts"

Write-Host ""
Write-Host "Local storage configuration:" -ForegroundColor Cyan
Write-Host "  Endpoint: $endpoint" -ForegroundColor White
Write-Host "  Container: $containerName" -ForegroundColor White
Write-Host ""
Write-Host "Set these environment variables in your development environment:" -ForegroundColor Yellow
Write-Host "  AZURE_BLOB_STORAGE_ENDPOINT=$endpoint" -ForegroundColor White
Write-Host "  AZURE_BLOB_STORAGE_CONTAINER_NAME=$containerName" -ForegroundColor White
Write-Host ""
Write-Host "For Visual Studio / VS Code, add these to your launchSettings.json or .env file" -ForegroundColor Yellow
Write-Host ""
Write-Host "Note: The container will be created automatically when the application first accesses it." -ForegroundColor Cyan
