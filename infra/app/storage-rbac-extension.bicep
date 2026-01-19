param storageAccountName string
param apiPrincipalId string

// Reference the existing storage account to scope the role assignment
// This module is deployed after both storage and api modules to avoid circular dependencies
resource storageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' existing = {
  name: storageAccountName
}

// Create role assignment for API managed identity to access Storage Account
// Using the built-in "Storage Blob Data Contributor" role
// IMPORTANT: The scope property ensures the role is assigned at the storage account level, not the resource group level
resource apiStorageRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(apiPrincipalId, storageAccount.id, 'ba92f5b4-2d11-453d-a403-e96b0029c9fe')
  scope: storageAccount
  properties: {
    roleDefinitionId: '/subscriptions/${subscription().subscriptionId}/providers/Microsoft.Authorization/roleDefinitions/ba92f5b4-2d11-453d-a403-e96b0029c9fe' // Storage Blob Data Contributor
    principalId: apiPrincipalId
    principalType: 'ServicePrincipal'
  }
}
