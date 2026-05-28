# 阶段二：数据层 & 认证系统

**日期**: 2026-05-20  
**预估时间**: 2-3 天  
**优先级**: 🔴 P0 - 核心功能  
**前置依赖**: 阶段一（基础设施）

---

## 我是如何考虑的

### 设计思路

这个阶段的核心是**数据模型**和**认证体系**：

1. **先建实体** - 按照需求文档创建所有 16+ 数据实体，全部继承基类
2. **CodeFirst 建表** - 应用启动时自动创建数据库表结构
3. **认证体系** - JWT 签发 + 校验 + 密码加密
4. **仓储实现** - 每个实体对应一个 Repository，继承 BaseRepository<T>
5. **种子数据** - 初始化默认配置和测试数据

### 复用设计

本阶段**大量复用阶段一的基础设施**，避免重复代码：

| 复用的组件 | 复用自 | 本阶段复用方式 |
|----------|-------|--------------|
| AuditableEntity | 阶段一 | 16+ 实体继承基类，自动获得 CreatedAt/UpdatedAt |
| UserOwnedEntity | 阶段一 | 所有用户实体继承基类，自动获得 NtId/IsDeleted |
| BaseRepository<T> | 阶段一 | 4 个 Repository 直接继承，无需重复写 CRUD |
| ApiResponse<T> | 阶段一 | Auth Endpoints 统一使用此响应格式 |
| ExceptionHandlingMiddleware | 阶段一 | 全局异常处理自动生效 |
| PaginationHelper | 阶段一 | 列表查询自动分页（后续会用到） |
| JsonHelper | 阶段一 | JSON 配置存储（后续会用到） |
| TimeHelper | 阶段一 | 时间处理（后续会用到） |

### 本阶段新增的可复用功能

| 复用组件 | 被哪些阶段复用 |
|---------|--------------|
| UserRepository | 阶段二~十 |
| GlobalConfigRepository | 阶段二~十 |
| UserConfigRepository | 阶段二~十 |
| LLMModelConfigRepository | 阶段二~十 |
| JwtService | 阶段二~十 |
| JwtAuthMiddleware | 阶段二~十 |
| PasswordHelper | 阶段二~十 |
| AuthService | 阶段二~十 |
| 所有 16+ 实体 | 阶段二~十 |

### 为什么这样安排？

- **实体先行** - 仓储和服务都依赖实体定义
- **认证基础** - 后续所有 API 都需要认证保护
- **数据隔离** - UserOwnedEntity 确保每个用户只能看到自己的数据

### 实体设计原则

| 原则 | 说明 |
|------|------|
| 必须继承基类 | 所有实体继承 AuditableEntity 或 UserOwnedEntity |
| Guid 主键 | 所有表主键使用 Guid |
| 软删除 | 所有用户数据使用 IsDeleted 字段 |
| NtId 隔离 | 所有用户数据包含 NtId 字段 |
| 时间戳自动维护 | CreatedAt/UpdatedAt 自动填充 |

---

## 本阶段复用的功能清单（来自阶段一）

| 复用组件 | 文件路径 | 复用方式 | 避免重复代码（预估） |
|---------|---------|---------|------------------|
| AuditableEntity | `Core/Entities/Base/AuditableEntity.cs` | 所有实体继承 | 32 个属性定义（每个实体 2 个） |
| UserOwnedEntity | `Core/Entities/Base/UserOwnedEntity.cs` | 所有用户实体继承 | 48 个属性定义（每个实体 3 个） |
| BaseRepository<T> | `Infrastructure/Repositories/Base/BaseRepository.cs` | 4 个 Repository 继承 | 200+ 行 CRUD 代码 |
| ApiResponse<T> | `Core/Models/ApiResponse.cs` | Auth Endpoints 使用 | 避免 3 个端点重复写响应格式化 |
| ExceptionHandlingMiddleware | `Api/Middleware/ExceptionHandlingMiddleware.cs` | 全局自动生效 | 避免所有端点重复写异常处理 |

---

## 本阶段新增的可复用功能清单

