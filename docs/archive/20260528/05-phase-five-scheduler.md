# 阶段五：定时任务调度

**日期**: 2026-05-20  
**预估时间**: 2 天  
**优先级**: 🟡 P1 - 重要功能  
**前置依赖**: 阶段一、二、四

---

## 我是如何考虑的

### 设计思路

定时任务是**任务中心的扩展**：

1. **复用任务执行器** - 定时触发后复用阶段四的任务执行逻辑
2. **Cron 表达式解析** - 使用成熟的 Cron 解析库
3. **调度器 BackgroundService** - 定期检查需要执行的定时任务
4. **执行历史记录** - ScheduledTaskExecution 表记录每次执行
5. **下次运行时间计算** - 展示给用户

### 复用设计

本阶段**大量复用阶段一、二、四的基础设施**，避免重复代码：

| 复用的组件 | 复用自 | 本阶段复用方式 |
|---------|-------|--------------|
| UserOwnedEntity | 阶段一 | ScheduledTaskEntity、ScheduledTaskExecutionEntity 继承基类 |
| BaseRepository<T> | 阶段一 | ScheduledTaskRepository、ScheduledTaskExecutionRepository 继承 |
| ApiResponse<T> | 阶段一 | ScheduledTask Endpoints 统一响应格式 |
| ExceptionHandlingMiddleware | 阶段一 | 全局异常处理自动生效 |
| PaginationHelper | 阶段一 | 定时任务列表分页查询 |
| UserRepository | 阶段二 | 获取当前用户信息 |
| JwtAuthMiddleware | 阶段二 | 认证保护所有定时任务相关端点 |
| TimeHelper | 阶段一 | 定时任务时间戳处理 |
| TaskService | 阶段四 | 复用任务执行逻辑 |
| TaskExecutor | 阶段四 | 复用任务执行器 |
| TaskRepository | 阶段四 | 复用任务仓储 |

### 本阶段新增的可复用功能

| 复用组件 | 被哪些阶段复用 |
|---------|--------------|
| ScheduledTaskRepository | 阶段五 |
| ScheduledTaskExecutionRepository | 阶段五 |
| ScheduledTaskService | 阶段五 |
| CronSchedulerService | 阶段五 |

### Cron 库选择

推荐使用 `Hangfire.Core` 或 `Quartz.NET`，MVP 阶段也可以用简单的 `CronExpressionDescriptor` + 自己实现调度逻辑。

---

## 本阶段复用的功能清单（来自其他阶段）

| 复用组件 | 文件路径 | 复用方式 | 避免重复代码 |
|---------|---------|---------|-----------|
| UserOwnedEntity | `Core/Entities/Base/UserOwnedEntity.cs` | ScheduledTaskEntity、ScheduledTaskExecutionEntity 继承 | 6 个属性定义（每个实体 3 个） |
| BaseRepository<T> | `Infrastructure/Repositories/Base/BaseRepository.cs` | ScheduledTaskRepository、ScheduledTaskExecutionRepository 继承 | 100+ 行 CRUD/软删除/分页代码 |
| ApiResponse<T> | `Core/Models/ApiResponse.cs` | ScheduledTask Endpoints 使用 | 避免 5 个端点重复写响应格式化 |
| ExceptionHandlingMiddleware | `Api/Middleware/ExceptionHandlingMiddleware.cs` | 全局自动生效 | 避免所有端点重复写异常处理 |
| PaginationHelper | `Infrastructure/Helpers/PaginationHelper.cs` | 定时任务列表分页 | 避免列表分页重复代码 |
| TimeHelper | `Infrastructure/Helpers/TimeHelper.cs` | 定时任务时间戳处理 | 避免时间处理重复代码 |
| UserRepository | `Infrastructure/Repositories/UserRepository.cs` | 获取当前用户 | 避免用户查询重复代码 |
| JwtAuthMiddleware | `Api/Middleware/JwtAuthMiddleware.cs` | 认证保护 | 避免认证重复代码 |
| TaskService | `Application/Services/TaskService.cs` | 复用任务执行逻辑 | 避免任务执行重复代码 |
| TaskExecutor | `Infrastructure/BackgroundServices/TaskExecutor.cs` | 复用任务执行器 | 避免任务执行重复代码 |
| TaskRepository | `Infrastructure/Repositories/TaskRepository.cs` | 复用任务仓储 | 避免任务查询重复代码 |

---

## 本阶段新增的可复用功能清单

