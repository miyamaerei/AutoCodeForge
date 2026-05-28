# AutoCodeForge 项目功能文档

## 1. 项目概述

### 1.1 项目简介
**AutoCodeForge** 是一个 AI 驱动的代码自动化平台，提供自然语言需求提交流程、代码生成、任务管理等核心功能。

### 1.2 技术栈

| 类别 | 技术 | 版本 |
|------|------|------|
| 框架 | Vue 3 | ^3.5.32 |
| 语言 | TypeScript | ~6.0.0 |
| 状态管理 | Pinia | ^3.0.4 |
| 路由 | Vue Router | ^5.0.7 |
| UI组件 | Element Plus | ^2.14.0 |
| HTTP客户端 | Axios | ^1.16.1 |
| 构建工具 | Vite | ^8.0.8 |
| 测试 | Vitest | ^4.1.4 |
| 图表 | ECharts | ^6.0.0 |

### 1.3 架构特点

- **模块化架构**：每个功能模块独立，便于维护和扩展
- **PC-First 设计**：优先优化桌面端体验（最小宽度 1280px）
- **Mock/Real API 切换**：支持开发阶段使用 Mock 数据
- **类型安全**：全项目使用 TypeScript

---

## 2. 功能模块

### 2.1 Console（AI开发控制台）

**概述**：统一的 AI 交互控制台，支持多种交互模式。

| 页面 | 路径 | 功能描述 |
|------|------|----------|
| 控制台入口 | `/` | 控制台主页，展示各功能模块入口 |
| 聊天（会话） | `/session` | 多轮对话模式，支持历史会话管理 |
| 聊天（提问） | `/ask` | 单次提问模式，无需会话上下文 |
| Wiki | `/wiki` | 文档知识库浏览 |
| Review | `/review` | 代码审查功能 |
| Automations | `/automations` | 自动化工作流管理 |

**核心功能**：
- ✅ 统一聊天界面
- ✅ 会话管理（创建、选择、删除）
- ✅ 消息发送与接收
- ✅ 建议问题快捷选择
- ✅ 会话预览

---

### 2.2 Task Center（AI任务中心）

**概述**：自然语言提需求、多轮对话、执行步骤、日志和 Diff 展示。

| 页面 | 路径 | 功能描述 |
|------|------|----------|
| 任务列表 | `/task-center` | 展示所有任务列表，支持状态筛选 |
| 创建任务 | `/task-center/create` | 创建新任务，填写任务信息 |
| 任务详情 | `/task-center/:id` | 查看任务详情、步骤、日志、Diff |

**核心功能**：
- ✅ 任务列表展示（ID、标题、状态、创建时间）
- ✅ 任务状态管理（运行中、已完成、已暂停、失败）
- ✅ 创建任务（标题、描述、仓库、分支）
- ✅ 任务详情查看（步骤、日志、聊天、Diff）
- ✅ 任务聊天功能

**数据模型**：

```typescript
interface TaskSummaryDto {
  id: string           // 任务ID
  title: string        // 任务标题
  state: '运行中' | '已完成' | '已暂停' | '失败'
  createdAt: string    // 创建时间
}

interface TaskDetailDto {
  id: string
  title: string
  state: '运行中' | '已完成' | '已暂停' | '失败'
  steps: TaskStepDto[] // 执行步骤
}

interface TaskCreateRequestDto {
  title: string       // 任务标题
  description: string // 任务描述
  repository: string  // 目标仓库
  branch: string      // 目标分支
}
```

---

### 2.7 Agent Center（Agent 管理）

**概述**：配置和管理 AI Agent，支持自动选择和手动引用。

| 页面 | 路径 | 功能描述 |
|------|------|----------|
| Agent 管理 | `/agent` | 管理所有 Agent 列表 |

