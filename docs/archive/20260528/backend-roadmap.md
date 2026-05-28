# AutoCodeForge 后端整体规划

**文档版本**: v2.0
**生成日期**: 2026-05-19
**项目类型**: MVP（最小可行产品）

---

## 一、项目概览

### 1.1 技术栈

| 组件 | 选型 | 说明 |
|------|------|------|
| 运行时 | .NET 10 | 最新 LTS 版本 |
| Web 框架 | ASP.NET Core Minimal API | 轻量化快速开发 |
| ORM | SqlSugarCore | 高性能、轻量级、CodeFirst |
| 数据库 | SQLite | 零配置、文件型，适合 MVP |
| AI 框架 | Microsoft Agent Framework | Semantic Kernel 继承者 |
| 认证 | JWT Bearer Token | 简化用户认证 |
| 后台任务 | BackgroundService | 轻量级定时任务 |
| API 文档 | Swashbuckle (Swagger) | 自动生成 API 文档 |

### 1.2 目录结构规划

```
AutoCodeForge/
├── client/                          # 前端（已存在）
└── server/                          # 后端（新建）
    ├── src/
    │   ├── AutoCodeForge.Api/      # API 层
    │   │   ├── Endpoints/          # Minimal API 端点分组
    │   │   ├── Middleware/         # 中间件（全局复用）
    │   │   └── Program.cs
    │   ├── AutoCodeForge.Core/     # 核心层（全项目复用）
    │   │   ├── Entities/           # 实体模型（基类继承复用）
    │   │   ├── DTOs/               # 数据传输对象
    │   │   ├── Exceptions/         # 自定义异常（全局复用）
    │   │   ├── Interfaces/         # 接口定义
    │   │   ├── Models/             # 通用模型（ApiResponse、分页等）
    │   │   └── Constants/          # 常量定义
    │   ├── AutoCodeForge.Infrastructure/ # 基础设施层（核心复用层）
    │   │   ├── Data/               # 数据库（SqlSugar 配置、全局过滤器）
    │   │   ├── Repositories/       # 仓储（BaseRepository 基类复用）
    │   │   ├── AI/                 # Microsoft Agent Framework 封装（Agent/会话/LLM 复用）
    │   │   ├── Git/                # Git 平台对接（IGitProvider 接口复用）
    │   │   ├── BackgroundServices/ # 后台服务（任务调度基类复用）
    │   │   └── Helpers/            # 工具类（全局复用）
    │   └── AutoCodeForge.Application/ # 应用层
    │       ├── Services/           # 业务服务（配置/日志等服务复用）
    │       ├── Mappers/            # 实体映射
    │       └── Extensions/         # 扩展方法（复用）
    ├── tests/
    │   └── AutoCodeForge.Tests/    # 测试项目
    └── AutoCodeForge.sln           # 解决方案文件
```

---

## 二、可复用架构设计（核心）

### 2.1 复用分层图

