---
name: feature-code-guide
description: 查询功能模块的前后端代码位置引导。当需要修改某个功能时，快速定位前端页面、后端API、涉及模块和相关组件。适用于不知道功能代码位置、需要了解功能涉及的文件和模块、或需要查找相关组件、Store、Service等场景。
---

# 功能代码位置引导

## 描述

该技能用于帮助开发人员快速定位功能对应的代码位置。当需要修改某个功能时，可以根据功能名称快速找到前端页面、后端API、涉及模块和相关组件的具体文件路径。

## 使用场景

- 当需要修改某个功能时，不知道前端页面在哪个文件
- 想了解某个功能涉及哪些后端模块和API
- 需要查找功能相关的组件、Store、Service等
- 接到新功能开发任务，需要快速了解代码结构

## 指令

### 第一步：解析功能名称

接收用户输入的功能名称，支持以下匹配方式：

| 功能名称 | 模糊匹配关键词 |
|---------|---------------|
| AI任务中心 | 任务、任务中心 |
| Agent中心 | Agent、代理、agent |
| 定时任务 | 定时、定时任务、scheduled |
| 仓库管理 | 仓库、repo、仓库管理 |
| Dashboard工作台 | Dashboard、工作台、dashboard |
| 流水线中心 | 流水线、pipeline |
| 系统配置 | 配置、settings、config |
| AI聊天控制台 | 聊天、console、AI聊天 |
| Wiki系统 | Wiki、wiki、文档 |
| 代码审查 | 审查、review、代码审查 |
| 认证登录 | 登录、auth、认证 |
| Git技能 | Git、git |

### 第二步：确定详细程度

根据用户指定的 detailLevel 参数（或默认 basic）：

- **basic**: 仅显示核心文件路径（模块名、主要文件）
- **detail**: 显示完整的文件列表（所有视图、组件、API等）
- **full**: 显示文件路径 + 简要说明 + 关联关系

### 第三步：输出结果

按照以下格式输出功能对应的代码位置：

```
功能: [功能名称]

【前端模块】
└── [模块名]
    └── [文件路径] (说明)

【后端模块】
├── [服务层]
├── [API层]
└── [数据层]
```

## 功能映射数据

### AI任务中心 (task-center)

**前端:**
- 主页面: `client/src/modules/task-center/views/TaskCenterView.vue`
- 任务列表: `client/src/modules/task-center/views/TaskCenterListView.vue`
- 任务详情: `client/src/modules/task-center/views/TaskCenterDetailView.vue`
- 创建任务: `client/src/modules/task-center/views/TaskCenterCreateView.vue`
- 状态管理: `client/src/modules/task-center/store/useTaskCenterStore.ts`
- API调用: `client/src/modules/task-center/task-center.api.ts`
- 类型定义: `client/src/modules/task-center/task-center.types.ts`
- 路由配置: `client/src/modules/task-center/routes.ts`

**后端:**
- API端点: `server/src/AutoCodeForge.Api/Endpoints/TaskEndpoints.cs`
- 应用服务: `server/src/AutoCodeForge.Application/Services/TaskService.cs`
- 数据仓储: `server/src/AutoCodeForge.Infrastructure/Repositories/TaskRepository.cs`
- 任务实体: `server/src/AutoCodeForge.Core/Entities/TaskEntity.cs`
- 任务日志: `server/src/AutoCodeForge.Core/Entities/TaskLogEntity.cs`
- 请求DTO: `server/src/AutoCodeForge.Core/DTOs/Task/CreateTaskRequest.cs`
- 响应DTO: `server/src/AutoCodeForge.Core/DTOs/Task/TaskResponse.cs`

---

### Agent中心 (agent-center)

**前端:**
- 主页面: `client/src/modules/agent-center/views/AgentCenterView.vue`
- 状态管理: `client/src/modules/agent-center/store/useAgentStore.ts`
- API调用: `client/src/modules/agent-center/agent.api.ts`
- 类型定义: `client/src/modules/agent-center/api/agent.types.ts`
- Composables: `client/src/modules/agent-center/composables/useAgent.ts`

**后端:**
- API端点: `server/src/AutoCodeForge.Api/Endpoints/AgentEndpoints.cs`
- 应用服务: `server/src/AutoCodeForge.Application/Services/AgentService.cs`
- 数据仓储: `server/src/AutoCodeForge.Infrastructure/Repositories/AgentRepository.cs`
- Agent实体: `server/src/AutoCodeForge.Core/Entities/AgentEntity.cs`
- Agent技能端点: `server/src/AutoCodeForge.Api/Endpoints/AgentSkillEndpoints.cs`

