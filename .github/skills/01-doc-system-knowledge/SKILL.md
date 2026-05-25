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
| **Vue** | 3.5.32 | UI 框架 | `client/package.json#L17` |
| **Vite** | 8.0.8 | 构建工具 | `client/package.json#L41` |
| **Vue Router** | 5.0.7 | 路由管理 | `client/package.json#L18` |
| **Pinia** | 3.0.4 | 状态管理 | `client/package.json#L14` |
| **Element Plus** | 2.14.0 | UI 组件库 | `client/package.json#L10` |
| **TypeScript** | 6.0.0 | 类型系统 | `client/package.json#L37` |
| **Axios** | 1.16.1 | HTTP 客户端 | `client/package.json#L9` |
| **Vitest** | 4.1.4 | 单元测试 | `client/package.json#L43` |
| **ECharts** | 6.0.0 | 数据可视化 | `client/package.json#L11` |
| **VeeValidate + Zod** | 4.15.1 + 3.24.0 | 表单验证 | `client/package.json#L15-L16` |

**Node 版本要求**: `^20.19.0 || >=22.12.0`（证据：`client/package.json#L46-L48`）

### 后端技术栈

| 技术 | 版本 | 用途 | 证据文件 |
|------|------|------|----------|
| **.NET** | 10.0 | 后端框架 | `server/src/AutoCodeForge.Api/AutoCodeForge.Api.csproj#L4` |
| **SqlSugarCore** | 5.1.4.214 | ORM | `server/src/AutoCodeForge.Api/AutoCodeForge.Api.csproj#L11` |
| **Swashbuckle** | 10.1.7 | Swagger/OpenAPI | `server/src/AutoCodeForge.Api/AutoCodeForge.Api.csproj#L12` |
| **SQLite** | - | 开发数据库 | `server/src/AutoCodeForge.Api/appsettings.json#L6-L8` |
| **JWT** | HS256 | 认证机制 | `server/src/AutoCodeForge.Api/appsettings.json#L9-L14` |

**四层架构**:
- **Api**: HTTP 端点与中间件（`server/src/AutoCodeForge.Api/`）
- **Application**: 业务服务层（`server/src/AutoCodeForge.Application/`）
- **Core**: 实体与接口（`server/src/AutoCodeForge.Core/`）
- **Infrastructure**: 仓储与外部集成（`server/src/AutoCodeForge.Infrastructure/`）

**证据**: `server/AutoCodeForge.sln#L6-L18`

## 模块映射

### 前端模块（12 个）

| 模块名称 | 路由前缀 | 主要功能 | 目录 | 证据 |
|---------|---------|---------|------|------|
| **auth** | `/login` | 用户认证 | `client/src/modules/auth/` | `client/src/router/index.ts#L3` |
| **console** | `/console` | 控制台主页 | `client/src/modules/console/` | `client/src/router/index.ts#L4` |
| **dashboard** | `/dashboard` | 数据仪表板 | `client/src/modules/dashboard/` | `client/src/router/index.ts#L5` |
| **task-center** | `/tasks` | 任务管理 | `client/src/modules/task-center/` | `client/src/router/index.ts#L9` |
| **agent-center** | `/agents` | Agent 管理 | `client/src/modules/agent-center/` | `client/src/router/index.ts#L10` |
| **repo-management** | `/repos` | 仓库管理 | `client/src/modules/repo-management/` | `client/src/router/index.ts#L8` |
| **pipeline-center** | `/pipelines` | 流水线管理 | `client/src/modules/pipeline-center/` | `client/src/router/index.ts#L7` |
| **scheduled-task** | `/scheduled-tasks` | 定时任务 | `client/src/modules/scheduled-task/` | `client/src/router/index.ts#L11` |
| **workflow-center** | `/workflows` | 工作流管理 | `client/src/modules/workflow-center/` | `client/src/router/index.ts#L12` |
| **system-config** | `/system-config` | 系统配置 | `client/src/modules/system-config/` | `client/src/router/index.ts#L8` |
| **md-wiki** | `/wiki` | Wiki 文档 | `client/src/modules/md-wiki/` | `client/src/router/index.ts#L6` |
| **notification** | `/notifications` | 通知管理 | `client/src/modules/notification/` | 基于目录推断 |

