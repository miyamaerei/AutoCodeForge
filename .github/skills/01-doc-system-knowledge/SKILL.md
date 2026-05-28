# 01-doc-system-knowledge

## 目标和范围
提供 AutoCodeForge 项目的系统级知识查询能力，包括架构设计、技术栈、模块映射、入口点、目录结构和状态流。所有输出必须基于代码仓库证据，未验证的声明需明确标记为待确认。

## 证据优先规则
- ✅ **可验证事实**：提供文件路径引用（如 `server/src/AutoCodeForge.Api/Program.cs#L10-L50`）
- ⚠️ **推断结论**：标记为 "基于 [文件路径] 推断"
- ❌ **待确认项**：明确标记为 "待确认：缺少直接证据"

## 系统定位

**项目名称**: AutoCodeForge  
**定位**: AI 驱动的研发流水线系统  
**架构风格**: 前后端分离  

**核心能力**:
- AI Agent 编排与任务执行
- 多平台 Git 仓库集成（GitHub/GitLab/Azure DevOps）
- 定时任务调度与流水线管理
- 实时聊天与流式 AI 响应
- 系统配置管理与 Wiki 文档系统

**证据**:
- `README.md#L1-L30` - 项目概览
- `docs/AutoCodeForge-Architecture-v1.0.0-20260521.md` - 架构文档
- `docs/MasterPlan.md#L1-L15` - 项目定位与规划
- `docs/PROJECT_SPEC.md#L1-L10` - 项目规范宪法

## 技术栈和版本

### 前端技术栈

| 技术 | 版本 | 用途 | 证据文件 |
|------|------|------|----------|
| **Vue** | 3.5.32 | UI 框架 | `client/package.json#L28` |
| **Vite** | 8.0.8 | 构建工具 | `client/package.json#L47` |
| **Vue Router** | 5.0.7 | 路由管理 | `client/package.json#L31` |
| **Pinia** | 3.0.4 | 状态管理 | `client/package.json#L26` |
| **Element Plus** | 2.14.0 | UI 组件库 | `client/package.json#L23` |
| **TypeScript** | ~6.0.0 | 类型系统 | `client/package.json#L44` |
| **Axios** | 1.16.1 | HTTP 客户端 | `client/package.json#L21` |
| **Vitest** | 4.1.4 | 单元测试 | `client/package.json#L49` |
| **ECharts** | 6.0.0 | 数据可视化 | `client/package.json#L22` |
| **VeeValidate + Zod** | 4.15.1 + 3.24.0 | 表单验证 | `client/package.json#L27, L32` |
| **Vue-Flow** | 1.48.2 | 工作流设计器 | `client/package.json#L18-L20` |

**Node 版本要求**: `^20.19.0 || >=22.12.0`（证据：`client/package.json#L53`）

### 后端技术栈

| 技术 | 版本 | 用途 | 证据文件 |
|------|------|------|----------|
| **.NET** | 10.0 | 后端框架 | `server/src/AutoCodeForge.Api/AutoCodeForge.Api.csproj#L4` |
| **SqlSugarCore** | 5.1.4.214 | ORM | `server/src/AutoCodeForge.Infrastructure/AutoCodeForge.Infrastructure.csproj#L18` |
| **Swashbuckle** | 10.1.7 | Swagger/OpenAPI | `server/src/AutoCodeForge.Api/AutoCodeForge.Api.csproj#L13` |
| **SQLite** | - | 开发数据库 | `server/src/AutoCodeForge.Api/appsettings.json#L9` |
| **JWT** | HS256 | 认证机制 | `server/src/AutoCodeForge.Api/appsettings.json#L11-L16` |
| **System.IdentityModel.Tokens.Jwt** | 8.15.0 | JWT 令牌处理 | `server/src/AutoCodeForge.Application/AutoCodeForge.Application.csproj#L9` |
| **Cronos** | 0.8.4 | Cron 表达式解析 | `server/src/AutoCodeForge.Infrastructure/AutoCodeForge.Infrastructure.csproj#L12` |
| **LibGit2Sharp** | 0.31.0 | Git 操作库 | `server/src/AutoCodeForge.Infrastructure/AutoCodeForge.Infrastructure.csproj#L13` |
| **Microsoft.Agents.AI** | 1.0.0-preview.251016.1 | AI Agent 框架 | `server/src/AutoCodeForge.Infrastructure/AutoCodeForge.Infrastructure.csproj#L14` |
| **Microsoft.Agents.AI.OpenAI** | 1.0.0-preview.251016.1 | OpenAI 集成 | `server/src/AutoCodeForge.Infrastructure/AutoCodeForge.Infrastructure.csproj#L15` |
| **Azure.AI.OpenAI** | 2.0.0 | Azure OpenAI SDK | `server/src/AutoCodeForge.Infrastructure/AutoCodeForge.Infrastructure.csproj#L16` |
| **Azure.Identity** | 1.17.1 | Azure 身份认证 | `server/src/AutoCodeForge.Infrastructure/AutoCodeForge.Infrastructure.csproj#L17` |

**四层架构**:
- **Api**: HTTP 端点与中间件（`server/src/AutoCodeForge.Api/`）
- **Application**: 业务服务层（`server/src/AutoCodeForge.Application/`）
- **Core**: 实体与接口（`server/src/AutoCodeForge.Core/`）
- **Infrastructure**: 仓储与外部集成（`server/src/AutoCodeForge.Infrastructure/`）

**证据**: `server/AutoCodeForge.sln`（解决方案包含四个项目）

## 模块映射

### 前端模块（14 个）

