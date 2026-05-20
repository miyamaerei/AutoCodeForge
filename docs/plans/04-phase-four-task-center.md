# 阶段四：任务中心模块

**日期**: 2026-05-20  
**预估时间**: 2-3 天  
**优先级**: 🟡 P1 - 重要功能  
**前置依赖**: 阶段一、二、三

---

## 我是如何考虑的

### 设计思路

任务中心是**AI 能力的实际应用**：

1. **复用 AgentExecutor** - 任务执行复用 AI 核心模块的 AgentExecutor
2. **异步解耦** - 任务创建后立即返回，后台执行
3. **状态管理** - TaskStatus 枚举（Pending/Running/Completed/Failed/Paused）
4. **任务日志** - 详细记录执行过程
5. **防重复执行** - 状态检查 + 锁机制

### 复用设计

本阶段**大量复用阶段一、二、三的基础设施**，避免重复代码：

| 复用的组件 | 复用自 | 本阶段复用方式 |
|---------|-------|--------------|
| UserOwnedEntity | 阶段一 | TaskEntity、TaskLogEntity 继承基类 |
| BaseRepository<T> | 阶段一 | TaskRepository、TaskLogRepository 继承 |
| ApiResponse<T> | 阶段一 | Task Endpoints 统一响应格式 |
| ExceptionHandlingMiddleware | 阶段一 | 全局异常处理自动生效 |
| PaginationHelper | 阶段一 | 任务列表分页查询 |
| UserRepository | 阶段二 | 获取当前用户信息 |
| JwtAuthMiddleware | 阶段二 | 认证保护所有任务相关端点 |
| JsonHelper | 阶段一 | 任务参数和结果的 JSON 序列化 |
| TimeHelper | 阶段一 | 任务时间戳处理 |
| LlmGateway | 阶段三 | 任务执行可能需要 LLM 调用 |
| AgentExecutor | 阶段三 | 任务执行直接复用 AgentExecutor |
| AgentService | 阶段三 | 获取任务绑定的 Agent |

### 本阶段新增的可复用功能

| 复用组件 | 被哪些阶段复用 |
|---------|--------------|
| TaskRepository | 阶段四、五 |
| TaskLogRepository | 阶段四、五 |
| TaskService | 阶段四、五 |
| TaskQueueService | 阶段四、五 |
| TaskExecutor | 阶段四、五 |

### 为什么这样安排？

- **复用优先** - 任务执行直接使用阶段三的 AgentExecutor
- **异步可靠** - BackgroundService 保证任务不会因为请求结束而中断

---

## 本阶段复用的功能清单（来自其他阶段）

| 复用组件 | 文件路径 | 复用方式 | 避免重复代码 |
|---------|---------|---------|-----------|
| UserOwnedEntity | `Core/Entities/Base/UserOwnedEntity.cs` | TaskEntity、TaskLogEntity 继承 | 6 个属性定义（每个实体 3 个） |
| BaseRepository<T> | `Infrastructure/Repositories/Base/BaseRepository.cs` | TaskRepository、TaskLogRepository 继承 | 100+ 行 CRUD/软删除/分页代码 |
| ApiResponse<T> | `Core/Models/ApiResponse.cs` | Task Endpoints 使用 | 避免 5 个端点重复写响应格式化 |
| ExceptionHandlingMiddleware | `Api/Middleware/ExceptionHandlingMiddleware.cs` | 全局自动生效 | 避免所有端点重复写异常处理 |
| PaginationHelper | `Infrastructure/Helpers/PaginationHelper.cs` | 任务列表分页 | 避免列表分页重复代码 |
| JsonHelper | `Infrastructure/Helpers/JsonHelper.cs` | 任务参数和结果 JSON 序列化 | 避免 JSON 处理重复代码 |
| TimeHelper | `Infrastructure/Helpers/TimeHelper.cs` | 任务时间戳处理 | 避免时间处理重复代码 |
| UserRepository | `Infrastructure/Repositories/UserRepository.cs` | 获取当前用户 | 避免用户查询重复代码 |
| JwtAuthMiddleware | `Api/Middleware/JwtAuthMiddleware.cs` | 认证保护 | 避免认证重复代码 |
| LlmGateway | `Infrastructure/AI/LlmGateway.cs` | 任务执行可能需要 LLM 调用 | 避免 LLM 调用重复代码 |
| AgentExecutor | `Infrastructure/AI/AgentExecutor.cs` | 任务执行直接复用 | 避免 Agent 执行重复代码 |
| AgentService | `Application/Services/AgentService.cs` | 获取任务绑定的 Agent | 避免 Agent 查询重复代码 |

