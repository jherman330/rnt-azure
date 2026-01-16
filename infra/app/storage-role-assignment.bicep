param storageAccountResourceId string
param apiPrincipalId string

// Create a role assignment for the API's managed identity to access Storage Account
// Using the built-in "Storage Blob Data Contributor" role
resource apiStorageRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(apiPrincipalId, storageAccountResourceId, 'ba92f5b4-2d11-453d-a403-e96b0029c9fe')
  properties: {
    roleDefinitionId: '/subscriptions/${subscription().subscriptionId}/providers/Microsoft.Authorization/roleDefinitions/ba92f5b4-2d11-453d-a403-e96b0029c9fe' // Storage Blob Data Contributor
    principalId: apiPrincipalId
    principalType: 'ServicePrincipal'
  }
}
