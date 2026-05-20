# AutoCodeForge Backend

This directory contains the backend solution for AutoCodeForge.

## Projects

- `src/AutoCodeForge.Core`: entities, models, interfaces, exceptions, helpers.
- `src/AutoCodeForge.Infrastructure`: SqlSugar setup, repository base, infrastructure services.
- `src/AutoCodeForge.Application`: application layer for business services.
- `src/AutoCodeForge.Api`: minimal API host, middleware, Swagger configuration.

## Quick Start

```powershell
cd server
dotnet restore AutoCodeForge.sln
dotnet build AutoCodeForge.sln
dotnet run --project src/AutoCodeForge.Api
```

Then open `https://localhost:<port>/swagger`.

## Notes

- Database provider defaults to SQLite.
- Connection strings are configured in `appsettings*.json`.