| 模块名称 | 路由前缀 | 主要功能 | 目录 | 证据 |
|---------|---------|---------|------|------|
| **auth** | `/login` | 用户认证 | `client/src/modules/auth/` | `client/src/router/index.ts#L3` |
| **console** | `/console` | 控制台主页 | `client/src/modules/console/` | `client/src/router/index.ts#L4` |
| **dashboard** | `/dashboard` | 数据仪表板 | `client/src/modules/dashboard/` | `client/src/router/index.ts#L5` |
| **task-center** | `/tasks` | 任务管理 | `client/src/modules/task-center/` | `client/src/router/index.ts#L10` |
| **agent-center** | `/agents` | Agent 管理 | `client/src/modules/agent-center/` | `client/src/router/index.ts#L11` |
| **repo-management** | `/repos` | 仓库管理 | `client/src/modules/repo-management/` | `client/src/router/index.ts#L8` |
| **pipeline-center** | `/pipelines` | 流水线管理 | `client/src/modules/pipeline-center/` | `client/src/router/index.ts#L7` |
| **scheduled-task** | `/scheduled-tasks` | 定时任务 | `client/src/modules/scheduled-task/` | `client/src/router/index.ts#L12` |
| **workflow-center** | `/workflow-center` | 工作流管理（旧版） | `client/src/modules/workflow-center/` | `client/src/router/index.ts#L13` |
| **workflow** | `/workflow` | 工作流管理（新版） | `client/src/modules/workflow/` | `client/src/router/index.ts#L14` |
| **system-config** | `/system-config` | 系统配置 | `client/src/modules/system-config/` | `client/src/router/index.ts#L9` |
| **md-wiki** | `/wiki` | Wiki 文档 | `client/src/modules/md-wiki/` | `client/src/router/index.ts#L6` |
| **notification** | - | 通知管理 | `client/src/modules/notification/` | 基于目录推断 |

**模块结构约定**（证据：`client/src/modules/auth/` 目录结构）:
```
module-name/
  ├── index.ts         # 模块导出
  ├── routes.ts        # 路由定义
  ├── *.api.ts         # API 调用
  ├── *.types.ts       # TypeScript 类型
  ├── store/           # Pinia Store（可选）
  ├── composables/     # 组合式函数（可选）
  └── views/           # 页面组件
```

### 后端端点（24 个）

| 端点类 | 路由前缀 | 主要功能 | 文件 |
|--------|---------|---------|------|
| **AuthEndpoints** | `/api/auth` | 登录/注册/令牌刷新 | `server/src/AutoCodeForge.Api/Endpoints/AuthEndpoints.cs` |
| **AgentEndpoints** | `/api/agents` | Agent CRUD | `server/src/AutoCodeForge.Api/Endpoints/AgentEndpoints.cs` |
| **AgentRegistrationEndpoints** | `/api/agent-registrations` | Agent 注册 | `server/src/AutoCodeForge.Api/Endpoints/AgentRegistrationEndpoints.cs` |
| **AgentSkillEndpoints** | `/api/agent-skills` | Agent 技能管理 | `server/src/AutoCodeForge.Api/Endpoints/AgentSkillEndpoints.cs` |
| **AgentCommunicationEndpoints** | `/api/agent-communications` | Agent 通信 | `server/src/AutoCodeForge.Api/Endpoints/AgentCommunicationEndpoints.cs` |
| **TaskEndpoints** | `/api/tasks` | 任务 CRUD | `server/src/AutoCodeForge.Api/Endpoints/TaskEndpoints.cs` |
| **TaskStepEndpoints** | `/api/task-steps` | 任务步骤管理 | `server/src/AutoCodeForge.Api/Endpoints/TaskStepEndpoints.cs` |
| **TaskOrchestrationEndpoints** | `/api/task-orchestrations` | 任务编排 | `server/src/AutoCodeForge.Api/Endpoints/TaskOrchestrationEndpoints.cs` |
| **ScheduledTaskEndpoints** | `/api/scheduled-tasks` | 定时任务管理 | `server/src/AutoCodeForge.Api/Endpoints/ScheduledTaskEndpoints.cs` |
| **PipelineEndpoints** | `/api/pipelines` | 流水线管理 | `server/src/AutoCodeForge.Api/Endpoints/PipelineEndpoints.cs` |
| **RepositoryEndpoints** | `/api/repositories` | 仓库管理 | `server/src/AutoCodeForge.Api/Endpoints/RepositoryEndpoints.cs` |
| **RepoSyncEndpoints** | `/api/repo-syncs` | 仓库同步 | `server/src/AutoCodeForge.Api/Endpoints/RepoSyncEndpoints.cs` |
| **ChatEndpoints** | `/api/chat` | 聊天对话 | `server/src/AutoCodeForge.Api/Endpoints/ChatEndpoints.cs` |
| **ChatStreamEndpoints** | `/api/chat/stream` | 流式聊天 | `server/src/AutoCodeForge.Api/Endpoints/ChatStreamEndpoints.cs` |
| **WikiEndpoints** | `/api/wiki` | Wiki 页面管理 | `server/src/AutoCodeForge.Api/Endpoints/WikiEndpoints.cs` |
| **ConfigEndpoints** | `/api/config` | 配置管理 | `server/src/AutoCodeForge.Api/Endpoints/ConfigEndpoints.cs` |
| **DashboardEndpoints** | `/api/dashboard` | 仪表板数据 | `server/src/AutoCodeForge.Api/Endpoints/DashboardEndpoints.cs` |
| **NotificationEndpoints** | `/api/notifications` | 通知管理 | `server/src/AutoCodeForge.Api/Endpoints/NotificationEndpoints.cs` |
| **ReviewEndpoints** | `/api/reviews` | 代码审查 | `server/src/AutoCodeForge.Api/Endpoints/ReviewEndpoints.cs` |
| **HumanGateEndpoints** | `/api/human-gates` | 人工审批 | `server/src/AutoCodeForge.Api/Endpoints/HumanGateEndpoints.cs` |
| **FailureRecoveryEndpoints** | `/api/failure-recovery` | 故障恢复 | `server/src/AutoCodeForge.Api/Endpoints/FailureRecoveryEndpoints.cs` |
| **AdminEndpoints** | `/api/admin` | 管理员操作 | `server/src/AutoCodeForge.Api/Endpoints/AdminEndpoints.cs` |
| **HealthEndpoints** | `/api/health` | 健康检查 | `server/src/AutoCodeForge.Api/Endpoints/HealthEndpoints.cs` |
| **SystemEndpoints** | `/api/system` | 系统信息 | `server/src/AutoCodeForge.Api/Endpoints/SystemEndpoints.cs` |
| **WorkflowEndpoints** | `/api/workflows` | 工作流管理 | `server/src/AutoCodeForge.Api/Endpoints/WorkflowEndpoints.cs` |

