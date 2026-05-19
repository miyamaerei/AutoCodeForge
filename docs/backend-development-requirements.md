# AutoCodeForge 后端开发需求文档

**文档版本**: v2.0
**生成日期**: 2026-05-19
**技术框架**: .NET 10 / ASP.NET Core Minimal API / Microsoft Agent Framework / SugarSql
**项目类型**: MVP（最小可行产品）
**适用对象**: 后端开发团队

---

## 1. 项目概述

### 1.1 项目背景

AutoCodeForge 是一个 AI 驱动的代码自动化平台，前端采用 Vue 3 构建，目前已完成基础功能开发并使用 Mock 数据进行演示。项目需要后端服务支撑，以实现真实的数据持久化和 AI Agent 能力。

**MVP 目标**: 用最少的开发工作量，实现核心功能的可运行版本，验证产品价值。

### 1.2 后端技术选型

| 技术组件 | 选型 | 说明 |
|---------|------|------|
| 运行时 | .NET 10 | 最新 LTS 版本 |
| Web 框架 | ASP.NET Core Minimal API | 轻量化 API 开发 |
| ORM | SugarSql | 高性能、轻量级 ORM |
| 数据库 | SQLite | 零配置、文件型数据库，适合 MVP |
| AI 框架 | Microsoft Agent Framework | Semantic Kernel 继承者，AI Agent 编排 |
| 身份认证 | JWT Bearer Token | 简化的用户认证 |
| 定时任务 | .NET Background Services | 轻量级后台任务调度 |
| API 文档 | Swagger / OpenAPI | API 自文档化 |

### 1.3 架构设计原则

```
MVP 优先：
1. 简单架构 - 不引入不必要的复杂度
2. 快速开发 - 选择学习曲线平缓的技术
3. 可迭代 - 架构支持后续扩展
```

### 1.4 系统架构图

```
┌─────────────────────────────────────────────────────────────────┐
│                        Frontend (Vue 3)                         │
│  Task Center / Console / Agent / Repo / Pipeline / Config       │
└─────────────────────────┬───────────────────────────────────────┘
                          │ HTTP/REST
┌─────────────────────────▼───────────────────────────────────────┐
│                    ASP.NET Core Minimal API                      │
│              JWT Authentication / Authorization                  │
├─────────────────────────────────────────────────────────────────┤
│  ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────┐            │
│  │ Task API │ │ Chat API │ │ Agent API│ │  Repo   │            │
│  │ Service  │ │ Service  │ │ Service  │ │ Service │            │
│  └────┬─────┘ └────┬─────┘ └────┬─────┘ └───┬────┘            │
│       │            │             │           │                   │
│  ┌────▼────────────▼─────────────▼───────────▼────────┐        │
│  │           Microsoft Agent Framework                 │        │
│  │   AIAgent / Agent Thread / Tools / Workflows      │        │
│  └──────────────────────┬──────────────────────────────┘        │
│                        │                                        │
│  ┌─────────────────────▼──────────────────────────────┐        │
│  │     LLM Gateway (Azure OpenAI / OpenAI)            │        │
│  └───────────────────────────────────────────────────┘        │
│                                                              │
│  ┌───────────────────────────────────────────────────┐        │
│  │              SugarSql + SQLite                    │        │
│  │      Tasks / Sessions / Agents / Config           │        │
│  └───────────────────────────────────────────────────┘        │
└───────────────────────────────────────────────────────────────┘
```

---

## 2. 技术栈详解

### 2.1 Microsoft Agent Framework

**概述**: Microsoft Agent Framework 是 Semantic Kernel 和 AutoGen 的统一继承者，于 2025年10月发布公开预览版。它结合了 Semantic Kernel 的企业级工程底座和 AutoGen 的创新性多代理编排模式。

**核心组件**:

| 组件 | 说明 |
|------|------|
| `AIAgent` | AI Agent 基础类型，处理用户输入、调用工具、生成响应 |
| `AgentThread` | 线程/会话状态管理，维护对话历史和上下文 |
| `Tools` | 工具函数，Agent 调用外部系统的能力 |
| `Workflows` | 图形化工作流，编排多个 Agent 执行复杂任务 |

**NuGet 包**:

```bash
dotnet add package Microsoft.Agents.AI
dotnet add package Microsoft.Agents.AI.ChatCompletion
dotnet add package Microsoft.Extensions.AI
```

**命名空间**: `Microsoft.Agents.AI`

**官方文档**: https://learn.microsoft.com/en-us/agent-framework/

---

### 2.2 SugarSql ORM

**概述**: SugarSql 是一款轻量级、高性能的 .NET ORM 框架，被称为"创业神器"，支持零 SQL 实体操作。

**核心特性**:

| 特性 | 说明 |
|------|------|
| 零 SQL 实体操作 | CodeFirst 自动建表 |
| 多数据库支持 | MySQL/SQLServer/SQLite/PostgreSQL/Oracle 等 |
| 链式查询 | 流畅的查询语法 |
| 高性能 | 接近原生 ADO.NET |
| 批量操作 | BulkCopy 快速插入 |

