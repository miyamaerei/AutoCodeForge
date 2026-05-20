# 配置功能架构文档

## 概述

本文档详细描述了 AutoCodeForge 后端配置管理系统的整体架构设计，包括分层架构、核心组件、数据模型和数据流。

---

## 一、架构设计

### 1.1 分层架构

配置系统采用典型的三层架构模式：

```
┌─────────────────────────────────────────────────────────────┐
│                      API 层 (Presentation)                  │
│  ┌─────────────────┐  ┌─────────────────┐                  │
│  │  ConfigEndpoints │  │ HealthEndpoints │                  │
│  └────────┬────────┘  └─────────────────┘                  │
└───────────┼────────────────────────────────────────────────┘
            │
┌───────────▼────────────────────────────────────────────────┐
│                    应用层 (Application)                     │
│  ┌─────────────────┐                                        │
│  │   ConfigService │                                        │
│  │  (配置业务逻辑)  │                                        │
│  └────────┬────────┘                                        │
└───────────┼────────────────────────────────────────────────┘
            │
┌───────────▼────────────────────────────────────────────────┐
│                    基础设施层 (Infrastructure)               │
│  ┌─────────────────────┐  ┌─────────────────────┐          │
│  │GlobalConfigRepository│  │UserConfigRepository │          │
│  │     (全局配置仓储)    │  │    (用户配置仓储)    │          │
│  └─────────┬───────────┘  └──────────┬──────────┘          │
│            │                         │                      │
│            └───────────┬─────────────┘                      │
│                        ▼                                    │
│              ┌─────────────────┐                            │
│              │   SqlSugar ORM  │                            │
│              │   (数据库访问)   │                            │
│              └─────────────────┘                            │
└─────────────────────────────────────────────────────────────┘
```

### 1.2 模块划分

| 层级 | 组件 | 职责 |
|------|------|------|
| API层 | ConfigEndpoints | 处理HTTP请求，路由分发，权限校验 |
| API层 | HealthEndpoints | 健康检查端点，无需认证 |
| 应用层 | ConfigService | 配置业务逻辑，DTO转换，验证 |
| 基础设施层 | GlobalConfigRepository | 全局配置数据访问 |
| 基础设施层 | UserConfigRepository | 用户配置数据访问 |

---

## 二、核心组件设计

### 2.1 ConfigService

**职责**：统一管理全局配置和用户配置的业务逻辑

**核心方法**：

| 方法 | 功能 | 参数 | 返回值 |
|------|------|------|--------|
| `GetGlobalConfigAsync` | 获取单个全局配置 | `configKey`, `cancellationToken` | `ConfigResponse?` |
| `GetGlobalConfigsAsync` | 分页获取全局配置列表 | `page`, `pageSize`, `cancellationToken` | `PagedResult<ConfigResponse>` |
| `UpsertGlobalConfigAsync` | 创建/更新全局配置 | `request`, `cancellationToken` | `ConfigResponse` |
| `DeleteGlobalConfigAsync` | 删除全局配置 | `configKey`, `cancellationToken` | `Task` |
| `GetUserConfigAsync` | 获取单个用户配置 | `configKey`, `cancellationToken` | `ConfigResponse?` |
| `GetUserConfigsAsync` | 分页获取用户配置列表 | `page`, `pageSize`, `cancellationToken` | `PagedResult<ConfigResponse>` |
| `UpsertUserConfigAsync` | 创建/更新用户配置 | `request`, `cancellationToken` | `ConfigResponse` |
| `DeleteUserConfigAsync` | 删除用户配置 | `configKey`, `cancellationToken` | `Task` |
| `GetSandboxConfigAsync` | 获取沙盒配置 | `cancellationToken` | `SandboxConfigDto?` |
| `UpsertSandboxConfigAsync` | 创建/更新沙盒配置 | `sandboxConfig`, `cancellationToken` | `SandboxConfigDto` |

**设计特点**：
- **统一入口**：所有配置操作通过单一服务入口
- **验证集成**：内置输入验证，使用 `SandboxConfigValidator`
- **DTO 转换**：自动处理实体与响应DTO的映射
- **异常处理**：统一抛出业务异常，由中间件统一处理

### 2.2 Repository 设计

**GlobalConfigRepository** 和 **UserConfigRepository** 均继承自 `BaseRepository<T>`，复用基础 CRUD 操作。

**自定义方法**：
```csharp
public Task<GlobalConfigEntity?> GetByKeyAsync(string configKey, CancellationToken cancellationToken);
public Task<UserConfigEntity?> GetByKeyAsync(string configKey, CancellationToken cancellationToken);
```

**设计特点**：
- **继承复用**：继承 `BaseRepository<T>` 获取 CRUD、分页、软删除能力
- **用户隔离**：`UserConfigRepository` 自动过滤当前用户数据
- **异步优先**：所有方法均为异步实现