**端点注册方式**: 通过扩展方法在 `Program.cs` 中调用（证据：`server/src/AutoCodeForge.Api/Program.cs#L105-L129`）

## 入口点和路由

### 前端入口
- **主入口**: `client/src/main.ts`（创建 Vue 应用并挂载）
- **路由文件**: `client/src/router/index.ts`（聚合所有模块路由）
- **开发服务器**: `npm run dev`（Vite 启动）
- **构建输出**: `npm run build`（输出到 `client/dist/`）

**路由守卫**（证据：`client/src/router/index.ts#L62-L78`）:
- 除 `/login` 外所有路由需 JWT 令牌
- 令牌存储在 `localStorage.getItem('auth_token')`
- 未认证用户重定向到 `/login`

### 后端入口
- **主入口**: `server/src/AutoCodeForge.Api/Program.cs`（ASP.NET Core 启动）
- **端口配置**: 默认 5000（检查 `launchSettings.json`）
- **Swagger UI**: 仅开发环境启用（证据：`Program.cs#L85-L89`）
- **根路由**: `GET /` 返回 `"AutoCodeForge API is running"`（证据：`Program.cs#L104`）

**中间件链**（证据：`Program.cs#L82-L96`）:
1. `ExceptionHandlingMiddleware` - 全局异常处理
2. `UseCors("AllowSpecificOrigins")` - CORS 策略
3. `UseSwagger/UseSwaggerUI` - API 文档（仅开发环境）
4. `UseHttpsRedirection` - HTTPS 重定向（生产环境）
5. `JwtAuthMiddleware` - JWT 认证

**服务注册顺序**（证据：`Program.cs#L10-L62`）:
1. CORS 配置
2. JWT 和 Git 选项配置
3. 核心服务、数据保护、SqlSugar、HTTP 客户端
4. 仓储、应用服务、AI 服务、Agent 工具、Git 服务、后台服务
5. Swagger 配置

## 目录结构

```
AutoCodeForge/
├── .autoCodeForge/                   # 自动生成配置与文档
│   ├── config/                       # 配置规则
│   ├── docs/                         # 生成文档
│   ├── history/                      # 历史记录
│   ├── plans/                        # 执行计划
│   ├── registry/                     # 注册中心
│   ├── reports/                      # 报告
│   ├── security-audit/               # 安全审计
│   ├── specs/                        # 规范文档
│   ├── templates/                    # 文档模板
│   └── trash/                        # 回收站
│
├── .github/                          # GitHub 配置
│   ├── agents/                       # Agent 定义
│   ├── instructions/                 # 指令文档
│   ├── skills/                       # 技能定义（本文档所在目录）
│   └── workflows/                    # CI/CD 工作流
│
├── .trae/                            # Trae IDE 技能配置
│   └── skills/                       # Trae 技能定义
│
├── Wiki/                             # 项目 Wiki 文档
│
├── client/                           # 前端应用（Vue 3 + Vite）
│   ├── src/
│   │   ├── api/                      # HTTP 客户端配置
│   │   ├── assets/                   # 静态资源
│   │   ├── components/               # 共享组件
│   │   ├── composables/              # Composition API 可组合函数
│   │   ├── config/                   # 前端配置
│   │   ├── host/                     # 主机配置
│   │   ├── lib/                      # 第三方库封装
│   │   ├── mock/                     # Mock 数据
│   │   ├── modules/                  # 功能模块（模块优先架构）
│   │   ├── router/                   # 路由配置
│   │   ├── stores/                   # Pinia 全局 Store
│   │   ├── types/                    # 全局 TypeScript 类型
│   │   ├── views/                    # 页面视图
│   │   ├── App.vue                   # 根组件
│   │   └── main.ts                   # 应用入口
│   ├── public/                       # 公共静态资源
│   ├── package.json                  # 前端依赖
│   ├── vite.config.ts               # Vite 配置
│   ├── tsconfig.json                # TypeScript 配置
│   └── vitest.config.ts             # Vitest 配置
│
├── server/                           # 后端应用（.NET 10.0）
│   ├── src/
│   │   ├── AutoCodeForge.Api/        # API 层（HTTP 端点、中间件）
│   │   │   ├── Endpoints/            # 24 个端点类
│   │   │   ├── Extensions/           # DI 扩展方法
│   │   │   ├── Middleware/           # 中间件（JWT、异常处理）
│   │   │   ├── Properties/           # 项目属性
│   │   │   ├── appsettings.json      # 配置文件
│   │   │   └── Program.cs            # 应用入口
│   │   ├── AutoCodeForge.Application/ # 应用层（业务服务）
│   │   │   ├── AI/                   # AI 服务（LLM 网关）
│   │   │   ├── Configuration/        # 配置服务
│   │   │   ├── Models/               # 业务模型
│   │   │   ├── Security/             # 安全服务
│   │   │   ├── Services/             # 业务服务层（28+ 服务）
│   │   │   ├── StateMachines/        # 状态机（Agent 状态）
│   │   │   ├── Tools/                # Agent 工具
│   │   │   └── Validators/           # 业务验证器
│   │   ├── AutoCodeForge.Core/       # 核心层（实体、接口）
│   │   │   ├── DTOs/                 # 数据传输对象（12+ 目录）
│   │   │   ├── Entities/             # 数据实体（32 个实体）
│   │   │   ├── Enums/                # 枚举类型
│   │   │   ├── Exceptions/           # 自定义异常
│   │   │   ├── Helpers/              # 辅助工具
│   │   │   ├── Interfaces/           # 核心接口（10+ 接口）
│   │   │   ├── Models/               # 领域模型
│   │   │   ├── Providers/            # 提供者接口
│   │   │   └── ValueObjects/         # 值对象
│   │   └── AutoCodeForge.Infrastructure/ # 基础设施层
│   │       ├── AI/                   # AI 基础设施
│   │       ├── BackgroundServices/   # 后台服务（6+ 服务）
│   │       ├── Data/                 # 数据库初始化
│   │       ├── Git/                  # Git 提供者（4 种）
│   │       ├── Helpers/              # 辅助工具
│   │       ├── Logging/              # 日志服务
│   │       ├── Notification/         # 通知渠道
│   │       ├── Repositories/         # 仓储实现（30+ 仓储类）
│   │       ├── Review/               # 审查引擎
│   │       ├── Sandbox/              # 沙箱管理
│   │       └── Services/             # 基础设施服务
│   ├── tests/
│   │   └── AutoCodeForge.Tests/      # 单元测试项目
│   └── AutoCodeForge.sln             # 解决方案文件
│
└── README.md                         # 项目说明
```