| 复用组件 | 文件路径 | 说明 | 被复用次数（预估） |
|---------|---------|------|-----------------|
| UserEntity | `Core/Entities/UserEntity.cs` | 用户实体 | 所有阶段 |
| 其他 15+ 实体 | `Core/Entities/` | 业务实体 | 对应业务阶段 |
| UserRepository | `Infrastructure/Repositories/UserRepository.cs` | 用户仓储 | 所有阶段 |
| GlobalConfigRepository | `Infrastructure/Repositories/GlobalConfigRepository.cs` | 全局配置仓储 | 所有阶段 |
| UserConfigRepository | `Infrastructure/Repositories/UserConfigRepository.cs` | 用户配置仓储 | 所有阶段 |
| LLMModelConfigRepository | `Infrastructure/Repositories/LLMModelConfigRepository.cs` | LLM 配置仓储 | 阶段三 |
| JwtService | `Application/Services/JwtService.cs` | JWT 服务 | 所有阶段 |
| JwtAuthMiddleware | `Api/Middleware/JwtAuthMiddleware.cs` | JWT 认证中间件 | 所有阶段 |
| PasswordHelper | `Infrastructure/Helpers/PasswordHelper.cs` | 密码加密工具 | 阶段二 |
| AuthService | `Application/Services/AuthService.cs` | 认证服务 | 所有阶段 |

---

## 任务清单