**NuGet 包**:

```bash
dotnet add package SqlSugarCore
```

**数据库连接**:

```csharp
// SQLite 连接字符串示例
"Data Source=autocodeforge.db"
```

---

## 3. 数据模型设计

### 3.1 设计原则

1. **每个数据实体必须包含用户字段**: `NtId` - 标识数据归属（NT 域账号）
2. **软删除优先**: 使用 `IsDeleted` 字段，而非物理删除
3. **时间戳**: 所有实体包含 `CreatedAt` 和 `UpdatedAt`
4. **Guid 主键**: 使用 Guid 作为主键类型

### 3.2 实体模型

#### 3.2.1 用户 (User)

```csharp
[SugarTable("Users")]
public class UserEntity
{
    [SugarColumn(IsPrimaryKey = true)]
    public Guid Id { get; set; }

    public string NtId { get; set; }             // NT 域账号标识（唯一用户标识）
    public string UserName { get; set; }         // 用户名
    public string? Email { get; set; }           // 邮箱
    public string? PasswordHash { get; set; }     // 密码哈希
    public bool IsDeleted { get; set; }          // 软删除
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

#### 3.2.2 任务 (Task)

```csharp
[SugarTable("Tasks")]
public class TaskEntity
{
    [SugarColumn(IsPrimaryKey = true)]
    public Guid Id { get; set; }