**证据来源**:
- 前端结构：`client/` 目录实际结构
- 后端结构：`server/` 目录实际结构
- 文档结构：`docs/` 和 `.autoCodeForge/` 目录实际结构

## 后端服务列表

### 应用层服务（28 个）

| 服务名称 | 功能描述 | 文件路径 |
|---------|---------|----------|
| **AgentService** | Agent 管理服务 | `server/src/AutoCodeForge.Application/Services/AgentService.cs` |
| **AgentRegistryService** | Agent 注册服务 | `server/src/AutoCodeForge.Application/Services/AgentRegistryService.cs` |
| **AgentIdleMonitorService** | Agent 空闲监控 | `server/src/AutoCodeForge.Application/Services/AgentIdleMonitorService.cs` |
| **AuthService** | 认证服务 | `server/src/AutoCodeForge.Application/Services/AuthService.cs` |
| **JwtService** | JWT 令牌服务 | `server/src/AutoCodeForge.Application/Services/JwtService.cs` |
| **EncryptionService** | 加密服务 | `server/src/AutoCodeForge.Application/Services/EncryptionService.cs` |
| **ChatService** | 聊天服务 | `server/src/AutoCodeForge.Application/Services/ChatService.cs` |
| **ChatDefaultsProvisioningService** | 聊天默认配置 | `server/src/AutoCodeForge.Application/Services/ChatDefaultsProvisioningService.cs` |
| **TaskService** | 任务管理服务 | `server/src/AutoCodeForge.Application/Services/TaskService.cs` |
| **TaskStepService** | 任务步骤服务 | `server/src/AutoCodeForge.Application/Services/TaskStepService.cs` |
| **TaskStepFlowService** | 任务步骤流转 | `server/src/AutoCodeForge.Application/Services/TaskStepFlowService.cs` |
| **TaskOrchestrator** | 任务编排器 | `server/src/AutoCodeForge.Application/Services/TaskOrchestrator.cs` |
| **TaskReviewService** | 任务审查服务 | `server/src/AutoCodeForge.Application/Services/TaskReviewService.cs` |
| **HumanGateService** | 人工审批服务 | `server/src/AutoCodeForge.Application/Services/HumanGateService.cs` |
| **FailureRecoveryService** | 故障恢复服务 | `server/src/AutoCodeForge.Application/Services/FailureRecoveryService.cs` |
| **RepositoryService** | 仓库管理服务 | `server/src/AutoCodeForge.Application/Services/RepositoryService.cs` |
| **RepoSyncService** | 仓库同步服务 | `server/src/AutoCodeForge.Application/Services/RepoSyncService.cs` |
| **RepositoryReviewSettingsService** | 仓库审查设置 | `server/src/AutoCodeForge.Application/Services/RepositoryReviewSettingsService.cs` |
| **ReviewService** | 审查服务 | `server/src/AutoCodeForge.Application/Services/ReviewService.cs` |
| **ReviewRuleSetService** | 审查规则集服务 | `server/src/AutoCodeForge.Application/Services/ReviewRuleSetService.cs` |
| **PipelineService** | 流水线服务 | `server/src/AutoCodeForge.Application/Services/PipelineService.cs` |
| **ScheduledTaskService** | 定时任务服务 | `server/src/AutoCodeForge.Application/Services/ScheduledTaskService.cs` |
| **WorkflowService** | 工作流服务 | `server/src/AutoCodeForge.Application/Services/WorkflowService.cs` |
| **WikiService** | Wiki 服务 | `server/src/AutoCodeForge.Application/Services/WikiService.cs` |
| **ConfigService** | 配置服务 | `server/src/AutoCodeForge.Application/Services/ConfigService.cs` |
| **ConfigInitializationService** | 配置初始化 | `server/src/AutoCodeForge.Application/Services/ConfigInitializationService.cs` |
| **ConfigExportService** | 配置导出 | `server/src/AutoCodeForge.Application/Services/ConfigExportService.cs` |
| **ConfigHistoryService** | 配置历史服务 | `server/src/AutoCodeForge.Application/Services/ConfigHistoryService.cs` |
| **NotificationService** | 通知服务 | `server/src/AutoCodeForge.Application/Services/NotificationService.cs` |
| **LlmConfigService** | LLM 配置服务 | `server/src/AutoCodeForge.Application/Services/LlmConfigService.cs` |
| **GitSkillPolicyService** | Git 技能策略 | `server/src/AutoCodeForge.Application/Services/GitSkillPolicyService.cs` |
| **ContextChainService** | 上下文链服务 | `server/src/AutoCodeForge.Application/Services/ContextChainService.cs` |
| **AdminAuditService** | 管理员审计服务 | `server/src/AutoCodeForge.Application/Services/AdminAuditService.cs` |
| **LeastLoadAgentSelectionStrategy** | 最小负载 Agent 选择策略 | `server/src/AutoCodeForge.Application/Services/LeastLoadAgentSelectionStrategy.cs` |
| **InMemoryTaskEventPublisher** | 内存任务事件发布器 | `server/src/AutoCodeForge.Application/Services/InMemoryTaskEventPublisher.cs` |

