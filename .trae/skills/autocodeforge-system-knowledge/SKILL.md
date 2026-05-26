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

AutoCodeForge 是一个 **AI 研发流水线控制台**（SPA），采用前后端分离架构，核心定位是：
- 统一的 AI 驱动开发编排平台
- 支持多 Git 平台（GitHub/GitLab/Azure DevOps）
- 任务/定时任务/流水线自动化执行
- 代码审查、Wiki、系统配置一体化管理

### 2. 技术栈

**前端 (client/)**:
| 维度 | 技术栈 | 版本 |
|------|--------|------|
| 框架 | Vue 3 + Composition API | 3.5.x |
| 构建 | Vite | 8.x |
| 状态管理 | Pinia | 3.x |
| 路由 | Vue Router | 5.x |
| UI框架 | Element Plus | 2.14.x |
| 表单验证 | VeeValidate + Zod | 4.15 / 3.24 |
| HTTP客户端 | Axios | 1.16.x |
| 测试 | Vitest | latest |

**后端 (.NET 分层)**:
| 层级 | 职责 | 关键组件 |
|-----|------|---------|
| Api层 | HTTP端点、请求验证 | Controllers, ApiResponse, JwtAuthMiddleware |
| Application层 | 业务逻辑、事务管理 | Services, Dtos, Mappers, Validators |
| Core层 | 领域模型、仓储接口 | Entities, Repositories, Enums |
| Infrastructure层 | 数据库、外部服务 | DbContext, SqlSugar, LlmGateway, GitProviders |

### 3. 功能模块

| 模块 | 描述 | 状态 |
|-----|------|------|
| 基础设施 | 四层项目解决方案、全局异常中间件、统一响应模型 | ✅ 完成 |
| 数据与认证 | 16实体数据层、JWT认证、数据种子初始化 | ✅ 完成 |
| AI核心 | Microsoft Agent Framework集成、LLM网关、工具注册 | ✅ 完成 |
| 任务中心 | 任务CRUD、异步执行、状态流转、任务日志 | ✅ 完成 |
| 定时任务调度 | Cronos集成、BackgroundService、调度端点 | ✅ 完成 |
| Git仓库集成 | 多平台提供者、凭据加密、仓库管理端点 | ✅ 完成 |
| 流水线模块 | Pipeline/Build实体、30秒轮询状态流转 | ✅ 完成 |
| Workflow模块 | Agent工作流可视化编排、Kanban任务看板、状态管理 | ✅ 完成 |
| Wiki模块 | Markdown存储、版本管理、渲染 | ⏳ 进行中 |

### 4. 核心服务

| 服务 | 职责 | 关键方法 |
|-----|------|---------|
| AuthService | JWT签发、凭据验证 | IssueToken, ValidateCredentials |
| UserService | 用户CRUD、角色管理 | CreateUser, GetUserRoles |
| AgentService | Agent实例化、工具注册、执行链路 | RegisterTools, ExecuteAgent |
| ChatService | Chat Session创建、消息历史、SSE推送 | CreateSession, StreamChat |
| TaskService | 任务创建、执行、状态轮询 | CreateTask, ExecuteTask |
| RepositoryService | Git多平台集成、仓库操作 | ListRepositories, GetBranches |
| PipelineService | 流水线CRUD、构建触发 | CreatePipeline, TriggerBuild |
| WorkflowService | 工作流编排、执行控制、状态同步 | ExecuteWorkflow, PauseWorkflow, ResumeWorkflow |
| WikiService | Markdown存储、版本管理 | CreatePage, GetRevisions |
| ConfigService | 全局配置管理 | GetConfig, UpdateConfig |

### 5. API端点

**认证端点**:
- `POST /api/auth/login` → JWT Token
- `POST /api/auth/refresh` → 刷新Token
- `POST /api/auth/logout` → 退出登录

**核心业务端点**:
| 模块 | 端点示例 |
|-----|---------|
| Agent/Chat | POST `/api/agents/:id/execute`, SSE `/api/chat/stream` |
| Task | POST `/api/tasks`, GET `/api/tasks/:id/status` |
| Scheduled Task | POST `/api/scheduled-tasks`, POST `/api/scheduled-tasks/:id/trigger` |
| Repository | GET `/api/repositories`, GET `/api/repositories/:id/branches` |
| Pipeline | POST `/api/pipelines`, POST `/api/pipelines/:id/trigger` |
| Workflow | POST `/api/workflows`, POST `/api/workflows/:id/execute`, SSE `/api/workflows/instances/:id/events` |
| Wiki | POST `/api/wiki/pages`, GET `/api/wiki/pages/:id` |
| Config | GET `/api/config`, POST `/api/config` |

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
│   │   │   └── workflow/   # Workflow模块（工作流编排、Kanban看板）
│   │   ├── router/         # 路由配置
│   │   ├── stores/         # 全局 Store
│   │   └── mock/           # Mock 数据
│   └── public/             # 静态资源
├── server/                 # .NET 后端服务
├── docs/                   # 项目文档
└── .autoCodeForge/         # 治理配置与模板
```

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
- 统一的 AI 驱动开发编排平台
- 支持多 Git 平台（GitHub/GitLab/Azure DevOps）
- 任务/定时任务/流水线自动化执行
- 代码审查、Wiki、系统配置一体化管理
```

### 示例2: 查询技术栈

输入: `系统采用什么技术栈？`

