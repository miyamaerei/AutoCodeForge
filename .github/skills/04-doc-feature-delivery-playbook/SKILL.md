# 04-doc-feature-delivery-playbook

## 目标和范围
提供 AutoCodeForge 项目特性交付的逐步执行指南，包括文件脚手架、实现模板、编码门控、验收清单。基于技能 03（特性影响分析）、技能 02（模块影响发现）和技能 01（系统知识）。

## 输入假设
在执行本技能前，应已完成：
- [x] 技能 03 特性影响分析（已生成影响报告）
- [x] 技能 02 模块影响发现（已识别模块边界）
- [x] 技能 01 系统知识查询（已理解架构和技术栈）

## 逐步交付流程

### 阶段 1: 准备工作（Pre-Development）

#### 1.1 确认需求和影响范围

**输入**: 技能 03 生成的特性影响分析报告

**检查清单**:
- [ ] 特性需求明确（功能描述、验收标准）
- [ ] 影响范围已识别（前端/后端/数据库）
- [ ] 上下游模块已确认（无遗漏）
- [ ] 风险评估已完成（风险等级 + 缓解措施）
- [ ] 技术方案已评审（架构师/Tech Lead 批准）

**输出**: 需求确认清单 + 技术方案文档

---

#### 1.2 创建任务分支

**分支命名规范**:
```
feature/[module-name]/[feature-short-name]
例如: feature/task-center/add-priority-field
```

**Git 命令**:
```bash
git checkout main
git pull origin main
git checkout -b feature/task-center/add-priority-field
```

---

### 阶段 2: 数据库层变更（如适用）

#### 2.1 实体层变更

**文件位置**: `server/src/AutoCodeForge.Core/Entities/[Entity].cs`

**变更模板**（新增字段）:
```csharp
// 在 [Entity].cs 中添加新字段
public class TaskEntity : UserOwnedEntity
{
    // 现有字段...
    
    /// <summary>
    /// 任务优先级（1: 低, 2: 中, 3: 高, 4: 紧急）
    /// </summary>
    public int Priority { get; set; } = 2; // 默认值: 中
    
    /// <summary>
    /// 优先级更新时间
    /// </summary>
    public DateTime? PriorityUpdatedAt { get; set; }
}
```

**编码门控**:
- [ ] 新增字段有 XML 注释
- [ ] 有合理的默认值（避免现有数据问题）
- [ ] 必填字段有非空约束 `required`
- [ ] 外键字段有导航属性 `public virtual RelatedEntity Related { get; set; }`

---

#### 2.2 数据库迁移

**SQLite 迁移脚本**（开发环境）:
```sql
-- 文件: server/migrations/[timestamp]_add_task_priority.sql
ALTER TABLE Task ADD COLUMN Priority INTEGER NOT NULL DEFAULT 2;
ALTER TABLE Task ADD COLUMN PriorityUpdatedAt TEXT; -- SQLite 存储 DateTime 为 TEXT
```

**迁移执行**:
```bash
# AutoCodeForge 使用 SqlSugar，迁移通过 DatabaseInitializer 自动执行
# 确认 Program.cs 中 InitializeAsync() 调用存在
dotnet run --project server/src/AutoCodeForge.Api
```

**验证迁移**:
```bash
# 检查数据库表结构
sqlite3 server/src/AutoCodeForge.Api/autocodeforge.dev.db
.schema Task
```

**编码门控**:
- [ ] 迁移脚本有回滚方案
- [ ] 默认值与实体定义一致
- [ ] 在开发环境验证迁移成功

---

### 阶段 3: 后端层变更

#### 3.1 DTO 层变更

**文件位置**: `server/src/AutoCodeForge.Core/DTOs/[Module]/[Dto].cs`