### 后台服务（6 个）

| 服务名称 | 功能描述 | 文件路径 |
|---------|---------|----------|
| **CronSchedulerService** | Cron 调度服务 | `server/src/AutoCodeForge.Infrastructure/BackgroundServices/CronSchedulerService.cs` |
| **TaskExecutor** | 任务执行器 | `server/src/AutoCodeForge.Infrastructure/BackgroundServices/TaskExecutor.cs` |
| **TaskQueueService** | 任务队列服务 | `server/src/AutoCodeForge.Infrastructure/BackgroundServices/TaskQueueService.cs` |
| **PipelineSyncService** | 流水线同步服务 | `server/src/AutoCodeForge.Infrastructure/BackgroundServices/PipelineSyncService.cs` |
| **AgentHeartbeatMonitor** | Agent 心跳监控 | `server/src/AutoCodeForge.Infrastructure/BackgroundServices/AgentHeartbeatMonitor.cs` |
| **RepoSyncTaskHandler** | 仓库同步任务处理器 | `server/src/AutoCodeForge.Infrastructure/BackgroundServices/Handlers/RepoSyncTaskHandler.cs` |
| **ReviewTaskHandler** | 审查任务处理器 | `server/src/AutoCodeForge.Infrastructure/BackgroundServices/Handlers/ReviewTaskHandler.cs` |

### Git 提供者（5 个）

| 提供者名称 | 功能描述 | 文件路径 |
|-----------|---------|----------|
| **GitHubProvider** | GitHub 提供者 | `server/src/AutoCodeForge.Infrastructure/Git/GitHubProvider.cs` |
| **GitLabProvider** | GitLab 提供者 | `server/src/AutoCodeForge.Infrastructure/Git/GitLabProvider.cs` |
| **AzureDevOpsProvider** | Azure DevOps 提供者 | `server/src/AutoCodeForge.Infrastructure/Git/AzureDevOpsProvider.cs` |
| **LibGit2SharpProvider** | LibGit2Sharp 提供者 | `server/src/AutoCodeForge.Infrastructure/Git/LibGit2SharpProvider.cs` |
| **GitProviderFactory** | Git 提供者工厂 | `server/src/AutoCodeForge.Infrastructure/Git/GitProviderFactory.cs` |

### AI 基础设施服务

| 服务名称 | 功能描述 | 文件路径 |
|---------|---------|----------|
| **AgentExecutor** | Agent 执行器 | `server/src/AutoCodeForge.Infrastructure/AI/AgentExecutor.cs` |
| **AgentFactory** | Agent 工厂 | `server/src/AutoCodeForge.Infrastructure/AI/AgentFactory.cs` |
| **AgentFrameworkGateway** | Agent 框架网关 | `server/src/AutoCodeForge.Infrastructure/AI/AgentFrameworkGateway.cs` |
| **AgentMatcher** | Agent 匹配器 | `server/src/AutoCodeForge.Infrastructure/AI/AgentMatcher.cs` |
| **ChatSessionManager** | 聊天会话管理器 | `server/src/AutoCodeForge.Infrastructure/AI/ChatSessionManager.cs` |
| **LlmGateway** | LLM 网关 | `server/src/AutoCodeForge.Infrastructure/AI/LlmGateway.cs` |
| **GitHubCopilotCliService** | GitHub Copilot CLI 服务 | `server/src/AutoCodeForge.Infrastructure/AI/GitHubCopilotCliService.cs` |
| **GitContextHydrator** | Git 上下文注入器 | `server/src/AutoCodeForge.Infrastructure/AI/GitContextHydrator.cs` |

### 核心接口（11 个）

| 接口名称 | 功能描述 | 文件路径 |
|---------|---------|----------|
| **IAgentRegistryService** | Agent 注册服务接口 | `server/src/AutoCodeForge.Core/Interfaces/IAgentRegistryService.cs` |
| **IAgentSelectionStrategy** | Agent 选择策略接口 | `server/src/AutoCodeForge.Core/Interfaces/IAgentSelectionStrategy.cs` |
| **IAgentTool** | Agent 工具接口 | `server/src/AutoCodeForge.Core/Interfaces/IAgentTool.cs` |
| **IArtifactStore** | 工件存储接口 | `server/src/AutoCodeForge.Core/Interfaces/IArtifactStore.cs` |
| **IBaseRepository** | 基础仓储接口 | `server/src/AutoCodeForge.Core/Interfaces/IBaseRepository.cs` |
| **ICurrentUser** | 当前用户接口 | `server/src/AutoCodeForge.Core/Interfaces/ICurrentUser.cs` |
| **IGitProvider** | Git 提供者接口 | `server/src/AutoCodeForge.Core/Interfaces/IGitProvider.cs` |
| **ILlmGateway** | LLM 网关接口 | `server/src/AutoCodeForge.Core/Interfaces/ILlmGateway.cs` |
| **INotificationService** | 通知服务接口 | `server/src/AutoCodeForge.Core/Interfaces/INotificationService.cs` |
| **IReviewEngine** | 审查引擎接口 | `server/src/AutoCodeForge.Core/Interfaces/IReviewEngine.cs` |
| **ITaskEventPublisher** | 任务事件发布器接口 | `server/src/AutoCodeForge.Core/Interfaces/ITaskEventPublisher.cs` |

**证据**: `server/src/AutoCodeForge.Application/Services/`、`server/src/AutoCodeForge.Infrastructure/BackgroundServices/`、`server/src/AutoCodeForge.Core/Interfaces/` 目录

## 单元测试文件结构

### 后端测试（C#）

**测试目录**: `server/tests/AutoCodeForge.Tests/`

**测试分类**:

| 分类前缀 | 类型 | 数量 | 说明 |
|---------|------|------|------|
| **Unit_** | 单元测试 | 30+ | 单个类/方法的独立测试 |
| **Intg_** | 集成测试 | 15+ | 多个组件协作测试 |
| **E2E_** | 端到端测试 | 7 | 完整业务流程测试 |
| **Perf_** | 性能测试 | 1 | 性能基准测试 |

