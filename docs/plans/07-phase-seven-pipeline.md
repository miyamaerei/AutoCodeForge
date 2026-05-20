# 阶段七：流水线模块

**日期**: 2026-05-20  
**预估时间**: 1-2 天  
**优先级**: 🟢 P2 - 一般功能  
**前置依赖**: 阶段一、二、六

---

## 我是如何考虑的

### 设计思路

流水线模块相对独立，主要是**CI/CD 流水线的配置和状态同步**：

1. **复用 Repository 关联** - 流水线关联到 Git 仓库
2. **外部流水线 ID** - 存储第三方 CI/CD 系统的流水线 ID
3. **状态定期同步** - BackgroundService 定期拉取最新状态
4. **构建历史记录** - Build 表记录每次构建

### 复用设计

本阶段**大量复用阶段一、二、六的基础设施**，避免重复代码：

| 复用的组件 | 复用自 | 本阶段复用方式 |
|---------|-------|--------------|
| UserOwnedEntity | 阶段一 | PipelineEntity、BuildEntity 继承基类 |
| BaseRepository<T> | 阶段一 | PipelineRepository、BuildRepository 继承 |
| ApiResponse<T> | 阶段一 | Pipeline Endpoints 统一响应格式 |
| ExceptionHandlingMiddleware | 阶段一 | 全局异常处理自动生效 |
| PaginationHelper | 阶段一 | 流水线列表分页查询 |
| UserRepository | 阶段二 | 获取当前用户信息 |
| JwtAuthMiddleware | 阶段二 | 认证保护所有流水线相关端点 |
| TimeHelper | 阶段一 | 流水线时间戳处理 |
| RepositoryRepository | 阶段六 | 复用仓库仓储 |
| RepositoryService | 阶段六 | 复用仓库服务 |
| IGitProvider | 阶段六 | 复用 Git 提供程序接口 |

### 本阶段新增的可复用功能

| 复用组件 | 被哪些阶段复用 |
|---------|--------------|
| PipelineRepository | 阶段七 |
| BuildRepository | 阶段七 |
| PipelineService | 阶段七 |
| PipelineSyncService | 阶段七 |

---

## 本阶段复用的功能清单（来自其他阶段）

| 复用组件 | 文件路径 | 复用方式 | 避免重复代码 |
|---------|---------|---------|-----------|
| UserOwnedEntity | `Core/Entities/Base/UserOwnedEntity.cs` | PipelineEntity、BuildEntity 继承 | 6 个属性定义（每个实体 3 个） |
| BaseRepository<T> | `Infrastructure/Repositories/Base/BaseRepository.cs` | PipelineRepository、BuildRepository 继承 | 100+ 行 CRUD/软删除/分页代码 |
| ApiResponse<T> | `Core/Models/ApiResponse.cs` | Pipeline Endpoints 使用 | 避免 5 个端点重复写响应格式化 |
| ExceptionHandlingMiddleware | `Api/Middleware/ExceptionHandlingMiddleware.cs` | 全局自动生效 | 避免所有端点重复写异常处理 |
| PaginationHelper | `Infrastructure/Helpers/PaginationHelper.cs` | 流水线列表分页 | 避免列表分页重复代码 |
| TimeHelper | `Infrastructure/Helpers/TimeHelper.cs` | 流水线时间戳处理 | 避免时间处理重复代码 |
| UserRepository | `Infrastructure/Repositories/UserRepository.cs` | 获取当前用户 | 避免用户查询重复代码 |
| JwtAuthMiddleware | `Api/Middleware/JwtAuthMiddleware.cs` | 认证保护 | 避免认证重复代码 |
| RepositoryRepository | `Infrastructure/Repositories/RepositoryRepository.cs` | 复用仓库仓储 | 避免仓库查询重复代码 |
| RepositoryService | `Application/Services/RepositoryService.cs` | 复用仓库服务 | 避免仓库服务重复代码 |
| IGitProvider | `Core/Interfaces/IGitProvider.cs` | 复用 Git 提供程序接口 | 避免接口定义重复代码 |