```
┌─────────────────────────────────────────────────────────────┐
│                     API 层 (Minimal API)                      │
│  ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────┐        │
│  │  Auth    │ │  Chat    │ │  Task    │ │  Repo   │        │
│  │ Endpoints│ │ Endpoints│ │ Endpoints│ │Endpoints│  ...    │
│  └────┬─────┘ └────┬─────┘ └────┬─────┘ └────┬─────┘        │
└───────┼────────────┼────────────┼────────────┼───────────────┘
        │            │            │            │
        └────────────┴────────────┴────────────┘
                     │
         ┌───────────┴───────────┐
         │   复用中间件层         │
         │ - ExceptionMiddleware │
         │ - JwtMiddleware       │
         │ - RequestLogging      │
         └───────────┬───────────┘
                     │
    ┌────────────────┴────────────────┐
    │         应用服务层               │
    │  ┌──────────────────────────┐   │
    │  │  ConfigService (复用)    │   │
    │  │  LoggingService (复用)   │   │
    │  └──────────────────────────┘   │
    └────────────────┬────────────────┘
                     │
    ┌────────────────┴────────────────┐
    │      基础设施复用层 (核心)       │
    │  ┌──────────────────────────┐   │
    │  │ BaseRepository<T>        │   │  ← 所有仓储继承
    │  │ - CRUD                   │   │
    │  │ - 软删除                 │   │
    │  │ - 用户隔离 (NtId)       │   │
    │  │ - 分页查询               │   │
    │  └──────────────────────────┘   │
    │  ┌──────────────────────────┐   │
    │  │ LlmGateway (复用)        │   │  ← AI 模块共用
    │  │ - 多模型切换             │   │
    │  │ - 超时重试               │   │
    │  │ - 熔断                   │   │
    │  └──────────────────────────┘   │
    │  ┌──────────────────────────┐   │
    │  │ AgentExecutor (复用)     │   │  ← 任务/聊天共用
    │  │ - Agent 调用             │   │
    │  │ - 工具函数               │   │
    │  └──────────────────────────┘   │
    │  ┌──────────────────────────┐   │
    │  │ ChatSessionManager (复用)│   │  ← 会话历史管理
    │  └──────────────────────────┘   │
    │  ┌──────────────────────────┐   │
    │  │ TaskSchedulerBase (复用) │   │  ← 定时/即时任务共用
    │  └──────────────────────────┘   │
    │  ┌──────────────────────────┐   │
    │  │ IGitProvider (接口复用)  │   │  ← 多 Git 平台
    │  └──────────────────────────┘   │
    └────────────────┬────────────────┘
                     │
    ┌────────────────┴────────────────┐
    │         核心层 (复用基础)        │
    │  ┌──────────────────────────┐   │
    │  │ AuditableEntity (基类)   │   │  ← 时间戳自动维护
    │  └──────────────────────────┘   │
    │  ┌──────────────────────────┐   │
    │  │ UserOwnedEntity (基类)   │   │  ← 用户隔离
    │  └──────────────────────────┘   │
    │  ┌──────────────────────────┐   │
    │  │ ApiResponse<T> (统一响应) │   │  ← 所有 API 共用
    │  └──────────────────────────┘   │
    │  ┌──────────────────────────┐   │
    │  │ CustomExceptions (异常)  │   │  ← 全局异常处理
    │  └──────────────────────────┘   │
    └─────────────────────────────────┘
```

### 2.2 可复用模块清单

| 模块 | 复用位置 | 复用范围 | 说明 |
|------|---------|---------|------|
| **BaseRepository&lt;T&gt;** | Infrastructure/Repositories | 所有实体仓储 | CRUD + 软删除 + 用户隔离 + 分页 |
| **AuditableEntity** | Core/Entities | 所有需审计实体 | CreatedAt/UpdatedAt 自动填充 |
| **UserOwnedEntity** | Core/Entities | 所有用户数据实体 | NtId 用户隔离字段 |
| **ApiResponse&lt;T&gt;** | Core/Models | 所有 API 端点 | 统一响应格式 |
| **ExceptionHandlingMiddleware** | Api/Middleware | 全局 | 统一异常捕获 + 日志记录 |
| **JwtAuthMiddleware** | Api/Middleware | 所有需认证端点 | Token 校验 + 用户解析 |
| **LlmGateway** | Infrastructure/AI | 聊天/任务/Agent 配置 | LLM 调用封装（多模型/重试/熔断） |
| **AgentExecutor** | Infrastructure/AI | 聊天/任务执行 | Microsoft Agent Framework 封装 |
| **ChatSessionManager** | Infrastructure/AI | 聊天/任务会话 | 会话历史 + Token 管理 |
| **TaskSchedulerBase** | Infrastructure/BackgroundServices | 定时任务/即时任务 | 任务调度基类 |
| **IGitProvider** | Infrastructure/Git | GitHub/GitLab/AzureDevOps | Git 平台统一接口 |
| **ConfigService** | Application/Services | 全局配置/用户配置 | 配置统一读取/更新 |
| **LoggingService** | Application/Services | 全局 | 结构化日志封装 |
| **PaginationHelper** | Infrastructure/Helpers | 所有列表查询 | 分页参数/结果封装 |
| **JsonSerializationHelper** | Infrastructure/Helpers | 全局 | JSON 序列化/反序列化 |
| **TimeHelper** | Infrastructure/Helpers | 全局 | 时间处理（UTC/时区） |