**核心测试文件**:

**单元测试（Unit_）**:
| 测试文件 | 测试对象 |
|---------|---------|
| `Unit_AgentStateMachineTests.cs` | Agent 状态机 |
| `Unit_AgentExecutorToolTests.cs` | Agent 执行器工具 |
| `Unit_AgentFrameworkGatewayTests.cs` | Agent 框架网关 |
| `Unit_AgentIdleMonitorServiceTests.cs` | Agent 空闲监控服务 |
| `Unit_AuthServiceTests.cs` | 认证服务 |
| `Unit_JwtServiceTests.cs` | JWT 服务 |
| `Unit_EncryptionServiceTests.cs` | 加密服务 |
| `Unit_ConfigServiceTests.cs` | 配置服务 |
| `Unit_ConfigHistoryServiceTests.cs` | 配置历史服务 |
| `Unit_ConfigExportServiceTests.cs` | 配置导出服务 |
| `Unit_ConfigInitializationServiceTests.cs` | 配置初始化服务 |
| `Unit_TaskServiceTests.cs` | 任务服务 |
| `Unit_TaskStepServiceTests.cs` | 任务步骤服务 |
| `Unit_TaskStepFlowServiceTests.cs` | 任务步骤流转服务 |
| `Unit_TaskReviewServiceTests.cs` | 任务审查服务 |
| `Unit_ScheduledTaskServiceTests.cs` | 定时任务服务 |
| `Unit_CronSchedulerServiceTests.cs` | Cron 调度服务 |
| `Unit_RepositoryServiceTests.cs` | 仓库服务 |
| `Unit_RepoSyncServiceTests.cs` | 仓库同步服务 |
| `Unit_GitProviderTests.cs` | Git 提供者 |
| `Unit_LibGit2SharpProviderTests.cs` | LibGit2Sharp 提供者 |
| `Unit_GitSkillPolicyServiceTests.cs` | Git 技能策略服务 |
| `Unit_GitSkillPermissionGuardTests.cs` | Git 技能权限守卫 |
| `Unit_GitHubCopilotCliServiceTests.cs` | GitHub Copilot CLI 服务 |
| `Unit_LlmGatewayTests.cs` | LLM 网关 |
| `Unit_LlmConfigServiceTests.cs` | LLM 配置服务 |
| `Unit_ReviewServiceTests.cs` | 审查服务 |
| `Unit_RuleBasedReviewEngineTests.cs` | 规则审查引擎 |
| `Unit_WikiServiceTests.cs` | Wiki 服务 |
| `Unit_PasswordHelperTests.cs` | 密码辅助工具 |
| `Unit_SandboxConfigValidatorTests.cs` | 沙箱配置验证器 |
| `Unit_SandboxPathResolverTests.cs` | 沙箱路径解析器 |
| `Unit_BaseRepositoryTests.cs` | 基础仓储 |
| `Unit_AdminAuditServiceTests.cs` | 管理员审计服务 |
| `Unit_GeneralTaskAgentExecutionTests.cs` | 通用任务 Agent 执行 |

**集成测试（Intg_）**:
| 测试文件 | 测试场景 |
|---------|---------|
| `Intg_AgentServiceTests.cs` | Agent 服务集成 |
| `Intg_AgentRegistrationTests.cs` | Agent 注册集成 |
| `Intg_AgentCommunicationTests.cs` | Agent 通信集成 |
| `Intg_AgentStateMachineTests.cs` | Agent 状态机集成 |
| `Intg_AuthEndpointsTests.cs` | 认证端点集成 |
| `Intg_ConfigInitializationTests.cs` | 配置初始化集成 |
| `Intg_HumanGateServiceTests.cs` | 人工审批服务集成 |
| `Intg_HumanGateIntegrationTests.cs` | 人工审批完整集成 |
| `Intg_FailureRecoveryTests.cs` | 故障恢复测试 |
| `Intg_FailureRecoveryIntegrationTests.cs` | 故障恢复集成 |
| `Intg_TaskOrchestrationTests.cs` | 任务编排集成 |
| `Intg_TaskPipelineIntegrationTests.cs` | 任务流水线集成 |
| `Intg_TaskStepFlowServiceTests.cs` | 任务步骤流服务集成 |
| `Intg_TaskWorkflowTests.cs` | 任务工作流测试 |
| `Intg_TaskWorkflowAgentTests.cs` | 任务工作流 Agent 测试 |
| `Intg_MultiAgentFullIntegrationTests.cs` | 多 Agent 完整集成 |
| `Intg_MultiAgentCollaborationFullTests.cs` | 多 Agent 协作完整集成 |
| `Intg_RepoSyncFullTests.cs` | 仓库同步完整测试 |
| `Intg_NotificationTests.cs` | 通知测试 |
| `Intg_GitConfigTests.cs` | Git 配置测试 |

**端到端测试（E2E_）**:
| 测试文件 | 测试场景 |
|---------|---------|
| `E2E_01_SevenStepPipeline.cs` | 7步工序流水线 |
| `E2E_02_HumanGate.cs` | 人工审批门控 |
| `E2E_03_Intervention.cs` | 人工介入 |
| `E2E_04_DormantState.cs` | 休眠状态 |
| `E2E_05_LearningMechanism.cs` | 学习机制 |
| `E2E_06_ContinuousOperation.cs` | 持续运行 |
| `E2E_RealEndToEndTaskTests.cs` | 真实端到端任务 |

**性能测试（Perf_）**:
| 测试文件 | 测试内容 |
|---------|---------|
| `Perf_LlmGatewayTests.cs` | LLM 网关性能 |

**测试基础设施**:
| 文件 | 用途 |
|-----|------|
| `GlobalUsings.cs` | 全局 using 声明 |
| `TestBase.cs` | 测试基类 |
| `IntegrationTestContext.cs` | 集成测试上下文 |
| `TestDataFactory.cs` | 测试数据工厂 |
| `TestWebApplicationFactory.cs` | Web 应用测试工厂 |