**变更模板**（新增字段到 DTO）:
```csharp
// TaskDto.cs
public class TaskDto
{
    public long Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TaskStatus Status { get; set; }
    
    /// <summary>
    /// 任务优先级（1: 低, 2: 中, 3: 高, 4: 紧急）
    /// </summary>
    public int Priority { get; set; }
    
    /// <summary>
    /// 优先级更新时间
    /// </summary>
    public DateTime? PriorityUpdatedAt { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

// TaskCreateRequest.cs
public class TaskCreateRequest
{
    [Required]
    public string Title { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    /// <summary>
    /// 任务优先级，默认 2（中）
    /// </summary>
    [Range(1, 4, ErrorMessage = "优先级必须在 1-4 之间")]
    public int Priority { get; set; } = 2;
}

// TaskUpdateRequest.cs
public class TaskUpdateRequest
{
    [Required]
    public string Title { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    [Range(1, 4, ErrorMessage = "优先级必须在 1-4 之间")]
    public int Priority { get; set; }
}
```

**编码门控**:
- [ ] DTO 字段与实体字段一致
- [ ] 请求 DTO 有数据验证特性（`[Required]`, `[Range]` 等）
- [ ] 所有公共属性有 XML 注释
- [ ] 可选字段用 `?` 标记

---

#### 3.2 仓储层变更

**文件位置**: `server/src/AutoCodeForge.Infrastructure/Repositories/[Repository].cs`

**变更模板**（如需新增查询方法）:
```csharp
public class TaskRepository : BaseRepository<TaskEntity>
{
    public TaskRepository(ISqlSugarClient db) : base(db) { }
    
    /// <summary>
    /// 按优先级和状态查询任务
    /// </summary>
    public async Task<List<TaskEntity>> GetByPriorityAndStatusAsync(int priority, TaskStatus status)
    {
        return await DbSet
            .Where(t => t.Priority == priority && t.Status == status)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }
}
```

**编码门控**:
- [ ] 新增方法有 XML 注释
- [ ] 使用 `BaseRepository<T>` 的查询过滤器（用户隔离）
- [ ] 使用异步方法（`async/await`）
- [ ] 查询有合理的排序和分页

---

#### 3.3 服务层变更

**文件位置**: `server/src/AutoCodeForge.Application/Services/[Service].cs`

**变更模板**（更新业务方法）:
```csharp
public class TaskService
{
    private readonly TaskRepository _taskRepository;
    
    public TaskService(TaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }
    
    /// <summary>
    /// 创建任务
    /// </summary>
    public async Task<TaskDto> CreateTaskAsync(TaskCreateRequest request, string ntId)
    {
        var entity = new TaskEntity
        {
            Title = request.Title,
            Description = request.Description,
            Priority = request.Priority,
            PriorityUpdatedAt = DateTime.UtcNow, // 新增字段初始化
            Status = TaskStatus.Pending,
            NtId = ntId,
            CreatedAt = DateTime.UtcNow
        };
        
        var id = await _taskRepository.InsertReturnIdAsync(entity);
        entity.Id = id;
        
        return MapToDto(entity);
    }
    
    /// <summary>
    /// 更新任务优先级
    /// </summary>
    public async Task UpdateTaskPriorityAsync(long id, int priority, string ntId)
    {
        var task = await _taskRepository.GetByIdAsync(id, ntId);
        if (task == null)
            throw new NotFoundException($"Task {id} not found");
        
        task.Priority = priority;
        task.PriorityUpdatedAt = DateTime.UtcNow;
        task.UpdatedAt = DateTime.UtcNow;
        
        await _taskRepository.UpdateAsync(task);
    }
    
    private TaskDto MapToDto(TaskEntity entity)
    {
        return new TaskDto
        {
            Id = entity.Id,
            Title = entity.Title,
            Description = entity.Description,
            Status = entity.Status,
            Priority = entity.Priority, // 新增字段映射
            PriorityUpdatedAt = entity.PriorityUpdatedAt,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }
}
```

**编码门控**:
- [ ] 业务方法有完整的异常处理
- [ ] 实体到 DTO 映射包含所有字段
- [ ] 更新操作有 `UpdatedAt` 时间戳
- [ ] 权限校验（NtId 隔离）

---

#### 3.4 端点层变更

**文件位置**: `server/src/AutoCodeForge.Api/Endpoints/[Endpoints].cs`