### 2.3 复用实现示例

#### 示例 1：BaseRepository<T>（所有仓储复用）

```csharp
// 所有仓储都继承这个基类，不用重复写 CRUD
public class BaseRepository<T> where T : class, new()
{
    protected readonly ISqlSugarClient _db;
    protected readonly string _currentUserNtId;

    public BaseRepository(ISqlSugarClient db, IHttpContextAccessor httpContextAccessor)
    {
        _db = db;
        _currentUserNtId = GetCurrentUserNtId(httpContextAccessor);
    }

    // 自动应用用户隔离和软删除过滤器
    protected ISugarQueryable<T> Queryable => _db.Queryable<T>()
        .Where(e => EF.Property<bool>(e, "IsDeleted") == false)
        .Where(e => EF.Property<string>(e, "NtId") == _currentUserNtId);

    public virtual async Task<T> GetByIdAsync(Guid id) => await Queryable.In(id).FirstAsync();
    public virtual async Task<List<T>> GetAllAsync() => await Queryable.ToListAsync();
    public virtual async Task<PagedResult<T>> GetPagedAsync(int page, int pageSize) { ... }
    public virtual async Task<T> CreateAsync(T entity) { ... }
    public virtual async Task UpdateAsync(T entity) { ... }
    public virtual async Task SoftDeleteAsync(Guid id) { ... }
}
```

**复用场景**：
- TaskRepository : BaseRepository<TaskEntity>
- ChatSessionRepository : BaseRepository<ChatSessionEntity>
- AgentRepository : BaseRepository<AgentEntity>
- RepositoryRepository : BaseRepository<RepositoryEntity>
- ... 其他 10+ 仓储

#### 示例 2：LlmGateway（AI 模块复用）

```csharp
// 聊天、任务执行、Agent 配置都复用这个 LLM 网关
public interface ILlmGateway
{
    Task<LlmResponse> ChatAsync(LlmRequest request);
    Task<LlmResponse> ChatWithToolsAsync(LlmRequest request, IEnumerable<AgentTool> tools);
}

public class LlmGateway : ILlmGateway
{
    private readonly LlmModelConfigEntity _defaultModel;
    private readonly IEnumerable<LlmModelConfigEntity> _availableModels;

    // 统一的 LLM 调用：多模型切换、超时、重试、熔断
    public async Task<LlmResponse> ChatAsync(LlmRequest request)
    {
        var model = SelectModel(request.PreferredModel);
        return await ExecuteWithRetryAsync(async () => await CallLlmAsync(model, request));
    }
}
```

**复用场景**：
- ChatService 调用 LlmGateway 进行对话
- TaskExecutor 调用 LlmGateway 解析任务
- AgentMatcher 调用 LlmGateway 做智能匹配

#### 示例 3：IGitProvider（多平台复用）

```csharp
// 统一接口，GitHub/GitLab/AzureDevOps 都实现这个接口
public interface IGitProvider
{
    Task<List<GitBranch>> GetBranchesAsync(GitProviderType provider, string repoId);
    Task<PullRequest> CreatePullRequestAsync(GitProviderType provider, string repoId, CreatePRRequest request);
    Task<List<GitCommit>> GetCommitsAsync(GitProviderType provider, string repoId, string branch);
    // ... 其他 Git 操作
}

// 各平台实现
public class GitHubProvider : IGitProvider { ... }
public class GitLabProvider : IGitProvider { ... }
public class AzureDevOpsProvider : IGitProvider { ... }
```

