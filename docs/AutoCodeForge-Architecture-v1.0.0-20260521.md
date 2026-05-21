# AutoCodeForge 架构设计文档

**版本**: v1.0.0  
**生成时间**: 2026-05-21  
**架构风格**: 前后端分离 + .NET 分层 + 模块驱动  

---

## 一、整体架构视图

```
┌─────────────────────────────────────────────────────────────────┐
│                     浏览器 (SPA - Vue 3)                         │
├─────────────────────────────────────────────────────────────────┤
│
│  ┌─ Router (lazy-load) ──> Pinia Store ──> Components
│  │
│  └─ Modules (module-first)
│      ├─ auth/
│      ├─ console/
│      ├─ task-center/
│      ├─ repo-management/
│      ├─ system-config/
│      └─ ...
│
├────────── HTTP + JWT Auth ──────────┐
│                                      │
│  ┌─────────────────────────────────▼────────────────────────┐
│  │           .NET API Gateway (Express/ASP.NET Core)         │
│  │                   + JWT Middleware                        │
│  ├──────────────────────────────────────────────────────────┤
│  │
│  │  ┌──────────────────────────────────────────────────┐
│  │  │              API Controllers                      │
│  │  │  ┌────────────────────────────────────────────┐  │
│  │  │  │  Auth | Agent | Chat | Task | Repo | ...  │  │
│  │  │  └────────────────────────────────────────────┘  │
│  │  └──────────────────────────────────────────────────┘
│  │          ↓
│  │  ┌──────────────────────────────────────────────────┐
│  │  │       Application Layer (Services)               │
│  │  │  ┌────────────────────────────────────────────┐  │
│  │  │  │ AuthService | AgentService | TaskService   │  │
│  │  │  │ ChatService | RepositoryService | ...      │  │
│  │  │  └────────────────────────────────────────────┘  │
│  │  └──────────────────────────────────────────────────┘
│  │          ↓
│  │  ┌──────────────────────────────────────────────────┐
│  │  │        Core Layer (Entities & Repositories)      │
│  │  │  ┌────────────────────────────────────────────┐  │
│  │  │  │ User | Task | Repository | Pipeline | ...  │  │
│  │  │  │         + IRepository<T>                   │  │
│  │  │  └────────────────────────────────────────────┘  │
│  │  └──────────────────────────────────────────────────┘
│  │          ↓
│  │  ┌──────────────────────────────────────────────────┐
│  │  │   Infrastructure Layer (Data & External)        │
│  │  │  ┌────────────────────────────────────────────┐  │
│  │  │  │  DbContext (SqlSugar) | LlmGateway |       │  │
│  │  │  │  GitProviders | Task Queue | Cache | ...   │  │
│  │  │  └────────────────────────────────────────────┘  │
│  │  └──────────────────────────────────────────────────┘
│  │          ↓
│  │  ┌─────────────────────────────────────────────────┐
│  │  │  BackgroundServices (Scheduled & Async)        │
│  │  │  ├─ ScheduledTaskExecutor (30s poll)          │
│  │  │  ├─ PipelineSyncService (30s poll)            │
│  │  │  └─ ...                                        │
│  │  └─────────────────────────────────────────────────┘
│  │
│  └──────────────────────────────────────────────────────────┘
│
├─ 数据持久化 ───> SQLite (dev) / SQL Server (prod)
├─ 消息队列 ────> 内存队列 + BackgroundService
├─ 认证 ────────> JWT HS256
├─ 外部集成 ───> LLM API (OpenAI/Azure) | Git API (GitHub/GitLab)
│
└─────────────────────────────────────────────────────────────────┘
```

---

## 二、分层设计详解

### 2.1 API 层（Controllers）
**职责**: HTTP 请求处理、参数验证、响应格式化  

**关键文件**:
- `AutoCodeForge.Api/Controllers/*Controller.cs`
- `AutoCodeForge.Api/Middleware/JwtAuthMiddleware.cs`
- `AutoCodeForge.Api/Models/ApiResponse.cs`

**关键方法**:
```csharp
// 统一响应格式
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public int StatusCode { get; set; }
    public T Data { get; set; }
    public string Message { get; set; }
    public Dictionary<string, List<string>> Errors { get; set; }
    public DateTime Timestamp { get; set; }
}

// JWT 认证中间件
public class JwtAuthMiddleware
{
    // 验证 Bearer Token
    // 从 Claims 或 Header 提取 UserId/NtId
    // 注入 HttpContext.Items["UserId"]
}
```