**模块结构约定**（证据：`client/src/modules/auth/` 目录结构）:
```
module-name/
  ├── index.ts         # 模块导出
  ├── routes.ts        # 路由定义
  ├── *.api.ts         # API 调用
  ├── *.types.ts       # TypeScript 类型
  ├── store/           # Pinia Store（可选）
  └── views/           # 页面组件
```

### 后端端点（23 个）

| 端点类 | 路由前缀（推断） | 主要功能 | 文件 |
|--------|-----------------|---------|------|
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

**端点注册方式**: 通过扩展方法在 `Program.cs` 中调用（证据：`server/src/AutoCodeForge.Api/Program.cs#L95-L100`）

## 入口点和路由

### 前端入口
- **主入口**: `client/src/main.ts`（创建 Vue 应用并挂载）
- **路由文件**: `client/src/router/index.ts`（聚合所有模块路由）
- **开发服务器**: `npm run dev`（Vite 启动，默认端口待确认）
- **构建输出**: `npm run build`（输出到 `client/dist/`，待确认）

**路由守卫**（证据：`client/src/router/index.ts#L31-L48`）:
- 除 `/login` 外所有路由需 JWT 令牌
- 令牌存储在 `localStorage.getItem('auth_token')`
- 未认证用户重定向到 `/login`

### 后端入口
- **主入口**: `server/src/AutoCodeForge.Api/Program.cs`（ASP.NET Core 启动）
- **端口配置**: 待确认（检查 `appsettings.json` 或 `launchSettings.json`）
- **Swagger UI**: 仅开发环境启用（证据：`Program.cs#L80-L84`）
- **根路由**: `GET /` 返回 `"AutoCodeForge API is running"`（证据：`Program.cs#L95`）

**中间件链**（证据：`Program.cs#L75-L88`）:
1. `ExceptionHandlingMiddleware` - 全局异常处理
2. `UseCors("AllowSpecificOrigins")` - CORS 策略
3. `UseSwagger/UseSwaggerUI` - API 文档（仅开发环境）
4. `UseHttpsRedirection` - HTTPS 重定向
5. `JwtAuthMiddleware` - JWT 认证

## 目录结构

