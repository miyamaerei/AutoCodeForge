---
name: autocodeforge-project
description: "AutoCodeForge 项目专用开发 Skill。支持：模块创建、代码生成、规范检查、API 集成、测试补齐。使用本 Skill 后，无需调用其他通用开发 Skill (auto-developer、vue3-page-builder、fe-be-integration)。"
argument-hint: "提供要执行的功能 (create-module, add-page, add-api, check-spec, etc.) 和具体需求描述。"
---

# AutoCodeForge 项目专用开发 Skill

## 概述

本 Skill 是 AutoCodeForge 项目的**统一开发工具**，集成了所有常见的开发操作，包括：
- 前端模块创建与页面生成
- 后端实体、服务、API 端点生成
- 规范合规性检查
- 前后端 API 集成
- 单元测试补齐

**使用此 Skill 后，无需调用**:
- ❌ auto-developer
- ❌ vue3-page-builder  
- ❌ fe-be-integration
- ❌ vue3-api-model-router-store

所有功能都在此 Skill 中提供。

---

## 使用场景与触发词

### 场景 1: 创建新前端模块
**触发词**: `创建模块`, `新增模块`, `搭建模块`, `add module`

**示例**:
```
/autocodeforge-project 创建 wiki-editor 模块，包含 create/edit/preview 三个页面
/autocodeforge-project 新增 dashboard-analytics 模块，需要集成 ECharts 图表
```

**产出物**:
- ✅ 模块文件夹结构 (api.ts, types.ts, store, routes.ts, views/)
- ✅ 命名符合规范的 Vue 组件
- ✅ Pinia store (setup 语法)
- ✅ API 层代码框架
- ✅ 路由配置 (meta.requiresAuth)
- ✅ 单元测试框架

### 场景 2: 新增后端实体、服务、API
**触发词**: `添加实体`, `新增服务`, `创建端点`, `add entity`

**示例**:
```
/autocodeforge-project 新增 Issue 实体及管理 API，包含 CRUD 和状态流转逻辑
/autocodeforge-project 创建 EmailNotification 服务，支持异步发送和重试
```

**产出物**:
- ✅ Core 层 Entity (继承 UserOwnedEntity)
- ✅ Application 层 Service & DTO
- ✅ Repository 与数据库映射
- ✅ API Controller & 端点
- ✅ 单元测试 (xUnit + Moq)
- ✅ Swagger 文档

### 场景 3: 补齐规范与测试
**触发词**: `补齐测试`, `完善规范`, `修复规范`, `fix compliance`

**示例**:
```
/autocodeforge-project 检查 console 模块规范，补齐缺失的注释和单元测试
/autocodeforge-project 修复 HTTP 拦截器，完善错误处理和 Token 刷新
```

**产出物**:
- ✅ 规范检查报告 (违规项、改进建议)
- ✅ 自动生成的规范修复代码
- ✅ 补齐的单元测试
- ✅ XML 文档注释

### 场景 4: 前后端 API 集成
**触发词**: `集成 API`, `连接后端`, `绑定接口`, `wire backend`

**示例**:
```
/autocodeforge-project 将 task-center 模块与后端 Task API 集成
/autocodeforge-project 为 console-chat 组件集成后端 SSE 流式响应
```

**产出物**:
- ✅ 前端 API 调用层 (types & api.ts)
- ✅ DTO ↔ Model mapper
- ✅ Store action 与错误处理
- ✅ 端到端集成测试框架

### 场景 5: 路由与权限配置
**触发词**: `配置路由`, `设置权限`, `添加路由`, `add route`

**示例**:
```
/autocodeforge-project 为 task-center 模块配置路由，所有页面都需要认证
/autocodeforge-project 为管理员页面设置权限检查
```

**产出物**:
- ✅ 路由配置 (lazy-load + meta.requiresAuth)
- ✅ 权限守卫代码
- ✅ 路由中间件

---

## 工作流程 (Workflow)

### 执行流程
```
用户输入需求
    ↓
解析意图 (create-module / add-api / check-spec 等)
    ↓
收集必要参数
    ↓
代码生成与框架搭建
    ↓
规范检查 (lint + compliance)
    ↓
自动化测试框架创建
    ↓
输出文件清单 & 下一步指导
```

### 质量门禁
所有输出都需通过：
1. ✅ 代码格式 (ESLint / SonarQube)
2. ✅ 命名规范 (Governance 3.1/3.2)
3. ✅ 组件复用检查 (Governance 2.1/2.2)
4. ✅ 类型安全 (TypeScript strict)
5. ✅ 异常处理完整性

