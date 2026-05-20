---
name: vue3-feature-planner
description: 'Plan new features for Vue 3 project. Use for: break down complex features, identify required modules/pages/APIs, create implementation roadmap, estimate effort.'
argument-hint: 'Describe the feature you want to implement (e.g. user management with CRUD operations)'
---

# Vue3 Feature Planner

## When to Use
- You need to plan a new feature or module.
- You want to break down complex requirements into manageable tasks.
- You need to identify required components, APIs, and stores.
- You want to create an implementation roadmap.

## Planning Process

### Step 1: Requirement Analysis
首先明确需求：

1. **功能描述**：这个功能要做什么？
2. **用户场景**：用户如何使用这个功能？
3. **数据实体**：涉及哪些数据实体？
4. **操作类型**：需要哪些操作（CRUD、搜索、过滤等）？
5. **权限要求**：是否需要权限控制？

### Step 2: Architecture Design
设计架构：

1. **模块划分**：需要创建哪些模块？
2. **页面设计**：需要哪些页面？
3. **API 设计**：需要哪些 API 接口？
4. **数据模型**：需要哪些数据模型？
5. **状态管理**：需要哪些 Store？

### Step 3: Task Breakdown
拆分任务：

1. **基础设施**：Mock 数据、API 层、类型定义
2. **核心功能**：Store、Composable、业务逻辑
3. **UI 实现**：页面、组件、样式
4. **集成测试**：单元测试、E2E 测试
5. **文档完善**：API 文档、使用说明

### Step 4: Implementation Order
确定实现顺序：

1. 数据层 → 2. 业务层 → 3. 展示层 → 4. 测试层

## Planning Template

### Feature Planning Document

```markdown
# Feature: <Feature Name>

## 1. Overview
- **Description**: <功能描述>
- **Priority**: High / Medium / Low
- **Module**: <模块名称>
- **Route**: <路由路径>

## 2. Requirements
### Functional Requirements
- [ ] 需求1
- [ ] 需求2
- [ ] 需求3

### Non-Functional Requirements
- [ ] 性能要求
- [ ] 安全要求
- [ ] 兼容性要求

## 3. Data Model
### Entities
- **Entity 1**: <实体描述>
  - Fields: id, name, createdAt, updatedAt
- **Entity 2**: <实体描述>
  - Fields: id, title, status, userId

### DTOs
- `ListItemDto`: 列表项数据
- `DetailDto`: 详情数据
- `CreateRequestDto`: 创建请求数据
- `UpdateRequestDto`: 更新请求数据

## 4. API Design
### Endpoints
| Method | Path | Description | Auth |
|--------|------|-------------|------|
| GET | /api/<entity> | 获取列表 | Yes/No |
| GET | /api/<entity>/:id | 获取详情 | Yes/No |
| POST | /api/<entity> | 创建 | Yes/No |
| PUT | /api/<entity>/:id | 更新 | Yes/No |
| DELETE | /api/<entity>/:id | 删除 | Yes/No |

## 5. UI Design
### Pages
- **List Page**: `/module` - 列表页
  - Features: 搜索、过滤、分页、排序
- **Detail Page**: `/module/:id` - 详情页
  - Features: 信息展示、操作按钮
- **Form Page**: `/module/create` - 创建页
  - Features: 表单验证、提交

### Components
- `<Entity>Table`: 列表表格组件
- `<Entity>Form`: 表单组件
- `<Entity>Detail`: 详情组件
- `<Entity>Filter`: 过滤组件

## 6. State Management
### Store
- **Store Name**: `use<Module>Store`
- **State**:
  - items: 列表数据
  - selectedItem: 选中的项
  - loading: 加载状态
  - error: 错误信息
- **Actions**:
  - fetchItems: 获取列表
  - fetchDetail: 获取详情
  - createItem: 创建项
  - updateItem: 更新项
  - deleteItem: 删除项

### Composables
- `use<Entity>List`: 列表逻辑
- `use<Entity>Form`: 表单逻辑
- `use<Entity>Filter`: 过滤逻辑

## 7. Implementation Tasks
### Phase 1: Data Layer (基础数据层)
- [ ] 创建 Mock 数据 (`/vue3-mock-builder`)
- [ ] 创建 DTO 类型定义
- [ ] 创建 API 函数 (`/vue3-api-mock-integration`)

### Phase 2: Business Layer (业务逻辑层)
- [ ] 创建 Pinia Store (`/vue3-store-builder`)
- [ ] 创建 Composable (`/vue3-composable-builder`)
- [ ] 实现业务逻辑

### Phase 3: Presentation Layer (展示层)
- [ ] 创建页面组件 (`/vue3-page-builder`)
- [ ] 创建共享组件 (`/vue3-component-builder`)
- [ ] 实现样式和交互

### Phase 4: Testing Layer (测试层)
- [ ] 创建单元测试 (`/vue3-test-builder`)
- [ ] 创建 E2E 测试
- [ ] 测试覆盖率检查

### Phase 5: Integration (集成)
- [ ] 路由配置
- [ ] 导航菜单
- [ ] 权限控制
- [ ] 文档更新

## 8. Dependencies
### External Dependencies
- [ ] Element Plus 组件
- [ ] Axios
- [ ] 其他库

### Internal Dependencies
- [ ] 共享组件
- [ ] 工具函数
- [ ] 配置文件

## 9. Risks and Mitigation
| Risk | Impact | Mitigation |
|------|--------|------------|
| 风险1 | High/Medium/Low | 缓解措施 |
| 风险2 | High/Medium/Low | 缓解措施 |

## 10. Timeline
- **Phase 1**: 1-2 days
- **Phase 2**: 2-3 days
- **Phase 3**: 3-4 days
- **Phase 4**: 1-2 days
- **Phase 5**: 1 day
- **Total**: 8-12 days

## 11. Success Criteria
- [ ] 所有功能需求已实现
- [ ] 所有测试通过
- [ ] 类型检查通过
- [ ] 代码审查通过
- [ ] 文档已更新
```