| 编号 | 任务名称 | 文件路径 | 产出物 | 复用自 | 是否为复用功能 | 前置依赖 | 验证方式 |
|------|---------|---------|-------|------|-------------|---------|---------|
| **2.1** | 创建 User 实体 | `server/src/AutoCodeForge.Core/Entities/UserEntity.cs` | User 实体类（继承基类） | 阶段一（AuditableEntity） | ✅ 是 | 阶段一 | 代码编译，属性与需求文档一致 |
| **2.2** | 创建 Task 实体 | `server/src/AutoCodeForge.Core/Entities/TaskEntity.cs` | Task 实体类 + TaskStatus 枚举 | 阶段一（UserOwnedEntity） | ✅ 是 | 2.1 | 代码编译，属性与需求文档一致 |
| **2.3** | 创建 TaskLog 实体 | `server/src/AutoCodeForge.Core/Entities/TaskLogEntity.cs` | TaskLog 实体类 | 阶段一（UserOwnedEntity） | ✅ 是 | 2.2 | 代码编译，属性与需求文档一致 |
| **2.4** | 创建 ChatSession 实体 | `server/src/AutoCodeForge.Core/Entities/ChatSessionEntity.cs` | ChatSession 实体类 | 阶段一（UserOwnedEntity） | ✅ 是 | 2.1 | 代码编译，属性与需求文档一致 |
| **2.5** | 创建 ChatMessage 实体 | `server/src/AutoCodeForge.Core/Entities/ChatMessageEntity.cs` | ChatMessage 实体类 + MessageType 枚举 | 阶段一（UserOwnedEntity） | ✅ 是 | 2.4 | 代码编译，属性与需求文档一致 |
| **2.6** | 创建 Agent 实体 | `server/src/AutoCodeForge.Core/Entities/AgentEntity.cs` | Agent 实体类 | 阶段一（UserOwnedEntity） | ✅ 是 | 2.1 | 代码编译，属性与需求文档一致 |
| **2.7** | 创建 Repository 实体 | `server/src/AutoCodeForge.Core/Entities/RepositoryEntity.cs` | Repository 实体类 + GitProvider/AuthenticationType/MergeStrategy 枚举 | 阶段一（UserOwnedEntity） | ✅ 是 | 2.1 | 代码编译，属性与需求文档一致 |
| **2.8** | 创建 ScheduledTask 实体 | `server/src/AutoCodeForge.Core/Entities/ScheduledTaskEntity.cs` | ScheduledTask 实体类 + TriggerType/ScheduleStatus 枚举 | 阶段一（UserOwnedEntity） | ✅ 是 | 2.1 | 代码编译，属性与需求文档一致 |
| **2.9** | 创建 ScheduledTaskExecution 实体 | `server/src/AutoCodeForge.Core/Entities/ScheduledTaskExecutionEntity.cs` | ScheduledTaskExecution 实体类 + ExecutionStatus 枚举 | 阶段一（UserOwnedEntity） | ✅ 是 | 2.8 | 代码编译，属性与需求文档一致 |
| **2.10** | 创建 Pipeline 实体 | `server/src/AutoCodeForge.Core/Entities/PipelineEntity.cs` | Pipeline 实体类 + PipelineStatus 枚举 | 阶段一（UserOwnedEntity） | ✅ 是 | 2.1 | 代码编译，属性与需求文档一致 |
| **2.11** | 创建 Build 实体 | `server/src/AutoCodeForge.Core/Entities/BuildEntity.cs` | Build 实体类 + BuildStatus 枚举 | 阶段一（UserOwnedEntity） | ✅ 是 | 2.10 | 代码编译，属性与需求文档一致 |
| **2.12** | 创建 GlobalConfig 实体 | `server/src/AutoCodeForge.Core/Entities/GlobalConfigEntity.cs` | GlobalConfig 实体类 | 阶段一（AuditableEntity） | ✅ 是 | 阶段一 | 代码编译，属性与需求文档一致 |
| **2.13** | 创建 UserConfig 实体 | `server/src/AutoCodeForge.Core/Entities/UserConfigEntity.cs` | UserConfig 实体类 | 阶段一（UserOwnedEntity） | ✅ 是 | 2.1, 2.12 | 代码编译，属性与需求文档一致 |
| **2.14** | 创建 LLMModelConfig 实体 | `server/src/AutoCodeForge.Core/Entities/LLMModelConfigEntity.cs` | LLMModelConfig 实体类 + LLMProvider 枚举 | 阶段一（UserOwnedEntity） | ✅ 是 | 2.1 | 代码编译，属性与需求文档一致 |
| **2.15** | 创建 AISessionConfig 实体 | `server/src/AutoCodeForge.Core/Entities/AISessionConfigEntity.cs` | AISessionConfig 实体类 | 阶段一（UserOwnedEntity） | ✅ 是 | 2.1 | 代码编译，属性与需求文档一致 |
| **2.16** | 创建 WikiPage 实体 | `server/src/AutoCodeForge.Core/Entities/WikiPageEntity.cs` | WikiPage 实体类 | 阶段一（UserOwnedEntity） | ✅ 是 | 2.1 | 代码编译，属性与需求文档一致 |
| **2.17** | 配置 CodeFirst 自动建表 | `server/src/AutoCodeForge.Infrastructure/Data/DatabaseInitializer.cs` | DatabaseInitializer 类（初始化表+种子数据） | - | ❌ - | 2.1-2.16 | 代码编译，包含 InitTables() 方法 |
| **2.18** | 创建密码加密工具 | `server/src/AutoCodeForge.Infrastructure/Helpers/PasswordHelper.cs` | PasswordHelper 类（PBKDF2/Argon2 加密） | - | ✅ 是 | 阶段一 | 代码编译，包含 HashPassword/VerifyPassword 方法 |
| **2.19** | 创建 JWT 服务 | `server/src/AutoCodeForge.Application/Services/JwtService.cs` | JwtService 类（生成 Token/验证 Token） | - | ✅ 是 | 阶段一 | 代码编译，包含 GenerateToken/ValidateToken 方法 |
| **2.20** | 创建 JWT 中间件 | `server/src/AutoCodeForge.Api/Middleware/JwtAuthMiddleware.cs` | JwtAuthMiddleware 类（Token 验证+用户解析） | - | ✅ 是 | 2.19 | 代码编译，从 Header 提取并验证 Token |
| **2.21** | 创建 Auth DTO | `server/src/AutoCodeForge.Core/DTOs/Auth/` | LoginRequest/RegisterRequest/AuthResponse DTO | - | ❌ - | 阶段一 | 代码编译，DTO 定义完整 |
| **2.22** | 创建 AuthService | `server/src/AutoCodeForge.Application/Services/AuthService.cs` | AuthService 类（登录/注册/获取当前用户） | - | ✅ 是 | 2.1, 2.18, 2.19 | 代码编译，包含 Login/Register/GetCurrentUser 方法 |
| **2.23** | 创建 UserRepository | `server/src/AutoCodeForge.Infrastructure/Repositories/UserRepository.cs` | UserRepository 类（继承 BaseRepository<UserEntity>） | 阶段一（BaseRepository） | ✅ 是 | 2.1, 阶段一 | 代码编译 |
| **2.24** | 创建 GlobalConfigRepository | `server/src/AutoCodeForge.Infrastructure/Repositories/GlobalConfigRepository.cs` | GlobalConfigRepository 类 | 阶段一（BaseRepository） | ✅ 是 | 2.12, 阶段一 | 代码编译 |
| **2.25** | 创建 UserConfigRepository | `server/src/AutoCodeForge.Infrastructure/Repositories/UserConfigRepository.cs` | UserConfigRepository 类 | 阶段一（BaseRepository） | ✅ 是 | 2.13, 阶段一 | 代码编译 |
| **2.26** | 创建 LLMModelConfigRepository | `server/src/AutoCodeForge.Infrastructure/Repositories/LLMModelConfigRepository.cs` | LLMModelConfigRepository 类 | 阶段一（BaseRepository） | ✅ 是 | 2.14, 阶段一 | 代码编译 |
| **2.27** | 创建 Auth Endpoints | `server/src/AutoCodeForge.Api/Endpoints/AuthEndpoints.cs` | /api/v1/auth/login /register /me 端点 | 阶段一（ApiResponse） | ❌ - | 2.21, 2.22 | 代码编译，包含 3 个 Minimal API 端点 |
| **2.28** | 在 Program.cs 注册认证 | `server/src/AutoCodeForge.Api/Program.cs` | 注册 JwtService/AuthService/Repository，启用 JwtMiddleware | - | ❌ - | 2.20, 2.22-2.26 | 代码编译，服务已注册 |
| **2.29** | 创建种子数据初始化器 | `server/src/AutoCodeForge.Infrastructure/Data/SeedData.cs` | SeedData 类（初始化默认 Agent/配置/测试用户） | - | ❌ - | 2.6, 2.12, 2.14 | 代码编译，包含 InitializeSeedData 方法 |
| **2.30** | 验证认证功能 | `server/src/AutoCodeForge.Api/` | 可以注册/登录/获取当前用户，数据库表已创建 | - | ❌ - | 2.1-2.29 | 运行应用，使用 Swagger 测试 API，查看数据库文件 |

