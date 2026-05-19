---
name: dotnet-init
description: 'Initialize .NET 10 project with ASP.NET Core Minimal API, SugarSql ORM, JWT authentication, and standardized file structure. Includes project setup, database configuration, middleware initialization, Git engineering setup, and file structure standardization.'
argument-hint: 'Project name and location (e.g. AutoCodeForge.Server in server/src)'
---

# .NET 10 Project Initialization Skill

## When to Use
- You need to create a new .NET 10 backend project quickly.
- You want consistent project structure and conventions.
- You need pre-configured middleware, authentication, and database setup.
- You want Git engineering best practices applied from day one.

## Fixed Conventions
1. Project uses .NET 10 SDK and ASP.NET Core Minimal API.
2. ORM is SqlSugarCore with SQLite for MVP.
3. Authentication uses JWT Bearer Token.
4. API response follows standardized ApiResponse<T> format.
5. File structure follows modular organization.
6. Global exception handling with RFC 7807 Problem Details.
7. Swagger/OpenAPI documentation enabled by default.

## Required Input
1. Project name (PascalCase), e.g. AutoCodeForge.Server.
2. Project location/path.
3. Database type: SQLite (default), MySQL, SQL Server, PostgreSQL.
4. Enable authentication: true (default) or false.
5. Enable Swagger: true (default) or false.

## Output Targets
```
<project-root>/
├── src/
│   ├── Program.cs                    # Entry point
│   ├── appsettings.json              # Configuration
│   ├── appsettings.Development.json  # Dev configuration
│   ├── launchSettings.json           # Launch settings
│   ├── Configuration/                # Configuration classes
│   │   └── AppSettings.cs
│   ├── Entities/                     # Database entities
│   │   ├── UserEntity.cs
│   │   └── ...
│   ├── Repositories/                 # Data access layer
│   │   └── UserRepository.cs
│   ├── Services/                     # Business logic
│   │   ├── AuthService.cs
│   │   └── ...
│   ├── Api/                          # API endpoints
│   │   ├── AuthApi.cs
│   │   └── ...
│   ├── Middleware/                   # Custom middleware
│   │   ├── AuthMiddleware.cs
│   │   └── ExceptionMiddleware.cs
│   └── Extensions/                   # Extension methods
│       └── SqlSugarSetup.cs
├── tests/                            # Unit tests
├── .gitignore                        # Git ignore rules
├── .editorconfig                     # Editor configuration
├── Directory.Build.props             # Build configuration
└── <ProjectName>.sln                 # Solution file
```

## Execution Checklist

### 1. Project Initialization
- Create .NET 10 Web API project using dotnet new webapi.
- Set correct TargetFramework to net10.
- Add required NuGet packages:
  - SqlSugarCore (5.1+)
  - Microsoft.AspNetCore.Authentication.JwtBearer
  - Swashbuckle.AspNetCore
  - Microsoft.Extensions.Configuration.Binder

### 2. Database Initialization
- Configure SqlSugar with selected database provider.
- Create ConnectionConfig with appropriate connection string.
- Register ISqlSugarClient in DI container.
- Set up CodeFirst initialization for all entities.
- Execute database migration on application startup.
- Initialize seed data (admin user, default configurations, default agent).

### 3. Base Middleware Initialization
- Add authentication middleware with JWT Bearer.
- Add authorization middleware.
- Add Swagger/OpenAPI middleware.
- Add global exception handling middleware.
- Configure CORS policy.
- Add response compression.

### 4. Git Engineering Initialization
- Create .gitignore file for .NET projects.
- Create .editorconfig with .NET conventions.
- Create Directory.Build.props for centralized build settings.
- Set up .gitattributes for line ending normalization.

### 5. File Structure Standardization
- Create Configuration folder for settings classes.
- Create Entities folder for database entities.
- Create Repositories folder for data access.
- Create Services folder for business logic.
- Create Api folder for endpoint definitions.
- Create Middleware folder for custom middleware.
- Create Extensions folder for extension methods.

## Template Assets
- [Program.template.cs](./assets/Program.template.cs)
- [appsettings.template.json](./assets/appsettings.template.json)
- [launchSettings.template.json](./assets/launchSettings.template.json)
- [SqlSugarSetup.template.cs](./assets/SqlSugarSetup.template.cs)
- [DatabaseInitializer.template.cs](./assets/DatabaseInitializer.template.cs)
- [AuthMiddleware.template.cs](./assets/AuthMiddleware.template.cs)
- [ExceptionMiddleware.template.cs](./assets/ExceptionMiddleware.template.cs)
- [ApiResponse.template.cs](./assets/ApiResponse.template.cs)
- [AppSettings.template.cs](./assets/AppSettings.template.cs)
- [UserEntity.template.cs](./assets/UserEntity.template.cs)
- [TaskEntity.template.cs](./assets/TaskEntity.template.cs)
- [ChatSessionEntity.template.cs](./assets/ChatSessionEntity.template.cs)
- [AgentEntity.template.cs](./assets/AgentEntity.template.cs)
- [RepositoryEntity.template.cs](./assets/RepositoryEntity.template.cs)
- [ScheduledTaskEntity.template.cs](./assets/ScheduledTaskEntity.template.cs)
- [PipelineEntity.template.cs](./assets/PipelineEntity.template.cs)
- [GlobalConfigEntity.template.cs](./assets/GlobalConfigEntity.template.cs)
- [LLMModelConfigEntity.template.cs](./assets/LLMModelConfigEntity.template.cs)
- [AuthService.template.cs](./assets/AuthService.template.cs)
- [AuthApi.template.cs](./assets/AuthApi.template.cs)
- [gitignore.template](./assets/gitignore.template)
- [editorconfig.template](./assets/editorconfig.template)

## Placeholder Tokens
- __ProjectName__: PascalCase project name, example AutoCodeForge.Server.
- __project_name__: kebab-case project name, example autocodeforge-server.
- __DatabaseType__: SQLite, MySQL, SQLServer, PostgreSQL.
- __ConnectionString__: Database connection string.
- __JwtSecret__: JWT secret key.
- __JwtIssuer__: JWT issuer.
- __JwtAudience__: JWT audience.

## Done Criteria
1. Project builds successfully.
2. Database connection is configured.
3. Middleware pipeline is complete.
4. Authentication endpoints are working.
5. Swagger UI is accessible at /swagger.
6. Global exception handling returns standardized error responses.
7. Git configuration files are in place.
8. File structure follows the standard layout.

## Example Prompts
- /dotnet-init create AutoCodeForge.Server in server/src with SQLite and JWT
- /dotnet-init initialize backend project MyProject.Server with MySQL
- /dotnet-init setup .NET 10 API project with PostgreSQL and authentication
