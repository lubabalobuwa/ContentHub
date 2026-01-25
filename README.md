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