**规范**:
- ✅ 所有端点返回 `ApiResponse<T>` 或错误 ApiResponse
- ✅ 参数验证使用 Data Annotations / FluentValidation
- ✅ 异常由全局异常中间件捕获并转换为 ApiResponse
- ✅ 所有受保护端点检查 `meta.requiresAuth` 对应关系

### 2.2 Application 层（Services）
**职责**: 业务逻辑编排、事务管理、DTOs 转换  

**关键文件**:
- `AutoCodeForge.Application/Services/*Service.cs`
- `AutoCodeForge.Application/Dtos/*Dto.cs`
- `AutoCodeForge.Application/Mappers/*Mapper.cs`

**关键模式**:
```csharp
public interface IUserService
{
    Task<UserResponseDto> GetUserAsync(string ntId);
    Task<UserResponseDto> CreateUserAsync(CreateUserDto dto);
    Task UpdateUserAsync(string ntId, UpdateUserDto dto);
}

public class UserService : IUserService
{
    private readonly IRepository<User> _userRepository;
    
    public async Task<UserResponseDto> GetUserAsync(string ntId)
    {
        var user = await _userRepository.GetAsync(u => u.NtId == ntId);
        return user.MapToDto(); // 数据转换
    }
}
```

**规范**:
- ✅ 所有数据库操作通过 IRepository<T> 接口
- ✅ DTOs 必须通过 Mapper 转换为 Model/Entity
- ✅ 异步操作使用 async/await
- ✅ 事务边界明确（Service 级）

### 2.3 Core 层（Entities & Repositories）
**职责**: 领域模型定义、仓储接口、业务规则  

**关键实体**:
```csharp
// 基础可审计实体
public abstract class AuditableEntity
{
    public string Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
}

// 用户拥有的实体
public abstract class UserOwnedEntity : AuditableEntity
{
    public string OwnerId { get; set; }
    public User Owner { get; set; }
}

// 核心实体
public class User : AuditableEntity
{
    public string NtId { get; set; }
    public string Email { get; set; }
    public string DisplayName { get; set; }
    public List<Role> Roles { get; set; }
}

public class Task : UserOwnedEntity
{
    public string Title { get; set; }
    public string Description { get; set; }
    public TaskStatus Status { get; set; }
    public List<TaskLog> Logs { get; set; }
}

public class Pipeline : UserOwnedEntity
{
    public string RepositoryId { get; set; }
    public string DefinitionPath { get; set; }
    public List<Build> Builds { get; set; }
}
```

**仓储接口**:
```csharp
public interface IRepository<T> where T : AuditableEntity
{
    // 查询（支持 QueryFilter 自动应用）
    Task<T> GetAsync(Expression<Func<T, bool>> predicate);
    Task<List<T>> GetListAsync(Expression<Func<T, bool>> predicate = null);
    
    // 修改
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity); // 软删除
    Task SaveChangesAsync();
}
```

**规范**:
- ✅ 所有实体继承 AuditableEntity（自动跟踪 CreatedAt/UpdatedAt/IsDeleted）
- ✅ 用户拥有的实体继承 UserOwnedEntity（自动隔离）
- ✅ 仓储返回 IQueryable<T> 支持链式查询
- ✅ QueryFilter 自动过滤 IsDeleted=true 记录

### 2.4 Infrastructure 层（数据和外部服务）
**职责**: 数据库访问、第三方服务集成  

**关键组件**:
```csharp
// DbContext 配置
public class AutoCodeForgeDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Task> Tasks { get; set; }
    public DbSet<Pipeline> Pipelines { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // 自动配置 IsDeleted 全局过滤
        modelBuilder.ApplyQueryFilters();
        
        // 种子数据
        modelBuilder.ApplySeedData();
    }
}

// LLM 网关
public interface ILlmGateway
{
    Task<AgentExecutionResult> ExecuteAgentAsync(
        string agentName, 
        string userMessage, 
        List<Tool> tools);
    
    IAsyncEnumerable<string> StreamChatAsync(
        string systemPrompt, 
        List<Message> messages);
}

// Git 提供者工厂
public interface IGitProviderFactory
{
    IGitProvider CreateProvider(string providerType, string credentials);
}

public interface IGitProvider
{
    Task<List<Repository>> ListRepositoriesAsync();
    Task<List<Branch>> GetBranchesAsync(string repoName);
    Task<PullRequest> CreatePullRequestAsync(string repoName, CreatePRDto dto);
}
```