**复用场景**：
- RepositoryService 通过工厂获取对应 Provider
- Agent 工具调用 Git 操作时复用统一接口

### 2.4 复用依赖关系

```
阶段一（基础设施复用层）
    ↓
创建：BaseRepository、基类实体、ApiResponse、中间件、工具类
    ↓
阶段二~阶段九（业务层复用基础设施）
    ↓
所有业务模块都复用阶段一的基础设施
    ↓
阶段十（测试）
```

---

## 三、阶段规划

### 阶段一：项目初始化 & 基础设施（1-2天）✅ **复用层核心**

**目标**：搭建项目骨架，实现所有可复用基础设施

#### 任务清单

| 任务 | 优先级 | 是否复用 | 复用对象 | 说明 |
|------|--------|---------|---------|------|
| 创建 .NET 10 解决方案 | P0 | - | - | sln + 4 个项目（Api/Core/Infrastructure/Application） |
| 配置依赖注入容器 | P0 | - | - | Program.cs 中注册服务 |
| 实现全局异常处理中间件 | P0 | ✅ | 所有 API | 统一异常格式 + 日志记录 |
| 实现统一响应格式 | P0 | ✅ | 所有 API | ApiResponse<T> 标准结构 |
| 配置 Swagger | P0 | - | - | 接口文档自动生成 |
| SqlSugar 配置初始化 | P0 | ✅ | 所有仓储 | CodeFirst + 连接字符串配置 |
| **基类实体设计** | P0 | ✅ | 所有实体 | AuditableEntity + UserOwnedEntity |
| **通用仓储基类** | P0 | ✅ | 所有仓储 | BaseRepository<T>（CRUD + 软删除 + 用户隔离 + 分页） |
| **全局过滤器配置** | P0 | ✅ | 所有查询 | 软删除 + NtId 用户隔离自动应用 |
| **配置多环境** | P0 | ✅ | 全局 | appsettings.Development.json / appsettings.Production.json |
| **工具类实现** | P0 | ✅ | 全局 | PaginationHelper + JsonHelper + TimeHelper |
| .gitignore 配置 | P0 | - | - | 忽略 db、bin、obj 等 |

#### 交付物

- 可运行的空项目
- Swagger 页面可访问
- **完整的复用基础设施层**（BaseRepository、基类、中间件、工具类）

---

### 阶段二：数据层 & 认证系统（2-3天）

**目标**：完整数据模型 + 用户认证体系

#### 任务清单

| 任务 | 优先级 | 复用内容 | 说明 |
|------|--------|---------|------|
| 实现所有实体模型 | P0 | AuditableEntity/UserOwnedEntity | 继承基类，无需重复写时间戳/用户字段 |
| CodeFirst 自动建表 | P0 | SqlSugar 配置（复用） | 应用启动时初始化表结构 |
| 种子数据初始化 | P0 | - | 默认 Agent、默认配置、测试用户 |
| **JWT 认证中间件** | P0 | ExceptionMiddleware（复用） | Token 签发 + 校验 + 异常处理 |
| **AuthService** | P0 | BaseRepository（复用） | 登录/注册/获取当前用户 |
| 密码哈希加密 | P0 | - | PBKDF2 或 Argon2 |
| **Auth Endpoints** | P0 | ApiResponse（复用） | /api/v1/auth/login /register /me |
| **用户配置仓储** | P1 | BaseRepository（复用） | GlobalConfig + UserConfig 仓储继承基类 |
| **LLM 模型配置仓储** | P1 | BaseRepository（复用） | LLMModelConfig 仓储继承基类 |

#### 交付物

- 完整数据库表结构
- 可注册/登录/获取当前用户
- API 端点受认证保护

---

### 阶段三：AI 核心模块（3-4天）

**目标**：Microsoft Agent Framework 集成 + 核心对话功能

#### 任务清单