---

### 定时任务 (scheduled-task)

**前端:**
- 主页面: `client/src/modules/scheduled-task/views/ScheduledTaskView.vue`
- 状态管理: `client/src/modules/scheduled-task/store/useScheduledTaskStore.ts`
- API调用: `client/src/modules/scheduled-task/scheduled-task.api.ts`
- 类型定义: `client/src/modules/scheduled-task/api/scheduled-task.types.ts`
- Mock数据: `client/src/modules/scheduled-task/api/scheduled-task.mock.ts`
- Composables: `client/src/modules/scheduled-task/composables/useScheduledTask.ts`

**后端:**
- API端点: `server/src/AutoCodeForge.Api/Endpoints/ScheduledTaskEndpoints.cs`
- 应用服务: `server/src/AutoCodeForge.Application/Services/ScheduledTaskService.cs`
- 数据仓储: `server/src/AutoCodeForge.Infrastructure/Repositories/ScheduledTaskRepository.cs`
- 调度服务: `server/src/AutoCodeForge.Infrastructure/BackgroundServices/CronSchedulerService.cs`
- 任务实体: `server/src/AutoCodeForge.Core/Entities/ScheduledTaskEntity.cs`
- 执行记录: `server/src/AutoCodeForge.Core/Entities/ScheduledTaskExecutionEntity.cs`

---

### 仓库管理 (repo-management)

**前端:**
- 主页面: `client/src/modules/repo-management/views/RepoManagementView.vue`
- 仓库列表: `client/src/modules/repo-management/views/RepoManagementListView.vue`
- 分支管理: `client/src/modules/repo-management/views/RepoManagementBranchesView.vue`
- PR管理: `client/src/modules/repo-management/views/RepoManagementPRsView.vue`
- 状态管理: `client/src/modules/repo-management/store/useRepoManagementStore.ts`
- API调用: `client/src/modules/repo-management/api/repo-management.api.ts`
- 类型定义: `client/src/modules/repo-management/api/repo-management.types.ts`

**后端:**
- API端点: `server/src/AutoCodeForge.Api/Endpoints/RepositoryEndpoints.cs`
- 应用服务: `server/src/AutoCodeForge.Application/Services/RepositoryService.cs`
- 数据仓储: `server/src/AutoCodeForge.Infrastructure/Repositories/RepositoryRepository.cs`
- 仓库实体: `server/src/AutoCodeForge.Core/Entities/RepositoryEntity.cs`
- 沙箱同步: `server/src/AutoCodeForge.Application/Services/RepoSyncService.cs`
- 同步端点: `server/src/AutoCodeForge.Api/Endpoints/RepoSyncEndpoints.cs`

---

### Dashboard工作台 (dashboard)

**前端:**
- 主页面: `client/src/modules/dashboard/views/DashboardView.vue`

---

### 流水线中心 (pipeline-center)

**前端:**
- 主页面: `client/src/modules/pipeline-center/views/PipelineCenterView.vue`
- 流水线列表: `client/src/modules/pipeline-center/views/PipelineCenterListView.vue`
- 构建状态: `client/src/modules/pipeline-center/views/PipelineCenterBuildsView.vue`
- 状态管理: `client/src/modules/pipeline-center/store/usePipelineCenterStore.ts`
- API调用: `client/src/modules/pipeline-center/pipeline-center.api.ts`

**后端:**
- API端点: `server/src/AutoCodeForge.Api/Endpoints/PipelineEndpoints.cs`
- 应用服务: `server/src/AutoCodeForge.Application/Services/PipelineService.cs`
- 数据仓储: `server/src/AutoCodeForge.Infrastructure/Repositories/PipelineRepository.cs`
- 构建仓储: `server/src/AutoCodeForge.Infrastructure/Repositories/BuildRepository.cs`
- 流水线实体: `server/src/AutoCodeForge.Core/Entities/PipelineEntity.cs`
- 构建实体: `server/src/AutoCodeForge.Core/Entities/BuildEntity.cs`

---

### 系统配置 (system-config)

