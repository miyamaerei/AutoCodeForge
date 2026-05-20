# 阶段一：项目初始化 & 基础设施（复用层核心）

**日期**: 2026-05-20  
# 阶段一：项目初始化 & 基础设施（复用层核心）

**日期**: 2026-05-20  
**预估时间**: 1-2 天  
**优先级**: 🔴 P0 - 必须最先完成

---

## 我是如何考虑的

### 设计思路

这是**整个项目的基础**，所有后续开发都依赖这个阶段的产出。我按照以下优先级设计：

1. **先搭架子** - 创建项目结构和解决方案文件
2. **再建基类** - 实体基类是复用的基础，所有实体都会继承
3. **然后是仓储** - BaseRepository<T> 是最核心的复用组件
4. **中间件** - 全局异常处理和认证中间件
5. **工具类** - 通用工具，如分页、JSON、时间处理（应放在 Core）
6. **配置** - 多环境配置和 Swagger

### 复用设计（核心）

本阶段创建的**所有功能都是可复用的基础设施**，供后续所有阶段使用：

| 复用组件 | 复用范围 | 被哪些阶段复用 |
|---------|---------|--------------|
| AuditableEntity | 所有需要审计的实体 | 阶段二~十 |
| UserOwnedEntity | 所有用户数据实体 | 阶段二~十 |
| BaseRepository<T> | 所有 Repository | 阶段二~十 |
| ApiResponse<T> | 所有 API 端点 | 阶段二~十 |
| ExceptionHandlingMiddleware | 全局所有请求 | 阶段二~十 |
| PaginationHelper | 所有列表查询 | 阶段二~十 |
| JsonHelper | 全局 JSON 操作（放 Core） | 阶段二~十 |
| TimeHelper | 全局时间操作（放 Core） | 阶段二~十 |

### 为什么这样安排？

- **依赖关系**: 仓储依赖基类，服务依赖仓储，API 依赖服务
- **复用优先**: 先创建可复用的基础设施，避免后续重复代码
- **风险控制**: 基类和仓储如果有缺陷，会影响所有模块，所以优先做并充分测试

### 技术决策

| 决策点 | 选择 | 原因 |
|--------|------|------|
| 项目结构 | 4 层（Api/Core/Infrastructure/Application） | 清晰的职责分离，便于复用 |
| ORM | SqlSugarCore | 轻量高性能，CodeFirst 自动建表 |
| API | Minimal API | 简洁快速，适合 MVP |
| 配置 | appsettings.{Environment}.json | 标准多环境配置 |
| 当前用户获取 | ICurrentUser 接口解耦 | 仓储不依赖 HttpContext，非 Web 环境也能复用 |

### ICurrentUser 解耦设计（重要）

为避免 `BaseRepository` 直接依赖 `IHttpContextAccessor`（违反依赖倒置原则），采用接口解耦方案：

```
Core/Interfaces/
└─ ICurrentUser.cs           // 接口定义

Infrastructure/Services/
└─ CurrentUser.cs            // 实现：读取 HttpContext 或其他来源
```

**优点**：
- 仓储不依赖 ASP.NET Core 特有类型
- 非 Web 环境（Console、后台服务）也能复用仓储
- 更符合整洁架构（Clean Architecture）原则
- 便于单元测试时 mock

---

## 本阶段创建的可复用功能清单

> 注意：下面的文件路径是逻辑建议。工具类（JsonHelper / TimeHelper / PaginationHelper）应属于 Core，因为它们不依赖数据库或外部实现，属于最底层通用工具。Infrastructure 只放与外部资源（数据库、文件、第三方 SDK）强耦合的实现。