**变更模板**（更新 HTTP 端点）:
```csharp
public static class TaskEndpoints
{
    public static void MapTaskEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/tasks").WithTags("Task");
        
        /// <summary>
        /// 创建任务（包含优先级）
        /// </summary>
        group.MapPost("/", async (TaskCreateRequest request, TaskService service, HttpContext context) =>
        {
            var ntId = context.GetNtId();
            var task = await service.CreateTaskAsync(request, ntId);
            return Results.Ok(ApiResponse<TaskDto>.Ok(task));
        })
        .RequireAuthorization()
        .WithOpenApi();
        
        /// <summary>
        /// 更新任务优先级
        /// </summary>
        group.MapPatch("/{id}/priority", async (long id, [FromBody] UpdatePriorityRequest request, TaskService service, HttpContext context) =>
        {
            var ntId = context.GetNtId();
            await service.UpdateTaskPriorityAsync(id, request.Priority, ntId);
            return Results.Ok(ApiResponse<object>.Ok("优先级更新成功"));
        })
        .RequireAuthorization()
        .WithOpenApi();
    }
}

// 新增请求模型
public class UpdatePriorityRequest
{
    [Range(1, 4)]
    public int Priority { get; set; }
}
```

**编码门控**:
- [ ] 端点有 XML 注释（Swagger 文档生成）
- [ ] 所有端点有 `.RequireAuthorization()`（除公共端点）
- [ ] 响应统一使用 `ApiResponse<T>`
- [ ] 异常由 `ExceptionHandlingMiddleware` 全局捕获

---

### 阶段 4: 前端层变更

#### 4.1 Types 定义

**文件位置**: `client/src/modules/task-center/task.types.ts`

**变更模板**（新增字段到类型）:
```typescript
// task.types.ts
export interface Task {
  id: number
  title: string
  description?: string
  status: TaskStatus
  priority: TaskPriority // 新增字段
  priorityUpdatedAt?: string
  createdAt: string
  updatedAt?: string
}

export enum TaskPriority {
  Low = 1,
  Medium = 2,
  High = 3,
  Urgent = 4
}

export interface TaskCreateRequest {
  title: string
  description?: string
  priority?: TaskPriority // 新增可选字段，默认 Medium
}

export interface TaskUpdateRequest {
  title: string
  description?: string
  priority: TaskPriority
}

export interface UpdatePriorityRequest {
  priority: TaskPriority
}
```

**编码门控**:
- [ ] 类型定义与后端 DTO 完全一致
- [ ] 枚举值与后端枚举一致
- [ ] 可选字段用 `?` 标记
- [ ] 导出所有公共类型

---

#### 4.2 API 调用

**文件位置**: `client/src/modules/task-center/task.api.ts`

**变更模板**（更新 API 方法）:
```typescript
// task.api.ts
import axios from 'axios'
import type { Task, TaskCreateRequest, TaskUpdateRequest, UpdatePriorityRequest } from './task.types'

const BASE_URL = '/api/tasks'

export const taskApi = {
  /**
   * 创建任务（包含优先级）
   */
  async create(request: TaskCreateRequest): Promise<Task> {
    const { data } = await axios.post<ApiResponse<Task>>(BASE_URL, request)
    return data.data
  },
  
  /**
   * 更新任务优先级
   */
  async updatePriority(id: number, request: UpdatePriorityRequest): Promise<void> {
    await axios.patch(`${BASE_URL}/${id}/priority`, request)
  },
  
  /**
   * 获取任务详情（包含优先级）
   */
  async getById(id: number): Promise<Task> {
    const { data } = await axios.get<ApiResponse<Task>>(`${BASE_URL}/${id}`)
    return data.data
  }
}

interface ApiResponse<T> {
  success: boolean
  data: T
  statusCode: number
}
```

**编码门控**:
- [ ] 所有方法有 JSDoc 注释
- [ ] 请求参数类型正确
- [ ] 响应数据类型正确
- [ ] 错误处理由 Axios 拦截器统一处理

---

#### 4.3 Store 状态管理

**文件位置**: `client/src/modules/task-center/store/task.store.ts`