**核心功能**：
- ✅ Agent 列表展示（名称、描述、关键词、启用状态）
- ✅ 创建/编辑/删除 Agent
- ✅ 配置 Agent 名称、图标、描述
- ✅ 配置系统提示词
- ✅ 配置自动选择关键词（支持权重）
- ✅ 启用/禁用 Agent
- ✅ 根据聊天内容自动选择最合适的 Agent

**数据模型**：

```typescript
interface AgentDto {
  id: string           // Agent ID
  name: string        // Agent 名称
  description: string // Agent 描述
  icon: string       // 头像/图标
  systemPrompt: string // 系统提示词
  keywords: AgentKeyword[] // 自动选择关键词
  enabled: boolean    // 是否启用
  createdAt: string  // 创建时间
  updatedAt: string   // 更新时间
}

interface AgentKeyword {
  keyword: string    // 关键词
  weight: number     // 权重
}
```

**自动选择机制**：
- 根据聊天消息内容与 Agent 关键词的匹配度自动选择
- 匹配度 = 关键词权重之和（仅计算匹配到的关键词）
- 只有匹配度超过阈值（0.5）才返回结果，否则返回 null

---

### 2.8 Scheduled Task（定时任务）

**概述**：配置和管理定时任务，支持 Cron 表达式、固定间隔、一次性执行。

| 页面 | 路径 | 功能描述 |
|------|------|----------|
| 定时任务 | `/scheduled-task` | 管理定时任务列表和执行记录 |

**核心功能**：
- ✅ 任务列表展示（名称、关联 Agent、仓库、触发方式、状态）
- ✅ 创建/编辑/删除定时任务
- ✅ 支持三种触发方式：
  - **Cron 表达式**：如 `0 9 * * *` 每天 9:00 执行
  - **固定间隔**：如每 6 小时、每 30 分钟
  - **一次性**：指定具体时间执行一次
- ✅ 关联仓库和分支
- ✅ 关联 Agent
- ✅ 支持任务模板快速创建
- ✅ 启用/禁用任务
- ✅ 手动立即执行
- ✅ 查看执行记录（开始时间、耗时、状态、结果）
- ✅ 统计成功率

**数据模型**：

```typescript
interface ScheduledTaskDto {
  id: string
  name: string                    // 任务名称
  description: string             // 任务描述
  templateId?: string            // 关联模板 ID
  triggerType: 'cron' | 'interval' | 'once'  // 触发类型
  cronExpression: string          // Cron 表达式
  intervalMs: number             // 间隔毫秒
  onceTime: string               // 一次性执行时间
  agentId: string                // 关联 Agent ID
  agentName: string              // 关联 Agent 名称
  repo?: TaskRepoRef             // 关联仓库
  params: string                 // 任务参数（JSON）
  status: TaskStatus             // 任务状态
  nextRunTime: string            // 下次执行时间
  lastRunTime: string            // 上次执行时间
  totalRuns: number              // 总执行次数
  successRuns: number            // 成功次数
  failedRuns: number             // 失败次数
  enabled: boolean               // 是否启用
}

interface TaskRepoRef {
  repoId: string    // 仓库 ID
  repoName: string  // 仓库名称
  repoUrl: string  // 仓库 URL
  branch: string   // 目标分支
  path?: string   // 目标路径
}

interface TaskTemplateDto {
  id: string
  name: string                    // 模板名称
  description: string             // 模板描述
  agentId: string                // 预设 Agent
  triggerType: TriggerType       // 预设触发方式
  cronExpression?: string        // 预设 Cron
  intervalMs?: number            // 预设间隔
  defaultParams: string          // 预设参数
  isBuiltIn: boolean             // 是否内置
}
```

**内置任务模板**：