| 复用组件 | 文件路径 | 说明 | 被复用次数（预估） |
|---------|---------|------|-----------------|
| AuditableEntity | `Core/Entities/Base/AuditableEntity.cs` | 时间戳基类（CreatedAt/UpdatedAt） | 16+ 实体 |
| UserOwnedEntity | `Core/Entities/Base/UserOwnedEntity.cs` | 用户隔离基类（NtId/IsDeleted） | 16+ 实体 |
| BaseRepository<T> | `Infrastructure/Repositories/Base/BaseRepository.cs` | 通用仓储基类（CRUD+软删除+用户隔离+分页） | 12+ Repository |
| IBaseRepository<T> | `Core/Interfaces/IBaseRepository.cs` | 仓储接口（支持可选用户隔离） | 12+ Repository |
| ICurrentUser | `Core/Interfaces/ICurrentUser.cs` | 当前用户接口（解耦 HttpContext） | 所有 Repository、服务 |
| CurrentUser | `Infrastructure/Services/CurrentUser.cs` | 当前用户实现（读取 HttpContext） | Api 项目 |
| ApiResponse<T> | `Core/Models/ApiResponse.cs` | 统一响应格式 | 30+ API 端点 |
| PagedResult<T> | `Core/Models/PagedResult.cs` | 分页结果格式 | 10+ 列表接口 |
| ExceptionHandlingMiddleware | `Api/Middleware/ExceptionHandlingMiddleware.cs` | 全局异常处理 | 全局所有请求 |
| CustomException 系列 | `Core/Exceptions/` | 自定义异常基类 | 全局所有异常处理 |
| PaginationHelper | `Core/Helpers/PaginationHelper.cs` | 分页辅助工具 | 所有列表查询 |
| JsonHelper | `Core/Helpers/JsonHelper.cs` | JSON 序列化/反序列化辅助方法 | 全局 JSON 操作 |
| TimeHelper | `Core/Helpers/TimeHelper.cs` | 时间处理工具（UTC/时区） | 全局时间操作 |
| SqlSugarSetup | `Infrastructure/Data/SqlSugarSetup.cs` | SqlSugar 配置 | 所有 Repository |

---

## 可用命令：使用 PowerShell 初始化后端工程（一步到位）

下面提供一组 PowerShell 脚本命令（Win PowerShell），用于在 `server/` 目录下初始化解决方案和 4 个项目（Core/Infrastructure/Application/Api），并将项目加入解决方案、添加引用、以及在 Api 项目启用 XML 注释与 Swagger 所需包。

将这些命令复制到 PowerShell（工作目录为仓库根目录 `e:\git\AutoFrog\AutoCodeForge`）：

```powershell
# 进入 server 目录
mkdir -Force server; cd server

# 创建 solution
dotnet new sln -n AutoCodeForge

# 创建项目目录
mkdir -Force src; cd src

# Core (class lib)
dotnet new classlib -n AutoCodeForge.Core

# Infrastructure (class lib)
dotnet new classlib -n AutoCodeForge.Infrastructure

# Application (class lib)
dotnet new classlib -n AutoCodeForge.Application

# Api (minimal web)
dotnet new web -n AutoCodeForge.Api

# 返回到 server 根并添加项目到 solution
cd ..\..\n+dotnet sln AutoCodeForge.sln add src\\AutoCodeForge.Core\\AutoCodeForge.Core.csproj
dotnet sln AutoCodeForge.sln add src\\AutoCodeForge.Infrastructure\\AutoCodeForge.Infrastructure.csproj
dotnet sln AutoCodeForge.sln add src\\AutoCodeForge.Application\\AutoCodeForge.Application.csproj
dotnet sln AutoCodeForge.sln add src\\AutoCodeForge.Api\\AutoCodeForge.Api.csproj

# 在项目间添加引用（Application -> Core, Infrastructure -> Core, Api -> Application, Infrastructure -> Core）
cd src
cd AutoCodeForge.Infrastructure
dotnet add reference ..\\AutoCodeForge.Core\\AutoCodeForge.Core.csproj
cd ..\\AutoCodeForge.Application
dotnet add reference ..\\AutoCodeForge.Core\\AutoCodeForge.Core.csproj
cd ..\\AutoCodeForge.Api
dotnet add reference ..\\AutoCodeForge.Application\\AutoCodeForge.Application.csproj
dotnet add reference ..\\AutoCodeForge.Infrastructure\\AutoCodeForge.Infrastructure.csproj

# 可选：在 Api 中安装 SqlSugar 和 Swashbuckle
dotnet add package SqlSugarCore
dotnet add package Swashbuckle.AspNetCore

# 启用 XML 文档生成以供 Swagger 使用（编辑 csproj）
# PowerShell one-liner: 设置 GenerateDocumentationFile 为 true
(Get-Content -Path ..\\AutoCodeForge.Api\\AutoCodeForge.Api.csproj) -replace '(</PropertyGroup>)','  <GenerateDocumentationFile>true</GenerateDocumentationFile>`n  <NoWarn>1591</NoWarn>`n</PropertyGroup>' | Set-Content -Path ..\\AutoCodeForge.Api\\AutoCodeForge.Api.csproj

# 恢复并构建
dotnet restore
dotnet build

Write-Host '初始化完成：运行 dotnet run --project src\\AutoCodeForge.Api 来启动 API。'
```