**变更模板**（更新 Store）:
```typescript
// task.store.ts
import { defineStore } from 'pinia'
import { ref } from 'vue'
import { taskApi } from '../task.api'
import type { Task, TaskCreateRequest, UpdatePriorityRequest } from '../task.types'

export const useTaskStore = defineStore('task', () => {
  const tasks = ref<Task[]>([])
  const currentTask = ref<Task | null>(null)
  
  /**
   * 创建任务（包含优先级）
   */
  async function createTask(request: TaskCreateRequest) {
    const task = await taskApi.create(request)
    tasks.value.push(task)
    return task
  }
  
  /**
   * 更新任务优先级
   */
  async function updateTaskPriority(id: number, priority: number) {
    await taskApi.updatePriority(id, { priority })
    
    // 更新本地状态
    const task = tasks.value.find(t => t.id === id)
    if (task) {
      task.priority = priority
      task.priorityUpdatedAt = new Date().toISOString()
    }
  }
  
  return {
    tasks,
    currentTask,
    createTask,
    updateTaskPriority
  }
})
```

**编码门控**:
- [ ] 使用 Pinia setup store 格式
- [ ] 所有 Action 有 JSDoc 注释
- [ ] 更新远程数据后同步本地状态
- [ ] 错误处理（try-catch 或全局拦截器）

---

#### 4.4 页面组件

**文件位置**: `client/src/modules/task-center/views/TaskForm.vue`

**变更模板**（表单新增字段）:
```vue
<template>
  <el-form :model="form" :rules="rules" ref="formRef">
    <el-form-item label="标题" prop="title">
      <el-input v-model="form.title" placeholder="请输入任务标题" />
    </el-form-item>
    
    <el-form-item label="描述" prop="description">
      <el-input v-model="form.description" type="textarea" />
    </el-form-item>
    
    <!-- 新增优先级字段 -->
    <el-form-item label="优先级" prop="priority">
      <el-radio-group v-model="form.priority">
        <el-radio :label="1">低</el-radio>
        <el-radio :label="2">中</el-radio>
        <el-radio :label="3">高</el-radio>
        <el-radio :label="4">紧急</el-radio>
      </el-radio-group>
    </el-form-item>
    
    <el-form-item>
      <el-button type="primary" @click="handleSubmit">提交</el-button>
    </el-form-item>
  </el-form>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { useTaskStore } from '../store/task.store'
import type { TaskCreateRequest } from '../task.types'
import { TaskPriority } from '../task.types'

const formRef = ref()
const taskStore = useTaskStore()

const form = ref<TaskCreateRequest>({
  title: '',
  description: '',
  priority: TaskPriority.Medium // 默认值：中
})

const rules = {
  title: [{ required: true, message: '请输入任务标题' }],
  priority: [{ required: true, message: '请选择优先级' }]
}

async function handleSubmit() {
  await formRef.value.validate()
  await taskStore.createTask(form.value)
  // 跳转或提示成功
}
</script>
```

**编码门控**:
- [ ] 表单有验证规则（`rules`）
- [ ] 新增字段有默认值
- [ ] 组件使用 Composition API
- [ ] 样式符合 Element Plus 规范

---

### 阶段 5: 测试编写

#### 5.1 后端单元测试

**文件位置**: `server/tests/AutoCodeForge.Tests/Services/TaskServiceTests.cs`

**变更模板**（新增测试用例）:
```csharp
[Fact]
public async Task CreateTask_WithPriority_ShouldSetPriorityCorrectly()
{
    // Arrange
    var request = new TaskCreateRequest
    {
        Title = "测试任务",
        Description = "测试描述",
        Priority = 3 // 高优先级
    };
    
    // Act
    var result = await _taskService.CreateTaskAsync(request, "test-user");
    
    // Assert
    Assert.Equal(3, result.Priority);
    Assert.NotNull(result.PriorityUpdatedAt);
}

[Fact]
public async Task UpdateTaskPriority_ShouldUpdatePriorityAndTimestamp()
{
    // Arrange
    var task = await CreateTestTaskAsync();
    
    // Act
    await _taskService.UpdateTaskPriorityAsync(task.Id, 4, "test-user");
    
    // Assert
    var updated = await _taskRepository.GetByIdAsync(task.Id, "test-user");
    Assert.Equal(4, updated.Priority);
    Assert.NotNull(updated.PriorityUpdatedAt);
}
```

