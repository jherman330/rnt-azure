param accountName string
param location string = resourceGroup().location
param tags object = {}

@description('Optional. Container name to create. Defaults to narrative-artifacts.')
param containerName string = 'narrative-artifacts'

@description('Optional. Storage account SKU. Defaults to Standard_LRS for cost efficiency in Phase-0.')
param sku string = 'Standard_LRS'

@description('Optional. Access tier. Defaults to Hot for active narrative artifacts.')
param accessTier string = 'Hot'

@description('Optional. Kind of storage account. Defaults to StorageV2 (General Purpose v2).')
param kind string = 'StorageV2'

// Create the storage account
resource storageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' = {
  name: accountName
  location: location
  tags: tags
  kind: kind
  sku: {
    name: sku
  }
  properties: {
    accessTier: accessTier
    supportsHttpsTrafficOnly: true
    allowBlobPublicAccess: false
    minimumTlsVersion: 'TLS1_2'
    networkAcls: {
      defaultAction: 'Allow'
      bypass: 'AzureServices'
    }
  }
}

// Create the blob service (required for containers)
resource blobService 'Microsoft.Storage/storageAccounts/blobServices@2023-01-01' = {
  name: 'default'
  parent: storageAccount
  properties: {
    deleteRetentionPolicy: {
      enabled: false
    }
  }
}

resource container 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-01-01' = {
  name: containerName
  parent: blobService
  properties: {
    publicAccess: 'None'
    metadata: {}
  }
}

output accountName string = storageAccount.name
output endpoint string = storageAccount.properties.primaryEndpoints.blob
output resourceId string = storageAccount.id
output containerName string = containerName