```
AutoCodeForge/
├── client/                          # 前端应用（Vue 3 + Vite）
│   ├── src/
│   │   ├── api/                     # HTTP 客户端配置
│   │   ├── assets/                  # 静态资源
│   │   ├── components/              # 共享组件
│   │   ├── composables/             # Composition API 可组合函数
│   │   ├── config/                  # 前端配置
│   │   ├── host/                    # 待确认（可能是主机配置）
│   │   ├── lib/                     # 第三方库封装
│   │   ├── mock/                    # Mock 数据
│   │   ├── modules/                 # 功能模块（模块优先架构）
│   │   ├── router/                  # 路由配置
│   │   ├── stores/                  # Pinia 全局 Store
│   │   ├── types/                   # 全局 TypeScript 类型
│   │   ├── views/                   # 页面视图
│   │   ├── App.vue                  # 根组件
│   │   └── main.ts                  # 应用入口
│   ├── public/                      # 公共静态资源
│   │   ├── web.config               # IIS 部署配置
│   │   └── AutoCodeForge.wiki/      # Wiki 静态文档
│   ├── package.json                 # 前端依赖
│   ├── vite.config.ts               # Vite 配置
│   ├── tsconfig.json                # TypeScript 配置
│   └── vitest.config.ts             # Vitest 配置
│
├── server/                          # 后端应用（.NET 10.0）
│   ├── src/
│   │   ├── AutoCodeForge.Api/       # API 层（HTTP 端点、中间件）
│   │   │   ├── Endpoints/           # 23 个端点类
│   │   │   ├── Extensions/          # DI 扩展方法
│   │   │   ├── Middleware/          # 中间件（JWT、异常处理）
│   │   │   ├── Properties/          # 项目属性
│   │   │   ├── appsettings.json     # 配置文件
│   │   │   ├── Program.cs           # 应用入口
│   │   │   └── autocodeforge.dev.db # 开发数据库（SQLite）
│   │   ├── AutoCodeForge.Application/ # 应用层（业务服务）
│   │   │   ├── AI/                  # AI 服务（LLM 网关、Agent 执行）
│   │   │   ├── Configuration/       # 配置服务
│   │   │   ├── Models/              # 业务模型
│   │   │   ├── Security/            # 安全服务（JWT、加密）
│   │   │   ├── Services/            # 业务服务层
│   │   │   ├── StateMachines/       # 状态机（任务、流水线状态）
│   │   │   ├── Tools/               # Agent 工具
│   │   │   └── Validators/          # 业务验证器
│   │   ├── AutoCodeForge.Core/      # 核心层（实体、接口）
│   │   │   ├── DTOs/                # 数据传输对象
│   │   │   ├── Entities/            # 数据实体（32 个实体）
│   │   │   ├── Enums/               # 枚举类型
│   │   │   ├── Exceptions/          # 自定义异常
│   │   │   ├── Helpers/             # 辅助工具
│   │   │   ├── Interfaces/          # 核心接口
│   │   │   ├── Models/              # 领域模型
│   │   │   ├── Providers/           # 提供者接口
│   │   │   └── ValueObjects/        # 值对象
│   │   └── AutoCodeForge.Infrastructure/ # 基础设施层（数据访问、外部集成）
│   │       ├── AI/                  # AI 基础设施（GitHub Copilot CLI 服务）
│   │       ├── Data/                # 数据库初始化、SqlSugar 配置
│   │       ├── Logging/             # 日志服务
│   │       ├── Providers/           # Git 提供者（GitHub/GitLab/Azure DevOps）
│   │       ├── Queue/               # 任务队列
│   │       ├── Repositories/        # 仓储实现（30+ 仓储类）
│   │       ├── Security/            # 安全基础设施（加密、JWT）
│   │       ├── Services/            # 基础设施服务（Git、后台服务）
│   │       └── Tools/               # 工具实现
│   ├── tests/
│   │   └── AutoCodeForge.Tests/     # 单元测试项目
│   └── AutoCodeForge.sln            # 解决方案文件
│
├── docs/                            # 项目文档
│   ├── MasterPlan.md                # 主计划文档
│   ├── PROJECT_SPEC.md              # 项目规范宪法
│   ├── AutoCodeForge-Architecture-v1.0.0-20260521.md # 架构文档
│   ├── AutoCodeForge-CodeOpinion-v1.0.0-20260521.md  # 代码意见
│   ├── AutoCodeForge-GovernanceInit-v1.0.0-20260521.md # 治理初始化
│   ├── AutoCodeForge-ProjectOverview-v1.0.0-20260521.md # 项目概览
│   ├── plans/                       # 执行计划
│   ├── reports/                     # 执行报告
│   └── templates/                   # 文档模板
│
├── tools/                           # 开发工具
│   └── generate-wiki.mjs            # Wiki 生成脚本
│
├── .github/                         # GitHub 配置（待创建技能目录）
│   ├── skills/                      # 技能定义（本文档所在目录）
│   └── workflows/                   # CI/CD 工作流（待确认）
│
└── 3033846/                         # 用户特定目录（待确认用途）
    └── AzureDevOps_jblprd_JGP_20Applications/
        └── PartNumberChange.sln     # 第三方解决方案（可能是迁移项目）
```

**证据来源**:
- 前端结构：`client/` 目录实际结构
- 后端结构：`server/` 目录实际结构 + `AutoCodeForge.sln` 项目引用
- 文档结构：`docs/` 目录实际结构

## 状态流

### 任务状态机

**状态定义**（推断，需验证 `server/src/AutoCodeForge.Core/Enums/TaskStatus.cs`）:
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

### 流水线状态机