---

## 具体功能清单

### 前端功能

#### 1. create-module
创建新的前端模块（module-first 结构）

**调用**:
```
/autocodeforge-project 创建模块: <module-name> [描述]
```

**参数**:
- `module-name`: kebab-case 模块名 (e.g., task-center, wiki-editor)
- `description`: 模块功能描述

**自动生成**:
```
src/modules/<module-name>/
├── [module-name].api.ts          # HTTP 调用层
├── [module-name].types.ts        # DTO 与 Model 类型
├── store/
│   └── use<ModulePascal>Store.ts # Pinia setup store
├── routes.ts                      # 路由定义
├── index.ts                       # 公开 API 入口
├── views/
│   ├── [Module]ListView.vue       # 列表页面
│   ├── [Module]DetailView.vue     # 详情页面
│   └── [Module]CreateView.vue     # 创建页面
└── __tests__/
    ├── store.test.ts
    ├── api.test.ts
    └── integration.test.ts
```

#### 2. add-page
在现有模块中添加新页面

**调用**:
```
/autocodeforge-project 添加页面: <module-name> <page-type> [名称]
```

**参数**:
- `module-name`: 模块名
- `page-type`: list / detail / create / custom
- `name`: 可选，自定义页面名称

**示例**:
```
/autocodeforge-project 添加页面: task-center list 我的任务
/autocodeforge-project 添加页面: console custom ai-评估
```

#### 3. add-store-action
为 Store 添加新的异步 action

**调用**:
```
/autocodeforge-project 添加 store action: <module-name> <action-name> <description>
```

**自动补齐**:
- ✅ async 函数框架
- ✅ try/catch/finally 错误处理
- ✅ loading/error 状态管理
- ✅ TypeScript 类型注解
- ✅ 单元测试

#### 4. integrate-api
将前端模块与后端 API 集成

**调用**:
```
/autocodeforge-project 集成 API: <module-name> <backend-endpoint>
```

**示例**:
```
/autocodeforge-project 集成 API: task-center /api/tasks
```

**自动产出**:
- ✅ API types 对齐后端响应格式
- ✅ Mapper 函数 (DTO → Model)
- ✅ Store action 调用 API
- ✅ 错误处理与加载状态
- ✅ Mock 数据（开发调试用）
- ✅ 集成测试框架

### 后端功能

#### 5. create-entity
创建新的后端数据实体

**调用**:
```
/autocodeforge-project 创建实体: <entity-name> [字段列表]
```

**示例**:
```
/autocodeforge-project 创建实体: WikiPage id:string, title:string, content:string, isPublished:bool
```

**自动生成**:
```
AutoCodeForge.Core/Entities/
└── WikiPage.cs

AutoCodeForge.Application/Dtos/
├── CreateWikiPageDto.cs
├── UpdateWikiPageDto.cs
├── WikiPageResponseDto.cs
└── WikiPageMapper.cs

AutoCodeForge.Application/Validators/
├── CreateWikiPageValidator.cs
└── UpdateWikiPageValidator.cs
```

**特性**:
- ✅ 自动继承 UserOwnedEntity (多租户隔离)
- ✅ 自动 XML 文档注释
- ✅ DTO 与 mapper 自动对齐
- ✅ FluentValidation 验证器
- ✅ 数据库映射配置

#### 6. create-service
创建新的后端业务服务

**调用**:
```
/autocodeforge-project 创建服务: <service-name> <entity-name>
```

**示例**:
```
/autocodeforge-project 创建服务: WikiService WikiPage
```

**自动生成**:
```
AutoCodeForge.Application/Services/
├── IWikiService.cs (接口)
└── WikiService.cs  (实现)

// 包含标准 CRUD 方法:
// - GetWikiPageAsync(id)
// - GetWikiPagesAsync(filter)
// - CreateWikiPageAsync(dto)
// - UpdateWikiPageAsync(id, dto)
// - DeleteWikiPageAsync(id)
```

**特性**:
- ✅ 依赖注入完整
- ✅ 异步操作 (async/await)
- ✅ 异常处理 (try/catch/finally)
- ✅ 事务管理 (SaveChangesAsync)
- ✅ XML 文档注释
- ✅ 单元测试框架 (xUnit + Moq)

#### 7. create-api-endpoint
创建新的 HTTP 端点

**调用**:
```
/autocodeforge-project 创建端点: <entity-name> [methods]
```

