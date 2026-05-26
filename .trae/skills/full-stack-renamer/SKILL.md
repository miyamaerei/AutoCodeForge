---
name: "full-stack-renamer"
description: "Renames entities and updates all references across backend (Entity/DTO/Repository/Service/Endpoint) and frontend (api/types/store/views). Invoke when user asks to rename entities across the full stack or do a complete refactor."
---

# Full Stack Renamer

批量重命名实体并更新全栈引用的完整指南。

## 重命名映射

| 层级 | 旧名称 | 新名称 |
|-----|-------|-------|
| Entity | `TaskEntity` | `WorkItemEntity` |
| Entity | `TaskStepEntity` | `WorkflowStepEntity` |
| Entity | `TaskReviewEntity` | `WorkflowStepAuditEntity` |
| Entity | `ReviewTaskEntity` | `CodeReviewStepEntity` |
| Entity | `ScheduledTaskExecutionEntity` | `ScheduledRunEntity` |
| Entity | `TaskLogEntity` | `WorkItemLogEntity` |

## 数据库表名映射

| 旧表名 | 新表名 |
|-------|-------|
| `Tasks` | `WorkItems` |
| `TaskSteps` | `WorkflowSteps` |
| `TaskReviews` | `WorkflowStepAudits` |
| `ReviewTasks` | `CodeReviewSteps` |
| `ScheduledTaskExecutions` | `ScheduledRuns` |
| `TaskLogs` | `WorkItemLogs` |

---

## 执行顺序（重要！）

必须按以下顺序执行，否则引用会断裂：

```
1. Entities      → 核心实体，先改
2. DTOs          → 依赖 Entity 类名
3. Repositories  → 依赖 Entity 和 DTO
4. Services      → 依赖 Repository 和 Entity
5. Interfaces    → 依赖 Entity
6. Endpoints    → 依赖 Service
7. Frontend Types → 依赖 Endpoint
8. Frontend API  → 依赖 Types
9. Frontend Store → 依赖 API 和 Types
10. Frontend Views → 依赖 Store 和 Types
11. Tests        → 依赖所有
```

---

## Step 1: 查找所有相关文件

```powershell
# 查找所有包含旧名称的文件
grep -r "TaskEntity" --include="*.cs" --include="*.ts" --include="*.vue"
grep -r "TaskStepEntity" --include="*.cs" --include="*.ts" --include="*.vue"
grep -r "TaskReviewEntity" --include="*.cs" --include="*.ts" --include="*.vue"
grep -r "ReviewTaskEntity" --include="*.cs" --include="*.ts" --include="*.vue"
grep -r "ScheduledTaskExecutionEntity" --include="*.cs" --include="*.ts" --include="*.vue"
grep -r "TaskLogEntity" --include="*.cs" --include="*.ts" --include="*.vue"

# 查找旧表名引用
grep -r "\"Tasks\"" --include="*.cs"
grep -r "\"TaskSteps\"" --include="*.cs"
grep -r "'Tasks'" --include="*.ts"
```

---

## Step 2: 重命名后端 Entity 文件

使用 `git mv` 重命名，保持 git 历史：

```powershell
# Entities
git mv TaskEntity.cs WorkItemEntity.cs
git mv TaskStepEntity.cs WorkflowStepEntity.cs
git mv TaskReviewEntity.cs WorkflowStepAuditEntity.cs
git mv ReviewTaskEntity.cs CodeReviewStepEntity.cs
git mv ScheduledTaskExecutionEntity.cs ScheduledRunEntity.cs
git mv TaskLogEntity.cs WorkItemLogEntity.cs
```

更新文件内容：
- 类名
- `[SugarTable("...")]` 表名
- 构造函数名

---

## Step 3: 重命名 DTO 文件

根据 Step 1 的搜索结果，重命名相关 DTO：

```powershell
# 示例 DTO 重命名
git mv CreateTaskRequest.cs CreateWorkItemRequest.cs
git mv UpdateTaskRequest.cs UpdateWorkItemRequest.cs
git mv TaskResponse.cs WorkItemResponse.cs
git mv TaskStepResponse.cs WorkflowStepResponse.cs
git mv TaskReviewResponse.cs WorkflowStepAuditResponse.cs
git mv ReviewTaskResponse.cs CodeReviewStepResponse.cs
```

更新内容：
- 类名
- 构造函数
- 属性类型引用

---

## Step 4: 重命名 Repository 文件

```powershell
git mv TaskRepository.cs WorkItemRepository.cs
git mv TaskStepRepository.cs WorkflowStepRepository.cs
git mv TaskReviewRepository.cs WorkflowStepAuditRepository.cs
git mv ReviewTaskRepository.cs CodeReviewStepRepository.cs
```

更新内容：
- 类名
- 继承的基类
- 泛型参数

---

## Step 5: 重命名 Service 文件