**证据**: `server/tests/AutoCodeForge.Tests/` 目录

### 前端测试（TypeScript + Vitest）

**测试目录**: `client/src/**/__tests__/`

**测试分类**:

| 模块 | 测试文件 | 测试对象 |
|-----|---------|---------|
| **auth** | `useAuthStore.spec.ts` | 认证 Store |
| **agent-center** | `useAgentStore.spec.ts` | Agent Store |
| **agent-center** | `useAgent.spec.ts` | Agent Composable |
| **console** | `useChatStore.spec.ts` | 聊天 Store |
| **console** | `useConsoleStore.spec.ts` | 控制台 Store |
| **console** | `useConsoleChat.spec.ts` | 控制台聊天 Composable |
| **task-center** | `useTaskCenterStore.spec.ts` | 任务中心 Store |
| **pipeline-center** | `usePipelineCenterCenterStore.spec.ts` | 流水线中心 Store |
| **repo-management** | `useRepoManagementStore.spec.ts` | 仓库管理 Store |
| **scheduled-task** | `useScheduledTaskStore.spec.ts` | 定时任务 Store |
| **system-config** | `useSystemConfigStore.spec.ts` | 系统配置 Store |
| **dashboard** | `DashboardLargeScreenView.spec.ts` | 仪表板组件 |
| **composables** | `useOnboarding.spec.ts` | 引导 Composable |
| **stores** | `useAppStore.spec.ts` | 应用 Store |
| **stores** | `useRepoStore.spec.ts` | 仓库 Store |
| **mock** | `chat.spec.ts` | 聊天 Mock |
| **mock** | `task.spec.ts` | 任务 Mock |
| **mock** | `log.spec.ts` | 日志 Mock |
| **mock** | `diff.spec.ts` | 差异 Mock |
| **mock** | `project.spec.ts` | 项目 Mock |
| **components** | `HelloWorld.spec.ts` | 示例组件 |

**测试框架**:
- **Vitest** 4.1.4 - 单元测试框架
- **@vue/test-utils** 2.4.6 - Vue 组件测试工具

**证据**: `client/package.json#L49`、`client/src/**/__tests__/` 目录

## 核心实体列表（32 个）

| 实体名称 | 所属目录 | 用途 |
|---------|---------|------|
| **AgentEntity** | Entities/ | Agent 实体 |
| **AgentRegistrationEntity** | Entities/ | Agent 注册记录 |
| **AgentLearningRecordEntity** | Entities/ | Agent 学习记录 |
| **AgentDormantRecordEntity** | Entities/ | Agent 休眠记录 |
| **TaskEntity** | Entities/ | 任务实体 |
| **TaskStepEntity** | Entities/ | 任务步骤 |
| **TaskLogEntity** | Entities/ | 任务日志 |
| **HumanGateEntity** | Entities/ | 人工审批门控 |
| **ChatSessionEntity** | Entities/ | 聊天会话 |
| **ChatMessageEntity** | Entities/ | 聊天消息 |
| **RepositoryEntity** | Entities/ | 仓库实体 |
| **PipelineEntity** | Entities/ | 流水线实体 |
| **BuildEntity** | Entities/ | 构建记录 |
| **ScheduledTaskEntity** | Entities/ | 定时任务 |
| **ScheduledTaskExecutionEntity** | Entities/ | 定时任务执行记录 |
| **ReviewTaskEntity** | Entities/ | 审查任务 |
| **ReviewFindingEntity** | Entities/ | 审查发现 |
| **WorkflowEntity** | Entities/ | 工作流定义 |
| **WorkflowInstanceEntity** | Entities/ | 工作流实例 |
| **WorkflowEventEntity** | Entities/ | 工作流事件 |
| **WikiPageEntity** | Entities/ | Wiki 页面 |
| **NotificationEntity** | Entities/ | 通知记录 |
| **UserEntity** | Entities/ | 用户实体 |
| **GlobalConfigEntity** | Entities/ | 全局配置 |
| **UserConfigEntity** | Entities/ | 用户配置 |
| **ConfigHistoryEntity** | Entities/ | 配置历史 |
| **LLMModelConfigEntity** | Entities/ | LLM 模型配置 |
| **AdminAuditLogEntity** | Entities/ | 管理员审计日志 |
| **AgentToolInvocationEntity** | Entities/ | Agent 工具调用记录 |
| **GitSkillGrantEntity** | Entities/ | Git 技能授权 |
| **RepoSandboxWorkspaceEntity** | Entities/ | 仓库沙箱工作区 |
| **TaskReviewEntity** | Entities/ | 任务审查 |

**证据**: `server/src/AutoCodeForge.Core/Entities/` 目录

## 状态流

### 任务状态机

**状态定义**（基于代码推断）:
```
Pending → Running → Completed
   ↓         ↓
Cancelled  Failed
```

**状态转换规则**（基于 `docs/MasterPlan.md` 推断）:
- `Pending → Running`: 任务执行器拾取任务
- `Running → Completed`: 任务执行成功
- `Running → Failed`: 任务执行失败
- `Pending/Running → Cancelled`: 用户取消任务

### Agent 状态机

**状态定义**（证据：`server/src/AutoCodeForge.Application/StateMachines/AgentStateMachine.cs`）:
```
Idle → Handling → Learning → Dormant
         ↓         ↑
        ←──────────
```

**状态转换规则**:
- `Idle → Handling`: 分配任务
- `Handling → Idle`: 任务完成
- `Idle → Learning`: 空闲超时触发学习
- `Learning → Idle`: 学习完成
- `Idle → Dormant`: 长时间空闲进入休眠
- `Dormant → Idle`: 被唤醒

### 流水线状态机

**状态定义**（推断）:
```
Created → Queued → Running → Completed
             ↓       ↓
          Cancelled  Failed
```

**状态同步**（证据：`server/src/AutoCodeForge.Infrastructure/BackgroundServices/PipelineSyncService.cs`）:
- 后台服务每 30 秒轮询流水线状态

### 定时任务状态

