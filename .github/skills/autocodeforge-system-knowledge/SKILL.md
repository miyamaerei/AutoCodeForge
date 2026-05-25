---
name: autocodeforge-system-knowledge
description: '回答用户关于 AutoCodeForge 系统的知识问答，包括系统定位、技术栈、功能模块、架构设计、API 端点等方面的问题。'
argument-hint: '可选参数: topic - 指定查询的主题，如: 技术栈、功能模块、架构、API等'
---

# AutoCodeForge 系统知识问答

## 描述

该技能用于回答用户关于 AutoCodeForge 系统的各类知识问题，帮助用户快速了解系统的核心功能、技术架构和使用方式。

## 使用场景

- 用户想了解 AutoCodeForge 是什么系统
- 用户询问系统采用的技术栈
- 用户想了解系统包含哪些功能模块
- 用户询问系统的架构设计
- 用户想了解系统的 API 端点
- 用户需要了解系统的项目结构

## 知识覆盖范围

### 1. 系统定位

AutoCodeForge 是一个 **AI 研发流水线控制台**（SPA），采用前后端分离架构，当前核心定位是：
- 统一的 AI 驱动开发编排平台（任务、步骤、Agent、审查、通知）
- 支持多 Git 平台集成（GitHub/GitLab/Azure DevOps）
- 任务/定时任务/流水线/仓库同步自动化执行
- 人工审批关卡（Human Gate）、失败恢复、审查规则集一体化
- Wiki、系统配置、仪表盘监控统一管理

### 2. 技术栈

**前端 (client/)**:
| 维度 | 技术栈 | 版本 |
|------|--------|------|
| 框架 | Vue 3 + Composition API | 3.5.32 |
| 构建 | Vite | 8.0.8 |
| 状态管理 | Pinia | 3.0.4 |
| 路由 | Vue Router | 5.0.7 |
| UI框架 | Element Plus | 2.14.0 |
| 表单验证 | VeeValidate + Zod | 4.15.1 / 3.24.0 |
| HTTP客户端 | Axios | 1.16.1 |
| 测试 | Vitest | 4.1.4 |
| 其他核心依赖 | ECharts / Markdown-It / Konva | 6.0.0 / 14.1.1 / 10.3.0 |

**后端 (server/, .NET 10 分层 + Minimal API)**:
| 层级 | 职责 | 关键组件 |
|-----|------|---------|
| Api层 | Minimal API 端点、认证中间件、异常处理 | Endpoints, JwtAuthMiddleware, ExceptionHandlingMiddleware, Swagger |
| Application层 | 业务逻辑、编排、审查、配置管理 | Services, Security, Validators, Tooling |
| Core层 | 领域模型、DTO、仓储抽象、通用模型 | Entities, DTOs, Interfaces, ApiResponse |
| Infrastructure层 | 数据访问、Git集成、AI网关、后台任务 | SqlSugar, LibGit2Sharp, AgentFrameworkGateway, BackgroundServices |

**后端关键版本**:
- 目标框架: .NET 10 (`net10.0`)
- ORM: SqlSugar `5.1.4.214`
- Git: LibGit2Sharp `0.31.0`
- 调度: Cronos `0.8.4`
- AI: Microsoft.Agents.AI `1.0.0-preview.251016.1` + Azure.AI.OpenAI `2.0.0`

### 3. 功能模块