说明与注意事项：
- 脚本为示例，运行前请备份工作区中可能已有的 `server/` 文件夹。
- 根据团队需求可替换 SqlSugar 为其它 ORM。当前脚本只安装 `SqlSugarCore` 的 NuGet 包。
- 如果需要 SQLite 数据库，请在 `appsettings.json` 中配置相对路径并在 `SqlSugarSetup` 中读取。

---

## 任务清单（更新后）

（与原列表基本一致，已将工具类路径修正到 Core）

| 编号 | 任务名称 | 文件路径 | 产出物 | 是否为复用功能 | 复用范围 | 前置依赖 | 验证方式 |
|------|---------|---------|-------|-------------|---------|---------|---------|
| **1.1** | 创建解决方案文件 | `server/AutoCodeForge.sln` | .sln 解决方案文件 | ❌ - | - | 项目文件存在 |
| **1.2** | 创建 Core 项目 | `server/src/AutoCodeForge.Core/AutoCodeForge.Core.csproj` | Core 类库项目文件 | ❌ - | - | 1.1 | 项目编译成功 |
| **1.3** | 创建 Infrastructure 项目 | `server/src/AutoCodeForge.Infrastructure/AutoCodeForge.Infrastructure.csproj` | Infrastructure 类库项目文件 | ❌ - | - | 1.2 | 项目编译成功，引用 Core |
| **1.4** | 创建 Application 项目 | `server/src/AutoCodeForge.Application/AutoCodeForge.Application.csproj` | Application 类库项目文件 | ❌ - | - | 1.3 | 项目编译成功，引用 Core & Infrastructure |
| **1.5** | 创建 Api 项目 | `server/src/AutoCodeForge.Api/AutoCodeForge.Api.csproj` | Minimal API 项目文件 | ❌ - | - | 1.4 | 项目编译成功，引用所有其他项目 |
| **1.6** | 将所有项目添加到解决方案 | `server/AutoCodeForge.sln` | 解决方案包含所有 4 个项目 | ❌ - | - | 1.1-1.5 | `dotnet sln list` 显示 4 个项目 |
| **1.7** | 创建基类实体 AuditableEntity | `server/src/AutoCodeForge.Core/Entities/Base/AuditableEntity.cs` | 包含 CreatedAt/UpdatedAt 的基类 | ✅ 是 | 16+ 实体 | 1.2 | 代码编译，属性定义正确 |
| **1.8** | 创建基类实体 UserOwnedEntity | `server/src/AutoCodeForge.Core/Entities/Base/UserOwnedEntity.cs` | 继承 AuditableEntity，包含 NtId/IsDeleted | ✅ 是 | 16+ 实体 | 1.7 | 代码编译，属性定义正确 |
| **1.9** | 创建统一响应模型 ApiResponse<T> | `server/src/AutoCodeForge.Core/Models/ApiResponse.cs` | ApiResponse<T> 泛型类 | ✅ 是 | 30+ API 端点 | 1.2 | 代码编译，包含 Success/Message/Data/TraceId |
| **1.10** | 创建分页模型 PagedResult<T> | `server/src/AutoCodeForge.Core/Models/PagedResult.cs` | PagedResult<T> 分页结果类 | ✅ 是 | 10+ 列表接口 | 1.9 | 代码编译，包含 Items/TotalCount/Page/PageSize |
| **1.11** | 创建自定义异常基类 | `server/src/AutoCodeForge.Core/Exceptions/CustomException.cs` | CustomException 基类 | ✅ 是 | 全局异常处理 | 1.2 | 代码编译 |
| **1.12** | 创建常用自定义异常 | `server/src/AutoCodeForge.Core/Exceptions/` | ValidationException/NotFoundException/UnauthorizedException 等 | ✅ 是 | 全局异常处理 | 1.11 | 代码编译，每个异常都有适当构造函数 |
| **1.13** | 创建 ICurrentUser 接口 | `server/src/AutoCodeForge.Core/Interfaces/ICurrentUser.cs` | 当前用户接口（GetCurrentNtId） | ✅ 是 | 所有 Repository、服务 | 1.2 | 代码编译 |
| **1.14** | 创建 CurrentUser 实现 | `server/src/AutoCodeForge.Infrastructure/Services/CurrentUser.cs` | 当前用户实现（读取 HttpContext） | ✅ 是 | Api 项目 DI | 1.3, 1.13 | 代码编译 |
| **1.15** | 配置 SqlSugar | `server/src/AutoCodeForge.Infrastructure/Data/SqlSugarSetup.cs` | SqlSugar 配置类 | ✅ 是 | 所有 Repository | 1.3 | 代码编译，包含连接字符串配置 |
| **1.16** | 创建 BaseRepository<T> 基类 | `server/src/AutoCodeForge.Infrastructure/Repositories/Base/BaseRepository.cs` | 通用仓储基类（CRUD+软删除+用户隔离+分页） | ✅ 是 | 12+ Repository | 1.8, 1.10, 1.13, 1.15 | 代码编译，依赖 ICurrentUser 而非 HttpContextAccessor |
| **1.17** | 创建 IBaseRepository<T> 接口 | `server/src/AutoCodeForge.Core/Interfaces/IBaseRepository.cs` | 仓储接口定义 | ✅ 是 | 12+ Repository | 1.16 | 代码编译，接口与实现匹配 |
| **1.18** | 创建全局异常处理中间件 | `server/src/AutoCodeForge.Api/Middleware/ExceptionHandlingMiddleware.cs` | ExceptionHandlingMiddleware 类 | ✅ 是 | 全局所有请求 | 1.12, 1.9 | 代码编译，包含异常捕获、日志记录、标准化响应 |
| **1.19** | 创建工具类 PaginationHelper | `server/src/AutoCodeForge.Core/Helpers/PaginationHelper.cs` | 分页辅助方法 | ✅ 是 | 所有列表查询 | 1.10 | 代码编译，包含分页参数验证、结果包装 |
| **1.20** | 创建工具类 JsonHelper | `server/src/AutoCodeForge.Core/Helpers/JsonHelper.cs` | JSON 序列化/反序列化辅助方法 | ✅ 是 | 全局 JSON 操作 | 1.3 | 代码编译，包含 System.Text.Json 配置 |
| **1.21** | 创建工具类 TimeHelper | `server/src/AutoCodeForge.Core/Helpers/TimeHelper.cs` | 时间处理辅助方法（UTC/时区） | ✅ 是 | 全局时间操作 | 1.3 | 代码编译，包含常用时间转换方法 |
| **1.22** | 配置依赖注入容器 | `server/src/AutoCodeForge.Api/Program.cs` | Program.cs 中注册所有服务 | ❌ - | - | 1.14, 1.17, 1.18-1.21 | 代码编译，包含 AddSqlSugar、AddRepositories、AddHelpers、AddCurrentUser 等 |
| **1.23** | 配置 Swagger | `server/src/AutoCodeForge.Api/Program.cs` | Swagger 配置，XML 注释支持 | ❌ - | - | 1.22 | 代码编译，Swagger 服务已注册 |
| **1.24** | 配置多环境设置 | `server/src/AutoCodeForge.Api/appsettings.json` `server/src/AutoCodeForge.Api/appsettings.Development.json` `server/src/AutoCodeForge.Api/appsettings.Production.json` | 3 个配置文件（基础/开发/生产） | ❌ - | - | 1.5 | 文件存在，包含连接字符串等配置 |
| **1.25** | 创建 launchSettings.json | `server/src/AutoCodeForge.Api/Properties/launchSettings.json` | 启动配置文件 | ❌ - | - | 1.5 | 文件存在，包含开发环境配置 |
| **1.26** | 创建 .gitignore | `server/.gitignore` | Git 忽略文件配置 | ❌ - | - | 1.1 | 文件存在，忽略 bin/obj/db 等 |
| **1.27** | 创建 README.md | `server/README.md` | 后端项目说明文档 | ❌ - | - | 1.1 | 文件存在，包含项目介绍、运行方式 |
| **1.28** | 验证项目可以编译运行 | `server/src/AutoCodeForge.Api/` | 项目可以 dotnet build 和 dotnet run | ❌ - | - | 1.1-1.27 | 运行 `dotnet build && dotnet run`，访问 /swagger 页面 |

