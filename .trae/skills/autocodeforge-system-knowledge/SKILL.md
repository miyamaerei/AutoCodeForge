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