| 模块 | 描述 | 状态 |
|-----|------|------|
| 基础设施 | 四层解决方案、统一响应模型、全局异常处理中间件 | ✅ 已落地 |
| 数据与认证 | 32 个核心实体、JWT 登录、Windows 登录、数据初始化 | ✅ 已落地 |
| AI核心 | Agent Framework 网关、GitHub Copilot CLI 服务、工具注册 | ✅ 已落地 |
| Agent编排 | Agent 注册/心跳/状态管理、最小负载分配策略 | ✅ 已落地 |
| 任务中心 | 任务CRUD、日志、暂停/恢复、步骤流转 | ✅ 已落地 |
| 任务编排 | 任务分配/重分配、终止、需求更新、上下文链路 | ✅ 已落地 |
| 人工关卡 | 审批/驳回/修改通过/取消 | ✅ 已落地 |
| 失败恢复 | 卡住步骤恢复、应急解绑、恢复历史与统计 | ✅ 已落地 |
| 审查中心 | Review Task、RuleSet 管理、Findings 查询 | ✅ 已落地 |
| 仓库与同步 | 仓库CRUD、分支/提交/PR、仓库同步任务 | ✅ 已落地 |
| 流水线 | Pipeline/Build 管理、触发与同步 | ✅ 已落地 |
| 定时任务调度 | ScheduledTask CRUD、暂停/恢复、执行记录 | ✅ 已落地 |
| 通知中心 | 模板通知、用户通知、已读状态 | ✅ 已落地 |
| 仪表盘 | 快照、任务/Agent/日志实时流、系统指标 | ✅ 已落地 |
| Wiki模块 | Wiki 页面 CRUD、关键字检索、分页查询 | 🔄 持续迭代 |

### 4. 核心服务

| 服务 | 职责 | 关键能力 |
|-----|------|---------|
| AuthService | 注册、登录、当前用户解析 | Register/Login/WindowsLogin/GetCurrentUser |
| AgentService | Agent CRUD、匹配、状态流转 | Match/Assign/Complete/Fail/Learn/Dormant/Wake |
| AgentRegistryService | Agent 注册中心与心跳管理 | Register/Heartbeat/Unregister/ListAvailable |
| ChatService | 会话管理、消息存储、流式对话 | Session CRUD、Message、SSE Stream |
| TaskService | 任务生命周期管理 | Create/Update/Pause/Resume/Delete/GetLogs |
| TaskStepService | 步骤初始化与推进 | Init/Advance/Skip/Unbind |
| TaskOrchestrator | 编排分配与执行控制 | Assign/Reassign/Pause/Resume/Terminate |
| HumanGateService | 人工审批关卡处理 | Pending/Approve/Reject/ModifyApprove/Cancel |
| FailureRecoveryService | 失败恢复与卡点清理 | Recover/StuckSteps/EmergencyUnbind |
| ReviewService / ReviewRuleSetService | 代码审查任务与规则集 | RuleSet CRUD / ReviewTask / Findings |
| RepositoryService / RepoSyncService | 仓库操作与同步任务 | Branches/Commits/PR / SyncTask |
| PipelineService | 流水线与构建管理 | Pipeline CRUD / Trigger / Build 查询 |
| ScheduledTaskService | 定时任务与执行记录 | CRUD / Pause / Resume / Executions |
| ConfigService | 全局与用户配置管理 | CRUD / Init / Reset / Import / Export / History |
| NotificationService | 站内通知发送与已读管理 | Send / UserList / Read / ReadAll |
| WikiService | Wiki 页面管理 | Create/Update/Delete/List/GetById |

### 5. API端点

**认证端点**:
- `POST /api/v1/auth/register` → 注册并返回 Token
- `POST /api/v1/auth/login` → 登录并返回 Token
- `POST /api/v1/auth/windows-login` → Windows 身份登录
- `GET /api/v1/auth/me` → 获取当前登录用户

