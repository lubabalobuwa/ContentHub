targetScope = 'resourceGroup'

@description('Location for resources that support the selected region.')
param location string

@description('Location for Static Web Apps (must be a supported SWA region).')
param staticWebAppLocation string = 'Central US'

@description('App Service plan name.')
param appServicePlanName string

@description('App Service plan SKU name (e.g., F1, B1).')
param appServiceSkuName string = 'F1'

@description('App Service plan SKU tier (e.g., Free, Basic).')
param appServiceSkuTier string = 'Free'

@description('API App Service name.')
param apiAppName string

@description('RabbitMQ connection string.')
param rabbitMqConnectionString string

@secure()
@description('JWT signing key.')
param jwtKey string

@description('JWT issuer.')
param jwtIssuer string = 'ContentHub'

@description('JWT audience.')
param jwtAudience string = 'ContentHub'

@description('JWT expiration in minutes.')
param jwtExpiresMinutes int = 60

@description('JWT refresh token expiration in days.')
param jwtRefreshTokenDays int = 7

@description('Require email verification before login.')
param authRequireEmailVerification bool = true

@description('Email verification token expiration in hours.')
param authEmailVerificationTokenHours int = 24

@description('Password reset token expiration in hours.')
param authPasswordResetTokenHours int = 1

@description('Base URL used to build auth links.')
param authBaseUrl string

@description('Allowed hosts (comma separated) for ASP.NET Core.')
param allowedHosts string = '*'

@description('Allowed CORS origins for the API app.')
param corsAllowedOrigins array = []

@description('Enable reset password feature.')
param authEnableResetPassword bool = false

@description('Enable messaging (RabbitMQ).')
param messagingEnabled bool = false

@description('Run EF Core migrations on API startup.')
param migrationsEnabled bool = true

@description('Key Vault name (must be globally unique).')
param keyVaultName string

@description('Resource group name for Key Vault (defaults to current RG).')
param keyVaultResourceGroupName string = resourceGroup().name

@description('Use an existing Key Vault instead of creating one.')
param useExistingKeyVault bool = false

@description('Log Analytics workspace name (must be globally unique).')
param logAnalyticsWorkspaceName string

@description('Application Insights name (must be globally unique).')
param appInsightsName string

@description('SMTP host.')
param smtpHost string

@description('SMTP port.')
param smtpPort int = 587

@description('SMTP username.')
param smtpUsername string

@secure()
@description('SMTP password or API key.')
param smtpPassword string

@secure()
@description('Turnstile secret key.')
param turnstileSecretKey string

@description('SMTP from address.')
param smtpFromAddress string

@description('SMTP from name.')
param smtpFromName string = 'TechContentHub'

@description('SMTP UseStartTls.')
param smtpUseStartTls bool = true

@description('SQL server name (must be globally unique).')
param sqlServerName string

@description('SQL database name.')
param sqlDbName string = 'contenthub'

@description('SQL admin username.')
param sqlAdminLogin string

@secure()
@description('SQL admin password.')
param sqlAdminPassword string

@description('Service Bus namespace name (must be globally unique).')
param serviceBusNamespaceName string

@description('Service Bus queue name.')
param serviceBusQueueName string = 'content-published'

@description('Static Web App name.')
param staticWebAppName string

@description('Static Web App repository URL.')
param staticWebAppRepositoryUrl string

@description('Static Web App branch.')
param staticWebAppBranch string = 'main'

@description('Static Web App provider (GitHub or DevOps).')
param staticWebAppProvider string = 'GitHub'

@description('Static Web App app location (relative to repo root).')
param staticWebAppAppLocation string

@description('Static Web App artifact output location (relative to repo root).')
param staticWebAppArtifactLocation string

@description('Static Web App API location (relative to repo root).')
param staticWebAppApiLocation string = ''

@description('Storage account name for blob uploads (must be globally unique).')
param storageAccountName string

@description('Resource group name for the storage account (defaults to current RG).')
param storageResourceGroupName string = resourceGroup().name

@description('Use an existing storage account instead of creating one.')
param useExistingStorage bool = false

@description('Blob container name for uploads.')
param blobContainerName string = 'contenthub'

@description('Allowed CORS origins for Blob Storage uploads.')
param blobCorsAllowedOrigins array = []

var corsAppSettings = [for (origin, i) in corsAllowedOrigins: {
  name: 'Cors__AllowedOrigins__${i}'
  value: origin
}]

var sqlConnectionString = 'Server=tcp:${sqlServerName}.database.windows.net,1433;Initial Catalog=${sqlDbName};Persist Security Info=False;User ID=${sqlAdminLogin};Password=${sqlAdminPassword};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;'
var keyVaultScope = resourceGroup(keyVaultResourceGroupName)

resource existingKeyVault 'Microsoft.KeyVault/vaults@2022-07-01' existing = if (useExistingKeyVault) {
  name: keyVaultName
  scope: keyVaultScope
}