| 模板名称 | 说明 | Agent | 触发方式 |
|---------|------|-------|---------|
| 代码审查任务 | 自动化代码质量检查和安全扫描 | 代码审查助手 | Cron: 每天 9:00 |
| 数据库备份任务 | 定期执行数据库全量或增量备份 | 数据库专家 | 固定间隔: 6小时 |
| 文档生成任务 | 自动生成技术文档、周报、API文档 | 文档撰写助手 | Cron: 每周五 18:00 |
| 系统监控任务 | 监控系统性能指标，异常时告警 | 架构设计专家 | 固定间隔: 1小时 |
| 代码重构任务 | 对指定代码进行重构优化 | 前端开发助手 | 一次性 |

---

### 2.3 Repo Management（仓库管理）

**概述**：仓库列表、分支、文件树与 PR 视图。

| 页面 | 路径 | 功能描述 |
|------|------|----------|
| 仓库列表 | `/repo-management` | 展示所有仓库列表 |
| 分支管理 | `/repo-management/branches` | 查看和管理分支 |
| PR管理 | `/repo-management/prs` | 查看和管理 Pull Request |

**核心功能**：
- ✅ 仓库列表展示
- ✅ 分支列表展示
- ✅ Pull Request 管理

**数据模型**：

```typescript
interface RepositoryDto {
  id: string
  name: string
  description: string
  branchCount: number
  prCount: number
}

interface BranchDto {
  id: string
  name: string
  lastCommit: string
  commitDate: string
}

interface PullRequestDto {
  id: string
  title: string
  state: 'open' | 'closed' | 'merged'
  author: string
  createdAt: string
}
```

---

### 2.4 Pipeline Center（流水线中心）

**概述**：构建状态、步骤状态、日志查看（基础版）。

| 页面 | 路径 | 功能描述 |
|------|------|----------|
| 流水线列表 | `/pipeline-center` | 展示所有流水线 |
| 构建状态 | `/pipeline-center/builds` | 查看构建历史和状态 |

**核心功能**：
- ✅ 流水线列表展示
- ✅ 构建状态查看
- ✅ 构建日志查看

**数据模型**：

```typescript
interface PipelineDto {
  id: string
  name: string
  status: 'running' | 'success' | 'failed' | 'pending'
  lastBuildTime: string
}

interface BuildDto {
  id: string
  pipelineId: string
  status: 'running' | 'success' | 'failed'
  startTime: string
  duration: string
}
```

---

### 2.5 Dashboard（工作台）

**概述**：今日任务数、成功率、最近任务与告警。

| 页面 | 路径 | 功能描述 |
|------|------|----------|
| 工作台 | `/dashboard` | 数据统计与可视化展示 |

**核心功能**：
- ✅ 今日任务数统计
- ✅ 任务成功率统计
- ✅ 执行中任务数
- ✅ 告警提示数
- ✅ 任务趋势图表
- ✅ 系统健康状态

**统计指标**：

| 指标 | 说明 | 示例值 |
|------|------|--------|
| 今日任务 | 当日创建的任务数量 | 23 |
| 成功率 | 任务成功完成的比例 | 92.4% |
| 执行中 | 当前正在运行的任务数 | 5 |
| 告警提示 | 需要关注的异常数量 | 2 |

---

### 2.6 System Config（系统配置）

**概述**：API Key、模型选择、用户管理等系统配置。

| 页面 | 路径 | 功能描述 |
|------|------|----------|
| Preferences | `/settings/preferences` | 个人偏好设置 |
| Repositories | `/settings/repositories` | 仓库配置 |
| Knowledge | `/settings/knowledge` | 知识库管理 |
| Skill | `/settings/skill` | Skill 管理 |
| Schedules | `/settings/schedules` | 定时任务配置 |
| DeepWiki | `/settings/deepwiki` | 深度 Wiki 配置 |
| Review | `/settings/review` | 审查配置 |
| Integrations | `/settings/integrations` | 集成配置 |
| Notifications | `/settings/notifications` | 通知配置 |
| Sandbox | `/settings/sandbox` | 沙箱环境配置 |
| Workflow | `/settings/workflow` | 工作流配置 |
| API配置 | `/system-config/api` | API 相关配置 |
| 模型选择 | `/system-config/models` | AI 模型选择 |
| 用户管理 | `/system-config/users` | 用户权限管理 |