**示例**:
```
/autocodeforge-project 创建端点: WikiPage GET,POST,PUT,DELETE
```

**自动生成**:
```
AutoCodeForge.Api/Controllers/
└── WikiPageController.cs

// 包含标准 RESTful 端点:
// GET /api/wiki-pages - 列表
// GET /api/wiki-pages/{id} - 详情
// POST /api/wiki-pages - 创建
// PUT /api/wiki-pages/{id} - 编辑
// DELETE /api/wiki-pages/{id} - 删除
```

**特性**:
- ✅ ApiResponse 统一响应格式
- ✅ 参数验证 (ModelState)
- ✅ 异常处理
- ✅ Swagger 文档
- ✅ 权限检查 (authorize attribute)

#### 8. add-test
为服务或 API 补齐单元测试

**调用**:
```
/autocodeforge-project 补齐测试: <module-name> <class-name>
```

**示例**:
```
/autocodeforge-project 补齐测试: wiki WikiService
```

**自动生成**:
- ✅ TestBase 与 Mock 工厂
- ✅ Happy path 测试
- ✅ 异常场景测试
- ✅ 集成测试框架
- ✅ 70+ 行覆盖度

---

## 规范检查与合规性

### 前端规范检查 (check-frontend-spec)
**调用**:
```
/autocodeforge-project 检查前端规范: [module-name]
```

**检查项**:
- ✅ 文件命名 (kebab-case for folders, PascalCase for components)
- ✅ index.ts 入口 (是否只导出公开 API)
- ✅ 路由 meta.requiresAuth (每个路由是否定义)
- ✅ Store 结构 (是否有 loading/error state)
- ✅ API 层隔离 (是否直接调用 axios)
- ✅ 类型完整性 (是否有 any 类型)
- ✅ JSDoc 注释 (公开函数是否有文档)
- ✅ 异常处理 (API 调用是否有 try/catch)

**输出**:
- 📄 规范检查报告 (PDF/Markdown)
- 🔴 高风险违规项 (一票否决清单)
- 🟡 中风险改进项 (建议修改)
- 🟢 通过项统计

### 后端规范检查 (check-backend-spec)
**调用**:
```
/autocodeforge-project 检查后端规范: [namespace]
```

**检查项**:
- ✅ 命名规范 (PascalCase for class, camelCase for variable)
- ✅ 基类继承 (是否复用 AuditableEntity / UserOwnedEntity)
- ✅ 仓储接口 (是否通过 IRepository<T>)
- ✅ XML 文档注释 (public 成员是否完整)
- ✅ 异常处理 (是否有 catch 块)
- ✅ async/await 规范 (是否有 .Result / .Wait())
- ✅ 验证器 (DTO 是否有 FluentValidation)
- ✅ 依赖注入 (是否注册接口)

**输出**:
- 📄 规范检查报告
- 🔴 高风险违规 (禁止通过)
- 🟡 中风险项 (打回整改)
- 🟢 合规统计

### 整体规范检查 (check-governance)
**调用**:
```
/autocodeforge-project 检查整体规范
```

**检查范围**:
- 前端 + 后端全覆盖
- 组件复用合规性 (Governance 2.1/2.2)
- 编码规范 (Governance 3.1/3.2)
- Git 提交规范 (Governance 4)
- 文档完整性 (Architecture / Module Design)

---

## API 合约对齐

### 自动生成 API 合约 (generate-api-contract)
**调用**:
```
/autocodeforge-project 生成 API 合约: <endpoint> <method>
```

**产出**:
- ✅ 后端 OpenAPI Spec (JSON)
- ✅ 前端 TypeScript 类型 (自动从 OpenAPI 生成)
- ✅ 前端 API 函数框架
- ✅ Mock 数据

**示例**:
```json
// 后端 OpenAPI Spec
{
  "/api/tasks": {
    "post": {
      "requestBody": { "TaskCreateDto": {...} },
      "responses": { "200": { "TaskResponseDto": {...} } }
    }
  }
}

// 自动生成前端 types.ts
export interface TaskCreateDto { ... }
export interface TaskResponseDto { ... }

// 自动生成前端 api.ts
export async function createTask(dto: TaskCreateDto): Promise<TaskResponseDto> { ... }
```

---

## 常见场景速查