**核心业务端点**:
| 模块 | 端点示例 |
|-----|---------|
| Agent | GET `/api/v1/agents`, POST `/api/v1/agents/{id}/assign`, GET `/api/v1/agents/dormant` |
| Agent注册 | POST `/api/v1/agents/register`, PUT `/api/v1/agents/heartbeat`, GET `/api/v1/agents/available` |
| Chat | POST `/api/v1/chat/sessions`, POST `/api/v1/chat/sessions/{id}/messages`, POST `/api/v1/chat/sessions/{id}/stream` |
| Task | POST `/api/v1/tasks`, GET `/api/v1/tasks/{id}/logs`, POST `/api/v1/tasks/{id}/pause` |
| TaskStep | GET `/api/v1/tasks/{taskId}/steps`, POST `/api/v1/tasks/{taskId}/steps/init`, POST `/api/v1/tasks/{taskId}/steps/{stepId}/advance` |
| Orchestration | POST `/api/v1/orchestration/tasks/{taskId}/assign`, POST `/api/v1/orchestration/tasks/{taskId}/reassign` |
| Human Gate | GET `/api/v1/human-gates/pending`, POST `/api/v1/human-gates/{id}/approve` |
| Failure Recovery | POST `/api/v1/failure/recover`, GET `/api/v1/failure/stuck-steps`, POST `/api/v1/failure/emergency-unbind/{stepId}` |
| Review | GET `/api/v1/reviews/rule-sets`, POST `/api/v1/reviews/tasks`, GET `/api/v1/reviews/tasks/{taskId}/findings` |
| Repo Sync | POST `/api/v1/repo-sync/tasks`, GET `/api/v1/repo-sync/tasks/{taskId}` |
| Repository | GET `/api/v1/repositories`, GET `/api/v1/repositories/{id}/branches`, POST `/api/v1/repositories/{id}/pull-requests` |
| Pipeline | GET `/api/v1/pipelines`, POST `/api/v1/pipelines/{id}/trigger`, GET `/api/v1/pipelines/{id}/builds` |
| Scheduled Task | GET `/api/v1/scheduled-tasks`, POST `/api/v1/scheduled-tasks/{id}/pause`, GET `/api/v1/scheduled-tasks/{id}/executions` |
| Notification | POST `/api/v1/notifications/send`, GET `/api/v1/notifications/user/{userId}` |
| Dashboard | GET `/api/v1/dashboard/snapshot`, GET `/api/v1/dashboard/live/stream`（兼容 `/api/dashboard/*`） |
| Wiki | GET `/api/v1/wiki`, POST `/api/v1/wiki`, GET `/api/v1/wiki/{id}` |
| Config | GET `/api/v1/configs/{configType}`, PUT `/api/v1/configs/{configType}/{configKey}` |
| System/Health | GET `/system/info`, GET `/health`, GET `/health/ready` |

### 6. 项目结构

```
AutoCodeForge/
├── client/                 # Vue 3 前端应用
│   ├── src/
│   │   ├── assets/         # 静态资源
│   │   ├── components/     # 共享组件
│   │   ├── composables/    # 组合式函数
│   │   ├── config/         # 配置文件
│   │   ├── modules/        # 功能模块
│   │   ├── router/         # 路由配置
│   │   ├── stores/         # 全局 Store
│   │   └── mock/           # Mock 数据
│   └── public/             # 静态资源
├── server/                 # .NET 10 后端服务
│   ├── src/
│   │   ├── AutoCodeForge.Api            # Minimal API Host
│   │   ├── AutoCodeForge.Application    # 应用服务层
│   │   ├── AutoCodeForge.Core           # 核心领域层
│   │   └── AutoCodeForge.Infrastructure # 基础设施层
│   ├── tests/               # 单元与集成测试
│   └── docs/                # 后端文档
├── docs/                   # 项目文档
└── .autoCodeForge/         # 治理配置与模板
```

### 7. 核心实体（Entity）

当前后端在 `AutoCodeForge.Core/Entities` 下定义了 **32 个业务实体**（不含基类）：

**身份与配置**:
- UserEntity
- GlobalConfigEntity
- UserConfigEntity
- LLMModelConfigEntity
- AISessionConfigEntity
- ConfigurationEntry
- ConfigHistoryEntity

**任务与编排**:
- TaskEntity
- TaskLogEntity
- TaskStepEntity
- TaskReviewEntity
- HumanGateEntity
- ScheduledTaskEntity
- ScheduledTaskExecutionEntity