---

## 本阶段新增的可复用功能清单

| 复用组件 | 文件路径 | 说明 | 被复用次数（预估） |
|---------|---------|------|------------------|
| PipelineRepository | `Infrastructure/Repositories/PipelineRepository.cs` | 流水线仓储 | 1 次 |
| BuildRepository | `Infrastructure/Repositories/BuildRepository.cs` | 构建仓储 | 1 次 |
| PipelineService | `Application/Services/PipelineService.cs` | 流水线服务 | 1 次 |
| PipelineSyncService | `Infrastructure/BackgroundServices/PipelineSyncService.cs` | 流水线同步服务 | 1 次 |

---

## 任务清单

| 编号 | 任务名称 | 文件路径 | 产出物 | 复用自 | 是否为复用功能 | 前置依赖 | 验证方式 |
|------|---------|---------|-------|------|-------------|---------|---------|
| **7.1** | 创建 Pipeline DTO | `server/src/AutoCodeForge.Core/DTOs/Pipeline/` | CreatePipelineRequest 等 DTO | - | ❌ - | 阶段一 | 代码编译 |
| **7.2** | 创建 PipelineRepository | `server/src/AutoCodeForge.Infrastructure/Repositories/PipelineRepository.cs` | PipelineRepository 类 | 阶段一 (BaseRepository) | ✅ 是 | 阶段二 | 代码编译 |
| **7.3** | 创建 BuildRepository | `server/src/AutoCodeForge.Infrastructure/Repositories/BuildRepository.cs` | BuildRepository 类 | 阶段一 (BaseRepository) | ✅ 是 | 阶段二 | 代码编译 |
| **7.4** | 创建 PipelineService | `server/src/AutoCodeForge.Application/Services/PipelineService.cs` | PipelineService 类 | RepositoryService (阶段六) | ✅ 是 | 7.2, 7.3, 阶段六 | 代码编译 |
| **7.5** | 创建 Pipeline Endpoints | `server/src/AutoCodeForge.Api/Endpoints/PipelineEndpoints.cs` | /api/v1/pipelines 相关端点 | ApiResponse | ❌ - | 7.1, 7.4 | 代码编译 |
| **7.6** | 创建流水线状态同步 BackgroundService | `server/src/AutoCodeForge.Infrastructure/BackgroundServices/PipelineSyncService.cs` | PipelineSyncService 类 | - | ✅ 是 | 7.4 | 代码编译 |
| **7.7** | 注册流水线相关服务 | `server/src/AutoCodeForge.Api/Program.cs` | 注册 PipelineService/PipelineSyncService | - | ❌ - | 7.4, 7.6 | 代码编译 |
| **7.8** | 验证流水线功能 | - | 可以创建流水线、查看构建历史 | - | ❌ - | 7.1-7.7 | 测试完整流水线功能 |

---

## 注意事项

⚠️ **重要提醒**

1. **外部系统兼容性** - 不同 CI/CD 系统的 API 格式不同
2. **状态同步频率** - 不要太频繁，避免 API 限流
3. **错误处理** - 外部系统不可用时要有降级处理
4. **数据一致性** - 本地状态和外部系统状态要一致
5. **构建日志处理** - 大日志文件考虑分页或异步加载

✅ **验收标准**

- 可以创建和管理流水线
- 可以查看构建历史
- 流水线状态可以正确同步
- 可以触发流水线运行
- 可以查看构建日志

---

## 阶段完成总结

### 复用收益

本阶段通过复用阶段一、二、六的基础设施，预计可以**避免 250+ 行重复代码**：
- 2 个实体无需重复写 CreatedAt/UpdatedAt/NtId/IsDeleted
- 2 个 Repository 无需重复写 CRUD/软删除/分页
- 5 个 API 端点无需重复写响应格式化
- 复用仓库相关服务，避免大量代码

### 本阶段新增复用

本阶段创建了 4 个可复用组件。

### 下一步

完成本阶段后，进入 **阶段八：Wiki 模块**，详见 `08-phase-eight-wiki.md`