---

## 三、数据模型设计

### 3.1 数据库表结构

#### GlobalConfig 表

| 字段名 | 类型 | 约束 | 说明 |
|--------|------|------|------|
| `Id` | VARCHAR(36) | PRIMARY KEY | 配置ID（UUID） |
| `ConfigKey` | VARCHAR(255) | NOT NULL, UNIQUE | 配置键名 |
| `ConfigValue` | TEXT | NOT NULL | 配置值 |
| `Description` | VARCHAR(500) | NULL | 配置描述 |
| `CreatedAtUtc` | DATETIME | NOT NULL | 创建时间（UTC） |
| `UpdatedAtUtc` | DATETIME | NOT NULL | 更新时间（UTC） |
| `IsDeleted` | BIT | DEFAULT 0 | 软删除标记 |
| `DeletedAtUtc` | DATETIME | NULL | 删除时间 |

#### UserConfig 表

| 字段名 | 类型 | 约束 | 说明 |
|--------|------|------|------|
| `Id` | VARCHAR(36) | PRIMARY KEY | 配置ID（UUID） |
| `UserId` | VARCHAR(36) | NOT NULL, INDEX | 用户ID |
| `ConfigKey` | VARCHAR(255) | NOT NULL | 配置键名 |
| `ConfigValue` | TEXT | NOT NULL | 配置值 |
| `CreatedAtUtc` | DATETIME | NOT NULL | 创建时间（UTC） |
| `UpdatedAtUtc` | DATETIME | NOT NULL | 更新时间（UTC） |
| `IsDeleted` | BIT | DEFAULT 0 | 软删除标记 |
| `DeletedAtUtc` | DATETIME | NULL | 删除时间 |

**唯一约束**：`(UserId, ConfigKey)` - 同一用户的配置键名唯一

### 3.2 实体类设计

#### GlobalConfigEntity

```csharp
public class GlobalConfigEntity : AuditableEntity
{
    public string ConfigKey { get; set; }
    public string ConfigValue { get; set; }
    public string? Description { get; set; }
}
```

#### UserConfigEntity

```csharp
public class UserConfigEntity : UserOwnedEntity
{
    public string ConfigKey { get; set; }
    public string ConfigValue { get; set; }
}
```

**继承关系**：
- `GlobalConfigEntity` → `AuditableEntity`（继承创建/更新时间、软删除）
- `UserConfigEntity` → `UserOwnedEntity`（继承用户ID、创建/更新时间、软删除）

---

## 四、数据流设计

### 4.1 配置读取流程

```
客户端请求 → ConfigEndpoints → ConfigService → Repository → 数据库
                                    ↓
                               DTO转换
                                    ↓
                            ApiResponse包装
                                    ↓
                              返回客户端
```

### 4.2 配置写入流程

```
客户端请求 → ConfigEndpoints → 权限校验
                                    ↓
                              ValidateModel验证
                                    ↓
                            ConfigService.Upsert
                                    ↓
                              Repository.Create/Update
                                    ↓
                                 数据库
                                    ↓
                              DTO转换 + ApiResponse包装
                                    ↓
                              返回客户端
```

### 4.3 沙盒配置特殊处理

```
客户端请求 → ConfigEndpoints.UpsertSandboxConfig
                              ↓
                       ConfigService.UpsertSandboxConfig
                              ↓
                      SandboxConfigValidator.ValidateAndThrow
                              ↓
                        JSON序列化配置
                              ↓
                   UserConfigRepository.Upsert
                              ↓
                              数据库
```

---

## 五、权限控制设计

### 5.1 权限矩阵

| 资源 | Admin | Developer | Viewer | 匿名 |
|------|-------|-----------|--------|------|
| 全局配置 CRUD | ✅ | ❌ | ❌ | ❌ |
| 用户配置 CRUD | ✅ | ✅ | ✅ | ❌ |
| 沙盒配置 CRUD | ✅ | ✅ | ✅ | ❌ |
| 健康检查 | ✅ | ✅ | ✅ | ✅ |

### 5.2 权限校验实现

在 `ConfigEndpoints` 中通过 `IsAdmin()` 方法校验：

```csharp
private static bool IsAdmin(HttpContext context)
{
    var role = context.User.FindFirst("Role")?.Value;
    return role == "Admin";
}
```

**校验时机**：全局配置相关端点在处理请求前进行权限校验，非管理员返回 403 Forbidden。

---

## 六、异常处理设计

### 6.1 异常类型

| 异常类型 | 触发场景 | HTTP 状态码 |
|----------|----------|-------------|
| `ValidationException` | 参数验证失败 | 400 Bad Request |
| `NotFoundException` | 配置不存在 | 404 Not Found |
| 未授权访问 | 无权限访问资源 | 403 Forbidden |
| 数据库异常 | 数据库操作失败 | 500 Internal Server Error |