---

## 关键文件内容预览

### UserEntity.cs

```csharp
// server/src/AutoCodeForge.Core/Entities/UserEntity.cs
[SugarTable("Users")]
public class UserEntity : AuditableEntity
{
    [SugarColumn(IsPrimaryKey = true)]
    public Guid Id { get; set; }

    public string NtId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? PasswordHash { get; set; }
    public bool IsDeleted { get; set; }
}
```

### UserRepository.cs

```csharp
// server/src/AutoCodeForge.Infrastructure/Repositories/UserRepository.cs
public class UserRepository : BaseRepository<UserEntity>
{
    public UserRepository(ISqlSugarClient db, ICurrentUser currentUser)
        : base(db, currentUser)
    {
    }

    // 额外的查询方法（如有需要）
    public async Task<UserEntity?> GetByNtIdAsync(string ntId)
    {
        return await _db.Queryable<UserEntity>()
            .Where(u => u.NtId == ntId && !u.IsDeleted)
            .FirstAsync();
    }
}
```

### AuthService.cs 核心方法

```csharp
// server/src/AutoCodeForge.Application/Services/AuthService.cs
public class AuthService
{
    private readonly UserRepository _userRepository;
    private readonly PasswordHelper _passwordHelper;
    private readonly JwtService _jwtService;

    public async Task<AuthResponse> LoginAsync(LoginRequest request) { ... }
    public async Task<AuthResponse> RegisterAsync(RegisterRequest request) { ... }
    public async Task<UserEntity> GetCurrentUserAsync(string ntId) { ... }
}
```

---

## 注意事项

⚠️ **重要提醒**

1. **SugarTable 和 SugarColumn 特性** - 不要忘记给实体和属性添加 SqlSugar 特性
2. **枚举类型** - 枚举需要和实体放在一起，或者放到单独的 Enums 文件夹
3. **密码哈希** - 不要存储明文密码，使用 PBKDF2 或 Argon2
4. **JWT 密钥** - 生产环境必须从环境变量读取，不要硬编码
5. **种子数据** - 只在开发环境初始化种子数据
6. **NtId 传递** - 认证中间件需要将 NtId 存储到 HttpContext.Items 中，供 BaseRepository 使用

✅ **验收标准**

- 所有实体已创建并继承基类
- 数据库表可以自动创建
- 用户可以注册和登录
- JWT Token 可以正确验证
- /api/v1/auth/me 可以返回当前用户信息
- 种子数据已正确初始化

---

## 阶段完成总结

### 复用收益

本阶段通过复用阶段一的基础设施，预计可以**避免 500+ 行重复代码**：
- 16+ 实体无需重复写 CreatedAt/UpdatedAt/NtId/IsDeleted
- 4 个 Repository 无需重复写 CRUD/软删除/分页
- 3 个 Auth Endpoints 无需重复写响应格式化

### 本阶段新增复用

本阶段创建了 8 个可复用组件，将被后续所有阶段使用。

### 下一步

完成本阶段后，进入 **阶段三：AI 核心模块**，详见 `03-phase-three-ai-core.md`