**前端页面:**
- 主容器: `client/src/modules/system-config/views/SystemConfigView.vue`
- 个人偏好: `client/src/modules/system-config/views/SystemConfigPreferencesView.vue`
- 仓库配置: `client/src/modules/system-config/views/SystemConfigRepositoriesView.vue`
- 知识库: `client/src/modules/system-config/views/SystemConfigKnowledgeView.vue`
- 技能管理: `client/src/modules/system-config/views/SystemConfigSkillView.vue`
- 定时任务: `client/src/modules/system-config/views/SystemConfigSchedulesView.vue`
- DeepWiki: `client/src/modules/system-config/views/SystemConfigDeepWikiView.vue`
- 代码审查: `client/src/modules/system-config/views/SystemConfigReviewView.vue`
- 第三方集成: `client/src/modules/system-config/views/SystemConfigIntegrationsView.vue`
- 通知设置: `client/src/modules/system-config/views/SystemConfigNotificationsView.vue`
- 沙箱配置: `client/src/modules/system-config/views/SystemConfigSandboxView.vue`
- 工作流: `client/src/modules/system-config/views/SystemConfigWorkflowView.vue`
- API配置: `client/src/modules/system-config/views/SystemConfigApiView.vue`
- 模型选择: `client/src/modules/system-config/views/SystemConfigModelsView.vue`
- 用户管理: `client/src/modules/system-config/views/SystemConfigUsersView.vue`
- 系统管理: `client/src/modules/system-config/views/SystemConfigManagementView.vue`

**前端Store:**
- 状态管理: `client/src/modules/system-config/store/useSystemConfigStore.ts`

**前端API:**
- API调用: `client/src/modules/system-config/api/config.api.ts`
- Mock数据: `client/src/modules/system-config/api/config.mock.ts`
- 类型定义: `client/src/modules/system-config/api/config.types.ts`
- 路由配置: `client/src/modules/system-config/routes.ts`

**后端:**
- API端点: `server/src/AutoCodeForge.Api/Endpoints/ConfigEndpoints.cs`
- 应用服务: `server/src/AutoCodeForge.Application/Services/ConfigService.cs`
- 初始化服务: `server/src/AutoCodeForge.Application/Services/ConfigInitializationService.cs`
- 历史服务: `server/src/AutoCodeForge.Application/Services/ConfigHistoryService.cs`
- 导出服务: `server/src/AutoCodeForge.Application/Services/ConfigExportService.cs`
- 数据仓储: `server/src/AutoCodeForge.Infrastructure/Repositories/ConfigRepository.cs`
- 历史仓储: `server/src/AutoCodeForge.Infrastructure/Repositories/ConfigHistoryRepository.cs`
- 全局配置实体: `server/src/AutoCodeForge.Core/Entities/GlobalConfigEntity.cs`
- 配置历史实体: `server/src/AutoCodeForge.Core/Entities/ConfigHistoryEntity.cs`
- 配置类型枚举: `server/src/AutoCodeForge.Core/Enums/ConfigType.cs`

---

### AI聊天控制台 (console)

**前端:**
- 入口页面: `client/src/modules/console/views/ConsoleEntryView.vue`
- 提问模式: `client/src/modules/console/views/ConsoleAskView.vue`
- 会话模式: `client/src/modules/console/views/ConsoleSessionView.vue`
- 聊天视图: `client/src/modules/console/views/ConsoleChatView.vue`
- 审查视图: `client/src/modules/console/views/ConsoleReviewView.vue`
- Wiki视图: `client/src/modules/console/views/ConsoleWikiView.vue`
- 自动化视图: `client/src/modules/console/views/ConsoleAutomationsView.vue`
- 状态管理: `client/src/modules/console/store/useChatStore.ts`
- 状态管理: `client/src/modules/console/store/useConsoleStore.ts`
- API调用: `client/src/modules/console/api/chat.api.ts`
- 类型定义: `client/src/modules/console/api/chat.types.ts`
- Composables: `client/src/modules/console/composables/useConsoleChat.ts`
- Composables: `client/src/modules/console/composables/useConsoleWiki.ts`

**后端:**
- API端点: `server/src/AutoCodeForge.Api/Endpoints/ChatEndpoints.cs`
- 流式端点: `server/src/AutoCodeForge.Api/Endpoints/ChatStreamEndpoints.cs`
- 应用服务: `server/src/AutoCodeForge.Application/Services/ChatService.cs`
- 会话管理: `server/src/AutoCodeForge.Infrastructure/AI/ChatSessionManager.cs`
- Agent执行器: `server/src/AutoCodeForge.Infrastructure/AI/AgentExecutor.cs`
- 数据仓储: `server/src/AutoCodeForge.Infrastructure/Repositories/ChatSessionRepository.cs`
- 消息仓储: `server/src/AutoCodeForge.Infrastructure/Repositories/ChatMessageRepository.cs`

---

### Wiki系统 (md-wiki)

**前端:**
- Wiki列表: `client/src/modules/md-wiki/views/MdWikiListView.vue`
- Composables: `client/src/modules/md-wiki/composables/useMdWiki.ts`