---

## SqlSugar：全局过滤器（防止软删除失效）

问题说明：
- 如果只在仓储查询中逐一加上 `.Where(e => !e.IsDeleted)`，很容易遗漏某些地方（例如直接使用 `_db.Queryable<T>()` 的地方或第三方库集成点），导致软删除（IsDeleted）被绕过。

建议：使用 SqlSugar 的全局 QueryFilter（或 QueryFilterWhere）在 SqlSugar 初始化时注册统一过滤条件，以确保所有查询默认跳过软删除记录。

示例：在 `Infrastructure/Data/SqlSugarSetup.cs` 的配置方法中添加：

```csharp
// 伪代码：SqlSugarSetup.cs
public static class SqlSugarSetup
{
    public static void AddSqlSugar(this IServiceCollection services, IConfiguration configuration)
    {
        var conn = configuration.GetConnectionString("DefaultConnection");
        var db = new SqlSugarScope(new ConnectionConfig()
        {
            ConnectionString = conn,
            DbType = DbType.Sqlite,
            IsAutoCloseConnection = true,
        });

        // 全局过滤器：自动过滤 IsDeleted=true
        // 注意：不同版本的 SqlSugar API 名称可能不同，请根据项目中实际使用的版本调整
        db.QueryFilter.Where((UserOwnedEntity e) => !e.IsDeleted);

        services.AddSingleton<ISqlSugarClient>(db);
    }
}
```

