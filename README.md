# TechContentHub
TechContentHub is a full-stack blog/content platform built with .NET 8, ASP.NET Core Minimal APIs, Clean Architecture, SQL Server, RabbitMQ, and Angular.

## Tech Stack
- .NET 8
- ASP.NET Core Minimal APIs
- SQL Server
- RabbitMQ (optional)
- Angular (frontend in `frontend/content-web`)
- Azure (App Service, Static Web Apps, Key Vault, Storage, App Insights)

## Architecture
Clean Architecture with separation between:
- Domain
- Application
- Infrastructure
- API

## Current Status (what's implemented)
- API implemented as ASP.NET Core minimal APIs and wired to the Infrastructure project.
- SMTP-based email sending implemented via `ContentHub.Infrastructure.Email.SmtpEmailSender` and configured by `SmtpSettings`.
- Infrastructure deployment described in Bicep (`infra/bicep/main.bicep`) and uses Key Vault for secrets.
- Static frontend (`frontend/content-web`) is built and deployed to Azure Static Web Apps (dev environment uses an existing SWA named `swa-contenthub-dev`).
- API is deployed to an Azure App Service (configured in Bicep with managed identity and Key Vault references).

## How email sending is implemented (high level)
- The application sends email using a simple SMTP-based sender implemented in `ContentHub.Infrastructure.Email.SmtpEmailSender`.
- Secrets and credentials are kept out of source control and provided to the app at deployment time (Key Vault or environment secrets).
- We have used Brevo and Zoho Mail as SMTP providers for deployments; either provider can be used without code changes because the implementation is provider-agnostic.
- For production you may prefer a provider or SDK that offers deliverability analytics and retry/monitoring features; switching to a provider SDK is straightforward if you want richer telemetry.

## DNS / Custom domain notes
- The repo provisions a Static Web App (or references an existing one) and an App Service for the API. Custom domain configuration is not automated in these Bicep templates; it must be added manually or via additional automation.
- Static Web Apps custom domain flow (summary): add your domain in the Azure Static Web Apps resource, create the TXT validation record and a CNAME record as instructed by Azure. After verification, Azure will manage TLS certificates for the custom domain.
- App Service custom domain flow (summary): verify ownership with the provided TXT record or create an A record and map the domain in the App Service -> Custom domains. App Service requires DNS verification and will provide instructions in the portal. TLS certs can be bound via App Service-managed certificates or your own certificate.
- If you want to automate DNS provisioning, add or update Bicep/ARM modules to create DNS zone records (Azure DNS) and include the DNS zone resource in your deployment, then create the required verification records before assigning the custom domain.

## Infra / secrets (what happens during Bicep deploy)
- `infra/bicep/main.bicep` provisions App Service, Storage, Key Vault references, App Insights, and wiring for app settings.
- Secrets created in Key Vault (via `modules/keyvault-secrets.bicep`) include: `jwt-key`, `sql-connection-string`, `rabbitmq-connection-string`, `smtp-password`, `turnstile-secret-key`, `blob-connection-string`.
- App settings injected into the API App include Key Vault references (e.g. `@Microsoft.KeyVault(SecretUri=...)`) — see the `siteConfig.appSettings` block in `main.bicep`.

## Deployment (GitHub Actions + Bicep)
This repo includes GitHub workflows to validate/deploy infra and to deploy app artifacts.

### Workflows
- `infra-dev`: validates and deploys Bicep to the dev resource group (`.github/workflows/infra-dev.yml`). Uses `infra/bicep/main.bicep` and the parameter file in `infra/bicep/params/`.
- `deploy-dev`: builds and deploys the API to App Service and the Angular app to Static Web Apps (`.github/workflows/deploy-dev.yml`).

### Required GitHub Secrets (example for dev workflows)
- `AZURE_CLIENT_ID`
- `AZURE_TENANT_ID`
- `AZURE_SUBSCRIPTION_ID`
- `AZURE_RG_DEV`
- `SQL_ADMIN_PASSWORD_DEV`
- `API_APP_NAME_DEV`
- `SWA_TOKEN_DEV`

### Run the pipelines
1) Push to GitHub: `git push`
2) Run infra: GitHub Actions -> `infra-dev` -> Run workflow
3) After infra completes, ensure Key Vault secrets (if not created by Bicep) are present and that `SWA_TOKEN_DEV` is set for SWA deployment
4) Run app deploy: GitHub Actions -> `deploy-dev` -> Run workflow

## What still needs to be added / TODO
- Auth & registration UX polish and hardened registration flow (email verification flow exists but needs review). See `src/ContentHub.Api/Endpoints` for endpoints.
- Turn on and validate password reset (`Auth__EnableResetPassword` controlled by Bicep parameter).
- Add automated custom domain provisioning and DNS zone management (if you want infrastructure to create DNS records).
- Add end-to-end tests for deployment and the email flows (send/verify/reset password).
- Consider replacing `SmtpClient` (legacy) with a modern API client (e.g., SendGrid SDK) if you want better deliverability metrics and async features.
- Add monitoring/alerts for failures (App Insights alert rules, Log Analytics alerts for critical exceptions).

## Important files & locations
- `infra/bicep/main.bicep` - infra template and app setting wiring
- `infra/bicep/modules/keyvault-secrets.bicep` - secrets created/managed in Key Vault
- `src/ContentHub.Infrastructure/Email/SmtpEmailSender.cs` - SMTP email sender implementation
- `src/ContentHub.Infrastructure/Email/SmtpSettings.cs` - SMTP configuration model
- `frontend/content-web` - Angular frontend project
- `.github/workflows/infra-dev.yml` and `.github/workflows/deploy-dev.yml` - CI/CD workflows