| 任务 | 优先级 | 复用内容 | 说明 |
|------|--------|---------|------|
| **LlmGateway 封装** | P0 | ConfigService（复用） | 多模型切换 + 超时重试 + 熔断 |
| **Agent 管理服务** | P0 | BaseRepository（复用） | Agent CRUD + 关键词匹配 |
| **Agent 执行器** | P0 | LlmGateway（复用） | 基于 Microsoft Agent Framework |
| **会话管理服务** | P0 | BaseRepository（复用） | ChatSession + ChatMessage CRUD |
| **会话历史管理** | P0 | - | 上下文维护 + Token 管理 |
| **智能 Agent 匹配逻辑** | P0 | LlmGateway（复用） | 关键词权重匹配 |
| **SSE 流式对话端点** | P0 | ApiResponse（复用） | /api/v1/chat/sessions/{id}/stream |
| **对话 Endpoints** | P0 | BaseRepository（复用） | 会话列表/创建/删除 |
| **AI 工具函数框架** | P1 | - | 工具注册 + 调用封装 |
| **基础工具实现** | P1 | IGitProvider（复用） | Git 相关工具 |

#### 交付物

- 可创建聊天会话
- 可与 Agent 对话（SSE 流式）
- 对话历史持久化

---

### 阶段四：任务中心模块（2-3天）

**目标**：任务创建、执行、监控

#### 任务清单

| 任务 | 优先级 | 复用内容 | 说明 |
|------|--------|---------|------|
| **TaskService** | P0 | BaseRepository（复用） | 任务 CRUD + 状态管理 |
| **任务执行器** | P0 | AgentExecutor（复用） | 异步执行 + 状态更新 |
| **任务日志服务** | P0 | BaseRepository（复用） | TaskLog 记录 |
| **任务队列 BackgroundService** | P0 | TaskSchedulerBase（复用） | 拉取待执行任务 |
| **任务防重复执行** | P0 | - | 状态检查 + 锁 |
| **Task Endpoints** | P0 | ApiResponse（复用） | /api/v1/tasks |
| **结果 JSON 存储** | P1 | JsonHelper（复用） | 执行结果序列化 |

#### 交付物

- 可创建任务
- 任务异步执行
- 任务状态实时更新

---

### 阶段五：定时任务调度（2天）

**目标**：Cron 定时任务管理

#### 任务清单

| 任务 | 优先级 | 复用内容 | 说明 |
|------|--------|---------|------|
| Cron 表达式解析 | P0 | - | Cron 解析库 |
| **调度器 BackgroundService** | P0 | TaskSchedulerBase（复用） | 定时触发检查 |
| **ScheduledTaskService** | P0 | BaseRepository（复用） | 定时任务 CRUD |
| **执行记录服务** | P0 | BaseRepository（复用） | ScheduledTaskExecution 记录 |
| **定时任务 Endpoints** | P0 | ApiResponse（复用） | /api/v1/scheduled-tasks |
| 下一次运行时间计算 | P1 | TimeHelper（复用） | 显示下次触发时间 |

#### 交付物

- 可创建定时任务
- Cron 定时触发
- 执行历史可查

---

### 阶段六：Git 仓库集成（2-3天）

**目标**：多 Git 平台对接

#### 任务清单

| 任务 | 优先级 | 复用内容 | 说明 |
|------|--------|---------|------|
| **IGitProvider 接口** | P0 | - | 统一抽象接口 |
| **GitHubProvider** | P0 | - | GitHub API 对接 |
| **GitLabProvider** | P1 | IGitProvider（复用接口） | GitLab API 对接 |
| **AzureDevOpsProvider** | P1 | IGitProvider（复用接口） | Azure DevOps API 对接 |
| **RepositoryService** | P0 | BaseRepository（复用） | 仓库 CRUD + 同步 |
| **Git 操作封装** | P0 | IGitProvider（复用） | 分支/PR/提交/文件读写 |
| **Repository Endpoints** | P0 | ApiResponse（复用） | /api/v1/repositories |
| 凭据安全存储 | P0 | ConfigService（复用） | Token 加密存储 |