**状态定义**（推断，需验证 `server/src/AutoCodeForge.Core/Enums/PipelineStatus.cs`）:
```
Created → Queued → Running → Completed
             ↓       ↓
          Cancelled  Failed
```

**状态同步**（证据：`docs/MasterPlan.md#L50`）:
- 后台服务每 30 秒轮询流水线状态
- 状态同步服务：`PipelineSyncService`（待确认文件路径）

### 定时任务状态

**状态定义**（推断）:
```
Enabled ⇄ Disabled
   ↓
Executing → (回到 Enabled/Disabled)
```

**执行机制**（证据：`docs/MasterPlan.md` + 后台服务推断）:
- 使用 Cronos 库解析 Cron 表达式
- 后台服务：`ScheduledTaskExecutor`（30 秒轮询）
- 执行记录存储在 `ScheduledTaskExecutionEntity`

## 输出模板

### 系统架构概述

```markdown
**项目**: AutoCodeForge  
**类型**: AI 研发流水线系统  
**架构**: 前后端分离 + .NET 四层架构  

**前端**: Vue 3.5.32 + Vite 8 + Pinia + Element Plus  
**后端**: .NET 10.0 + SqlSugarCore + JWT  
**数据库**: SQLite (开发) / SQL Server (生产，待确认)  

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
- [ ] 状态流定义引用了实际的枚举文件或状态机代码
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
- **测试**: Vitest 4.1.4

证据：`client/package.json#L9-L18`
```

### 示例 2: 查询任务中心模块

**用户**: "任务中心模块的代码在哪里？"

**输出**:
```
任务中心模块分布在前后端：

**前端**:
- 模块目录: `client/src/modules/task-center/`
- 路由注册: `client/src/router/index.ts#L9`（导入 `taskCenterRoutes`）
- 路由前缀: `/tasks`（推断，需验证 `task-center/routes.ts`）

**后端**:
- 端点: `server/src/AutoCodeForge.Api/Endpoints/TaskEndpoints.cs`
- 服务: `server/src/AutoCodeForge.Application/Services/TaskService.cs`（推断）
- 仓储: `server/src/AutoCodeForge.Infrastructure/Repositories/TaskRepository.cs`
- 实体: `server/src/AutoCodeForge.Core/Entities/TaskEntity.cs`

**关联模块**:
- `TaskStepEndpoints`: 任务步骤管理
- `TaskOrchestrationEndpoints`: 任务编排
- `TaskLogEntity`: 任务日志

证据：前端模块目录实际存在，后端端点文件在 `Endpoints/` 目录中验证。
```

### 示例 3: 查询后端入口点

**用户**: "后端服务启动流程是什么？"

**输出**:
```
后端服务入口点：`server/src/AutoCodeForge.Api/Program.cs`

**启动流程**（按代码顺序）：

1. **服务注册**（第 10-60 行）:
   - CORS 配置（开发环境允许任意来源）
   - JWT 和 Git 选项配置
   - 核心服务、数据层、仓储、业务服务注册
   - Swagger 配置（XML 注释文档）

2. **中间件链**（第 75-88 行）:
   - ExceptionHandlingMiddleware（全局异常捕获）
   - CORS 策略应用
   - Swagger UI（仅开发环境）
   - HTTPS 重定向
   - JwtAuthMiddleware（JWT 认证）

3. **数据库初始化**（第 88-92 行）:
   - DatabaseInitializer.InitializeAsync()
   - 开发环境执行种子数据

4. **端点注册**（第 95+ 行）:
   - 根路由: `GET /` 返回健康状态
   - 其他端点通过扩展方法注册（推断）

证据：`server/src/AutoCodeForge.Api/Program.cs#L1-L100`
```

## 更新历史

| 日期 | 版本 | 变更说明 |
|------|------|----------|
| 2026-05-25 | 1.0.0 | 初始版本，基于 AutoCodeForge 项目实际代码生成 |

---

**维护说明**: 当项目技术栈、架构或模块发生重大变更时，需更新本文档。变更时必须提供新的证据文件路径，并保留变更历史记录。