**编码门控**:
- [ ] 每个新增方法至少 2 个测试用例（正常流程 + 异常流程）
- [ ] 测试有清晰的 Arrange-Act-Assert 结构
- [ ] 测试数据通过 TestDataFactory 生成
- [ ] 所有测试可独立运行（无顺序依赖）

---

#### 5.2 前端单元测试

**文件位置**: `client/src/modules/task-center/__tests__/task.store.spec.ts`

**变更模板**（新增测试用例）:
```typescript
import { describe, it, expect, beforeEach } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { useTaskStore } from '../store/task.store'
import { TaskPriority } from '../task.types'

describe('TaskStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
  })
  
  it('should create task with priority', async () => {
    const store = useTaskStore()
    
    const task = await store.createTask({
      title: '测试任务',
      priority: TaskPriority.High
    })
    
    expect(task.priority).toBe(TaskPriority.High)
    expect(task.priorityUpdatedAt).toBeDefined()
  })
  
  it('should update task priority', async () => {
    const store = useTaskStore()
    
    const task = await store.createTask({ title: '测试任务' })
    await store.updateTaskPriority(task.id, TaskPriority.Urgent)
    
    const updated = store.tasks.find(t => t.id === task.id)
    expect(updated?.priority).toBe(TaskPriority.Urgent)
  })
})
```

**编码门控**:
- [ ] 使用 Vitest 作为测试框架
- [ ] 测试覆盖核心业务逻辑
- [ ] Mock 外部依赖（API 调用）
- [ ] 测试有清晰的 describe/it 结构

---

### 阶段 6: 集成验证

#### 6.1 构建验证

```bash
# 前端构建
cd client
npm run build
npm run type-check

# 后端构建
cd server
dotnet build AutoCodeForge.sln
```

**编码门控**:
- [ ] 前端构建无错误
- [ ] TypeScript 类型检查无错误
- [ ] 后端构建无错误
- [ ] 无新增编译警告

---

#### 6.2 测试验证

```bash
# 前端测试
cd client
npm run test:unit

# 后端测试
cd server
dotnet test
```

**编码门控**:
- [ ] 所有新增测试通过
- [ ] 现有测试无回归
- [ ] 测试覆盖率 ≥ 80%（核心业务逻辑）

---

#### 6.3 手动验证

**验收清单**:
- [ ] **前端**: 表单可正常提交，优先级字段正确显示
- [ ] **后端**: Swagger UI 显示新增端点，API 调用成功
- [ ] **数据库**: 查询 Task 表，Priority 字段正确存储
- [ ] **日志**: 无错误日志，关键操作有日志记录

---

### 阶段 7: 代码提交和审查

#### 7.1 代码审查清单

- [ ] 代码符合 PROJECT_SPEC.md 编码规范
- [ ] 所有公共方法有 XML 注释（C#）或 JSDoc 注释（TypeScript）
- [ ] 变量命名清晰，无魔法数字
- [ ] 异常处理完整，无裸抛异常
- [ ] 日志输出合理，无敏感信息泄漏
- [ ] 性能优化（查询索引、缓存策略）

#### 7.2 提交代码

```bash
# 暂存所有变更
git add .

# 提交（遵循 Conventional Commits）
git commit -m "feat(task-center): 添加任务优先级字段

- 新增 TaskEntity.Priority 和 PriorityUpdatedAt 字段
- 新增 DTO 和请求模型支持优先级
- 新增前端 TaskPriority 枚举和表单字段
- 新增单元测试覆盖优先级功能

Closes #123"

# 推送到远程
git push origin feature/task-center/add-priority-field
```

**编码门控**:
- [ ] 提交信息遵循 Conventional Commits 格式
- [ ] 提交信息包含变更说明和关联 Issue
- [ ] 每个提交原子化（单一职责）
- [ ] 提交前已运行测试并通过

---

## 文件脚手架检查清单

### 后端文件脚手架