**后端:**
- API端点: `server/src/AutoCodeForge.Api/Endpoints/WikiEndpoints.cs`
- 应用服务: `server/src/AutoCodeForge.Application/Services/WikiService.cs`
- 数据仓储: `server/src/AutoCodeForge.Infrastructure/Repositories/WikiPageRepository.cs`
- Wiki实体: `server/src/AutoCodeForge.Core/Entities/WikiPageEntity.cs`

---

### 认证登录 (auth)

**前端:**
- 登录页面: `client/src/modules/auth/views/AuthLoginView.vue`
- 状态管理: `client/src/modules/auth/store/useAuthStore.ts`
- API调用: `client/src/modules/auth/auth.api.ts`
- 类型定义: `client/src/modules/auth/auth.types.ts`

**后端:**
- API端点: `server/src/AutoCodeForge.Api/Endpoints/AuthEndpoints.cs`
- 应用服务: `server/src/AutoCodeForge.Application/Services/AuthService.cs`
- JWT服务: `server/src/AutoCodeForge.Application/Services/JwtService.cs`
- 用户仓储: `server/src/AutoCodeForge.Infrastructure/Repositories/UserRepository.cs`
- 用户实体: `server/src/AutoCodeForge.Core/Entities/UserEntity.cs`
- 认证中间件: `server/src/AutoCodeForge.Api/Middleware/JwtAuthMiddleware.cs`

---

### 代码审查 (review)

**前端:**
- 审查视图: `client/src/modules/console/views/ConsoleReviewView.vue`

**后端:**
- API端点: `server/src/AutoCodeForge.Api/Endpoints/ReviewEndpoints.cs`
- 应用服务: `server/src/AutoCodeForge.Application/Services/ReviewService.cs`
- 规则服务: `server/src/AutoCodeForge.Application/Services/ReviewRuleSetService.cs`
- 审查引擎: `server/src/AutoCodeForge.Infrastructure/Review/RuleBasedReviewEngine.cs`
- 数据仓储: `server/src/AutoCodeForge.Infrastructure/Repositories/ReviewRepository.cs`
- 规则仓储: `server/src/AutoCodeForge.Infrastructure/Repositories/ReviewRuleSetRepository.cs`
- 审查实体: `server/src/AutoCodeForge.Core/Entities/ReviewTaskEntity.cs`
- 发现实体: `server/src/AutoCodeForge.Core/Entities/ReviewFindingEntity.cs`

---

### Git技能 (git-skills)

**后端:**
- Git工具集: `server/src/AutoCodeForge.Application/Tools/GitTools.cs`
- 只读工具: `server/src/AutoCodeForge.Application/Tools/GitReadToolset.cs`
- 写工具: `server/src/AutoCodeForge.Application/Tools/GitWriteToolset.cs`
- 权限守卫: `server/src/AutoCodeForge.Application/Security/GitSkillPermissionGuard.cs`
- 策略服务: `server/src/AutoCodeForge.Application/Services/GitSkillPolicyService.cs`
- Git提供者: `server/src/AutoCodeForge.Infrastructure/Git/GitProviderFactory.cs`
- GitHub提供者: `server/src/AutoCodeForge.Infrastructure/Git/GitHubProvider.cs`
- GitLab提供者: `server/src/AutoCodeForge.Infrastructure/Git/GitLabProvider.cs`
- Azure提供者: `server/src/AutoCodeForge.Infrastructure/Git/AzureDevOpsProvider.cs`
- LibGit2Sharp: `server/src/AutoCodeForge.Infrastructure/Git/LibGit2SharpProvider.cs`

## 示例

### Basic模式输出

输入: `功能=任务中心`

```
功能: AI任务中心

【前端模块】
└── task-center

【后端模块】
├── TaskService
├── TaskEndpoints
└── TaskRepository
```

### Detail模式输出

输入: `功能=任务中心 detailLevel=detail`

```
功能: AI任务中心

【前端文件】
├── views/
│   ├── TaskCenterView.vue        (主页面)
│   ├── TaskCenterListView.vue    (任务列表)
│   ├── TaskCenterDetailView.vue  (任务详情)
│   └── TaskCenterCreateView.vue  (创建任务)
├── store/useTaskCenterStore.ts   (状态管理)
├── task-center.api.ts            (API调用)
├── task-center.types.ts         (类型定义)
└── routes.ts                    (路由配置)

【后端文件】
├── Endpoints/TaskEndpoints.cs   (API端点)
├── Services/TaskService.cs      (应用服务)
├── Repositories/TaskRepository.cs (数据仓储)
├── Entities/TaskEntity.cs       (任务实体)
└── DTOs/Task/*.cs               (数据传输对象)
```
