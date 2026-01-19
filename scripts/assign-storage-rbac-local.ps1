# Assign Storage Blob Data Contributor role to current user for local development
# This fixes 403 AuthorizationPermissionMismatch errors when using DefaultAzureCredential locally

param(
    [Parameter(Mandatory=$false)]
    [string]$StorageAccountName,
    
    [Parameter(Mandatory=$false)]
    [string]$ResourceGroupName,
    
    [Parameter(Mandatory=$false)]
    [string]$SubscriptionId
)

# Check if Azure CLI is installed and logged in
$azCliCheck = az version 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Error "Azure CLI is not installed or not in PATH. Please install Azure CLI first."
    exit 1
}

# Check if logged in
$accountInfo = az account show 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Error "Not logged in to Azure CLI. Please run 'az login' first."
    exit 1
}

# Get current user
$currentUser = az account show --query user.name -o tsv
if (-not $currentUser) {
    Write-Error "Could not determine current Azure user. Please ensure you're logged in."
    exit 1
}

Write-Host "Current Azure user: $currentUser" -ForegroundColor Cyan

# Get subscription ID if not provided
if (-not $SubscriptionId) {
    $SubscriptionId = az account show --query id -o tsv
    Write-Host "Using subscription: $SubscriptionId" -ForegroundColor Cyan
}

# Get storage account name if not provided
if (-not $StorageAccountName) {
    Write-Host ""
    Write-Host "Storage account name not provided. Listing available storage accounts..." -ForegroundColor Yellow
    $storageAccounts = az storage account list --query "[].{Name:name, ResourceGroup:resourceGroup}" -o table
    Write-Host $storageAccounts
    
    $StorageAccountName = Read-Host "Enter storage account name"
}

# Get resource group if not provided
if (-not $ResourceGroupName) {
    $ResourceGroupName = az storage account show --name $StorageAccountName --query resourceGroup -o tsv 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Could not find storage account '$StorageAccountName'. Please check the name and ensure you have access."
        exit 1
    }
    Write-Host "Found resource group: $ResourceGroupName" -ForegroundColor Cyan
}

# Get storage account resource ID
$storageAccountId = az storage account show `
    --name $StorageAccountName `
    --resource-group $ResourceGroupName `
    --query id -o tsv

if (-not $storageAccountId) {
    Write-Error "Could not retrieve storage account resource ID. Please verify the storage account name and resource group."
    exit 1
}

Write-Host ""
Write-Host "Storage Account: $StorageAccountName" -ForegroundColor Green
Write-Host "Resource Group: $ResourceGroupName" -ForegroundColor Green
Write-Host "Scope: $storageAccountId" -ForegroundColor Green
Write-Host ""

# Check if role assignment already exists
Write-Host "Checking for existing role assignment..." -ForegroundColor Yellow
$existingAssignment = az role assignment list `
    --scope $storageAccountId `
    --assignee $currentUser `
    --role "Storage Blob Data Contributor" `
    --query "[0].id" -o tsv 2>&1

if ($existingAssignment -and $existingAssignment -ne "") {
    Write-Host "Role assignment already exists!" -ForegroundColor Green
    Write-Host "Assignment ID: $existingAssignment" -ForegroundColor Gray
    Write-Host ""
    Write-Host "You should now be able to access blob storage. Try running your application again." -ForegroundColor Cyan
    exit 0
}

# Assign the role
Write-Host "Assigning 'Storage Blob Data Contributor' role..." -ForegroundColor Yellow
$roleAssignment = az role assignment create `
    --role "Storage Blob Data Contributor" `
    --assignee $currentUser `
    --scope $storageAccountId `
    --output json 2>&1

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "✓ Role assigned successfully!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Role: Storage Blob Data Contributor" -ForegroundColor Cyan
    Write-Host "Assigned to: $currentUser" -ForegroundColor Cyan
    Write-Host "Scope: Storage Account level" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "You should now be able to access blob storage. Try running your application again." -ForegroundColor Green
    Write-Host ""
    Write-Host "To verify, run:" -ForegroundColor Yellow
    Write-Host "  az storage blob list --account-name $StorageAccountName --container-name narrative-artifacts --auth-mode login" -ForegroundColor Gray
} else {
    Write-Error "Failed to assign role. Error: $roleAssignment"
    exit 1
}