## Feature Types

### 1. CRUD Module (增删改查模块)
**典型特征**：
- 列表页、详情页、创建/编辑页
- 标准 CRUD 操作
- 搜索、过滤、分页

**规划步骤**：
1. 定义数据模型和 DTO
2. 创建 Mock 数据
3. 创建 API 层
4. 创建 Store
5. 创建页面
6. 创建测试

**使用 Skills**：
```
/vue3-api-model-router-store create <module> with list detail form
```

### 2. Dashboard/Analytics (仪表盘/分析)
**典型特征**：
- 数据可视化
- 图表展示
- 实时更新

**规划步骤**：
1. 定义数据指标
2. 创建数据聚合 API
3. 创建图表组件
4. 实现数据刷新

**使用 Skills**：
```
/vue3-component-builder create <Chart> with echarts
/vue3-composable-builder create use<Dashboard> with refresh
```

### 3. Form/Workflow (表单/工作流)
**典型特征**：
- 复杂表单
- 多步骤流程
- 表单验证

**规划步骤**：
1. 定义表单字段和验证规则
2. 创建表单组件
3. 实现工作流逻辑
4. 创建 Composable

**使用 Skills**：
```
/vue3-component-builder create <Form> with validation
/vue3-composable-builder create use<Form> with steps
```

### 4. Chat/Communication (聊天/通讯)
**典型特征**：
- 实时消息
- 会话管理
- 消息历史

**规划步骤**：
1. 定义消息模型
2. 创建会话管理
3. 实现消息发送/接收
4. 创建 UI 组件

**使用 Skills**：
```
/vue3-composable-builder create use<Chat> with session
/vue3-component-builder create <ChatPanel> with messages
```

## Planning Checklist