    public string NtId { get; set; }             // 用户字段 - 任务归属
    public string Title { get; set; }             // 任务标题
    public string Description { get; set; }      // 任务描述（自然语言）
    public Guid? RepositoryId { get; set; }      // 目标仓库
    public string? Branch { get; set; }          // 目标分支
    public string? TargetPath { get; set; }       // 目标路径
    public Guid? AgentId { get; set; }           // 执行 Agent
    public TaskStatus Status { get; set; }       // 状态
    public string? ResultJson { get; set; }       // 执行结果
    public string? ErrorMessage { get; set; }     // 错误信息
    public DateTime? CompletedAt { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public enum TaskStatus
{
    Pending,      // 待执行
    Running,      // 运行中
    Completed,    // 已完成
    Failed,       // 失败
    Paused        // 暂停
}
```

#### 3.2.3 任务日志 (TaskLog)

```csharp
[SugarTable("TaskLogs")]
public class TaskLogEntity
{
    [SugarColumn(IsPrimaryKey = true)]
    public Guid Id { get; set; }

    public string NtId { get; set; }             // 用户字段
    public Guid TaskId { get; set; }             // 关联任务
    public string Level { get; set; }             // Log/Warning/Error
    public string Message { get; set; }           // 日志内容
    public DateTime CreatedAt { get; set; }
}
```

#### 3.2.4 聊天会话 (ChatSession)

```csharp
[SugarTable("ChatSessions")]
public class ChatSessionEntity
{
    [SugarColumn(IsPrimaryKey = true)]
    public Guid Id { get; set; }

    public string NtId { get; set; }             // 用户字段 - 会话归属
    public string Title { get; set; }            // 会话标题
    public Guid? AgentId { get; set; }           // 绑定的 Agent
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

#### 3.2.5 聊天消息 (ChatMessage)

```csharp
[SugarTable("ChatMessages")]
public class ChatMessageEntity
{
    [SugarColumn(IsPrimaryKey = true)]
    public Guid Id { get; set; }

    public string NtId { get; set; }             // 用户字段
    public Guid SessionId { get; set; }          // 关联会话
    public MessageType Type { get; set; }        // User/AI/System
    public string Content { get; set; }          // 消息内容
    public string? MetadataJson { get; set; }     // 额外数据
    public DateTime CreatedAt { get; set; }
}

public enum MessageType
{
    User,
    AI,
    System
}
```

#### 3.2.6 Agent

```csharp
[SugarTable("Agents")]
public class AgentEntity
{
    [SugarColumn(IsPrimaryKey = true)]
    public Guid Id { get; set; }

    public string NtId { get; set; }             // 用户字段 - Agent 归属
    public string Name { get; set; }             // Agent 名称
    public string Description { get; set; }       // Agent 描述
    public string Icon { get; set; }             // 图标
    public string SystemPrompt { get; set; }     // 系统提示词
    public string KeywordsJson { get; set; }     // 关键词 JSON
    public bool Enabled { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

#### 3.2.7 仓库配置 (Repository)

```csharp
[SugarTable("Repositories")]
public class RepositoryEntity
{
    [SugarColumn(IsPrimaryKey = true)]
    public Guid Id { get; set; }

    public string NtId { get; set; }             // 用户字段 - 仓库归属
    public string Name { get; set; }             // 仓库名称
    public string Description { get; set; }      // 描述
    public GitProvider Provider { get; set; }    // GitHub/GitLab/AzureDevOps
    public string Owner { get; set; }             // 组织/用户名
    public string Repository { get; set; }       // 仓库名
    public string DefaultBranch { get; set; }     // 默认分支
    public AuthenticationType AuthType { get; set; }
    public string? CredentialRef { get; set; }   // 凭据引用
    public MergeStrategy MergeStrategy { get; set; }
    public bool EnableCIPolicy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public enum GitProvider { GitHub, AzureDevOps, GitLab }
public enum AuthenticationType { Token, App, OAuth }
public enum MergeStrategy { Merge, Squash, Rebase }
```

#### 3.2.8 定时任务 (ScheduledTask)

```csharp
[SugarTable("ScheduledTasks")]
public class ScheduledTaskEntity
{
    [SugarColumn(IsPrimaryKey = true)]
    public Guid Id { get; set; }

    public string NtId { get; set; }             // 用户字段 - 任务归属
    public string Name { get; set; }
    public string Description { get; set; }
    public Guid? AgentId { get; set; }
    public Guid? RepositoryId { get; set; }
    public string? Branch { get; set; }
    public string? TargetPath { get; set; }
    public TriggerType TriggerType { get; set; }
    public string? CronExpression { get; set; }
    public long? IntervalMs { get; set; }
    public DateTime? OnceTime { get; set; }
    public string? ParamsJson { get; set; }
    public ScheduleStatus Status { get; set; }
    public DateTime? NextRunTime { get; set; }
    public DateTime? LastRunTime { get; set; }
    public int TotalRuns { get; set; }
    public int SuccessRuns { get; set; }
    public int FailedRuns { get; set; }
    public bool Enabled { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public enum TriggerType { Cron, Interval, Once }
public enum ScheduleStatus { Idle, Running, Failed, Disabled }
```

#### 3.2.9 定时任务执行记录 (ScheduledTaskExecution)

```csharp
[SugarTable("ScheduledTaskExecutions")]
public class ScheduledTaskExecutionEntity
{
    [SugarColumn(IsPrimaryKey = true)]
    public Guid Id { get; set; }

    public string NtId { get; set; }             // 用户字段
    public Guid ScheduledTaskId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public ExecutionStatus Status { get; set; }
    public string? ResultJson { get; set; }
    public string? ErrorMessage { get; set; }
}

public enum ExecutionStatus { Success, Failed, Running }
```

#### 3.2.10 流水线 (Pipeline)

```csharp
[SugarTable("Pipelines")]
public class PipelineEntity
{
    [SugarColumn(IsPrimaryKey = true)]
    public Guid Id { get; set; }

    public string NtId { get; set; }             // 用户字段
    public string Name { get; set; }
    public Guid RepositoryId { get; set; }
    public string? ExternalPipelineId { get; set; }
    public PipelineStatus Status { get; set; }
    public DateTime? LastBuildTime { get; set; }
    public string? WebUrl { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public enum PipelineStatus { Success, Failed, Running, Pending, Cancelled }
```

#### 3.2.11 构建 (Build)

```csharp
[SugarTable("Builds")]
public class BuildEntity
{
    [SugarColumn(IsPrimaryKey = true)]
    public Guid Id { get; set; }

    public string NtId { get; set; }             // 用户字段
    public Guid PipelineId { get; set; }
    public string? ExternalBuildId { get; set; }
    public string Branch { get; set; }
    public string? CommitSha { get; set; }
    public BuildStatus Status { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string? WebUrl { get; set; }
    public bool IsDeleted { get; set; }
}
```

#### 3.2.12 全局系统配置 (GlobalConfig)

```csharp
[SugarTable("GlobalConfigs")]
public class GlobalConfigEntity
{
    [SugarColumn(IsPrimaryKey = true)]
    public Guid Id { get; set; }

    public string Category { get; set; }          // 配置类别: system, security, ai, integrations
    public string ConfigKey { get; set; }        // 配置键
    public string ConfigValue { get; set; }      // 配置值 (JSON)
    public string? Description { get; set; }     // 配置描述
    public bool IsActive { get; set; }           // 是否启用
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

#### 3.2.13 用户私有配置 (UserConfig)

```csharp
[SugarTable("UserConfigs")]
public class UserConfigEntity
{
    [SugarColumn(IsPrimaryKey = true)]
    public Guid Id { get; set; }

    public string NtId { get; set; }             // 用户字段 - 配置归属
    public string Category { get; set; }          // 配置类别: preferences, notifications, workspace
    public string ConfigKey { get; set; }        // 配置键
    public string ConfigValue { get; set; }      // 配置值 (JSON)
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

#### 3.2.14 LLM 模型配置 (LLMModelConfig)

```csharp
[SugarTable("LLMModelConfigs")]
public class LLMModelConfigEntity
{
    [SugarColumn(IsPrimaryKey = true)]
    public Guid Id { get; set; }

    public string NtId { get; set; }             // 用户字段（为空表示全局配置）
    public string ModelName { get; set; }        // 模型名称（如 gpt-4o, gpt-4-turbo）
    public string Provider { get; set; }         // 提供商: OpenAI, AzureOpenAI
    public string Endpoint { get; set; }         // API 端点
    public string ApiKey { get; set; }           // API Key（加密存储）
    public int ContextWindow { get; set; }       // 上下文窗口大小（tokens）
    public double Temperature { get; set; }      // 温度参数 (0-2)
    public int MaxTokens { get; set; }           // 最大生成长度
    public double TopP { get; set; }             // Top P 参数
    public double FrequencyPenalty { get; set; } // 频率惩罚
    public double PresencePenalty { get; set; }  // 存在惩罚
    public int Weight { get; set; }              // 模型权重（用于多模型切换时的优先级）
    public int TimeoutSeconds { get; set; }      // 请求超时时间（秒）
    public int MaxRetries { get; set; }          // 最大重试次数
    public bool IsActive { get; set; }           // 是否启用
    public bool IsDefault { get; set; }          // 是否为默认模型
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public enum LLMProvider { OpenAI, AzureOpenAI }
```

#### 3.2.15 AI 会话配置 (AISessionConfig)

```csharp
[SugarTable("AISessionConfigs")]
public class AISessionConfigEntity
{
    [SugarColumn(IsPrimaryKey = true)]
    public Guid Id { get; set; }

    public string NtId { get; set; }             // 用户字段
    public int MaxMessagesPerSession { get; set; }    // 单会话最大消息数
    public int MaxContextTokens { get; set; }         // 最大上下文 Token 数
    public int SessionTimeoutHours { get; set; }      // 会话超时时间（小时）
    public bool AutoCleanupEnabled { get; set; }      // 是否启用自动清理
    public int AutoCleanupDays { get; set; }          // 自动清理天数
    public bool MessageCompression { get; set; }      // 是否启用消息压缩
    public int ToolCallTimeoutSeconds { get; set; }   // 工具调用超时时间
    public int ToolCallMaxRetries { get; set; }       // 工具调用最大重试次数
    public int CircuitBreakerThreshold { get; set; }  // 熔断阈值（连续失败次数）
    public int CircuitBreakerResetMinutes { get; set; } // 熔断重置时间（分钟）
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

#### 3.2.16 Wiki 页面 (WikiPage)

```csharp
[SugarTable("WikiPages")]
public class WikiPageEntity
{
    [SugarColumn(IsPrimaryKey = true)]
    public Guid Id { get; set; }

    public string NtId { get; set; }             // 用户字段 - 页面归属
    public Guid? RepositoryId { get; set; }
    public string Title { get; set; }
    public string Purpose { get; set; }
    public string? Notes { get; set; }
    public string? MetaJson { get; set; }
    public Guid AuthorId { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

---

## 4. API 规范

### 4.1 基础规范

- **协议**: HTTP/1.1
- **API 风格**: RESTful
- **数据格式**: JSON
- **认证方式**: JWT Bearer Token
- **分页**: Offset-based Pagination (`page` + `pageSize`)
- **错误码**: HTTP Status Code + RFC 7807 Problem Details

### 4.2 统一成功响应结构体

所有 API 成功响应统一使用如下格式：

```csharp
public class ApiResponse<T>
{
    public int Code { get; set; }           // 业务状态码，0 表示成功
    public string Message { get; set; }      // 响应消息
    public T Data { get; set; }             // 响应数据
    public string RequestId { get; set; }    // 请求唯一标识（用于日志追踪）
    public DateTime Timestamp { get; set; }  // 响应时间戳
}
```

**字段说明**:

| 字段 | 类型 | 说明 |
|------|------|------|
| `Code` | int | 业务状态码，`0` 表示成功，非零表示业务异常 |
| `Message` | string | 响应消息，成功时为 "Success"，失败时为错误描述 |
| `Data` | T | 泛型数据，实际响应内容 |
| `RequestId` | string | GUID，用于请求追踪和日志关联 |
| `Timestamp` | DateTime | 服务器响应时间（UTC） |

**成功响应示例**:

```json
{
    "code": 0,
    "message": "Success",
    "data": {
        "id": "550e8400-e29b-41d4-a716-446655440000",
        "name": "代码审查助手",
        "description": "专业的代码审查 AI 助手"
    },
    "requestId": "req-abc123",
    "timestamp": "2026-05-19T10:30:00Z"
}
```

### 4.3 全局异常返回体

所有 API 异常响应统一使用如下格式：

```csharp
public class ApiErrorResponse
{
    public int Code { get; set; }           // 错误码
    public string Message { get; set; }      // 错误消息
    public string Detail { get; set; }       // 详细错误信息（开发环境）
    public string RequestId { get; set; }    // 请求标识
    public string Path { get; set; }         // 请求路径
    public DateTime Timestamp { get; set; }  // 错误发生时间
    public IDictionary<string, string[]> Errors { get; set; }  // 字段级错误
}
```

**HTTP 状态码映射**:

| 状态码 | 说明 | 场景 |
|--------|------|------|
| 400 | Bad Request | 请求参数校验失败 |
| 401 | Unauthorized | 未认证或 Token 失效 |
| 403 | Forbidden | 无权限访问 |
| 404 | Not Found | 资源不存在 |
| 409 | Conflict | 资源冲突（如重复创建） |
| 500 | Internal Server Error | 服务器内部错误 |

**错误响应示例**:

```json
{
    "code": 401,
    "message": "Token expired",
    "detail": "JWT token has expired, please refresh or re-login",
    "requestId": "req-abc123",
    "path": "/api/v1/auth/me",
    "timestamp": "2026-05-19T10:30:00Z",
    "errors": null
}
```

**字段级校验错误示例**:

```json
{
    "code": 400,
    "message": "Validation failed",
    "detail": "Please check the validation errors below",
    "requestId": "req-abc123",
    "path": "/api/v1/auth/register",
    "timestamp": "2026-05-19T10:30:00Z",
    "errors": {
        "Email": ["The Email field is not a valid email address."],
        "Password": ["The Password must be at least 6 characters."]
    }
}
```

### 4.4 认证流程

```
1. 用户登录/注册 → 获取 JWT Token + Refresh Token
2. 后续请求 Header 携带: Authorization: Bearer <token>
3. 后端验证 Token，提取 NtId
4. 所有数据操作基于 NtId 过滤
5. Token 过期时使用 Refresh Token 获取新 Token
```

### 4.5 API 端点

#### 4.5.1 认证

| 方法 | 路径 | 说明 |
|------|------|------|
| POST | `/api/v1/auth/register` | 用户注册 |
| POST | `/api/v1/auth/login` | 用户登录 |
| POST | `/api/v1/auth/refresh` | 刷新 Token |
| GET | `/api/v1/auth/me` | 获取当前用户信息 |

#### 4.5.2 任务中心

| 方法 | 路径 | 说明 |
|------|------|------|
| GET | `/api/v1/tasks` | 获取任务列表 |
| POST | `/api/v1/tasks` | 创建任务 |
| GET | `/api/v1/tasks/{id}` | 获取任务详情 |
| DELETE | `/api/v1/tasks/{id}` | 删除任务 |
| POST | `/api/v1/tasks/{id}/execute` | 触发执行 |
| GET | `/api/v1/tasks/{id}/logs` | 获取日志 |

#### 4.5.3 聊天

| 方法 | 路径 | 说明 |
|------|------|------|
| POST | `/api/v1/chat/ask` | 单次提问 |
| GET | `/api/v1/chat/sessions` | 获取会话列表 |
| POST | `/api/v1/chat/sessions` | 创建会话 |
| DELETE | `/api/v1/chat/sessions/{id}` | 删除会话 |
| GET | `/api/v1/chat/sessions/{id}/messages` | 获取消息 |
| POST | `/api/v1/chat/sessions/{id}/messages` | 发送消息 |
| POST | `/api/v1/chat/sessions/{id}/stream` | 流式消息（SSE） |

#### 4.5.4 Agent

| 方法 | 路径 | 说明 |
|------|------|------|
| GET | `/api/v1/agents` | 获取 Agent 列表 |
| POST | `/api/v1/agents` | 创建 Agent |
| GET | `/api/v1/agents/{id}` | 获取详情 |
| PUT | `/api/v1/agents/{id}` | 更新 |
| DELETE | `/api/v1/agents/{id}` | 删除 |
| POST | `/api/v1/agents/auto-select` | 自动选择 |

#### 4.5.5 仓库

| 方法 | 路径 | 说明 |
|------|------|------|
| GET | `/api/v1/repositories` | 获取仓库列表 |
| POST | `/api/v1/repositories` | 添加仓库 |
| GET | `/api/v1/repositories/{id}` | 获取详情 |
| PUT | `/api/v1/repositories/{id}` | 更新 |
| DELETE | `/api/v1/repositories/{id}` | 删除 |
| GET | `/api/v1/repositories/{id}/branches` | 获取分支 |
| GET | `/api/v1/repositories/{id}/pull-requests` | 获取 PR |

#### 4.5.6 定时任务

| 方法 | 路径 | 说明 |
|------|------|------|
| GET | `/api/v1/scheduled-tasks` | 获取列表 |
| POST | `/api/v1/scheduled-tasks` | 创建 |
| GET | `/api/v1/scheduled-tasks/{id}` | 获取详情 |
| PUT | `/api/v1/scheduled-tasks/{id}` | 更新 |
| DELETE | `/api/v1/scheduled-tasks/{id}` | 删除 |
| POST | `/api/v1/scheduled-tasks/{id}/run` | 立即执行 |
| GET | `/api/v1/scheduled-tasks/{id}/executions` | 执行记录 |

#### 4.5.7 流水线

| 方法 | 路径 | 说明 |
|------|------|------|
| GET | `/api/v1/pipelines` | 获取流水线 |
| GET | `/api/v1/pipelines/{id}/builds` | 获取构建历史 |

#### 4.5.8 全局配置

| 方法 | 路径 | 说明 |
|------|------|------|
| GET | `/api/v1/config/global` | 获取全局配置列表 |
| GET | `/api/v1/config/global/{category}` | 获取指定类别配置 |
| POST | `/api/v1/config/global` | 创建全局配置 |
| PUT | `/api/v1/config/global/{id}` | 更新全局配置 |
| DELETE | `/api/v1/config/global/{id}` | 删除全局配置 |

#### 4.5.9 用户配置

| 方法 | 路径 | 说明 |
|------|------|------|
| GET | `/api/v1/config/user` | 获取当前用户配置 |
| GET | `/api/v1/config/user/{category}` | 获取指定类别用户配置 |
| PUT | `/api/v1/config/user` | 更新用户配置 |

#### 4.5.10 LLM 模型配置

| 方法 | 路径 | 说明 |
|------|------|------|
| GET | `/api/v1/config/models` | 获取模型配置列表 |
| POST | `/api/v1/config/models` | 创建模型配置 |
| GET | `/api/v1/config/models/{id}` | 获取模型配置详情 |
| PUT | `/api/v1/config/models/{id}` | 更新模型配置 |
| DELETE | `/api/v1/config/models/{id}` | 删除模型配置 |
| POST | `/api/v1/config/models/{id}/set-default` | 设置为默认模型 |

#### 4.5.11 AI 会话配置

| 方法 | 路径 | 说明 |
|------|------|------|
| GET | `/api/v1/config/session` | 获取会话配置 |
| PUT | `/api/v1/config/session` | 更新会话配置 |

#### 4.5.12 Wiki

| 方法 | 路径 | 说明 |
|------|------|------|
| GET | `/api/v1/wiki/pages` | 获取页面列表 |
| GET | `/api/v1/wiki/pages/{id}` | 获取页面详情 |

#### 4.5.13 Dashboard

| 方法 | 路径 | 说明 |
|------|------|------|
| GET | `/api/v1/dashboard/stats` | 获取统计数据 |

---

## 5. 异步任务与 AI 流式交互规范

### 5.1 异步任务处理

#### 5.1.1 任务状态流转

```
Pending → Running → Completed/Failed/Paused
     ↓(取消)
  Cancelled
```

#### 5.1.2 异步任务响应模式

**创建任务时返回任务 ID，客户端轮询状态**:

```json
// POST /api/v1/tasks 响应
{
    "code": 0,
    "message": "Success",
    "data": {
        "taskId": "550e8400-e29b-41d4-a716-446655440000",
        "status": "Pending"
    },
    "requestId": "req-abc123",
    "timestamp": "2026-05-19T10:30:00Z"
}
```

**轮询任务状态**:

```json
// GET /api/v1/tasks/{id} 响应
{
    "code": 0,
    "message": "Success",
    "data": {
        "id": "550e8400-e29b-41d4-a716-446655440000",
        "status": "Running",
        "progress": 65,
        "resultJson": null,
        "errorMessage": null
    },
    "requestId": "req-abc123",
    "timestamp": "2026-05-19T10:30:05Z"
}
```

### 5.2 AI 流式交互规范

#### 5.2.1 Server-Sent Events (SSE) 协议

**端点**: `POST /api/v1/chat/sessions/{id}/stream`

**请求头**:
- `Content-Type: application/json`
- `Authorization: Bearer <token>`
- `Accept: text/event-stream`

**请求体**:
```json
{
    "content": "帮我分析这段代码的潜在问题",
    "agentId": "550e8400-e29b-41d4-a716-446655440001"
}
```

**流式响应格式**:

```
event: message
data: {"type":"start","messageId":"msg-123"}

event: message
data: {"type":"chunk","content":"你"}

event: message
data: {"type":"chunk","content":"好"}

event: message
data: {"type":"chunk","content":"，我来分析"}

event: message
data: {"type":"end","messageId":"msg-123","finishReason":"completed"}
```

**事件类型说明**:

| 类型 | 说明 | 数据字段 |
|------|------|----------|
| `start` | 会话开始 | `messageId` |
| `chunk` | 内容片段 | `content` |
| `tool` | 工具调用 | `toolName`, `arguments` |
| `end` | 会话结束 | `messageId`, `finishReason` |
| `error` | 错误 | `errorCode`, `errorMessage` |

#### 5.2.2 LLM 调用管控

**限流策略**:

| 级别 | 限制 | 说明 |
|------|------|------|
| 用户级 | 10 次/分钟 | 单用户调用频率 |
| 会话级 | 5 并发 | 单会话最大并发请求 |
| 全局级 | 100 次/分钟 | 服务总调用量 |

**超时配置**:
- **流式响应超时**: 300 秒
- **单次 LLM 调用超时**: 60 秒
- **工具调用超时**: 30 秒

**错误重试**:
- **最大重试次数**: 3
- **重试间隔**: 1s, 2s, 4s (指数退避)
- **重试条件**: 网络错误、服务端 5xx 错误

**熔断机制**:
- **熔断阈值**: 连续失败 5 次触发熔断
- **熔断时长**: 30 分钟自动恢复
- **熔断期间**: 返回预定义降级响应，记录熔断日志

#### 5.2.3 会话管理规范

**会话生命周期**:
- **默认超时**: 24 小时无活动自动关闭
- **最大消息数**: 100 条/会话
- **最大历史长度**: 4096 tokens
- **会话过期**: 超过 30 天未访问自动标记删除

**消息清理规则**:
- **自动清理**: 每日凌晨 2:00 执行清理任务
- **清理条件**: 已删除会话或超过 30 天未活动的会话
- **消息压缩**: 超过 1000 字符的历史消息自动压缩存储
- **清理策略**: 软删除 → 7 天后物理删除

**会话上下文管理**:
```csharp
public class ChatSessionContext
{
    public Guid SessionId { get; set; }
    public string NtId { get; set; }
    public List<ChatMessage> History { get; set; }
    public DateTime LastActivityTime { get; set; }
    public int TokenUsage { get; set; }
    public bool IsActive { get; set; }
    public int MessageCount { get; set; }
}
```

**消息持久化**:
- 每条消息在发送/接收后立即持久化
- 支持消息编辑和删除（软删除）
- 会话删除时级联删除消息
- 历史消息支持压缩存储以节省空间

---

## 6. Microsoft Agent Framework 集成

### 5.1 Agent 定义示例

```csharp
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.ChatCompletion;
using Microsoft.Extensions.AI;

// 1. 创建 Chat Client
var chatClient = new ChatClient("gpt-4o", apiKey);

// 2. 创建 Agent
var agent = chatClient.AsAIAgent(instructions: """
    你是一个专业的代码审查助手，负责分析代码质量、发现潜在问题并提供改进建议。
    请用中文回复。
    """);

// 3. 创建 Thread (会话上下文)
var thread = agent.CreateThread();
thread.AddMessage(new ChatMessage(ChatRole.User, "帮我审查这段代码..."));

// 4. 执行
var response = await agent.InvokeAsync(thread);
Console.WriteLine(response.Message.Content);
```

### 5.2 工具定义

```csharp
// 定义工具函数
public class GitTools
{
    public async Task<string> GetFileContentAsync(string repoUrl, string path, string branch)
    {
        // Git Provider API 调用
    }

    public async Task<string> CreateCommitAsync(string repoUrl, string branch, string path, string content, string message)
    {
        // 创建提交
    }
}

// 注册工具
agent.AddTool(GitTools.GetFileContentAsync);
agent.AddTool(GitTools.CreateCommitAsync);
```

### 5.3 Agent 自动选择

```csharp
public async Task<Guid?> AutoSelectAgentAsync(string userMessage)
{
    // 1. 分词
    var words = userMessage.Split(' ', StringSplitOptions.RemoveEmptyEntries);

    // 2. 查询所有启用的 Agent
    var agents = await _agentRepository.GetAllEnabledAsync();

    // 3. 计算匹配度
    double maxScore = 0;
    Guid? selectedId = null;

    foreach (var agent in agents)
    {
        var keywords = JsonSerializer.Deserialize<List<AgentKeyword>>(agent.KeywordsJson);
        double score = keywords?
            .Where(k => words.Any(w => w.Contains(k.Keyword, StringComparison.OrdinalIgnoreCase)))
            .Sum(k => k.Weight) ?? 0;

        if (score > maxScore && score >= 0.5)
        {
            maxScore = score;
            selectedId = agent.Id;
        }
    }

    return selectedId;
}
```

---

## 6. 项目结构

```
AutoCodeForge/
├── server/
│   └── src/
│       ├── Program.cs                    # 入口
│       ├── appsettings.json              # 配置
│       ├── Configuration/                # 配置类
│       │   └── AppSettings.cs
│       ├── Entities/                      # 实体
│       │   ├── UserEntity.cs
│       │   ├── TaskEntity.cs
│       │   ├── ChatSessionEntity.cs
│       │   ├── ChatMessageEntity.cs
│       │   ├── AgentEntity.cs
│       │   ├── RepositoryEntity.cs
│       │   ├── ScheduledTaskEntity.cs
│       │   └── ...
│       ├── Repositories/                  # 数据访问
│       │   ├── UserRepository.cs
│       │   ├── TaskRepository.cs
│       │   └── ...
│       ├── Services/                     # 业务逻辑
│       │   ├── AuthService.cs
│       │   ├── TaskService.cs
│       │   ├── ChatService.cs
│       │   ├── AgentService.cs
│       │   └── ...
│       ├── AI/                           # AI 相关
│       │   ├── AgentExecutor.cs
│       │   └── GitTools.cs
│       ├── Api/                           # API 端点
│       │   ├── AuthApi.cs
│       │   ├── TaskApi.cs
│       │   ├── ChatApi.cs
│       │   └── ...
│       └── Middleware/                    # 中间件
│           └── AuthMiddleware.cs
├── autocodeforge.db                      # SQLite 数据库
└── package.json
```

---

## 7. 开发优先级

### Phase 1 - MVP 核心 (2-3 周)

| 优先级 | 功能 | 工作量 | 说明 |
|-------|------|-------|------|
| P0 | 项目基础架构 | 0.5 天 | .NET 10 项目、SugarSql 配置 |
| P0 | 全局异常处理 | 0.5 天 | 统一异常捕获、错误响应格式 |
| P0 | 用户认证 | 1.5 天 | 注册、登录、JWT、Refresh Token |
| P0 | JWT 刷新机制 | 0.5 天 | Token 过期自动刷新、双 Token 机制 |
| P0 | 基础日志框架 | 0.5 天 | 结构化日志、请求追踪、异常记录 |
| P0 | Agent 管理 | 1.5 天 | CRUD、自动选择 |
| P0 | 任务中心 | 2 天 | CRUD、状态管理、异步执行 |
| P0 | AI 流式对话 | 2 天 | SSE 协议、实时消息推送 |
| P0 | LLM 调用管控 | 1 天 | 限流、超时、重试策略 |
| P0 | 会话管理 | 1 天 | 会话上下文、消息持久化、生命周期 |
| P1 | 仓库管理 | 1.5 天 | CRUD、分支、PR |

**MVP 交付物**: 可运行的最小系统，支持用户登录、Agent 管理、任务创建、AI 流式聊天、会话管理

### Phase 2 - 完善功能 (2-3 周)

| 优先级 | 功能 | 工作量 | 说明 |
|-------|------|-------|------|
| P0 | AI Agent 执行 | 3 天 | Microsoft Agent Framework 集成 |
| P1 | 定时任务 | 2 天 | Background Services |
| P1 | Git Provider 集成 | 2 天 | GitHub/GitLab API |
| P2 | 流水线 | 1 天 | CI/CD 状态 |
| P2 | Dashboard | 0.5 天 | 统计 |

### Phase 3 - 配置与扩展 (1-2 周)

| 优先级 | 功能 | 工作量 | 说明 |
|-------|------|-------|------|
| P1 | 系统配置 | 1 天 | 知识库、模型配置 |
| P2 | Wiki | 1 天 | 页面管理 |
| P2 | 集成配置 | 1 天 | Azure DevOps、Copilot |

---

## 8. 技术栈版本

| 组件 | 版本 | NuGet |
|------|------|-------|
| .NET SDK | 10.0 | - |
| SugarSql | 5.1+ | `SqlSugarCore` |
| Microsoft Agent Framework | 1.0+ | `Microsoft.Agents.AI` |
| Microsoft.Extensions.AI | 1.0+ | `Microsoft.Extensions.AI` |
| JWT | - | `Microsoft.AspNetCore.Authentication.JwtBearer` |
| Swagger | - | `Swashbuckle.AspNetCore` |

---

## 9. 数据库初始化

```csharp
// Program.cs
builder.Services.AddSqlSugarSetup(new ConnectionConfig()
{
    ConnectionString = "Data Source=autocodeforge.db",
    DbType = DbType.Sqlite,
    IsAutoCloseConnection = true,
    InitKeyType = InitKeyType.Attribute
});

// CodeFirst - 自动创建表
var db = builder.Services.BuildServiceProvider().GetService<ISqlSugarClient>();
db.CodeFirst.InitTables(typeof(UserEntity));
db.CodeFirst.InitTables(typeof(TaskEntity));
db.CodeFirst.InitTables(typeof(AgentEntity));
// ... 其他实体
```

---

## 10. 附录

### 10.1 参考资料

- [Microsoft Agent Framework 官方文档](https://learn.microsoft.com/en-us/agent-framework/)
- [Semantic Kernel 迁移指南](https://learn.microsoft.com/en-us/agent-framework/migration-guide/from-semantic-kernel/)
- [SugarSql GitHub](https://github.com/donet5/SqlSugar)
- [SugarSql 文档](https://www.donet5.com/Home/Doc)

### 10.2 SQLite 适用场景

| 优点 | 缺点 |
|------|------|
| 零配置 | 不适合高并发写 |
| 单文件存储 | 不支持多用户并发写 |
| 部署简单 | 不适合大规模数据 |
| MVP 首选 | 未来可迁移到 PostgreSQL |

### 10.3 MVP 后续扩展方向

1. **数据库**: SQLite → PostgreSQL (多租户支持)
2. **缓存**: 按需引入 Redis
3. **消息队列**: 按需引入 RabbitMQ
4. **部署**: 单机 → Docker/Kubernetes 集群

---

**文档维护**: 后端开发团队
**审核状态**: 待审核