**Agent 与协作**:
- AgentEntity
- AgentLearningRecordEntity
- AgentDormantRecordEntity
- AgentRegistrationEntity
- AgentToolInvocationEntity
- GitSkillGrantEntity

**聊天与通知**:
- ChatSessionEntity
- ChatMessageEntity
- NotificationEntity

**仓库与审查**:
- RepositoryEntity
- RepoSandboxWorkspaceEntity
- ReviewRuleSetEntity
- ReviewTaskEntity
- ReviewFindingEntity

**流水线与 Wiki**:
- PipelineEntity
- BuildEntity
- WikiPageEntity
- AdminAuditLogEntity

**实体基类（抽象）**:
- AuditableEntity
- UserOwnedEntity

说明：数据库初始化器 `DatabaseInitializer` 当前通过 CodeFirst 显式注册了 32 个建表类型，已覆盖上述核心业务实体。

## 工作流程

1. **解析用户问题**: 识别用户询问的主题（技术栈、功能模块、架构、API等）
2. **匹配知识**: 根据主题匹配对应的系统知识
3. **生成回答**: 组织结构化的回答内容
4. **输出结果**: 以清晰的格式输出系统知识

## 示例

### 示例1: 查询系统定位

输入: `AutoCodeForge 是什么系统？`

输出:
```
AutoCodeForge 是一个 AI 研发流水线控制台（SPA），采用前后端分离架构。

核心定位:
- 统一的 AI 驱动开发编排平台（任务、步骤、Agent、审查、通知）
- 支持多 Git 平台（GitHub/GitLab/Azure DevOps）
- 任务/定时任务/流水线/仓库同步自动化执行
- 人工审批关卡、失败恢复、审查规则集一体化
- Wiki、系统配置、仪表盘监控统一管理
```

### 示例2: 查询技术栈

输入: `系统采用什么技术栈？`

输出:
```
【前端技术栈】
- 框架: Vue 3 + Composition API (3.5.32)
- 构建: Vite 8.0.8
- 状态管理: Pinia 3.0.4
- 路由: Vue Router 5.0.7
- UI框架: Element Plus 2.14.0
- HTTP客户端: Axios 1.16.1
- 测试: Vitest 4.1.4

【后端技术栈】
- 目标框架: .NET 10 (net10.0)
- API风格: Minimal API + Middleware
- ORM: SqlSugar 5.1.4.214
- AI: Microsoft.Agents.AI + Azure.AI.OpenAI
- 调度: Cronos + BackgroundService
```

### 示例3: 查询功能模块

输入: `系统有哪些功能模块？`

输出:
```
已完成的功能模块:

✅ 基础设施 - 四层项目解决方案、全局异常中间件
✅ 数据与认证 - 32实体数据层、JWT/Windows登录
✅ AI核心 - Microsoft Agent Framework集成、LLM网关
✅ 任务中心 - 任务CRUD、步骤流转、编排控制
✅ 人工关卡 - 审批/驳回/修改通过/取消
✅ 审查中心 - Review任务、RuleSet、Findings
✅ 定时任务调度 - Cronos集成、后台轮询执行
✅ Git仓库集成 - 多平台支持 + 仓库同步任务
✅ 流水线模块 - Pipeline/Build管理、触发、同步
✅ 仪表盘与通知 - 实时流、指标、通知模板

🔄 Wiki模块 - 页面CRUD、关键字检索（持续迭代）
```

### 示例4: 查询API端点

输入: `认证相关的API有哪些？`

输出:
```
【认证端点】

- POST /api/v1/auth/register      → 注册并返回 Token
- POST /api/v1/auth/login         → 登录并返回 Token
- POST /api/v1/auth/windows-login → Windows 身份登录
- GET  /api/v1/auth/me            → 获取当前登录用户

响应格式:
{
  "success": true,
  "statusCode": 200,
  "data": { ... },
  "message": "Operation completed successfully",
  "timestamp": "2026-05-21T10:30:00Z"
}
```