注意：不同版本的 SqlSugar 对 QueryFilter API 命名与参数有差异（AddGlobalFilter / Where / QueryFilterWhere 等），请参考当前项目中使用的 SqlSugar 版本文档并把过滤器放在 SqlSugar client 创建后立即注册的位置。

关键点：
- 过滤器应该基于最通用的父类（如 `UserOwnedEntity` 或 `AuditableEntity`）来注册；
- 过滤器必须注册在 `ISqlSugarClient` 被注入之前；
- 对于需要包含已删除记录的特殊查询，可以显式禁用过滤器或在仓库方法中使用无过滤的方法。

---

## 总览文件结构（完整版）

下面给出后端 `server/` 子目录的建议完整文件结构（含关键文件与建议位置）。注：这是逻辑目录树，实际可根据团队偏好微调。

server/
├─ AutoCodeForge.sln
├─ README.md
├─ .gitignore
└─ src/
   ├─ AutoCodeForge.Core/
   │  ├─ AutoCodeForge.Core.csproj
   │  ├─ Entities/
   │  │  └─ Base/
   │  │     ├─ AuditableEntity.cs
   │  │     └─ UserOwnedEntity.cs
   │  ├─ Interfaces/
   │  │  ├─ IBaseRepository.cs
   │  │  └─ ICurrentUser.cs          // 新增：当前用户接口
   │  ├─ Models/
   │  │  ├─ ApiResponse.cs
   │  │  └─ PagedResult.cs
   │  ├─ Exceptions/
   │  │  ├─ CustomException.cs
   │  │  ├─ ValidationException.cs
   │  │  ├─ NotFoundException.cs
   │  │  └─ UnauthorizedException.cs
   │  └─ Helpers/
   │     ├─ JsonHelper.cs
   │     ├─ TimeHelper.cs
   │     └─ PaginationHelper.cs
   ├─ AutoCodeForge.Infrastructure/
   │  ├─ AutoCodeForge.Infrastructure.csproj
   │  ├─ Data/
   │  │  └─ SqlSugarSetup.cs
   │  ├─ Repositories/
   │  │  └─ Base/
   │  │     └─ BaseRepository.cs
   │  └─ Services/
   │     └─ CurrentUser.cs           // 新增：当前用户实现
   ├─ AutoCodeForge.Application/
   │  ├─ AutoCodeForge.Application.csproj
   │  └─ Services/
   │     └─ (业务服务，依赖 Repository 接口)
   └─ AutoCodeForge.Api/
      ├─ AutoCodeForge.Api.csproj
      ├─ Program.cs
      ├─ appsettings.json
      ├─ appsettings.Development.json
      ├─ Properties/
      │  └─ launchSettings.json
      └─ Middleware/
         └─ ExceptionHandlingMiddleware.cs