输出:
```
【前端技术栈】
- 框架: Vue 3 + Composition API (3.5.x)
- 构建: Vite 8.x
- 状态管理: Pinia 3.x
- 路由: Vue Router 5.x
- UI框架: Element Plus 2.14.x
- HTTP客户端: Axios 1.16.x
- 测试: Vitest

【后端技术栈】
- 框架: .NET 分层架构
- ORM: SqlSugar
- 认证: JWT Token
- 消息队列: 内存队列
- 后台服务: .NET BackgroundService
```

### 示例3: 查询功能模块

输入: `系统有哪些功能模块？`

输出:
```
已完成的功能模块:

✅ 基础设施 - 四层项目解决方案、全局异常中间件
✅ 数据与认证 - 16实体数据层、JWT认证系统
✅ AI核心 - Microsoft Agent Framework集成、LLM网关
✅ 任务中心 - 任务CRUD、异步执行、状态流转
✅ 定时任务调度 - Cronos集成、后台轮询执行
✅ Git仓库集成 - 多平台支持(GitHub/GitLab/Azure)
✅ 流水线模块 - Pipeline管理、构建触发、状态同步

⏳ Wiki模块 - Markdown存储、版本管理(进行中)
```

### 示例4: 查询API端点

输入: `认证相关的API有哪些？`

输出:
```
【认证端点】

- POST /api/auth/login     → 获取JWT Token
- POST /api/auth/refresh   → 刷新Token
- POST /api/auth/logout    → 退出登录

响应格式:
{
  "success": true,
  "statusCode": 200,
  "data": { ... },
  "message": "Operation completed successfully",
  "timestamp": "2026-05-21T10:30:00Z"
}
```

### 示例5: 查询Workflow模块

输入: `Workflow 模块有哪些功能？`

输出:
```
【Workflow模块功能】

✅ 工作流可视化编排 - 基于 Vue Flow 的节点拖拽设计器
✅ Kanban任务看板 - 支持任务状态流转（todo/in_progress/review/done）
✅ Agent监控 - 显示 Agent 状态（idle/busy/offline）和任务分配
✅ 工作流执行控制 - 支持启动、暂停、恢复、终止操作
✅ 实时状态同步 - SSE 事件订阅，实时更新工作流进度

【路由配置】
- /workflow        → 工作流中心
- /workflow/designer → 工作流设计器
- /workflow/kanban   → 任务看板
- /workflow/agents   → Agent监控

【核心类型】
- Task: 任务（标题、状态、优先级、分配的Agent）
- Agent: AI执行者（角色: worker/manager/secretary）
- WorkflowNode: 工作流节点（agent/task/condition/start/end）
- WorkflowInstance: 工作流实例（运行状态、当前节点、进度）
```

### 示例6: 查询前端画板设计

输入: `前端工作流画板如何设计？`

输出:
```
【WorkflowCanvas 前端画板设计】

基于 Vue Flow 实现可视化工作流设计器，核心特性：

【节点类型】
├── Start      → 工作流起点（绿色）
├── End        → 工作流终点（红色）
├── Agent      → AI代理节点（紫色，支持角色配置）
├── Task       → 任务节点（蓝色，显示状态）
└── Condition  → 条件分支节点（橙色，支持是/否分支）

【交互功能】
- 拖拽添加节点：从工具栏拖拽节点到画布
- 节点连接：点击节点端口创建连线
- 节点配置：点击节点显示右侧配置面板
- 节点删除：支持删除节点及关联边

【执行控制】
- 保存：将设计的工作流持久化到后端
- 加载：从后端获取工作流并渲染到画布
- 执行：启动工作流实例，实时显示执行状态
- 暂停/恢复：支持工作流暂停和继续
- 终止：强制终止正在执行的工作流

【实时状态展示】
- 执行中节点高亮动画
- 底部执行指示器显示进度
- 执行完成后弹出结果弹窗

【项目文件结构】
client/src/modules/workflow/
├── types/workflow.ts      # 类型定义
├── api/workflow.ts        # API封装
├── store/useWorkflowStore.ts  # Pinia状态管理
└── components/WorkflowCanvas.vue  # 画布组件
```

### 示例7: 查询Workflow API对接

输入: `Workflow模块有哪些API方法？`

输出:
```
【Workflow API 对接方法】

【工作流管理】
├── getWorkflows()           → 获取工作流列表
├── getWorkflow(id)          → 获取单个工作流
├── createWorkflow(data)     → 创建工作流
├── updateWorkflow(id, data) → 更新工作流
└── deleteWorkflow(id)       → 删除工作流

【实例管理】
├── getWorkflowInstances(workflowId) → 获取工作流实例列表
├── getWorkflowInstance(instanceId)  → 获取单个实例详情
└── deleteWorkflowInstance(instanceId) → 删除实例

【执行控制】
├── executeWorkflow(workflowId, input) → 执行工作流
├── pauseWorkflow(instanceId)          → 暂停工作流
├── resumeWorkflow(instanceId)         → 恢复工作流
└── terminateWorkflow(instanceId)      → 终止工作流

【事件订阅】
└── subscribeToEvents(instanceId, callback) → SSE订阅实时事件

【后端端点】
POST   /api/workflows                    → 创建工作流
GET    /api/workflows                    → 获取工作流列表
GET    /api/workflows/:id                → 获取单个工作流
PUT    /api/workflows/:id                → 更新工作流
DELETE /api/workflows/:id                → 删除工作流
POST   /api/workflows/:id/execute        → 执行工作流
GET    /api/workflows/instances          → 获取实例列表
GET    /api/workflows/instances/:id      → 获取实例详情
POST   /api/workflows/instances/:id/pause    → 暂停
POST   /api/workflows/instances/:id/resume   → 恢复
POST   /api/workflows/instances/:id/terminate → 终止
SSE    /api/workflows/instances/:id/events   → 事件流
```