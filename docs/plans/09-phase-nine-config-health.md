# 阶段九：系统配置 & 健康检查

**日期**: 2026-05-20  
**预估时间**: 1 天  
**优先级**: 🟢 P2 - 完善功能  
**前置依赖**: 阶段一、二

---

## 我是如何考虑的

### 设计思路

配置管理和健康检查是**系统运维的基础**：

1. **统一配置服务** - ConfigService 统一管理 GlobalConfig 和 UserConfig
2. **健康检查端点** - /health 检查数据库连接和服务状态
3. **系统信息端点** - /system/info 显示版本、环境等信息
4. **结构化日志** - 配置 Serilog 或内置 ILogger

### 复用设计

本阶段**大量复用阶段一、二的基础设施**，避免重复代码：

| 复用的组件 | 复用自 | 本阶段复用方式 |
|---------|-------|--------------|
| AuditableEntity | 阶段一 | GlobalConfigEntity、UserConfigEntity 继承基类 |
| UserOwnedEntity | 阶段一 | UserConfigEntity 继承基类 |
| BaseRepository<T> | 阶段一 | GlobalConfigRepository、UserConfigRepository 继承 |
| ApiResponse<T> | 阶段一 | Config Endpoints 统一响应格式 |
| ExceptionHandlingMiddleware | 阶段一 | 全局异常处理自动生效 |
| UserRepository | 阶段二 | 获取当前用户信息 |
| JwtAuthMiddleware | 阶段二 | 认证保护所有配置相关端点 |
| TimeHelper | 阶段一 | 配置时间戳处理 |

### 本阶段新增的可复用功能

| 复用组件 | 被哪些阶段复用 |
|---------|--------------|
| GlobalConfigRepository | 阶段九 |
| UserConfigRepository | 阶段九 |
| ConfigService | 阶段九 |

---

## 本阶段复用的功能清单（来自其他阶段）

| 复用组件 | 文件路径 | 复用方式 | 避免重复代码 |
|---------|---------|---------|-----------|
| AuditableEntity | `Core/Entities/Base/AuditableEntity.cs` | GlobalConfigEntity、UserConfigEntity 继承 | 4 个属性定义（每个实体 2 个） |
| UserOwnedEntity | `Core/Entities/Base/UserOwnedEntity.cs` | UserConfigEntity 继承 | 3 个属性定义 |
| BaseRepository<T> | `Infrastructure/Repositories/Base/BaseRepository.cs` | GlobalConfigRepository、UserConfigRepository 继承 | 100+ 行 CRUD/软删除/分页代码 |
| ApiResponse<T> | `Core/Models/ApiResponse.cs` | Config Endpoints 使用 | 避免 5 个端点重复写响应格式化 |
| ExceptionHandlingMiddleware | `Api/Middleware/ExceptionHandlingMiddleware.cs` | 全局自动生效 | 避免所有端点重复写异常处理 |
| TimeHelper | `Infrastructure/Helpers/TimeHelper.cs` | 配置时间戳处理 | 避免时间处理重复代码 |
| UserRepository | `Infrastructure/Repositories/UserRepository.cs` | 获取当前用户 | 避免用户查询重复代码 |
| JwtAuthMiddleware | `Api/Middleware/JwtAuthMiddleware.cs` | 认证保护 | 避免认证重复代码 |

---

## 本阶段新增的可复用功能清单

| 复用组件 | 文件路径 | 说明 | 被复用次数（预估） |
|---------|---------|------|------------------|
| GlobalConfigRepository | `Infrastructure/Repositories/GlobalConfigRepository.cs` | 全局配置仓储 | 1 次 |
| UserConfigRepository | `Infrastructure/Repositories/UserConfigRepository.cs` | 用户配置仓储 | 1 次 |
| ConfigService | `Application/Services/ConfigService.cs` | 配置服务 | 1 次 |

---

## 任务清单

| 编号 | 任务名称 | 文件路径 | 产出物 | 复用自 | 是否为复用功能 | 前置依赖 | 验证方式 |
|------|---------|---------|-------|------|-------------|---------|---------|
| **9.1** | 创建 Config DTO | `server/src/AutoCodeForge.Core/DTOs/Config/` | UpdateConfigRequest 等 DTO | - | ❌ - | 阶段一 | 代码编译 |
| **9.2** | 创建 ConfigService | `server/src/AutoCodeForge.Application/Services/ConfigService.cs` | ConfigService 类（统一配置管理） | - | ✅ 是 | 阶段二 | 代码编译 |
| **9.3** | 创建 Config Endpoints | `server/src/AutoCodeForge.Api/Endpoints/ConfigEndpoints.cs` | /api/v1/config 相关端点 | ApiResponse | ❌ - | 9.1, 9.2 | 代码编译 |
| **9.4** | 创建健康检查端点 | `server/src/AutoCodeForge.Api/Endpoints/HealthEndpoints.cs` | /health 端点 | - | ❌ - | 阶段一 | 代码编译，检查数据库连接 |
| **9.5** | 创建系统信息端点 | `server/src/AutoCodeForge.Api/Endpoints/SystemEndpoints.cs` | /system/info 端点 | - | ❌ - | 阶段一 | 代码编译，返回版本、环境等 |
| **9.6** | 配置结构化日志 | `server/src/AutoCodeForge.Api/Program.cs` | Serilog 或 ILogger 配置 | - | ❌ - | 阶段一 | 代码编译，日志输出正常 |
| **9.7** | 注册配置相关服务 | `server/src/AutoCodeForge.Api/Program.cs` | 注册 ConfigService | - | ❌ - | 9.2 | 代码编译 |
| **9.8** | 验证配置和健康检查功能 | - | 配置可以读写、健康检查正常、日志正常输出 | - | ❌ - | 9.1-9.7 | 测试所有相关端点 |

---

## 注意事项

⚠️ **重要提醒**

1. **敏感配置** - 不要在 /system/info 端点暴露敏感信息
2. **配置缓存** - 全局配置可以考虑缓存，提升性能
3. **日志级别** - 生产环境应该使用 Warning 或 Error 级别
4. **健康检查深度** - 可以根据需要添加更多检查项（Redis 等）
5. **配置验证** - 修改配置时应该验证配置值的合法性

✅ **验收标准**

- 可以读取全局配置
- 可以修改全局配置（管理员）
- 可以读取和修改用户配置
- 健康检查端点正常工作
- 系统信息端点正常工作
- 日志输出正常

---

## 阶段完成总结

### 复用收益

本阶段通过复用阶段一、二的基础设施，预计可以**避免 250+ 行重复代码**：
- 2 个实体无需重复写 CreatedAt/UpdatedAt
- 1 个实体无需重复写 NtId/IsDeleted
- 2 个 Repository 无需重复写 CRUD/软删除/分页
- 5 个 API 端点无需重复写响应格式化

### 本阶段新增复用

本阶段创建了 3 个可复用组件。

### 下一步

完成本阶段后，进入 **阶段十：测试 & 优化**，详见 `10-phase-ten-testing.md`
