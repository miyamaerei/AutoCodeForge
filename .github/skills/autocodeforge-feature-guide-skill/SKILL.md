---
name: autocodeforge-feature-guide-skill
description: '查询功能模块的前后端代码位置引导。当需要修改某个功能时，快速定位前端页面、后端API、涉及模块和相关组件。'
argument-hint: '必填: featureName - 功能名称，如"任务中心"、"仓库管理"、"定时任务"等。可选: detailLevel - 详细程度(basic/detail/full)。'
---

# FeatureGuideSkill (功能代码位置引导查询 Skill)

## 核心功能

该Skill用于帮助开发人员快速定位功能对应的代码位置，解决以下场景：
- 当需要修改某个功能时，不知道前端页面在哪
- 想了解某个功能涉及哪些后端模块和API
- 需要查找功能相关的组件、Store、Service等

## 支持的功能模块

| 功能名称 | 功能说明 | 前端模块 | 后端模块 |
|---------|---------|---------|---------|
| AI任务中心 | 自然语言提需求、多轮对话、执行步骤 | task-center | TaskService/TaskEndpoints |
| Agent中心 | 配置AI Agent，支持自动选择和手动引用 | agent-center | AgentService/AgentEndpoints |
| 定时任务 | 配置自动化定时任务，支持仓库关联和模板 | scheduled-task | ScheduledTaskService/ScheduledTaskEndpoints |
| 仓库管理 | 仓库列表、分支、文件树与PR视图 | repo-management | RepositoryService/RepositoryEndpoints |
| Dashboard工作台 | 今日任务数、成功率、最近任务与告警 | dashboard | - |
| 流水线中心 | 构建状态、步骤状态、日志查看 | pipeline-center | PipelineService/PipelineEndpoints |
| 系统配置 | API Key、模型选择、用户管理 | system-config | ConfigService/ConfigEndpoints |
| AI聊天控制台 | 与AI助手对话，获取开发建议 | console | ChatService/ChatEndpoints |
| Wiki系统 | Markdown文档浏览和管理 | md-wiki | WikiService/WikiEndpoints |
| 认证登录 | 用户登录和认证 | auth | AuthService/AuthEndpoints |
| 代码审查 | 审查任务闭环，规则分层与审查报告 | console/review | ReviewService/ReviewEndpoints |
| Git技能 | Agent接入Git只读/变更技能 | - | GitTools/GitReadToolset/GitWriteToolset |

## 输入参数

### 必填参数
- `featureName`: 功能名称，如"任务中心"、"仓库管理"、"定时任务"、"Agent中心"等

### 可选参数
- `detailLevel`: 详细程度，默认`basic`
  - `basic`: 仅显示核心文件路径
  - `detail`: 显示完整的文件列表
  - `full`: 显示文件路径+简要说明+关联关系

## 输出格式

### Basic模式输出示例
```
功能: AI任务中心
├── 前端模块: task-center
│   └── 页面: TaskCenterView.vue
└── 后端模块: TaskService / TaskEndpoints
```

