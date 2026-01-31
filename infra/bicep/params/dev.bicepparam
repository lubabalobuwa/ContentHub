using '../main.bicep'

param location = 'South Africa North'
param staticWebAppLocation = 'Central US'

param appServicePlanName = 'asp-contenthub-dev'
param appServiceSkuName = 'F1'
param appServiceSkuTier = 'Free'
param apiAppName = 'app-contenthub-api-dev'

param sqlServerName = 'sql-contenthub-dev'
param sqlDbName = 'contenthub'
param sqlAdminLogin = 'contenthubadmin'

param jwtRefreshTokenDays = 7

param corsAllowedOrigins = [
  'http://localhost:4200'
  'https://yellow-moss-0baf47510.6.azurestaticapps.net'
  'https://www.techcontenthub.live'
  'https://techcontenthub.live'
]

param messagingEnabled = false

param migrationsEnabled = true

param keyVaultName = 'kv-contenthub-dev'

param serviceBusNamespaceName = 'sb-contenthub-dev'
param serviceBusQueueName = 'content-published'

param staticWebAppName = 'swa-contenthub-dev'