---

## 工具类位置错误（架构说明与修正）

问题：原文把 `JsonHelper` 和 `TimeHelper` 放在 `Infrastructure`，这会导致架构混淆：Infrastructure 应该只包含与外部资源（数据库、文件系统、第三方 SDK）强耦合的实现，而非纯内存、无副作用的通用工具。

修正建议：
- 将 `JsonHelper`、`TimeHelper`、`PaginationHelper` 等通用、无外部依赖的工具类移动到 `Core/Helpers/`。这样更清晰：Core 依赖最低层、其他所有项目可直接引用这些工具。
- Infrastructure 保持 `SqlSugarSetup`、第三方 SDK 封装、与外部资源交互的实现。

示例：
- 从 `server/src/AutoCodeForge.Infrastructure/Helpers/JsonHelper.cs` -> 移动到 `server/src/AutoCodeForge.Core/Helpers/JsonHelper.cs`
- 从 `server/src/AutoCodeForge.Infrastructure/Helpers/TimeHelper.cs` -> 移动到 `server/src/AutoCodeForge.Core/Helpers/TimeHelper.cs`

实现步骤（建议脚本）：
1. 在 `AutoCodeForge.Core` 中创建 `Helpers` 文件夹并添加 `JsonHelper.cs`、`TimeHelper.cs` 文件。
2. 在 `AutoCodeForge.Infrastructure` 中删除或移除原 helpers 文件。
3. 更新引用：所有使用这些 helper 的项目/文件改用 `using AutoCodeForge.Core.Helpers;` 并确保项目引用 `AutoCodeForge.Core`。

---

## 关键文件内容预览（已更新）

### ICurrentUser.cs 接口定义

```csharp
// server/src/AutoCodeForge.Core/Interfaces/ICurrentUser.cs
public interface ICurrentUser
{
    /// <summary>
    /// 获取当前用户的 NtId，如果不存在返回 null
    /// </summary>
    string? GetCurrentNtId();
}
```

### CurrentUser.cs 实现

```csharp
// server/src/AutoCodeForge.Infrastructure/Services/CurrentUser.cs
public class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? GetCurrentNtId()
    {
        // 从 Claims 中获取 NtId，初期也可从 Header 获取
        return _httpContextAccessor.HttpContext?.User?.FindFirst("NtId")?.Value;
    }
}
```