resource keyVault 'Microsoft.KeyVault/vaults@2022-07-01' = if (!useExistingKeyVault) {
  name: keyVaultName
  location: location
  properties: {
    tenantId: subscription().tenantId
    sku: {
      name: 'standard'
      family: 'A'
    }
  }
}

var keyVaultRef = useExistingKeyVault ? existingKeyVault : keyVault
var keyVaultUri = 'https://${keyVaultRef.name}.vault.azure.net/'
var jwtKeySecretName = 'jwt-key'
var sqlConnectionSecretName = 'sql-connection-string'
var rabbitMqSecretName = 'rabbitmq-connection-string'
var smtpPasswordSecretName = 'smtp-password'
var turnstileSecretName = 'turnstile-secret-key'
var blobConnectionSecretName = 'blob-connection-string'
var storageAccountScope = resourceGroup(storageResourceGroupName)

resource existingStorageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' existing = if (useExistingStorage) {
  name: storageAccountName
  scope: storageAccountScope
}

resource storageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' = if (!useExistingStorage) {
  name: storageAccountName
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {
    allowBlobPublicAccess: true
    minimumTlsVersion: 'TLS1_2'
  }
}

var storageAccountRef = useExistingStorage ? existingStorageAccount : storageAccount
var blobPublicBaseUrl = '${storageAccountRef.properties.primaryEndpoints.blob}${blobContainerName}'
var blobConnectionString = 'DefaultEndpointsProtocol=https;AccountName=${storageAccountRef.name};AccountKey=${listKeys(storageAccountRef.id, storageAccountRef.apiVersion).keys[0].value};EndpointSuffix=${environment().suffixes.storage}'

resource appServicePlan 'Microsoft.Web/serverfarms@2022-09-01' = {
  name: appServicePlanName
  location: location
  kind: 'linux'
  sku: {
    name: appServiceSkuName
    tier: appServiceSkuTier
  }
  properties: {
    reserved: true
  }
}

resource apiApp 'Microsoft.Web/sites@2022-09-01' = {
  name: apiAppName
  location: location
  kind: 'app,linux'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|8.0'
      appSettings: concat([
        {
          name: 'RabbitMq__ConnectionString'
          value: '@Microsoft.KeyVault(SecretUri=${keyVaultUri}secrets/${rabbitMqSecretName}/)'
        }
        {
          name: 'Jwt__Key'
          value: '@Microsoft.KeyVault(SecretUri=${keyVaultUri}secrets/${jwtKeySecretName}/)'
        }
        {
          name: 'Jwt__Issuer'
          value: jwtIssuer
        }
        {
          name: 'Jwt__Audience'
          value: jwtAudience
        }
        {
          name: 'Jwt__ExpiresMinutes'
          value: string(jwtExpiresMinutes)
        }
        {
          name: 'Jwt__RefreshTokenDays'
          value: string(jwtRefreshTokenDays)
        }
        {
          name: 'Auth__RequireEmailVerification'
          value: string(authRequireEmailVerification)
        }
        {
          name: 'Auth__EmailVerificationTokenHours'
          value: string(authEmailVerificationTokenHours)
        }
        {
          name: 'Auth__PasswordResetTokenHours'
          value: string(authPasswordResetTokenHours)
        }
        {
          name: 'Auth__BaseUrl'
          value: authBaseUrl
        }
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: appInsights.properties.ConnectionString
        }
        {
          name: 'Auth__EnableResetPassword'
          value: string(authEnableResetPassword)
        }
        {
          name: 'Messaging__Enabled'
          value: string(messagingEnabled)
        }
        {
          name: 'Migrations__Enabled'
          value: string(migrationsEnabled)
        }
        {
          name: 'Smtp__Host'
          value: smtpHost
        }
        {
          name: 'Smtp__Port'
          value: string(smtpPort)
        }
        {
          name: 'Smtp__Username'
          value: smtpUsername
        }
        {
          name: 'Smtp__Password'
          value: '@Microsoft.KeyVault(SecretUri=${keyVaultUri}secrets/${smtpPasswordSecretName}/)'
        }
        {
          name: 'Smtp__FromAddress'
          value: smtpFromAddress
        }
        {
          name: 'Smtp__FromName'
          value: smtpFromName
        }
        {
          name: 'Smtp__UseStartTls'
          value: string(smtpUseStartTls)
        }
        {
          name: 'Turnstile__SecretKey'
          value: '@Microsoft.KeyVault(SecretUri=${keyVaultUri}secrets/${turnstileSecretName}/)'
        }
        {
          name: 'BlobStorage__ConnectionString'
          value: '@Microsoft.KeyVault(SecretUri=${keyVaultUri}secrets/${blobConnectionSecretName}/)'
        }
        {
          name: 'BlobStorage__ContainerName'
          value: blobContainerName
        }
        {
          name: 'BlobStorage__PublicBaseUrl'
          value: blobPublicBaseUrl
        }
        {
          name: 'AllowedHosts'
          value: allowedHosts
        }
      ], corsAppSettings)
    }
  }
}