**规范**:
- ✅ 所有外部服务通过接口隔离（便于 Mock 和测试）
- ✅ DbContext 支持 QueryFilters 和 Seed 机制
- ✅ 凭据存储加密（SQL Server 支持 Data Protection API）

---

## 三、前端模块驱动设计

### 3.1 模块-优先结构（Module-First）
```
src/modules/auth/
├── index.ts                    # 公开 API
├── auth.api.ts                 # HTTP 调用（Axios）
├── auth.types.ts               # DTO/Request/Response 类型
├── store/
│   └── useAuthStore.ts         # Pinia setup store
├── routes.ts                   # 路由定义（lazy-load）
└── views/
    ├── LoginView.vue
    └── RegisterView.vue
```

### 3.2 数据流链路
```
Component (View)
    ↓ [computed/emit]
Pinia Store (state/actions)
    ↓ [call]
API Layer (auth.api.ts)
    ↓ [Axios]
HTTP Request to Backend
    ↓ [response]
[Mapper] DTO → Model
    ↓ [setState]
Store State Updated
    ↓ [watch/computed]
Component Re-render
```

### 3.3 关键文件规范

**auth.api.ts** (HTTP 调用层):
```typescript
import axios from 'axios'
import type { LoginDto, LoginResponseDto } from './auth.types'

export const authApi = {
  async login(dto: LoginDto): Promise<LoginResponseDto> {
    const response = await axios.post('/api/auth/login', dto)
    return response.data.data
  }
}
```

**auth.types.ts** (数据类型):
```typescript
export interface LoginDto {
  email: string
  password: string
}

export interface UserModel {
  id: string
  email: string
  displayName: string
  roles: string[]
}

export interface LoginResponseDto {
  token: string
  user: UserModel
}
```

**useAuthStore.ts** (状态管理):
```typescript
import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { authApi } from '../auth.api'
import type { LoginDto, UserModel } from '../auth.types'

export const useAuthStore = defineStore('auth', () => {
  const user = ref<UserModel | null>(null)
  const loading = ref(false)
  const error = ref<string | null>(null)
  
  const isAuthenticated = computed(() => user.value !== null)
  
  async function login(dto: LoginDto) {
    loading.value = true
    error.value = null
    try {
      const response = await authApi.login(dto)
      localStorage.setItem('token', response.token)
      user.value = response.user
    } catch (err) {
      error.value = (err as Error).message
    } finally {
      loading.value = false
    }
  }
  
  return {
    user,
    loading,
    error,
    isAuthenticated,
    login
  }
})
```

**routes.ts** (路由定义):
```typescript
import type { RouteRecordRaw } from 'vue-router'

export const authRoutes: RouteRecordRaw[] = [
  {
    path: '/login',
    name: 'auth.login',
    component: () => import('./views/LoginView.vue'),
    meta: { requiresAuth: false }
  },
  {
    path: '/register',
    name: 'auth.register',
    component: () => import('./views/RegisterView.vue'),
    meta: { requiresAuth: false }
  }
]
```

---

## 四、数据模型与关系

### 4.1 核心实体关系图
```
┌──────────────┐         ┌──────────────┐
│    User      │────────▶│    Role      │
│ (多)         │ has (多)│              │
│ - id         │         │ - id         │
│ - ntId       │         │ - name       │
│ - email      │         │              │
└──────────────┘         └──────────────┘
       │
       │ owns (多)
       ▼
┌──────────────────┐
│ UserOwnedEntity  │◀─── Task, Repository, Pipeline, etc.
│ (base class)     │
│ - id             │
│ - ownerId  ──────┘
│ - createdAt      │
│ - updatedAt      │
└──────────────────┘
       │
       │ contains (多)
       ▼
┌──────────────────┐
│   TaskLog        │
│ - id             │
│ - taskId         │
│ - logLevel       │
│ - message        │
└──────────────────┘

┌──────────────────┐      ┌──────────────────┐
│  Repository      │──────▶│   Branch         │
│ - id             │ has   │ - id             │
│ - name           │ (多)  │ - name           │
│ - provider       │       │ - lastCommit     │
└──────────────────┘       └──────────────────┘
       │
       │ contains (多)
       ▼
┌──────────────────┐
│   Pipeline       │
│ - id             │
│ - repositoryId   │
│ - definitionPath │
└──────────────────┘
       │
       │ has (多)
       ▼
┌──────────────────┐
│    Build         │
│ - id             │
│ - pipelineId     │
│ - status         │
│ - buildNumber    │
└──────────────────┘
```