| 场景 | 命令 | 产出 |
|------|------|------|
| 创建新模块 | `/autocodeforge-project 创建模块: xxx` | 完整模块结构 |
| 新增页面 | `/autocodeforge-project 添加页面: xxx list` | Vue 组件 + 路由 |
| 新增实体 | `/autocodeforge-project 创建实体: xxx` | Entity + Service + API |
| 集成后端 API | `/autocodeforge-project 集成 API: xxx /api/xxx` | Types + API + Store |
| 补齐测试 | `/autocodeforge-project 补齐测试: xxx YyyService` | 单元测试框架 |
| 检查规范 | `/autocodeforge-project 检查规范: xxx` | 合规性报告 |
| 修复规范 | `/autocodeforge-project 修复规范: xxx` | 自动修复代码 |

---

## 输出示例

### 示例 1: 创建模块
```
/autocodeforge-project 创建模块: issue-tracker 问题追踪系统
```

**产出清单**:
```
✅ 文件创建:
  - src/modules/issue-tracker/issue-tracker.api.ts (154 lines)
  - src/modules/issue-tracker/issue-tracker.types.ts (68 lines)
  - src/modules/issue-tracker/store/useIssueTrackerStore.ts (124 lines)
  - src/modules/issue-tracker/routes.ts (42 lines)
  - src/modules/issue-tracker/index.ts (8 lines)
  - src/modules/issue-tracker/views/IssueTrackerListView.vue (89 lines)
  - src/modules/issue-tracker/views/IssueTrackerDetailView.vue (105 lines)
  - src/modules/issue-tracker/views/IssueTrackerCreateView.vue (98 lines)
  - src/modules/issue-tracker/__tests__/store.test.ts (64 lines)
  - src/modules/issue-tracker/__tests__/api.test.ts (58 lines)

✅ 规范检查: PASSED (8/8 检查项)
✅ 编译检查: PASSED (无 TypeScript 错误)
✅ ESLint 检查: PASSED (无风格错误)

📋 下一步:
  1. 在 src/router/index.ts 中导入 issueTrackerRoutes
  2. 在 App.vue 导航菜单中添加链接
  3. 集成后端 API: /autocodeforge-project 集成 API: issue-tracker /api/issues
  4. 运行单元测试: npm run test
```

### 示例 2: 创建实体与服务
```
/autocodeforge-project 创建实体: Milestone id:string, projectId:string, name:string, dueDate:datetime, completed:bool
```

**产出清单**:
```
✅ 后端文件创建:
  - AutoCodeForge.Core/Entities/Milestone.cs (52 lines)
  - AutoCodeForge.Application/Dtos/CreateMilestoneDto.cs (28 lines)
  - AutoCodeForge.Application/Dtos/UpdateMilestoneDto.cs (28 lines)
  - AutoCodeForge.Application/Dtos/MilestoneResponseDto.cs (24 lines)
  - AutoCodeForge.Application/Mappers/MilestoneMapper.cs (38 lines)
  - AutoCodeForge.Application/Validators/CreateMilestoneValidator.cs (32 lines)
  - AutoCodeForge.Application/Services/IMilestoneService.cs (18 lines)
  - AutoCodeForge.Application/Services/MilestoneService.cs (124 lines)
  - AutoCodeForge.Api/Controllers/MilestoneController.cs (89 lines)
  - AutoCodeForge.Tests/Services/MilestoneServiceTests.cs (156 lines)

✅ 数据库迁移: Milestone 表已配置
✅ Swagger 文档: 已生成 6 个 REST 端点

📋 下一步:
  1. 运行 dotnet build 验证
  2. 运行 dotnet test 检查单元测试
  3. 在前端集成 API: /autocodeforge-project 集成 API: xxx /api/milestones
```

---

## 故障排查

### Q: 生成的代码无法编译
**A**: 
1. 检查是否缺少命名空间导入
2. 运行 `dotnet build` 查看具体错误
3. 提交 Issue 并注明错误信息

### Q: 前后端类型不匹配
**A**:
1. 验证后端 DTO 定义是否完整
2. 使用 `/autocodeforge-project 生成 API 合约` 重新同步
3. 运行集成测试验证

### Q: 规范检查失败
**A**:
1. 查看检查报告中的具体违规项
2. 使用 `/autocodeforge-project 修复规范: xxx` 自动修复
3. 手动调整不符合自动修复的部分

---

## 版本与更新

| 版本 | 发布日期 | 主要功能 |
|------|----------|--------|
| v1.0.0 | 2026-05-21 | 初始版本（模块创建、实体生成、规范检查） |

---

## 总结

通过本 Skill，AutoCodeForge 的所有开发工作都遵循统一的规范与流程，无需依赖多个外部工具。开发者只需按需调用相应功能，即可快速交付高质量的代码。