**核心功能**：
- ✅ 系统参数配置
- ✅ 第三方集成管理
- ✅ 通知设置
- ✅ AI 模型配置
- ✅ 用户管理

---

## 3. 核心功能流程

### 3.1 任务创建流程

```
用户 → 创建任务页面 → 填写任务信息 → 提交 → 任务列表
         ↓                    ↓
      表单验证           创建成功提示
```

**步骤说明**：
1. 用户访问 `/task-center/create`
2. 填写任务标题、描述、仓库、分支
3. 提交表单
4. 系统创建任务并返回成功消息
5. 自动跳转到任务列表或任务详情

### 3.2 聊天交互流程

```
用户输入 → 发送消息 → 等待响应 → 显示回复
            ↓              ↓
         清空输入        模拟延迟
```

**交互模式**：
- **Ask 模式**：单次提问，无会话上下文
- **Session 模式**：多轮对话，保持会话历史

### 3.3 数据流转架构

```
┌─────────────────────────────────────────────────────────────────┐
│                      UI Layer (Views/Components)               │
│  DashboardView  │  TaskCenterListView  │  ConsoleChatView      │
└─────────────────┼──────────────────────┼──────────────────────┘
                  ↓                      ↓
┌─────────────────────────────────────────────────────────────────┐
│                    State Management (Pinia Stores)              │
│  useTaskCenterStore  │  useConsoleStore  │  useRepoManagementStore│
└─────────────────┼──────────────────────┼──────────────────────┘
                  ↓                      ↓
┌─────────────────────────────────────────────────────────────────┐
│                        API Layer                                │
│  task-center.api.ts  │  repo-management.api.ts  │  mock/*.ts   │
│       ↓                       ↓                      ↓         │
│  USE_MOCK=true?           USE_MOCK=true?        Mock数据       │
│       ↓                       ↓                                │
│    Mock API              Mock API                              │
│       ↓                       ↓                                │
│  USE_MOCK=false?        USE_MOCK=false?                        │
│       ↓                       ↓                                │
│  axios request         axios request                            │
│       ↓                       ↓                                │
│  REST API             REST API                                  │
└─────────────────────────────────────────────────────────────────┘
```

---

## 4. 关键技术实现

### 4.1 Mock/Real API 切换

```typescript
// src/config/runtime.ts
export const USE_MOCK = true  // 切换为 false 使用真实 API

// src/modules/task-center/task-center.api.ts
export async function fetchTaskSummaries(): Promise<TaskSummaryDto[]> {
  if (USE_MOCK) {
    return getTaskSummariesMock()  // Mock 数据
  }
  const { data } = await request.get<TaskSummaryDto[]>('/task-center/tasks')
  return data
}
```

### 4.2 Pinia Setup Store 模式

```typescript
export const useTaskCenterStore = defineStore('module.task-center', () => {
  const tasks = ref<TaskSummaryDto[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)

  async function loadTasks(): Promise<void> {
    loading.value = true
    try {
      tasks.value = await fetchTaskSummaries()
    } catch (err) {
      error.value = err instanceof Error ? err.message : '加载失败'
    } finally {
      loading.value = false
    }
  }

  return { tasks, loading, error, loadTasks }
})
```

### 4.3 路由配置约定

```typescript
export const taskCenterRoutes: RouteRecordRaw[] = [
  {
    path: '/task-center',
    name: 'task-center.list',
    component: () => import('./views/TaskCenterListView.vue'),
    meta: {
      requiresAuth: false,  // 必须明确设置
      title: 'AI任务中心',   // 页面标题
    },
  },
]
```

---

## 5. 模块入口配置

### 5.1 模块导出

每个模块通过 `index.ts` 统一导出：