#### 交付物

- 可添加 Git 仓库
- 可查看分支/PR
- 可执行基本 Git 操作

---

### 阶段七：流水线模块（1-2天）

**目标**：CI/CD 流水线集成

#### 任务清单

| 任务 | 优先级 | 复用内容 | 说明 |
|------|--------|---------|------|
| **PipelineService** | P0 | BaseRepository（复用） | 流水线 CRUD |
| **BuildService** | P0 | BaseRepository（复用） | 构建记录 CRUD |
| 流水线状态同步 | P1 | BackgroundService（复用基类） | 定期拉取外部流水线状态 |
| **Pipeline Endpoints** | P0 | ApiResponse（复用） | /api/v1/pipelines |

#### 交付物

- 可管理流水线配置
- 可查看构建历史

---

### 阶段八：Wiki 模块（1天）

**目标**：知识库管理

#### 任务清单

| 任务 | 优先级 | 复用内容 | 说明 |
|------|--------|---------|------|
| **WikiPageService** | P0 | BaseRepository（复用） | Wiki 页面 CRUD |
| **Wiki Endpoints** | P0 | ApiResponse（复用） | /api/v1/wiki |
| 全文检索（可选） | P1 | - | 简单关键词搜索 |

#### 交付物

- Wiki 页面增删改查

---

### 阶段九：系统配置 & 健康检查（1天）

**目标**：配置管理 + 监控

#### 任务清单

| 任务 | 优先级 | 复用内容 | 说明 |
|------|--------|---------|------|
| **ConfigService** | P0 | BaseRepository（复用） | 配置统一读取/更新 |
| 配置变更通知 | P1 | - | 热更新（可选） |
| 健康检查端点 | P0 | - | /health |
| 系统信息端点 | P1 | - | /system/info |
| **日志配置** | P0 | LoggingService（复用） | Serilog 或内置 ILogger |

#### 交付物

- 配置管理 API
- 健康检查可用

---

### 阶段十：测试 & 优化（2-3天）

**目标**：完善测试 + 性能优化

#### 任务清单

| 任务 | 优先级 | 复用内容 | 说明 |
|------|--------|---------|------|
| 单元测试覆盖 | P1 | - | 核心 Service 测试 |
| 集成测试 | P1 | - | API 端点测试 |
| 性能基准测试 | P2 | - | 关键路径性能 |
| 代码 Review | P1 | - | 整体代码质量 |
| 文档完善 | P1 | - | API 文档 + 部署文档 |

#### 交付物

- 测试报告
- 部署文档

---

## 四、复用收益分析

### 4.1 代码复用率预估

| 模块 | 代码量（预估） | 复用次数 | 节省代码量 |
|------|--------------|---------|-----------|
| BaseRepository<T> | 150 行 | 12+ 个仓储 | 150 × 11 = 1,650 行 |
| 基类实体（Auditable/UserOwned） | 50 行 | 16+ 个实体 | 50 × 14 = 700 行 |
| 中间件（异常/JWT） | 100 行 | 全局 | 无需重复写 |
| ApiResponse<T> | 30 行 | 30+ API | 统一格式 |
| LlmGateway | 200 行 | 3+ 模块 | 200 × 2 = 400 行 |
| IGitProvider 接口 | 80 行 | 3 个实现 | 统一接口 |
| 工具类（分页/JSON/时间） | 100 行 | 全局 | 工具复用 |
| **总计** | - | - | **≈ 3,000+ 行** |

### 4.2 开发效率提升

| 阶段 | 无复用预估时间 | 有复用预估时间 | 节省时间 |
|------|--------------|--------------|---------|
| 阶段二（数据层） | 3 天 | 2 天 | 1 天 |
| 阶段三（AI 模块） | 5 天 | 3-4 天 | 1-2 天 |
| 阶段四（任务中心） | 3 天 | 2-3 天 | 0.5-1 天 |
| 阶段五（定时任务） | 3 天 | 2 天 | 1 天 |
| 阶段六（Git 集成） | 4 天 | 2-3 天 | 1-2 天 |
| 阶段七~九 | 4 天 | 2-3 天 | 1-2 天 |
| **总计** | **22 天** | **13-17 天** | **5-9 天** |

