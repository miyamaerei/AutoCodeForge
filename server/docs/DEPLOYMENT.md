# AutoCodeForge Deployment Guide

## Prerequisites

- .NET SDK 10.0+
- Writable filesystem for SQLite database or an alternate connection string target
- A strong JWT key supplied through configuration or environment variable

## Required Configuration

Set or override these settings before deployment:

- `ConnectionStrings:DefaultConnection`
- `Jwt:Issuer`
- `Jwt:Audience`
- `Jwt:Key`
- `Jwt:ExpireMinutes`

For production, prefer environment variables for secrets:

```powershell
$env:JWT_KEY = "replace-with-a-strong-secret-of-at-least-32-characters"
```

## Build and Start

```powershell
cd server
dotnet restore AutoCodeForge.sln
dotnet build AutoCodeForge.sln -c Release
dotnet run --project src/AutoCodeForge.Api -c Release
```

## First Startup Behavior

- The API initializes the database schema on startup.
- In development environment, baseline seed data is inserted automatically.
- Swagger UI is exposed only in development.

## Recommended Production Checklist

- Use a production-grade JWT secret from environment or secret store.
- Replace local SQLite if multi-instance deployment or high write concurrency is required.
- Put the API behind HTTPS termination or a reverse proxy.
- Configure log collection for console output.
- Review background services for task queue, scheduler, and pipeline sync capacity.

## Health Verification

After deployment, verify:

```powershell
curl http://<host>/health
curl http://<host>/health/live
curl http://<host>/health/ready
```

Expected result: HTTP `200` with a valid `ApiResponse` or health payload.

## Post-Deploy Smoke Test

1. Register a user through `/api/v1/auth/register`.
2. Call `/api/v1/auth/me` with the returned bearer token.
3. Create an agent through `/api/v1/agents`.
4. Create a chat session and send a message.
5. Verify `/health` and `/system/info`.

## Known Constraints

- SQLite is acceptable for MVP and local deployment, but not ideal for multi-instance scheduling workloads.
- Current LLM execution path uses the mock gateway behavior unless external model integration is configured later.