**状态定义**（推断）:
```
Enabled ⇄ Disabled
   ↓
Executing → (回到 Enabled/Disabled)
```

**执行机制**（证据：`server/src/AutoCodeForge.Infrastructure/BackgroundServices/CronSchedulerService.cs`）:
- 使用 Cron 表达式解析
- 后台服务轮询执行

## 输出模板

### 系统架构概述

```markdown
**项目**: AutoCodeForge  
**类型**: AI 研发流水线系统  
**架构**: 前后端分离 + .NET 四层架构  

**前端**: Vue 3.5.32 + Vite 8 + Pinia + Element Plus + Vue-Flow  
**后端**: .NET 10.0 + SqlSugarCore + JWT  
**数据库**: SQLite (开发)  

**核心模块**: Agent 中心、任务中心、仓库管理、流水线管理、定时任务、Wiki 文档  
**外部集成**: GitHub/GitLab/Azure DevOps、LLM API  
```

### 模块影响范围查询

```markdown
**模块**: [模块名称]  

**前端模块**: `client/src/modules/[module-name]/`  
**后端端点**: `server/src/AutoCodeForge.Api/Endpoints/[Module]Endpoints.cs`  
**业务服务**: `server/src/AutoCodeForge.Application/Services/[Module]Service.cs`  
**仓储层**: `server/src/AutoCodeForge.Infrastructure/Repositories/[Module]Repository.cs`  
**实体**: `server/src/AutoCodeForge.Core/Entities/[Module]Entity.cs`  

**依赖模块**: [列出依赖的其他模块]  
**被依赖模块**: [列出依赖此模块的其他模块]  
```

### 技术栈查询

```markdown
**查询**: [技术名称]  
**版本**: [版本号]  
**用途**: [使用场景]  
**配置文件**: [文件路径]  
**相关依赖**: [列出相关依赖]  
```

## 质量检查清单

使用本技能时，必须确保：

- [ ] 所有技术栈版本号都有 `package.json` 或 `.csproj` 文件路径引用
- [ ] 模块映射表包含实际目录路径，且路径存在
- [ ] 端点列表与 `Endpoints/` 目录实际文件一致
- [ ] 状态流定义引用了实际的状态机代码
- [ ] 未验证的推断明确标记为"待确认"或"基于...推断"
- [ ] 目录结构与实际代码仓库一致，不包含虚构路径
- [ ] 所有证据文件路径可在当前仓库中找到

## 示例查询

### 示例 1: 查询前端技术栈

**用户**: "前端使用了哪些技术？"

**输出**:
```
AutoCodeForge 前端使用 Vue 3.5.32 作为核心框架，配合以下技术栈：

- **构建工具**: Vite 8.0.8（快速热更新）
- **路由**: Vue Router 5.0.7（支持懒加载）
- **状态管理**: Pinia 3.0.4（Vue 官方推荐）
- **UI 组件**: Element Plus 2.14.0（企业级组件库）
- **HTTP 客户端**: Axios 1.16.1
- **表单验证**: VeeValidate 4.15.1 + Zod 3.24.0
- **图表**: ECharts 6.0.0 + Vue-ECharts 8.0.1
- **工作流设计**: Vue-Flow 1.48.2
- **测试**: Vitest 4.1.4

证据：`client/package.json#L14-L50`
```

### 示例 2: 查询任务中心模块

**用户**: "任务中心模块的代码在哪里？"

**输出**:
```
任务中心模块分布在前后端：

**前端**:
- 模块目录: `client/src/modules/task-center/`
- 路由注册: `client/src/router/index.ts#L10`
- 路由前缀: `/tasks`

**后端**:
- 端点: `server/src/AutoCodeForge.Api/Endpoints/TaskEndpoints.cs`
- 服务: `server/src/AutoCodeForge.Application/Services/TaskService.cs`
- 仓储: `server/src/AutoCodeForge.Infrastructure/Repositories/TaskRepository.cs`
- 实体: `server/src/AutoCodeForge.Core/Entities/TaskEntity.cs`

**关联模块**:
- `TaskStepEndpoints`: 任务步骤管理
- `TaskOrchestrationEndpoints`: 任务编排
- `HumanGateEndpoints`: 人工审批

证据：前端模块目录实际存在，后端端点文件在 `Endpoints/` 目录中验证。
```

### 示例 3: 查询后端启动流程

**用户**: "后端服务启动流程是什么？"

**输出**:
```
后端服务入口点：`server/src/AutoCodeForge.Api/Program.cs`

**启动流程**（按代码顺序）：

1. **配置加载**（第 10-46 行）:
   - CORS 配置（开发环境允许任意来源）
   - JWT 和 Git 选项配置

2. **服务注册**（第 48-61 行）:
   - 核心服务、数据保护、SqlSugar、HTTP 客户端
   - 仓储、应用服务、AI 服务、Agent 工具、Git 服务、后台服务
   - Swagger 配置（XML 注释文档）

3. **中间件链**（第 82-96 行）:
   - ExceptionHandlingMiddleware（全局异常捕获）
   - CORS 策略应用
   - Swagger UI（仅开发环境）
   - HTTPS 重定向（生产环境）
   - JwtAuthMiddleware（JWT 认证）

4. **数据库初始化**（第 98-102 行）:
   - DatabaseInitializer.InitializeAsync()
   - 开发环境执行种子数据

5. **端点注册**（第 104-129 行）:
   - 根路由: `GET /` 返回健康状态
   - 24 个端点模块通过扩展方法注册

证据：`server/src/AutoCodeForge.Api/Program.cs#L1-L132`
```

## 更新历史

| 日期 | 版本 | 变更说明 |
|------|------|----------|
| 2026-05-28 | 1.1.0 | 基于实际代码仓库验证，补充完整的模块映射、实体列表、目录结构 |
| 2026-05-25 | 1.0.0 | 初始版本，基于 AutoCodeForge 项目实际代码生成 |

---

**维护说明**: 当项目技术栈、架构或模块发生重大变更时，需更新本文档。变更时必须提供新的证据文件路径，并保留变更历史记录。