| 复用组件 | 文件路径 | 说明 | 被复用次数（预估） |
|---------|---------|------|------------------|
| ScheduledTaskRepository | `Infrastructure/Repositories/ScheduledTaskRepository.cs` | 定时任务仓储 | 1 次 |
| ScheduledTaskExecutionRepository | `Infrastructure/Repositories/ScheduledTaskExecutionRepository.cs` | 定时任务执行仓储 | 1 次 |
| ScheduledTaskService | `Application/Services/ScheduledTaskService.cs` | 定时任务服务 | 1 次 |
| CronSchedulerService | `Infrastructure/BackgroundServices/CronSchedulerService.cs` | Cron 调度器服务 | 1 次 |

---

## 任务清单

| 编号 | 任务名称 | 文件路径 | 产出物 | 复用自 | 是否为复用功能 | 前置依赖 | 验证方式 |
|------|---------|---------|-------|------|-------------|---------|---------|
| **5.1** | 安装 Cron 解析 NuGet | `server/src/AutoCodeForge.Infrastructure/AutoCodeForge.Infrastructure.csproj` | 添加 Cron 相关包 | - | ❌ - | 阶段一 | 项目文件包含正确引用 |
| **5.2** | 创建 ScheduledTask DTO | `server/src/AutoCodeForge.Core/DTOs/ScheduledTask/` | CreateScheduledTaskRequest 等 DTO | - | ❌ - | 阶段一 | 代码编译 |
| **5.3** | 创建 ScheduledTaskRepository | `server/src/AutoCodeForge.Infrastructure/Repositories/ScheduledTaskRepository.cs` | ScheduledTaskRepository 类 | 阶段一 (BaseRepository) | ✅ 是 | 阶段二 | 代码编译 |
| **5.4** | 创建 ScheduledTaskExecutionRepository | `server/src/AutoCodeForge.Infrastructure/Repositories/ScheduledTaskExecutionRepository.cs` | ScheduledTaskExecutionRepository 类 | 阶段一 (BaseRepository) | ✅ 是 | 阶段二 | 代码编译 |
| **5.5** | 创建 ScheduledTaskService | `server/src/AutoCodeForge.Application/Services/ScheduledTaskService.cs` | ScheduledTaskService 类（CRUD+调度） | TaskService (阶段四) | ✅ 是 | 5.3, 5.4, 阶段四 | 代码编译 |
| **5.6** | 创建 Cron 调度器 BackgroundService | `server/src/AutoCodeForge.Infrastructure/BackgroundServices/CronSchedulerService.cs` | CronSchedulerService 类 | TaskExecutor (阶段四) | ✅ 是 | 5.1, 5.5 | 代码编译 |
| **5.7** | 创建 ScheduledTask Endpoints | `server/src/AutoCodeForge.Api/Endpoints/ScheduledTaskEndpoints.cs` | /api/v1/scheduled-tasks 相关端点 | ApiResponse | ❌ - | 5.2, 5.5 | 代码编译 |
| **5.8** | 注册调度相关服务 | `server/src/AutoCodeForge.Api/Program.cs` | 注册 ScheduledTaskService/CronSchedulerService | - | ❌ - | 5.5, 5.6 | 代码编译 |
| **5.9** | 验证定时任务功能 | - | 可以创建定时任务、Cron 触发、查看执行历史 | - | ❌ - | 5.1-5.8 | 测试 Cron 触发，验证任务执行 |

---

## 注意事项

⚠️ **重要提醒**

1. **Cron 表达式验证** - 确保用户输入的 Cron 表达式有效
2. **时区处理** - 注意服务器时区和用户时区的差异
3. **防重复触发** - 相同定时任务同一时间只触发一次
4. **执行超时处理** - 定时任务执行需要超时机制
5. **并发控制** - 多个定时任务同时触发时的资源竞争

✅ **验收标准**

- 可以创建和管理定时任务
- Cron 表达式可以正确解析
- 定时任务可以按 Cron 计划触发
- 任务执行历史可以正确记录
- 下次运行时间可以正确计算

---

## 阶段完成总结

### 复用收益

本阶段通过复用阶段一、二、四的基础设施，预计可以**避免 300+ 行重复代码**：
- 2 个实体无需重复写 CreatedAt/UpdatedAt/NtId/IsDeleted
- 2 个 Repository 无需重复写 CRUD/软删除/分页
- 5 个 API 端点无需重复写响应格式化
- 直接复用任务执行逻辑，避免大量代码

### 本阶段新增复用

本阶段创建了 4 个可复用组件。

### 下一步

完成本阶段后，进入 **阶段六：Git 仓库集成**，详见 `06-phase-six-git-integration.md`