### 4.2 关键实体字段

**User**:
```
id: string (PK)
ntId: string (UK, 从 JWT Claims 提取)
email: string (UK)
displayName: string
roles: List<Role> (多对多)
createdAt: DateTime
updatedAt: DateTime
isDeleted: bool (软删除)
```

**Task**:
```
id: string (PK)
ownerId: string (FK → User.id)
title: string
description: string
status: TaskStatus (enum: Pending, Running, Completed, Failed)
startedAt: DateTime?
completedAt: DateTime?
logs: List<TaskLog>
createdAt: DateTime
updatedAt: DateTime
isDeleted: bool
```

**Pipeline**:
```
id: string (PK)
ownerId: string (FK → User.id)
repositoryId: string (FK → Repository.id)
definitionPath: string (YAML/JSON 路径)
status: PipelineStatus (Active, Inactive, Archived)
builds: List<Build>
createdAt: DateTime
updatedAt: DateTime
isDeleted: bool
```

---

## 五、后台服务与异步处理

### 5.1 BackgroundService 设计

**ScheduledTaskExecutor** (30 秒轮询):
```csharp
public class ScheduledTaskExecutor : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // 1. 查询所有 NextExecutionAt <= Now 且 Status = Pending 的任务
            var dueTasks = await _repository.GetListAsync(
                t => t.NextExecutionAt <= DateTime.UtcNow 
                  && t.Status == ScheduledTaskStatus.Pending);
            
            // 2. 并发执行（带信号量控制）
            foreach (var task in dueTasks)
            {
                await _taskExecutor.ExecuteAsync(task);
            }
            
            // 3. 30 秒休眠
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }
}
```

**PipelineSyncService** (30 秒轮询):
```csharp
public class PipelineSyncService : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // 1. 查询所有活跃 Pipeline 的最新 Build
            var pipelines = await _pipelineRepository.GetListAsync();
            
            // 2. 对每个 Pipeline 调用 Git API 获取构建状态
            foreach (var pipeline in pipelines)
            {
                var latestBuild = await _gitProvider.GetBuildStatusAsync(
                    pipeline.RepositoryId, 
                    pipeline.DefinitionId);
                
                // 3. 更新 Build 状态
                await _buildService.UpdateBuildStatusAsync(latestBuild);
            }
            
            // 4. 30 秒休眠
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }
}
```

### 5.2 任务队列与执行模型

```
[Task Received]
    ↓
[EnqueueTask] ──▶ Memory Queue
    ↓
[TaskExecutor.ExecuteAsync]
    ├─ Set Status = Running
    ├─ Call Business Logic (AgentService, etc.)
    ├─ Log Output/Errors
    ├─ Handle Exceptions
    └─ Set Status = Completed/Failed
    ↓
[Store Result]
    ├─ Update Task Record
    ├─ Save Logs
    └─ Update Frontend (optional SSE)
```

---

## 六、API 安全与身份验证

### 6.1 JWT 流程
```
[Login Request]
    ├─ Email + Password → Validate Credentials
    ├─ Generate JWT (HS256, 1 hour expiry)
    └─ Return { token, user }

[Subsequent Requests]
    ├─ Add Authorization: Bearer <token> header
    ├─ JwtAuthMiddleware Validates & Extracts Claims
    ├─ Inject UserId/NtId into HttpContext.Items
    └─ Controller Uses CurrentUser Property

[Token Refresh]
    ├─ POST /api/auth/refresh
    ├─ Validate Old Token (allow expired)
    └─ Issue New Token
```

### 6.2 权限隔离

**多租户隔离** (User-scoped):
```csharp
// 仓储自动过滤
var tasks = await _repository.GetListAsync(
    t => t.OwnerId == currentUser.Id); // 自动过滤
```

**管理员跨界策略** (Scoped Whitelist):
```
Admin 可访问其他用户的资源，但需满足：
1. NtId 在 Admin Whitelist 中
2. ResourceScope 匹配（All/Department/Team）
3. TargetTenant 明确指定
4. 操作被审计并落库
```

---

## 七、扩展点与集成接口

### 7.1 新功能集成点

**新增数据模型**:
1. 创建 Entity 类继承 AuditableEntity
2. 在 DbContext 中添加 DbSet<T>
3. 创建 IRepository<T> 实现
4. 在 Service 层调用

