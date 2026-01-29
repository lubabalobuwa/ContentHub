using '../main.bicep'

param location = 'South Africa North'
param staticWebAppLocation = 'Central US'

param appServicePlanName = 'asp-contenthub-dev'
param appServiceSkuName = 'B1'
param appServiceSkuTier = 'Basic'
param apiAppName = 'app-contenthub-api-dev'

param sqlServerName = 'sql-contenthub-dev'
param sqlDbName = 'contenthub'
param sqlAdminLogin = 'contenthubadmin'

param serviceBusNamespaceName = 'sb-contenthub-dev'
param serviceBusQueueName = 'content-published'

param staticWebAppName = 'swa-contenthub-dev'