### Before Starting
- [ ] 需求是否明确？
- [ ] 技术方案是否可行？
- [ ] 是否有类似功能可参考？
- [ ] 是否需要外部依赖？
- [ ] 是否需要后端支持？

### During Planning
- [ ] 模块划分是否合理？
- [ ] API 设计是否完整？
- [ ] 数据模型是否清晰？
- [ ] 状态管理是否需要？
- [ ] 组件复用性如何？

### After Planning
- [ ] 任务拆分是否足够细？
- [ ] 实现顺序是否合理？
- [ ] 风险是否识别？
- [ ] 时间估算是否合理？
- [ ] 成功标准是否明确？

## Decision Making

### When to Create a New Module
- 功能独立且复杂
- 有多个相关页面
- 需要独立的状态管理
- 未来可能扩展

### When to Add to Existing Module
- 功能简单
- 与现有功能强相关
- 共享现有状态
- 不需要独立路由

### When to Create a Composable
- 逻辑需要在多个组件间共享
- 需要封装复杂的状态管理
- 需要复用业务逻辑
- 需要独立测试

### When to Create a Shared Component
- 组件需要在多个地方使用
- 组件足够通用
- 组件有独立的状态和逻辑
- 组件需要独立维护

## Common Patterns

### Pattern 1: Master-Detail (主从模式)
```
List Page → Detail Page
- 列表页显示摘要信息
- 点击进入详情页
- 详情页显示完整信息
```

### Pattern 2: Wizard (向导模式)
```
Step 1 → Step 2 → Step 3 → Complete
- 多步骤表单
- 每步独立验证
- 最终提交所有数据
```

### Pattern 3: Dashboard (仪表盘模式)
```
Widget 1 | Widget 2 | Widget 3
Widget 4 | Widget 5 | Widget 6
- 多个独立组件
- 各自获取数据
- 支持自定义布局
```

### Pattern 4: CRUD (增删改查模式)
```
List → Create/Edit → Detail
- 列表页：搜索、过滤、分页
- 创建/编辑页：表单验证
- 详情页：信息展示、操作
```

## Example Planning

### Example 1: User Management Module

```markdown
# Feature: User Management

## Overview
- Description: 用户管理模块，支持用户的增删改查
- Priority: High
- Module: user-management
- Route: /user-management

## Data Model
- User
  - id: string
  - username: string
  - email: string
  - role: 'admin' | 'user' | 'guest'
  - status: 'active' | 'inactive'
  - createdAt: string
  - updatedAt: string

## Pages
- UserListView: /user-management
  - Features: 搜索、过滤、分页、角色筛选
- UserDetailView: /user-management/:id
  - Features: 用户信息展示、操作日志
- UserFormView: /user-management/create, /user-management/:id/edit
  - Features: 表单验证、角色选择

## Tasks
Phase 1: Data Layer
- [ ] Create mock data with 20 users
- [ ] Create UserDto types
- [ ] Create user API with CRUD

Phase 2: Business Layer
- [ ] Create useUserManagementStore
- [ ] Create useUserFilter composable

Phase 3: Presentation Layer
- [ ] Create UserListView with table
- [ ] Create UserDetailView with tabs
- [ ] Create UserFormView with validation

Phase 4: Testing
- [ ] Unit tests for store
- [ ] Unit tests for composable
- [ ] Component tests for views

## Skills to Use
1. /vue3-mock-builder create user mock with 20 items
2. /vue3-api-mock-integration create user API with CRUD
3. /vue3-store-builder create useUserManagementStore
4. /vue3-page-builder create user list page
5. /vue3-test-builder create user store tests
```

## Example Prompts
- /vue3-feature-planner plan user management module with CRUD operations
- /vue3-feature-planner plan dashboard with charts and real-time updates
- /vue3-feature-planner plan chat module with sessions and message history
- /vue3-feature-planner break down order management feature into tasks