---

## 本阶段新增的可复用功能清单

| 复用组件 | 文件路径 | 说明 | 被复用次数（预估） |
|---------|---------|------|------------------|
| TaskRepository | `Infrastructure/Repositories/TaskRepository.cs` | 任务仓储 | 2 次 |
| TaskLogRepository | `Infrastructure/Repositories/TaskLogRepository.cs` | 任务日志仓储 | 2 次 |
| TaskService | `Application/Services/TaskService.cs` | 任务服务 | 2 次 |
| TaskQueueService | `Infrastructure/BackgroundServices/TaskQueueService.cs` | 任务队列服务 | 2 次 |
| TaskExecutor | `Infrastructure/BackgroundServices/TaskExecutor.cs` | 任务执行器 | 2 次 |

---

## 任务清单

| 编号 | 任务名称 | 文件路径 | 产出物 | 复用自 | 是否为复用功能 | 前置依赖 | 验证方式 |
|------|---------|---------|-------|------|-------------|---------|---------|
| **4.1** | 创建 Task DTO | `server/src/AutoCodeForge.Core/DTOs/Task/` | CreateTaskRequest/UpdateTaskRequest/TaskResponse 等 | - | ❌ - | 阶段一 | 代码编译 |
| **4.2** | 创建 TaskRepository | `server/src/AutoCodeForge.Infrastructure/Repositories/TaskRepository.cs` | TaskRepository 类 | 阶段一 (BaseRepository) | ✅ 是 | 阶段二 | 代码编译 |
| **4.3** | 创建 TaskLogRepository | `server/src/AutoCodeForge.Infrastructure/Repositories/TaskLogRepository.cs` | TaskLogRepository 类 | 阶段一 (BaseRepository) | ✅ 是 | 阶段二 | 代码编译 |
| **4.4** | 创建 TaskService | `server/src/AutoCodeForge.Application/Services/TaskService.cs` | TaskService 类（CRUD+状态管理） | - | ✅ 是 | 4.2, 4.3, 阶段三 | 代码编译 |
| **4.5** | 创建任务队列 BackgroundService | `server/src/AutoCodeForge.Infrastructure/BackgroundServices/TaskQueueService.cs` | TaskQueueService 类（拉取待执行任务） | - | ✅ 是 | 4.4, 阶段三 | 代码编译 |
| **4.6** | 创建任务执行器 | `server/src/AutoCodeForge.Infrastructure/BackgroundServices/TaskExecutor.cs` | TaskExecutor 类（复用 AgentExecutor） | AgentExecutor (阶段三) | ✅ 是 | 4.5, 阶段三 | 代码编译 |
| **4.7** | 创建 Task Endpoints | `server/src/AutoCodeForge.Api/Endpoints/TaskEndpoints.cs` | /api/v1/tasks 相关端点 | ApiResponse | ❌ - | 4.1, 4.4 | 代码编译 |
| **4.8** | 注册任务相关服务 | `server/src/AutoCodeForge.Api/Program.cs` | 注册 TaskService/TaskQueueService 等 | - | ❌ - | 4.4-4.6 | 代码编译 |
| **4.9** | 验证任务功能 | - | 可以创建任务、查看状态、查看日志 | - | ❌ - | 4.1-4.8 | 使用 Swagger 测试完整任务流程 |

---

## 注意事项

⚠️ **重要提醒**

1. **任务状态流转** - 确保状态机定义正确
2. **防止重复执行** - 执行前检查状态，使用数据库锁或分布式锁
3. **异常处理** - 任务执行失败时记录详细错误信息
4. **超时处理** - 长时间运行的任务需要超时机制
5. **结果存储** - 大结果考虑压缩或分片存储

✅ **验收标准**

- 可以创建和管理任务
- 任务可以异步执行
- 任务状态正确更新
- 任务执行日志完整记录
- 任务执行结果正确保存

---

## 阶段完成总结

### 复用收益

本阶段通过复用阶段一、二、三的基础设施，预计可以**避免 300+ 行重复代码**：
- 2 个实体无需重复写 CreatedAt/UpdatedAt/NtId/IsDeleted
- 2 个 Repository 无需重复写 CRUD/软删除/分页
- 5 个 API 端点无需重复写响应格式化
- 直接复用 AgentExecutor，避免大量 AI 执行代码

### 本阶段新增复用

本阶段创建了 5 个可复用组件，将被后续阶段（五）使用。

### 下一步

完成本阶段后，进入 **阶段五：定时任务调度**，详见 `05-phase-five-scheduler.md`
