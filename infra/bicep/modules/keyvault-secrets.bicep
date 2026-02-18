targetScope = 'resourceGroup'

@description('Key Vault name (must be globally unique).')
param keyVaultName string

@description('Tenant ID for access policy.')
param tenantId string

@description('API app principal ID for access policy.')
param apiPrincipalId string

@secure()
@description('JWT signing key.')
param jwtKey string

@secure()
@description('SQL connection string.')
param sqlConnectionString string

@description('RabbitMQ connection string.')
param rabbitMqConnectionString string

@secure()
@description('SMTP password or API key.')
param smtpPassword string

@secure()
@description('Turnstile secret key.')
param turnstileSecretKey string

@secure()
@description('Blob connection string.')
param blobConnectionString string

var jwtKeySecretName = 'jwt-key'
var sqlConnectionSecretName = 'sql-connection-string'
var rabbitMqSecretName = 'rabbitmq-connection-string'
var smtpPasswordSecretName = 'smtp-password'
var turnstileSecretName = 'turnstile-secret-key'
var blobConnectionSecretName = 'blob-connection-string'

resource keyVault 'Microsoft.KeyVault/vaults@2022-07-01' existing = {
  name: keyVaultName
}

resource keyVaultAccessPolicy 'Microsoft.KeyVault/vaults/accessPolicies@2022-07-01' = {
  name: 'add'
  parent: keyVault
  properties: {
    accessPolicies: [
      {
        tenantId: tenantId
        objectId: apiPrincipalId
        permissions: {
          secrets: [
            'get'
            'list'
          ]
        }
      }
    ]
  }
}

resource keyVaultJwtKey 'Microsoft.KeyVault/vaults/secrets@2022-07-01' = {
  name: jwtKeySecretName
  parent: keyVault
  properties: {
    value: jwtKey
  }
}

resource keyVaultSqlConnection 'Microsoft.KeyVault/vaults/secrets@2022-07-01' = {
  name: sqlConnectionSecretName
  parent: keyVault
  properties: {
    value: sqlConnectionString
  }
}

resource keyVaultRabbitMq 'Microsoft.KeyVault/vaults/secrets@2022-07-01' = {
  name: rabbitMqSecretName
  parent: keyVault
  properties: {
    value: rabbitMqConnectionString
  }
}

resource keyVaultSmtpPassword 'Microsoft.KeyVault/vaults/secrets@2022-07-01' = {
  name: smtpPasswordSecretName
  parent: keyVault
  properties: {
    value: smtpPassword
  }
}

resource keyVaultTurnstileSecret 'Microsoft.KeyVault/vaults/secrets@2022-07-01' = {
  name: turnstileSecretName
  parent: keyVault
  properties: {
    value: turnstileSecretKey
  }
}

resource keyVaultBlobConnection 'Microsoft.KeyVault/vaults/secrets@2022-07-01' = {
  name: blobConnectionSecretName
  parent: keyVault
  properties: {
    value: blobConnectionString
  }
}