```powershell
git mv TaskService.cs WorkItemService.cs
git mv TaskStepService.cs WorkflowStepService.cs
git mv TaskReviewService.cs WorkflowStepAuditService.cs
git mv ReviewTaskService.cs CodeReviewStepService.cs
git mv ScheduledTaskService.cs ScheduledTaskService.cs  # 保留
git mv ScheduledTaskExecutionService.cs ScheduledRunService.cs
git mv TaskLogService.cs WorkItemLogService.cs
```

更新内容：
- 类名
- 依赖注入的 Repository/Service 引用
- 方法参数类型

---

## Step 6: 重命名 Interface 文件

```powershell
git mv ITaskService.cs IWorkItemService.cs
git mv ITaskRepository.cs IWorkItemRepository.cs
# ... 其他接口
```

---

## Step 7: 重命名 Endpoint/Controller 文件

```powershell
git mv TaskEndpoint.cs WorkItemEndpoint.cs
git mv TaskStepEndpoint.cs WorkflowStepEndpoint.cs
git mv ReviewTaskEndpoint.cs CodeReviewStepEndpoint.cs
git mv ScheduledTaskEndpoint.cs ScheduledTaskEndpoint.cs  # 保留
```

更新内容：
- 路由前缀
- 依赖注入的 Service 引用
- 泛型参数

---

## Step 8: 重命名前端文件

### 8.1 API 层
```powershell
# api/*.ts
git mv task.api.ts work-item.api.ts
git mv task-step.api.ts workflow-step.api.ts
git mv review-task.api.ts code-review-step.api.ts
```

### 8.2 Types 层
```powershell
# types/*.ts
git mv task.types.ts work-item.types.ts
git mv task-step.types.ts workflow-step.types.ts
```

### 8.3 Store 层
```powershell
# store/useXxxStore.ts
git mv useTaskStore.ts useWorkItemStore.ts
git mv useTaskStepStore.ts useWorkflowStepStore.ts
```

### 8.4 Views/Components
```powershell
# views/**/task*.vue
git mv TaskList.vue WorkItemList.vue
git mv TaskDetail.vue WorkItemDetail.vue
git mv TaskStep.vue WorkflowStep.vue
```

---

## Step 9: 更新所有引用

对于每个被重命名的文件，搜索并替换：

### 9.1 C# 引用更新
```powershell
# 类名替换
TaskEntity → WorkItemEntity
TaskStepEntity → WorkflowStepEntity
TaskReviewEntity → WorkflowStepAuditEntity
ReviewTaskEntity → CodeReviewStepEntity
ScheduledTaskExecutionEntity → ScheduledRunEntity
TaskLogEntity → WorkItemLogEntity

# 表名列替换
"Tasks" → "WorkItems"
"TaskSteps" → "WorkflowSteps"
"TaskReviews" → "WorkflowStepAudits"
"ReviewTasks" → "CodeReviewSteps"
"ScheduledTaskExecutions" → "ScheduledRuns"
"TaskLogs" → "WorkItemLogs"

# 命名空间替换
AutoCodeForge.Core.Entities.TaskEntity → AutoCodeForge.Core.Entities.WorkItemEntity
```

### 9.2 TypeScript 引用更新
```typescript
// 类型引用
import { TaskEntity } → import { WorkItemEntity }
import { TaskStepEntity } → import { WorkflowStepEntity }

// API 函数
getTask() → getWorkItem()
getTaskById() → getWorkItemById()
createTask() → createWorkItem()
updateTask() → updateWorkItem()
deleteTask() → deleteWorkItem()

// Store
useTaskStore() → useWorkItemStore()
useTaskStepStore() → useWorkflowStepStore()

// 路由
/task → /work-item
/task-step → /workflow-step
```

---

## Step 10: 更新单元测试

```powershell
git mv TaskEntityTests.cs WorkItemEntityTests.cs
git mv TaskServiceTests.cs WorkItemServiceTests.cs
git mv TaskRepositoryTests.cs WorkItemRepositoryTests.cs
```

更新测试文件内的：
- 类引用
- Mock 对象类型
- 断言消息

---

## Step 11: 验证完整性

```powershell
# 检查是否还有旧名称残留
grep -r "TaskEntity\|TaskStepEntity\|TaskReviewEntity\|ReviewTaskEntity\|ScheduledTaskExecutionEntity\|TaskLogEntity" --include="*.cs" --include="*.ts" --include="*.vue" server/ web/

# 检查表名残留
grep -r "\"Tasks\"\|\"TaskSteps\"\|\"TaskReviews\"\|\"ReviewTasks\"\|\"ScheduledTaskExecutions\"\|\"TaskLogs\"" --include="*.cs" server/

# 构建验证
cd server && dotnet build
cd web && npm run build
```

---

## 重要注意事项

1. **保持提交原子性**: 每完成一层重命名，提交一次
2. **逆序检查**: 从 Endpoint 层往回检查引用是否正确
3. **数据库迁移**: 如果是已有数据库，需要 SQL 脚本重命名表
4. **API 版本**: 如果有 API 版本控制，考虑是否需要版本升级
5. **CI/CD**: 更新流水线中的任何硬编码路径