### BaseRepository.cs 的核心结构（支持可选用户隔离）

```csharp
// server/src/AutoCodeForge.Infrastructure/Repositories/Base/BaseRepository.cs
public class BaseRepository<T> : IBaseRepository<T> where T : UserOwnedEntity, new()
{
    protected readonly ISqlSugarClient _db;
    protected readonly string? _currentUserNtId;

    public BaseRepository(ISqlSugarClient db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUserNtId = currentUser.GetCurrentNtId();
    }

    // 默认查询：带用户隔离 + 软删除过滤（适用于普通用户操作）
    protected ISugarQueryable<T> Queryable => _db.Queryable<T>()
        .Where(e => !e.IsDeleted)
        .Where(e => e.NtId == _currentUserNtId);

    // 无用户隔离的查询（适用于管理员、系统操作等场景）
    protected ISugarQueryable<T> QueryableWithoutNtIdFilter => _db.Queryable<T>()
        .Where(e => !e.IsDeleted);

    public virtual async Task<T?> GetByIdAsync(Guid id) { ... }
    public virtual async Task<List<T>> GetAllAsync() { ... }  // 使用默认 Queryable
    public virtual async Task<PagedResult<T>> GetPagedAsync(int page, int pageSize) { ... }  // 使用默认 Queryable

    // 不带用户隔离的方法（子类可覆盖）
    public virtual async Task<List<T>> GetAllAsync(bool includeAllUsers = false)
    {
        var query = includeAllUsers ? QueryableWithoutNtIdFilter : Queryable;
        return await query.ToListAsync();
    }

    public virtual async Task<PagedResult<T>> GetPagedAsync(int page, int pageSize, bool includeAllUsers = false)
    {
        var query = includeAllUsers ? QueryableWithoutNtIdFilter : Queryable;
        // ... 分页逻辑
    }

    public virtual async Task<T> CreateAsync(T entity) { ... }
    public virtual async Task UpdateAsync(T entity) { ... }
    public virtual async Task SoftDeleteAsync(Guid id) { ... }
}
```

**使用说明**：
- `GetAllAsync()` / `GetPagedAsync()` - 默认只返回当前用户的数据
- `GetAllAsync(includeAllUsers: true)` / `GetPagedAsync(includeAllUsers: true)` - 返回所有用户的数据（管理员场景）

**依赖变化**：
- 旧：`BaseRepository(ISqlSugarClient db, IHttpContextAccessor httpContextAccessor)`
- 新：`BaseRepository(ISqlSugarClient db, ICurrentUser currentUser)`

---

## 注意事项

⚠️ **重要提醒**

1. **基类设计要谨慎** - AuditableEntity 和 UserOwnedEntity 会被所有实体继承，设计时要考虑扩展性
2. **BaseRepository 的泛型约束** - 注意 where T : UserOwnedEntity 约束，确保所有实体都继承基类
3. **SQLite 连接字符串** - 路径配置要正确，使用相对路径
4. **依赖注入顺序** - 先注册基础服务，再注册业务服务
5. **Swagger XML 注释** - 需要在项目文件中启用 XML 文档生成
6. **NtId 获取方式** - 初期可以从 Claims 或 Header 获取，后续完善认证后调整

✅ **验收标准**

- 所有项目可以编译通过（`dotnet build`）
- API 项目可以运行（`dotnet run`）
- Swagger 页面可以访问（https://localhost:xxxx/swagger）
- 项目结构符合规划文档要求
- 所有可复用基类和工具类都已创建

---

## 阶段完成总结

### 复用收益

本阶段完成后，预计可以**避免 3000+ 行重复代码**，具体包括：
- 16+ 实体无需重复写 CreatedAt/UpdatedAt/NtId/IsDeleted
- 12+ Repository 无需重复写 CRUD/软删除/分页
- 30+ API 端点无需重复写异常处理和响应格式化

### 下一步

完成本阶段后，进入 **阶段二：数据层 & 认证系统**，详见 `02-phase-two-data-auth.md`