- [ ] **实体**: `server/src/AutoCodeForge.Core/Entities/[Entity].cs`
- [ ] **DTO**: `server/src/AutoCodeForge.Core/DTOs/[Module]/[Dto].cs`
- [ ] **仓储接口**: `server/src/AutoCodeForge.Core/Interfaces/I[Repository].cs`（可选）
- [ ] **仓储实现**: `server/src/AutoCodeForge.Infrastructure/Repositories/[Repository].cs`
- [ ] **服务接口**: `server/src/AutoCodeForge.Application/Interfaces/I[Service].cs`（可选）
- [ ] **服务实现**: `server/src/AutoCodeForge.Application/Services/[Service].cs`
- [ ] **端点**: `server/src/AutoCodeForge.Api/Endpoints/[Endpoints].cs`
- [ ] **单元测试**: `server/tests/AutoCodeForge.Tests/[Service]Tests.cs`

### 前端文件脚手架

- [ ] **Types**: `client/src/modules/[module]/[module].types.ts`
- [ ] **API**: `client/src/modules/[module]/[module].api.ts`
- [ ] **Store**: `client/src/modules/[module]/store/[module].store.ts`
- [ ] **Routes**: `client/src/modules/[module]/routes.ts`
- [ ] **Views**: `client/src/modules/[module]/views/[View].vue`
- [ ] **单元测试**: `client/src/modules/[module]/__tests__/[module].spec.ts`

---

## 验收和回归检查清单

### 功能验收

- [ ] 新增功能按需求实现
- [ ] 前端 UI 交互正常
- [ ] 后端 API 响应正确
- [ ] 数据库数据正确存储

### 性能验收

- [ ] API 响应时间 < 500ms
- [ ] 前端页面渲染时间 < 2s
- [ ] 数据库查询优化（使用索引）

### 安全验收

- [ ] 敏感数据加密
- [ ] JWT 认证正确
- [ ] 权限控制正确（NtId 隔离）
- [ ] 无 SQL 注入风险

### 兼容性验收

- [ ] API 向后兼容（或明确版本升级）
- [ ] 前端支持主流浏览器（Chrome, Edge, Firefox）
- [ ] 移动端响应式布局（如适用）

### 文档验收

- [ ] Swagger 文档已更新
- [ ] README 已更新（如有重大变更）
- [ ] Release Notes 已编写

---

## 实现模板/骨架

### 后端实体模板

```csharp
/// <summary>
/// [实体描述]
/// </summary>
public class [Entity]Entity : UserOwnedEntity // 或 AuditableEntity
{
    /// <summary>
    /// [字段描述]
    /// </summary>
    public [Type] [PropertyName] { get; set; } [= 默认值;]
    
    // 导航属性（外键关系）
    public virtual [RelatedEntity]? [RelatedProperty] { get; set; }
}
```

### 后端 Service 模板

```csharp
public class [Module]Service
{
    private readonly [Module]Repository _repository;
    
    public [Module]Service([Module]Repository repository)
    {
        _repository = repository;
    }
    
    /// <summary>
    /// [方法描述]
    /// </summary>
    public async Task<[ReturnType]> [MethodName]Async([Parameters])
    {
        // 业务逻辑
        // 1. 参数验证
        // 2. 数据查询/操作
        // 3. 返回结果
    }
}
```

### 前端 Store 模板

```typescript
export const use[Module]Store = defineStore('[module]', () => {
  const items = ref<[Type][]>([])
  const currentItem = ref<[Type] | null>(null)
  
  /**
   * [Action 描述]
   */
  async function [actionName]([parameters]) {
    // 调用 API
    // 更新状态
    // 返回结果
  }
  
  return {
    items,
    currentItem,
    [actionName]
  }
})
```

---

## 更新历史

| 日期 | 版本 | 变更说明 |
|------|------|----------|
| 2026-05-25 | 1.0.0 | 初始版本，基于 AutoCodeForge 四层架构和 Vue 3 模块结构 |

---

**维护说明**: 本手册基于 AutoCodeForge 实际架构和技术栈编写，包含具体的文件路径、代码模板和编码门控。当架构或技术栈变更时，需同步更新本手册。