### Detail模式输出示例
```
功能: AI任务中心
├── 前端模块: client/src/modules/task-center/
│   ├── views/
│   │   ├── TaskCenterView.vue      (主页面)
│   │   ├── TaskCenterListView.vue  (任务列表)
│   │   ├── TaskCenterDetailView.vue (任务详情)
│   │   └── TaskCenterCreateView.vue (创建任务)
│   ├── store/
│   │   └── useTaskCenterStore.ts   (状态管理)
│   ├── task-center.api.ts          (API调用)
│   ├── task-center.types.ts        (类型定义)
│   └── routes.ts                   (路由配置)
└── 后端模块:
    ├── API层: server/src/AutoCodeForge.Api/Endpoints/TaskEndpoints.cs
    ├── 服务层: server/src/AutoCodeForge.Application/Services/TaskService.cs
    ├── 数据层: server/src/AutoCodeForge.Infrastructure/Repositories/TaskRepository.cs
    └── 实体/DTO: server/src/AutoCodeForge.Core/Entities/TaskEntity.cs
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

**配置分组结构:**

| 分组名称 | 路由路径 | 页面文件 | 说明 |
|---------|---------|---------|------|
| 个人偏好 | `/settings/preferences` | SystemConfigPreferencesView.vue | 用户个人设置 |
| 仓库配置 | `/settings/repositories` | SystemConfigRepositoriesView.vue | GitHub/GitLab/Azure DevOps仓库配置 |
| 知识库 | `/settings/knowledge` | SystemConfigKnowledgeView.vue | 文档知识源配置 |
| 技能管理 | `/settings/skill` | SystemConfigSkillView.vue | 自定义Skill管理 |
| 定时任务 | `/settings/schedules` | SystemConfigSchedulesView.vue | 定时任务调度配置 |
| DeepWiki | `/settings/deepwiki` | SystemConfigDeepWikiView.vue | 向量知识库配置 |
| 代码审查 | `/settings/review` | SystemConfigReviewView.vue | 审查规则配置 |
| 第三方集成 | `/settings/integrations` | SystemConfigIntegrationsView.vue | Azure DevOps/GitHub Copilot集成 |
| 通知设置 | `/settings/notifications` | SystemConfigNotificationsView.vue | 通知推送配置 |
| 沙箱配置 | `/settings/sandbox` | SystemConfigSandboxView.vue | 本地沙箱环境配置 |
| 工作流 | `/settings/workflow` | SystemConfigWorkflowView.vue | 工作流自动化配置 |
| API配置 | `/settings/api` | SystemConfigApiView.vue | API访问配置 |
| 模型选择 | `/settings/models` | SystemConfigModelsView.vue | AI模型配置 |
| 用户管理 | `/settings/users` | SystemConfigUsersView.vue | 用户和权限管理 |
| 系统管理 | `/settings/management` | SystemConfigManagementView.vue | 系统管理配置 |

**前端页面:**
```
client/src/modules/system-config/views/
├── SystemConfigView.vue              (主容器页面)
├── SystemConfigPreferencesView.vue   (个人偏好)
├── SystemConfigRepositoriesView.vue  (仓库配置)
├── SystemConfigKnowledgeView.vue     (知识库)
├── SystemConfigSkillView.vue         (技能管理)
├── SystemConfigSchedulesView.vue     (定时任务)
├── SystemConfigDeepWikiView.vue      (DeepWiki)
├── SystemConfigReviewView.vue        (代码审查)
├── SystemConfigIntegrationsView.vue  (第三方集成)
├── SystemConfigNotificationsView.vue (通知设置)
├── SystemConfigSandboxView.vue       (沙箱配置)
├── SystemConfigWorkflowView.vue      (工作流)
├── SystemConfigApiView.vue           (API配置)
├── SystemConfigModelsView.vue        (模型选择)
├── SystemConfigUsersView.vue         (用户管理)
└── SystemConfigManagementView.vue    (系统管理)
```

**前端Composables:**
- `useSystemConfigIntegrations.ts` - 集成配置逻辑
- `useSystemConfigKnowledge.ts` - 知识库配置逻辑
- `useSystemConfigManagement.ts` - 系统管理逻辑
- `useSystemConfigNotifications.ts` - 通知配置逻辑
- `useSystemConfigSandbox.ts` - 沙箱配置逻辑
- `useSystemConfigSkills.ts` - 技能配置逻辑
- `useSystemConfigWorkflow.ts` - 工作流配置逻辑

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

## 使用示例

```
/autocodeforge-feature-guide-skill featureName=任务中心
/autocodeforge-feature-guide-skill featureName=仓库管理 detailLevel=detail
/autocodeforge-feature-guide-skill featureName=定时任务 detailLevel=full
/autocodeforge-feature-guide-skill featureName=Agent中心
/autocodeforge-feature-guide-skill featureName=系统配置 detailLevel=detail
```

## 模糊匹配

支持功能名称的模糊匹配：
- "任务" -> AI任务中心
- "仓库" -> 仓库管理
- "定时" -> 定时任务
- "Agent" 或 "代理" -> Agent中心
- "配置" -> 系统配置
- "流水线" -> 流水线中心
- "Dashboard" 或 "工作台" -> Dashboard工作台
- "聊天" 或 "Console" -> AI聊天控制台
- "Wiki" -> Wiki系统
- "审查" -> 代码审查
- "Git" -> Git技能

## 输出示例

### Basic模式
```
功能: AI任务中心

【前端模块】
└── task-center

【后端模块】
├── TaskService
├── TaskEndpoints
└── TaskRepository
```

### Detail模式
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
├── task-center.types.ts          (类型定义)
└── routes.ts                     (路由配置)

【后端文件】
├── Endpoints/TaskEndpoints.cs    (API端点)
├── Services/TaskService.cs       (应用服务)
├── Repositories/TaskRepository.cs (数据仓储)
├── Entities/TaskEntity.cs        (任务实体)
└── DTOs/Task/*.cs                (数据传输对象)
```