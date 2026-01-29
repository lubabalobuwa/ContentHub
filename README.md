# ContentHub
ContentHub is a full-stack blog/content platform built with .NET 8, ASP.NET Core Minimal APIs, Clean Architecture, SQL Server, RabbitMQ, and Angular.

## Tech Stack
- .NET 8
- ASP.NET Core Minimal APIs
- SQL Server
- RabbitMQ
- Angular
- Azure

## Architecture
The solution follows Clean Architecture, with clear separation between:
- Domain
- Application
- Infrastructure
- API

## Status
Work in progress

User creation endpoint is for demo purposes; auth/registration will be added later.

## API Notes
Content list/detail responses use `ContentSummaryDto` (mapped to `ContentSummaryResponse`) with:
- `id`
- `title`
- `body`
- `status` (Draft/Published/Archived)
- `rowVersion` (Base64 string for optimistic concurrency)

Validation errors use ProblemDetails with an `errors` dictionary, for example:
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "error": [
      "AuthorId is required."
    ]
  }
}
```

## Deployment (GitHub Actions + Bicep)
This repo uses GitHub Actions to provision infra (Bicep) and deploy the API + Angular app.

### Workflows
- `infra-dev`: validates and deploys Bicep to the dev resource group
- `deploy-dev`: deploys the API to App Service and the Angular app to Static Web Apps

### Required GitHub Secrets
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
3) After infra completes, copy the Static Web App deployment token to `SWA_TOKEN_DEV`
4) Run app deploy: GitHub Actions -> `deploy-dev` -> Run workflow
