targetScope = 'resourceGroup'

@description('Location for resources that support the selected region.')
param location string

@description('Location for Static Web Apps (must be a supported SWA region).')
param staticWebAppLocation string = 'Central US'

@description('App Service plan name.')
param appServicePlanName string

@description('App Service plan SKU name (e.g., F1, B1).')
param appServiceSkuName string = 'B1'

@description('App Service plan SKU tier (e.g., Free, Basic).')
param appServiceSkuTier string = 'Basic'

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

@description('Allowed hosts (comma separated) for ASP.NET Core.')
param allowedHosts string = '*'

@description('Enable reset password feature.')
param authEnableResetPassword bool = false

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
  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|8.0'
      appSettings: [
        {
          name: 'RabbitMq__ConnectionString'
          value: rabbitMqConnectionString
        }
        {
          name: 'Jwt__Key'
          value: jwtKey
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
          name: 'Auth__EnableResetPassword'
          value: string(authEnableResetPassword)
        }
        {
          name: 'AllowedHosts'
          value: allowedHosts
        }
      ]
      connectionStrings: [
        {
          name: 'DefaultConnection'
          type: 'SQLAzure'
          value: 'Server=tcp:${sqlServerName}.database.windows.net,1433;Initial Catalog=${sqlDbName};Persist Security Info=False;User ID=${sqlAdminLogin};Password=${sqlAdminPassword};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;'
        }
      ]
    }
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

output apiAppName string = apiApp.name
output staticWebAppName string = staticWebApp.name