resource apiConnectionStrings 'Microsoft.Web/sites/config@2022-09-01' = {
  name: '${apiApp.name}/connectionstrings'
  properties: {
    DefaultConnection: {
      type: 'SQLAzure'
      value: '@Microsoft.KeyVault(SecretUri=${keyVaultUri}secrets/${sqlConnectionSecretName}/)'
    }
  }
}

resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2022-10-01' = {
  name: logAnalyticsWorkspaceName
  location: location
  properties: {
    retentionInDays: 30
  }
  sku: {
    name: 'PerGB2018'
  }
}

resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: appInsightsName
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logAnalyticsWorkspace.id
  }
}

resource keyVaultAccessPolicy 'Microsoft.KeyVault/vaults/accessPolicies@2022-07-01' = {
  name: 'add'
  parent: keyVaultRef
  properties: {
    accessPolicies: [
      {
        tenantId: subscription().tenantId
        objectId: apiApp.identity.principalId
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
  name: '${keyVaultRef.name}/${jwtKeySecretName}'
  properties: {
    value: jwtKey
  }
}

resource keyVaultSqlConnection 'Microsoft.KeyVault/vaults/secrets@2022-07-01' = {
  name: '${keyVaultRef.name}/${sqlConnectionSecretName}'
  properties: {
    value: sqlConnectionString
  }
}

resource keyVaultRabbitMq 'Microsoft.KeyVault/vaults/secrets@2022-07-01' = {
  name: '${keyVaultRef.name}/${rabbitMqSecretName}'
  properties: {
    value: rabbitMqConnectionString
  }
}

resource keyVaultSmtpPassword 'Microsoft.KeyVault/vaults/secrets@2022-07-01' = {
  name: '${keyVaultRef.name}/${smtpPasswordSecretName}'
  properties: {
    value: smtpPassword
  }
}

resource keyVaultTurnstileSecret 'Microsoft.KeyVault/vaults/secrets@2022-07-01' = {
  name: '${keyVaultRef.name}/${turnstileSecretName}'
  properties: {
    value: turnstileSecretKey
  }
}

resource keyVaultBlobConnection 'Microsoft.KeyVault/vaults/secrets@2022-07-01' = {
  name: '${keyVaultRef.name}/${blobConnectionSecretName}'
  properties: {
    value: blobConnectionString
  }
}

resource sqlServer 'Microsoft.Sql/servers@2023-05-01-preview' = {
  name: sqlServerName
  location: location
  properties: {
    administratorLogin: sqlAdminLogin
    administratorLoginPassword: sqlAdminPassword
    version: '12.0'
  }
}

resource sqlFirewallAllowAzure 'Microsoft.Sql/servers/firewallRules@2023-05-01-preview' = {
  name: 'AllowAzureServices'
  parent: sqlServer
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

resource sqlDb 'Microsoft.Sql/servers/databases@2023-05-01-preview' = {
  name: sqlDbName
  parent: sqlServer
  location: location
  sku: {
    name: 'Basic'
    tier: 'Basic'
  }
  properties: {
    collation: 'SQL_Latin1_General_CP1_CI_AS'
  }
}

resource serviceBusNamespace 'Microsoft.ServiceBus/namespaces@2022-10-01-preview' = {
  name: serviceBusNamespaceName
  location: location
  sku: {
    name: 'Basic'
    tier: 'Basic'
  }
}

resource serviceBusQueue 'Microsoft.ServiceBus/namespaces/queues@2022-10-01-preview' = {
  name: serviceBusQueueName
  parent: serviceBusNamespace
  properties: {
    enablePartitioning: false
  }
}

resource staticWebApp 'Microsoft.Web/staticSites@2022-03-01' = {
  name: staticWebAppName
  location: staticWebAppLocation
  sku: {
    name: 'Free'
    tier: 'Free'
  }
  properties: {
    repositoryUrl: staticWebAppRepositoryUrl
    branch: staticWebAppBranch
    provider: staticWebAppProvider
    allowConfigFileUpdates: true
    buildProperties: {
      appLocation: staticWebAppAppLocation
      appArtifactLocation: staticWebAppArtifactLocation
      apiLocation: staticWebAppApiLocation
      skipGithubActionWorkflowGeneration: true
    }
  }
}

resource blobService 'Microsoft.Storage/storageAccounts/blobServices@2023-01-01' = {
  name: 'default'
  parent: storageAccountRef
  properties: {
    cors: {
      corsRules: [
        {
          allowedOrigins: blobCorsAllowedOrigins
          allowedMethods: [
            'GET'
            'PUT'
            'POST'
            'HEAD'
          ]
          allowedHeaders: [
            '*'
          ]
          exposedHeaders: [
            '*'
          ]
          maxAgeInSeconds: 3600
        }
      ]
    }
  }
}

resource blobContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-01-01' = {
  name: blobContainerName
  parent: blobService
  properties: {
    publicAccess: 'Blob'
  }
}

output apiAppName string = apiApp.name
output staticWebAppName string = staticWebApp.name