---

## 五、里程碑

| 里程碑 | 时间（预估） | 交付内容 | 复用完成度 |
|--------|-------------|---------|-----------|
| M1: 基础设施 | Day 2 | 复用层核心完成 | 100% 基础设施复用层 |
| M2: 数据与认证 | Day 4 | 用户系统可用 | 复用基类 + 仓储 |
| M3: AI 核心 | Day 8 | 对话功能可用 | 复用 LlmGateway + AgentExecutor |
| M4: 任务系统 | Day 11 | 任务执行可用 | 复用 TaskSchedulerBase + AgentExecutor |
| M5: 定时调度 | Day 13 | 定时任务可用 | 复用 TaskSchedulerBase |
| M6: Git 集成 | Day 16 | 仓库管理可用 | 复用 IGitProvider 接口 |
| M7: 完整 MVP | Day 18 | 所有核心功能可用 | 全量复用 |

---

## 六、关键依赖关系

```
阶段一（复用基础设施层）→ 阶段二
         ↓
      阶段三 → 阶段四 → 阶段五
         ↓
      阶段六 → 阶段七 → 阶段八
         ↓
      阶段九 → 阶段十
```

说明：
- **阶段一是所有其他阶段的基础**，必须优先完成复用层
- 阶段三依赖阶段二（需要用户认证）
- 阶段四/五/六/七/八可并行开发（依赖阶段二，并复用阶段一）
- 阶段十依赖所有前面阶段

---

## 七、风险与应对

| 风险 | 概率 | 影响 | 应对措施 |
|------|------|------|---------|
| Microsoft Agent Framework API 变动 | 中 | 高 | 封装一层抽象（AgentExecutor），便于适配变更 |
| SQLite 性能瓶颈 | 低 | 中 | 预留 PostgreSQL 迁移方案 |
| AI 调用成本高 | 中 | 中 | 实现限流 + 缓存 + Mock 模式（LlmGateway 支持） |
| 开发时间超出预期 | 高 | 低 | 复用架构已节省 5-9 天，风险降低 |
| 基类设计缺陷影响所有模块 | 低 | 高 | 阶段一重点 Review 基类设计，写单元测试覆盖 |

---

## 八、后续迭代方向（MVP 后）

1. **数据库升级**：SQLite → PostgreSQL
2. **实时通知**：SignalR 替代轮询
3. **任务队列**：引入 Redis/MessageQueue
4. **多租户**：支持企业级多租户（复用 UserOwnedEntity 扩展）
5. **API 网关**：限流、熔断、日志聚合
6. **监控告警**：Prometheus + Grafana
7. **单元测试**：提高测试覆盖率至 80%+

---

## 九、开发规范

### 代码规范
- 遵循 C# 编码约定
- 使用 XML 注释（函数级）
- async/await 异步编程
- 依赖注入原则
- **优先复用基类和基础设施，不重复造轮子**

### Git 规范
- 分支策略：main → develop → feature/*
- 提交信息：feat: / fix: / docs: / refactor: / test:
- 提交前运行：dotnet build

### API 规范
- RESTful 风格
- 统一响应格式：ApiResponse<T>（强制复用）
- 版本控制：/api/v1/*
- 认证方式：Bearer Token

### 复用规范
- 新实体**必须**继承基类（AuditableEntity/UserOwnedEntity）
- 新仓储**必须**继承 BaseRepository<T>
- 新 API**必须**返回 ApiResponse<T>
- 新异常**必须**使用自定义异常类
- LLM 调用**必须**通过 LlmGateway
- Git 操作**必须**通过 IGitProvider 接口