**新增 API 端点**:
1. 创建 Service 方法（应用层）
2. 创建 Controller action（API 层）
3. 添加 ApiResponse<T> 返回
4. 在 routes.ts 中定义前端路由

**新增前端模块**:
1. 创建 src/modules/<name>/ 目录
2. 按规范创建 api.ts, types.ts, store, routes.ts, views/
3. 在根级 router 中导入模块路由
4. 在 App.vue 导航菜单中挂载

### 7.2 外部服务集成模式

```csharp
// 新增 LLM 提供者
public interface ILlmProvider
{
    Task<string> CallAsync(string prompt);
}

public class OpenAiProvider : ILlmProvider { }
public class AzureProvider : ILlmProvider { }

// 在 Startup 中注册
services.AddScoped<ILlmGateway>(sp => 
    new LlmGateway(
        new OpenAiProvider(config["OpenAI:Key"]),
        new AzureProvider(config["Azure:Key"])
    ));
```

---

## 八、性能与扩展考虑

### 8.1 当前性能基线
- 单体部署：ASP.NET Core 托管
- 数据库：SQLite (dev) / SQL Server (prod)
- 连接池：默认 SqlSugar 配置
- 缓存：内存缓存（未使用 Redis）

### 8.2 瓶颈识别
⚠️ 30 秒轮询延迟（Pipeline/ScheduledTask）  
⚠️ 内存队列无持久化（服务重启丢失任务）  
⚠️ 单机分布式锁不可用（多实例场景）  

### 8.3 优化方向（下一阶段）
1. 引入 Redis 缓存与消息队列（RabbitMQ/Kafka）
2. 使用分布式锁（DistributedLock NuGet）
3. 实现 WebSocket 实时推送（替代轮询）
4. 添加数据库索引优化（QueryFilter 相关字段）

---

## 九、测试与质量策略

### 9.1 测试分层
```
Unit Tests (Services, Mappers)
    ↓
Integration Tests (Controllers, DbContext)
    ↓
Smoke Tests (核心流程端到端验证)
```

### 9.2 当前测试覆盖
✅ 75 tests / 0 failed  
✅ 核心服务：AuthService, AgentService, TaskService, GitProviderFactory  
✅ Repository + DbContext 集成  
✅ API 端点基础验证  

### 9.3 质量门禁（Pre-merge）
1. ✅ dotnet build 成功
2. ✅ dotnet test 全部通过
3. ✅ 代码复用规范检查（component/service）
4. ✅ API 合约检查（ResponseDto 格式）

---

## 十、部署与运维架构

### 10.1 部署拓扑（当前）
```
Docker Container (ASP.NET Core)
    ├─ App Service (Azure App Service / AWS EC2)
    ├─ SQL Server Container (外部 RDS)
    └─ File Storage (Azure Blob / AWS S3)

JavaScript SPA
    └─ CDN / Static Web App
```

### 10.2 环境配置
- **Development**: appsettings.Development.json (SQLite, in-memory)
- **Staging**: appsettings.Staging.json (SQL Server, test secrets)
- **Production**: 环境变量注入 (Key Vault / Secrets Manager)

### 10.3 关键配置项
```
JWT:Key                    # 签名密钥（最小 32 字符）
JWT:ExpirationMinutes      # Token 有效期
Database:ConnectionString  # 数据库连接
LLM:Provider              # OpenAI / Azure OpenAI
LLM:ApiKey                # LLM API 密钥
Git:Providers[]           # GitHub/GitLab/Azure DevOps 凭据
```

---

## 附录：架构决策记录 (ADR)

| 编号 | 决策 | 原因 | 替代方案 |
|------|------|------|---------|
| ADR-001 | 前后端分离 (SPA) | 独立扩展、复用性强 | 全栈 MVC |
| ADR-002 | Vue 3 Composition API | 现代化、类型友好 | Vue 2 / React |
| ADR-003 | Pinia Setup Store | 扁平化、易测试 | Options API |
| ADR-004 | .NET 四层分层 | 职责清晰、易维护 | DDD / Clean Architecture |
| ADR-005 | SqlSugar ORM | 轻量、原生 SQL 支持 | Entity Framework Core |
| ADR-006 | 内存任务队列 | MVP 快速交付 | Redis / RabbitMQ |
| ADR-007 | 30 秒轮询 | 简单可靠、开发成本低 | WebSocket / gRPC |
| ADR-008 | JWT HS256 | 无状态、低开销 | OAuth 2.0 / SAML |