### 6.2 异常处理流程

```
异常抛出 → ExceptionHandlingMiddleware 捕获
                ↓
         判断异常类型
                ↓
         构建统一错误响应
                ↓
         返回对应HTTP状态码
```

---

## 七、API 端点汇总

### 7.1 端点列表

| 端点 | HTTP方法 | Controller | 功能描述 |
|------|----------|------------|----------|
| `/api/v1/configs/global` | GET | ConfigEndpoints | 获取全局配置列表 |
| `/api/v1/configs/global/{key}` | GET | ConfigEndpoints | 获取单个全局配置 |
| `/api/v1/configs/global` | POST | ConfigEndpoints | 创建全局配置 |
| `/api/v1/configs/global/{key}` | PUT | ConfigEndpoints | 更新全局配置 |
| `/api/v1/configs/global/{key}` | DELETE | ConfigEndpoints | 删除全局配置 |
| `/api/v1/configs/user` | GET | ConfigEndpoints | 获取用户配置列表 |
| `/api/v1/configs/user/{key}` | GET | ConfigEndpoints | 获取单个用户配置 |
| `/api/v1/configs/user` | POST | ConfigEndpoints | 创建用户配置 |
| `/api/v1/configs/user/{key}` | PUT | ConfigEndpoints | 更新用户配置 |
| `/api/v1/configs/user/{key}` | DELETE | ConfigEndpoints | 删除用户配置 |
| `/api/v1/configs/user/sandbox` | GET | ConfigEndpoints | 获取沙盒配置 |
| `/api/v1/configs/user/sandbox` | PUT | ConfigEndpoints | 更新沙盒配置 |
| `/health` | GET | HealthEndpoints | 综合健康检查 |
| `/health/live` | GET | HealthEndpoints | 存活检查 |
| `/health/ready` | GET | HealthEndpoints | 就绪检查 |

### 7.2 响应格式

**成功响应**：
```json
{
  "success": true,
  "message": "操作描述",
  "data": { /* 响应数据 */ }
}
```

**失败响应**：
```json
{
  "success": false,
  "message": "错误描述",
  "data": null
}
```

---

## 八、扩展性设计

### 8.1 配置缓存

当前实现直接访问数据库，可扩展为：

```
客户端请求 → ConfigService → 缓存层(Redis)
                              ↓ 未命中
                        Repository → 数据库
                              ↓
                         更新缓存
```

### 8.2 配置变更通知

可扩展配置变更事件机制：

```
配置变更 → 发布 ConfigChangedEvent
              ↓
         订阅者接收通知
              ↓
         执行相应处理逻辑
```

### 8.3 配置版本管理

可扩展配置版本历史表，支持回滚：

| 字段名 | 类型 | 说明 |
|--------|------|------|
| `Id` | VARCHAR(36) | 版本记录ID |
| `ConfigId` | VARCHAR(36) | 关联配置ID |
| `ConfigValue` | TEXT | 历史值 |
| `ChangedAtUtc` | DATETIME | 变更时间 |
| `ChangedBy` | VARCHAR(36) | 变更人 |

---

## 九、安全考虑

### 9.1 敏感信息保护

- 沙盒配置中的路径信息不对外暴露完整路径
- 日志输出时对敏感信息进行脱敏（`maskSecretsInLogs`）

### 9.2 输入验证

- 所有输入参数进行严格验证
- 路径参数防止路径遍历攻击
- 配置值长度限制

### 9.3 审计日志

可扩展审计日志记录：
- 记录配置变更操作
- 记录操作人、操作时间、变更内容

---

## 十、部署与集成

### 10.1 服务注册

在 `Program.cs` 中注册配置相关服务：

```csharp
builder.Services.AddScoped<ConfigService>();
builder.Services.AddScoped<GlobalConfigRepository>();
builder.Services.AddScoped<UserConfigRepository>();
builder.Services.AddScoped<SandboxConfigValidator>();
```

### 10.2 端点映射

```csharp
app.MapConfigEndpoints();
app.MapHealthEndpoints();
```

---

## 十一、总结

配置管理系统采用分层架构设计，实现了：

1. **全局配置与用户配置分离**：管理员管理全局配置，用户管理个人配置
2. **权限控制**：全局配置仅管理员可操作
3. **统一响应格式**：所有 API 响应使用 `ApiResponse<T>` 包装
4. **异常统一处理**：通过中间件统一处理异常
5. **沙盒配置特殊支持**：提供专门的沙盒配置管理接口
6. **健康检查**：提供三个健康检查端点支持容器编排

该设计具有良好的扩展性，可根据需求扩展缓存、版本管理、变更通知等功能。