```typescript
// src/modules/task-center/index.ts
export { taskCenterRoutes } from './routes'
export {
  fetchTaskDetail,
  fetchTaskSummaries,
  createTask,
  type TaskCreateRequestDto,
  type TaskDetailDto,
  type TaskSummaryDto,
} from './task-center.api'
export { useTaskCenterStore } from './store/useTaskCenterStore'
```

### 5.2 路由注册

所有模块路由在主路由文件中统一注册：

```typescript
// src/router/index.ts
import { consoleRoutes } from '../modules/console'
import { taskCenterRoutes } from '../modules/task-center'
import { agentCenterRoutes } from '../modules/agent-center'
import { scheduledTaskRoutes } from '../modules/scheduled-task'
import { repoManagementRoutes } from '../modules/repo-management'
import { pipelineCenterRoutes } from '../modules/pipeline-center'
import { dashboardRoutes } from '../modules/dashboard'
import { systemConfigRoutes } from '../modules/system-config'

const routes: RouteRecordRaw[] = [
  ...consoleRoutes,
  ...taskCenterRoutes,
  ...agentCenterRoutes,
  ...scheduledTaskRoutes,
  ...repoManagementRoutes,
  ...pipelineCenterRoutes,
  ...dashboardRoutes,
  ...systemConfigRoutes,
]
```

---

## 6. 开发指南

### 6.1 快速开始

```bash
# 安装依赖
npm install

# 启动开发服务器
npm run dev

# 类型检查
npm run type-check

# 运行测试
npm run test:unit

# 构建生产版本
npm run build
```

### 6.2 环境变量

| 变量名 | 说明 | 默认值 |
|--------|------|--------|
| `VITE_API_BASE_URL` | API 基础地址 | `/api` |

### 6.3 目录结构

```
src/
├── assets/          # 静态资源
├── components/      # 共享组件
├── config/          # 配置文件
├── lib/             # 工具库
├── mock/            # Mock 数据
├── modules/         # 功能模块
│   ├── console/     # AI控制台
│   ├── task-center/ # 任务中心
│   ├── agent-center/ # Agent管理
│   ├── scheduled-task/ # 定时任务
│   ├── repo-management/ # 仓库管理
│   ├── pipeline-center/ # 流水线中心
│   ├── dashboard/   # 工作台
│   └── system-config/ # 系统配置
├── router/          # 路由配置
├── stores/          # 全局 Store
└── views/           # 全局视图
```

---

## 7. 功能矩阵

| 模块 | 状态 | 核心功能 | 完成度 |
|------|------|----------|--------|
| Console | ✅ | AI聊天、会话管理 | MVP |
| Task Center | ✅ | 任务CRUD、日志、聊天 | 核心 |
| Agent Center | ✅ | Agent配置、自动选择 | MVP |
| Scheduled Task | ✅ | 定时任务、模板、执行记录 | MVP |
| Repo Management | ✅ | 仓库、分支、PR管理 | MVP |
| Pipeline Center | ✅ | 流水线、构建状态 | 基础版 |
| Dashboard | ✅ | 统计、图表 | MVP |
| System Config | ✅ | 系统配置、用户管理 | 基础版 |

---

## 8. 未来规划

### 8.1 功能扩展

| 功能 | 描述 | 优先级 |
|------|------|--------|
| 实时通知 | 任务状态变化推送 | 高 |
| 团队协作 | 多用户协作功能 | 中 |
| 代码审查 | 集成代码审查工具 | 中 |
| 自动化部署 | CI/CD 流水线集成 | 中 |

### 8.2 技术优化

| 优化项 | 描述 |
|--------|------|
| 性能优化 | 懒加载、缓存策略 |
| 国际化 | 多语言支持 |
| 主题切换 | 深色/浅色模式 |
| 移动端适配 | 响应式优化 |

---

**文档版本**: v1.0  
**生成日期**: 2026-05-19  
**项目**: AutoCodeForge