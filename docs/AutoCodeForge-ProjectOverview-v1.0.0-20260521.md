# AutoCodeForge 项目总览

**版本**: v1.0.0  
**生成时间**: 2026-05-21  
**更新触发**: AI 治理初始化  

---

## 一、项目基本信息

### 1.1 项目定位
- **项目名称**: AutoCodeForge
- **项目类型**: AI 研发流水线控制台（SPA）
- **架构模式**: 前后端分离
- **当前阶段**: 7 个业务模块完成，进入第 15 轮迭代

### 1.2 核心任务
- 统一的 AI 驱动开发编排平台
- 支持多 Git 平台（GitHub/GitLab/Azure DevOps）
- 任务/定时任务/流水线 自动化执行
- 代码审查、Wiki、系统配置一体化管理

---

## 二、技术栈总结

### 2.1 前端 (client/)
| 维度 | 技术栈 | 版本 |
|------|--------|------|
| **框架** | Vue 3 + Composition API | 3.5.x |
| **构建** | Vite | 8.x |
| **状态管理** | Pinia | 3.x（setup store） |
| **路由** | Vue Router | 5.x（lazy-load + meta.requiresAuth） |
| **UI框架** | Element Plus | 2.14.x |
| **表单验证** | VeeValidate + Zod | 4.15 / 3.24 |
| **HTTP客户端** | Axios | 1.16.x（请求拦截器+认证头） |
| **图表** | ECharts + Vue-ECharts | 6.x / 8.x |
| **类型检查** | TypeScript + vue-tsc | 5.x |
| **测试** | Vitest | latest |
| **代码风格** | 模块-优先结构 | src/modules/** |

### 2.2 后端 (.NET 分层)
| 层级 | 职责 | 关键组件 |
|-----|------|---------|
| **Api 层** | HTTP 端点、请求验证、全局异常中间件 | Controllers, ApiResponse, JwtAuthMiddleware |
| **Application 层** | 业务逻辑、事务管理、服务编排 | Services, Dtos, Mappers, Validators |
| **Core 层** | 领域模型、仓储接口、业务规则 | Entities, Repositories, Enums |
| **Infrastructure 层** | 数据库、认证、外部服务集成 | DbContext, SqlSugar, LlmGateway, GitProviders |

### 2.3 数据库与消息
- **ORM**: SqlSugar（支持查询过滤、审计实体）
- **认证**: JWT Token（HS256, 配置驱动）
- **消息队列**: 内存队列（Task 异步执行）
- **后台服务**: .NET BackgroundService（定时任务、流水线状态轮询）

---

## 三、已完成模块清单（Phases 1-7, 10, 12-14）

### 3.1 基础设施（P0）
- ✅ 四层项目解决方案（Api/Application/Core/Infrastructure）
- ✅ 可复用基类与基础仓储（AuditableEntity, UserOwnedEntity, BaseRepository）
- ✅ 全局异常中间件与统一响应模型（ApiResponse）
- ✅ 公共类型 XML 注释补齐

### 3.2 数据与认证（P0）
- ✅ 16 实体数据层（User, Role, Config, Task, ScheduledTask, Repository, Pipeline 等）
- ✅ JWT 认证系统与登录 API
- ✅ 数据种子初始化（Seed）
- ✅ 管理员跨租户边界策略（scoped whitelist + 审计）

### 3.3 AI 核心（P0）
- ✅ Microsoft Agent Framework 集成
- ✅ LLM 网关（多模型支持）
- ✅ Agent/Chat 执行链路
- ✅ 服务端事件（SSE）流式响应
- ✅ 工具注册与调用机制

### 3.4 任务中心（P1）
- ✅ Task DTO/Repository/Service/Queue/Executor
- ✅ 异步执行、状态流转、任务日志
- ✅ Task 端点（CRUD、Execute、GetStatus）

### 3.5 定时任务调度（P1）
- ✅ Cronos 集成与 ScheduledTask 管理
- ✅ BackgroundService 与任务执行器
- ✅ 调度端点（CRUD、Trigger、Logs）

### 3.6 Git 仓库集成（P1）
- ✅ 多平台提供者（GitHub, GitLab, Azure DevOps）
- ✅ 凭据加密与多租户隔离
- ✅ 仓库管理端点（CRUD、Branches、PRs、Files）
- ✅ GitTools 工具集合

### 3.7 流水线模块（P2）
- ✅ Pipeline/Build 实体与完整业务层
- ✅ 30 秒轮询状态流转
- ✅ 流水线执行与日志查询
- ✅ BackgroundService 支持异步构建监听

### 3.8 测试与优化（P2）
- ✅ 回归修复与集成测试解封
- ✅ 性能基线与文档补齐
- ✅ 当前基线：75 tests passed / 0 failed

---

## 四、前端模块架构

### 4.1 模块结构（module-first）
```
src/modules/
├── auth/                 # 认证模块
│   ├── auth.api.ts
│   ├── auth.types.ts
│   ├── store/useAuthStore.ts
│   ├── routes.ts
│   └── index.ts
├── console/              # 控制台（核心功能）
│   ├── api/chat.api.ts
│   ├── store/useChatStore.ts, useConsoleStore.ts
│   ├── composables/useConsoleChat.ts, useConsoleWiki.ts
│   ├── views/ConsoleChatView.vue, ConsoleWikiView.vue ...
│   ├── routes.ts
│   └── index.ts
├── task-center/          # AI 任务中心
├── repo-management/      # 仓库管理
├── system-config/        # 系统配置
├── md-wiki/              # Wiki 模块
└── ...
```

### 4.2 命名规范
- **文件命名**: 
  - API: `<module-name>.api.ts`
  - Types: `<module-name>.types.ts`
  - Store: `use<ModulePascal>Store.ts`
  - Views: `<FeatureName>View.vue`（PascalCase）
  
- **导出规范**: 
  - 仅命名导出（无 default exports）
  - index.ts 作为公开 API 入口
  - 不导出 Views（除非明确需求）

- **路由规范**:
  - Lazy-load 所有路由
  - 每个路由包含 `meta.requiresAuth: true|false`
  - 路由名格式：`<module>.list`, `<module>.detail` 等

- **Store 规范**:
  - Pinia setup store（defineStore + setup function）
  - 必须暴露 loading, error, 和至少一个 computed 状态
  - 异步 action 包含 try/catch/finally 与状态流转

---

## 五、层级边界与关键约束

### 5.1 层级分工
```
Views/Components  ──[consume]──>  Store/Composables
     ↓
   不允许直接调用 axios，只能通过 API 层
     ↓
API Layer (chat.api.ts) ──[调用]──> axios
     ↓
[Mapper] DTO -> Model ──[转换]──> Store State
```

### 5.2 关键约束
1. **HTTP 访问**: Views 禁止直接调用 axios，仅通过 API 函数
2. **数据转换**: DTO 必须通过 mapper 转换为 Model 才能进入 store
3. **PC-First 布局**: 默认最小宽度 1280px，信息密集模块优先多列布局
4. **认证强制**: 所有非公开路由必须 `meta.requiresAuth: true`

---

## 六、后端服务清单

### 6.1 核心服务
| 服务 | 职责 | 关键方法 |
|-----|------|---------|
| **AuthService** | JWT 签发、凭据验证、Token 刷新 | IssueToken, ValidateCredentials |
| **UserService** | 用户 CRUD、角色管理、权限查询 | CreateUser, GetUserRoles, CheckAdminBoundary |
| **AgentService** | Agent 实例化、工具注册、执行链路 | RegisterTools, ExecuteAgent, StreamResponse |
| **ChatService** | Chat Session 创建、消息历史、SSE 推送 | CreateSession, AddMessage, StreamChat |
| **TaskService** | 任务创建、执行、状态轮询、日志查询 | CreateTask, ExecuteTask, GetTaskStatus |
| **ScheduledTaskService** | 定时任务 CRUD、Cron 编排、执行触发 | CreateSchedule, TriggerExecution |
| **RepositoryService** | Git 多平台集成、凭据管理、仓库操作 | ListRepositories, GetBranches, CreatePullRequest |
| **PipelineService** | 流水线 CRUD、构建触发、状态同步 | CreatePipeline, TriggerBuild, SyncBuildStatus |
| **WikiService** | Markdown 存储、版本管理、渲染 | CreatePage, GetRevisions, RenderMarkdown |
| **ConfigService** | 全局配置管理、模型选择、API 端点 | GetConfig, UpdateConfig, ValidateSettings |

### 6.2 后台服务（BackgroundService）
- **ScheduledTaskExecutor**: 30 秒轮询执行已触发的定时任务
- **PipelineSyncService**: 30 秒轮询更新流水线构建状态（GitHub Actions, GitLab CI 等）

---

## 七、API 合约概览

### 7.1 认证端点
- `POST /api/auth/login` → JWT Token
- `POST /api/auth/refresh` → New JWT
- `POST /api/auth/logout` → 清空 Token

### 7.2 核心业务端点
| 模块 | 端点示例 |
|-----|---------|
| **Agent/Chat** | POST `/api/agents/:id/execute`, GET `/api/chat/sessions/:id/messages`, SSE `/api/chat/stream` |
| **Task** | POST `/api/tasks`, GET `/api/tasks/:id`, POST `/api/tasks/:id/execute`, GET `/api/tasks/:id/status` |
| **Scheduled Task** | POST `/api/scheduled-tasks`, POST `/api/scheduled-tasks/:id/trigger` |
| **Repository** | GET `/api/repositories`, GET `/api/repositories/:id/branches`, POST `/api/repositories/:id/pull-requests` |
| **Pipeline** | POST `/api/pipelines`, POST `/api/pipelines/:id/trigger`, GET `/api/pipelines/:id/build-status` |
| **Wiki** | POST `/api/wiki/pages`, GET `/api/wiki/pages/:id`, GET `/api/wiki/pages/:id/revisions` |
| **Config** | GET `/api/config`, POST `/api/config` |

### 7.3 响应格式（统一 ApiResponse）
```json
{
  "success": true,
  "statusCode": 200,
  "data": { ... },
  "message": "Operation completed successfully",
  "timestamp": "2026-05-21T10:30:00Z"
}
```

错误响应：
```json
{
  "success": false,
  "statusCode": 400,
  "errors": { "fieldName": ["error message"] },
  "message": "Validation failed",
  "timestamp": "2026-05-21T10:30:00Z"
}
```

---

## 八、当前项目状态评估

### 8.1 优势
✅ 完整的前后端分离架构，职责边界清晰  
✅ 7 个业务模块已交付，核心流程闭环  
✅ 已建立统一的命名、编码、测试规范  
✅ 75 个单元/集成测试，质量基线稳定  
✅ 详细的 MasterPlan 与 PROJECT_SPEC 文档存档  

### 8.2 已知风险
⚠️ 前端模块数量增长快，需持续强化命名一致性  
⚠️ 后端 SqlSugar 查询过滤异常场景仍需完善  
⚠️ 生产环境密钥治理与分布式锁一致性待优化  
⚠️ Wiki 版本管理机制设计尚未落地  
⚠️ 前后端集成测试覆盖率需提升  

### 8.3 下一轮重点
- **P0**: 阶段八 Wiki 模块（Markdown 存储、渲染、版本管理）
- **P1**: 阶段九系统配置与健康检查
- **P2**: 前后端集成测试补齐与性能优化

---

## 九、治理基线建立

本报告产出的文档结构、模板、规范将作为后续开发的治理基线：
1. **文档体系**: 按标准文件夹结构落地，支持快速查询
2. **命名规范**: 强制适用于所有新增模块与功能
3. **流程规则**: PR 检查清单、分支策略、提交规范全覆盖
4. **项目 Skill**: 生成专用 skill，自动化落地规范检查与代码生成

---

## 附录：文件索引

| 文件 | 用途 |
|------|------|
| AutoCodeForge-Architecture-v1.0.0-20260521.md | 详细架构设计与数据模型 |
| AutoCodeForge-CodeOpinion-v1.0.0-20260521.md | 代码质量分析与优化建议 |
| AutoCodeForge-GovernanceInit-v1.0.0-20260521.md | 治理规则、命名、分支、PR 规范 |
| AutoCodeForge-OutputSummary-v1.0.0-20260521.md | 治理初始化最终总结与可交付清单 |

