# 阶段十五：配置重构

**日期**: 2026-05-20
**预估时间**: 3-4 天
**优先级**: 🟡 P1 - 核心功能
**前置依赖**: 阶段一、二
**文档依据**: `docs/config-management-optimization.md`、`docs/frontend-system-config-analysis.md`

---

## 我是如何考虑的

### 问题分析

原方案存在的问题：
1. ❌ 每个配置模块创建独立实体，导致表数量爆炸
2. ❌ 扩展性差，新增配置类型需要修改代码结构
3. ❌ 违反 DRY 原则，大量重复代码
4. ❌ **缺少默认配置初始化** - 用户登录后没有默认配置，导致空白状态
5. ❌ **缺少配置模板机制** - 无法为不同场景提供预设配置

### 优化方案：统一配置表设计

参考业界最佳实践，采用 **单表 key-value + JSON 值存储** 的设计模式：

```
┌─────────────────────────────────────────────────────────────────┐
│                  ConfigurationEntry 统一配置表                   │
├─────────────┬─────────────┬─────────────┬─────────────────────┤
│   字段名    │    类型      │    说明     │        约束         │
├─────────────┼─────────────┼─────────────┼─────────────────────┤
│ Id          │ Guid        │ 主键        │ PRIMARY KEY         │
│ ConfigType  │ Enum        │ 配置类型    │ NOT NULL            │
│ ConfigKey   │ VARCHAR     │ 配置键      │ NOT NULL, UNIQUE    │
│ ConfigValue │ TEXT(JSON)  │ 配置值      │ NOT NULL            │
│ NtId        │ VARCHAR(64) │ NTID        │ NULL=全局配置       │
│ IsEncrypted │ Boolean     │ 是否加密    │ DEFAULT false       │
│ IsEnabled   │ Boolean     │ 是否启用    │ DEFAULT true        │
│ Description │ VARCHAR     │ 描述说明    │                     │
│ Group       │ VARCHAR     │ 逻辑分组    │                     │
│ CreatedAt   │ DateTime    │ 创建时间    │                     │
│ UpdatedAt   │ DateTime    │ 更新时间    │                     │
│ CreatedBy   │ VARCHAR     │ 创建人      │                     │
│ UpdatedBy   │ VARCHAR     │ 更新人      │                     │
└─────────────┴─────────────┴─────────────┴─────────────────────┘
```

### 设计思路

1. **单一配置表**：所有配置（Global/User/Sandbox/Preferences 等）都存储在一张表中
2. **ConfigType 枚举**：区分配置类型（Global, User, Sandbox, Preferences, Workflow, Knowledge 等）
3. **ConfigKey**：唯一标识，采用命名规范 `{module}.{name}` 如 `sandbox.workspace_root`
4. **ConfigValue**：JSON 格式存储，支持复杂结构
5. **NtId**：空表示全局配置，非空表示用户级配置
6. **IsEncrypted**：标记敏感配置（如 API 密钥）需要加密存储

### 架构层次

```
┌─────────────────────────────────────────────────────────────┐
│                    配置管理端点层                            │
├─────────────────────────────────────────────────────────────┤
│  /api/v1/configs/{type}      - CRUD 端点                   │
│  /api/v1/configs/history     - 配置历史                     │
│  /api/v1/configs/export      - 导出配置                    │
│  /api/v1/configs/import      - 导入配置                    │
│  /api/v1/configs/batch       - 批量操作                    │
└─────────────────────────────────────────────────────────────┘
                              │
┌─────────────────────────────────────────────────────────────┐
│                    配置服务层                                │
├─────────────────────────────────────────────────────────────┤
│  ConfigService         - 统一配置服务（CRUD）               │
│  ConfigHistoryService  - 配置历史服务                       │
│  ConfigExportService   - 导入导出服务                       │
│  EncryptionService     - 敏感配置加密服务                   │
└─────────────────────────────────────────────────────────────┘
                              │
┌─────────────────────────────────────────────────────────────┐
│                    配置仓储层                                │
├─────────────────────────────────────────────────────────────┤
│  ConfigRepository      - 统一配置仓储                       │
│  ConfigHistoryRepository - 配置历史仓储                    │
└─────────────────────────────────────────────────────────────┘
                              │
┌─────────────────────────────────────────────────────────────┐
│                    数据库层                                  │
├─────────────────────────────────────────────────────────────┤
│  configuration_entries    - 统一配置表                      │
│  configuration_history    - 配置历史表                      │
└─────────────────────────────────────────────────────────────┘
```

---

### 默认配置初始化机制

**核心原则**：用户首次登录时，系统自动初始化默认配置，确保用户体验连贯。

#### 初始化触发时机

| 场景 | 触发条件 | 初始化内容 |
|------|----------|-----------|
| 新用户注册 | 用户创建成功 | 用户级默认配置（Preferences, Sandbox, Workflow 等） |
| 新租户创建 | 租户创建成功 | 全局默认配置 + 所有用户的默认配置模板 |
| 首次访问配置页 | 用户打开某个配置模块 | 仅初始化该模块的默认配置（懒加载） |
| 手动重置 | 用户点击"恢复默认" | 将指定模块配置重置为默认值 |

#### 初始化服务设计

```
┌─────────────────────────────────────────────────────────────┐
│                 ConfigInitializationService                  │
├─────────────────────────────────────────────────────────────┤
│  + InitializeUserDefaults()           - 初始化用户默认配置（自动获取当前用户NTID） │
│  + InitializeTenantDefaults()         - 初始化全局默认配置 │
│  + InitializeModuleDefaults(module)   - 按模块懒加载（自动获取当前用户NTID） │
│  + ResetToDefaults(module)            - 重置模块配置（自动获取当前用户NTID） │
│  + GetConfigTemplate(templateName)      - 获取配置模板       │
└─────────────────────────────────────────────────────────────┘
```

#### 配置模板机制

支持预定义配置模板，新用户可选择模板快速初始化：

| 模板名称 | 适用场景 | 说明 |
|----------|----------|------|
| `default` | 默认模板 | 平衡模式，适合大多数用户 |
| `strict` | 高安全要求 | 严格沙盒策略，适合高安全场景 |
| `developer` | 开发者模式 | 宽松策略，适合快速迭代 |
| `enterprise` | 企业模板 | 完整配置，适合大规模团队 |

---

### 前端配置模块映射

根据 `docs/frontend-system-config-analysis.md`，后端需要支持以下 15 个配置模块：

| 序号 | 模块名称 | ConfigType | 说明 |
|------|----------|------------|------|
| 1 | Preferences | `Preferences` | 用户偏好（语言、主题、时区） |
| 2 | Repositories | `Repository` | 仓库集成配置 |
| 3 | Knowledge | `Knowledge` | 知识库管理 |
| 4 | Skill | `Skill` | 技能配置 |
| 5 | Schedules | `Schedule` | 定时任务 |
| 6 | DeepWiki | `DeepWiki` | 向量索引配置 |
| 7 | Review | `Review` | 代码评审 |
| 8 | Integrations | `Integration` | 第三方集成 |
| 9 | Notifications | `Notification` | 通知策略 |
| 10 | Sandbox | `Sandbox` | 沙盒执行配置 |
| 11 | Workflow | `Workflow` | 流程编排 |
| 12 | API | `ApiKey` | API密钥管理 |
| 13 | Models | `Model` | 模型选择 |
| 14 | Users | `User` | 用户管理配置 |
| 15 | System（旧） | `Global` | 全局系统配置 |

---

## 本阶段复用的功能清单（来自其他阶段）

| 复用组件 | 文件路径 | 复用方式 | 避免重复代码 |
|---------|---------|---------|-------------|
| AuditableEntity | `Core/Entities/Base/AuditableEntity.cs` | ConfigurationEntry 继承 | 4 个属性定义 |
| BaseRepository<T> | `Infrastructure/Repositories/Base/BaseRepository.cs` | ConfigRepository 继承 | 100+ 行 CRUD/软删除/分页代码 |
| ApiResponse<T> | `Core/Models/ApiResponse.cs` | 所有配置端点使用 | 避免 15+ 端点重复写响应格式化 |
| ExceptionHandlingMiddleware | `Api/Middleware/ExceptionHandlingMiddleware.cs` | 全局自动生效 | 避免所有端点重复写异常处理 |
| UserRepository | `Infrastructure/Repositories/UserRepository.cs` | 获取当前用户 | 避免用户查询重复代码 |
| JwtAuthMiddleware | `Api/Middleware/JwtAuthMiddleware.cs` | 认证保护 | 避免认证重复代码 |
| JsonHelper | `Core/Helpers/JsonHelper.cs` | 配置序列化 | 避免序列化重复代码 |

---

## 本阶段新增的可复用功能清单

| 复用组件 | 文件路径 | 说明 | 被复用次数（预估） |
|---------|---------|------|------------------|
| ConfigType (enum) | `Core/Enums/ConfigType.cs` | 配置类型枚举（15个模块） | 10+ 次 |
| ConfigurationEntry | `Core/Entities/ConfigurationEntry.cs` | 统一配置实体 | 1 次 |
| ConfigRepository | `Infrastructure/Repositories/ConfigRepository.cs` | 统一配置仓储 | 1 次 |
| ConfigService | `Application/Services/ConfigService.cs` | 统一配置服务 | 1 次 |
| ConfigHistoryEntity | `Core/Entities/ConfigHistoryEntity.cs` | 配置历史实体 | 1 次 |
| ConfigHistoryRepository | `Infrastructure/Repositories/ConfigHistoryRepository.cs` | 配置历史仓储 | 1 次 |
| ConfigHistoryService | `Application/Services/ConfigHistoryService.cs` | 配置历史服务 | 1 次 |
| EncryptionService | `Application/Services/EncryptionService.cs` | 加密服务 | 1 次 |
| ConfigInitializationService | `Application/Services/ConfigInitializationService.cs` | 配置初始化服务 | 1 次 |
| ConfigDefaultsProvider | `Core/Providers/ConfigDefaultsProvider.cs` | 默认配置提供器 | 1 次 |

---

## 任务清单

### 阶段15-A：统一配置实体与枚举

| 编号 | 任务名称 | 文件路径 | 产出物 | 复用自 | 是否为复用功能 | 前置依赖 | 验证方式 |
|------|---------|---------|-------|------|-------------|---------|---------|
| **15-A.1** | 更新 ConfigType 枚举 | `Core/Enums/ConfigType.cs` | 15个配置类型枚举值 | - | ✅ 是 | 阶段一 | 代码编译 |
| **15-A.2** | 创建 ConfigurationEntry 实体 | `Core/Entities/ConfigurationEntry.cs` | 统一配置实体（继承 AuditableEntity） | AuditableEntity | ✅ 是 | 15-A.1 | 代码编译 |
| **15-A.3** | 创建 ConfigHistoryEntity 实体 | `Core/Entities/ConfigHistoryEntity.cs` | 配置历史实体 | AuditableEntity | ✅ 是 | 15-A.2 | 代码编译 |

### 阶段15-B：配置仓储层

| 编号 | 任务名称 | 文件路径 | 产出物 | 复用自 | 是否为复用功能 | 前置依赖 | 验证方式 |
|------|---------|---------|-------|------|-------------|---------|---------|
| **15-B.1** | 创建 ConfigRepository | `Infrastructure/Repositories/ConfigRepository.cs` | 统一配置仓储（按类型/用户查询） | BaseRepository | ✅ 是 | 15-A.2 | 代码编译 |
| **15-B.2** | 创建 ConfigHistoryRepository | `Infrastructure/Repositories/ConfigHistoryRepository.cs` | 配置历史仓储 | BaseRepository | ✅ 是 | 15-A.3 | 代码编译 |

### 阶段15-C：配置服务层

| 编号 | 任务名称 | 文件路径 | 产出物 | 复用自 | 是否为复用功能 | 前置依赖 | 验证方式 |
|------|---------|---------|-------|------|-------------|---------|---------|
| **15-C.1** | 创建 EncryptionService | `Application/Services/EncryptionService.cs` | 敏感配置加密服务（AES-256） | - | ✅ 是 | 阶段一 | 代码编译 + 加密验证 |
| **15-C.2** | 创建 ConfigService | `Application/Services/ConfigService.cs` | 统一配置服务（CRUD + 加密 + 历史记录） | EncryptionService | ✅ 是 | 15-B.1, 15-C.1 | 代码编译 |
| **15-C.3** | 创建 ConfigHistoryService | `Application/Services/ConfigHistoryService.cs` | 配置历史服务（查询、回滚） | - | ✅ 是 | 15-B.2 | 代码编译 |
| **15-C.4** | 创建 ConfigExportService | `Application/Services/ConfigExportService.cs` | 配置导入导出服务（JSON格式） | JsonHelper | ✅ 是 | 15-C.2 | 代码编译 |
| **15-C.5** | 创建 ConfigDefaultsProvider | `Core/Providers/ConfigDefaultsProvider.cs` | 默认配置提供器（15个模块默认值） | - | ✅ 是 | 15-A.1 | 代码编译 |
| **15-C.6** | 创建 ConfigInitializationService | `Application/Services/ConfigInitializationService.cs` | 配置初始化服务 | ConfigDefaultsProvider | ✅ 是 | 15-C.5 | 代码编译 + 初始化验证 |

### 阶段15-D：配置端点层

| 编号 | 任务名称 | 文件路径 | 产出物 | 复用自 | 是否为复用功能 | 前置依赖 | 验证方式 |
|------|---------|---------|-------|------|-------------|---------|---------|
| **15-D.1** | 创建 ConfigDto（请求/响应） | `Core/DTOs/Config/` | ConfigRequest、ConfigResponse、BatchConfigRequest | - | ❌ | 阶段一 | 代码编译 |
| **15-D.2** | 创建 ConfigEndpoints | `Api/Endpoints/ConfigEndpoints.cs` | /api/v1/configs/{type} CRUD 端点 | ApiResponse | ❌ | 15-C.2 | 代码编译 + API测试 |
| **15-D.3** | 添加历史/导入导出端点 | `Api/Endpoints/ConfigEndpoints.cs` | /history、/export、/import、/batch 端点 | - | ❌ | 15-C.3, 15-C.4 | 代码编译 + API测试 |
| **15-D.4** | 创建初始化端点 | `Api/Endpoints/ConfigEndpoints.cs` | /init、/reset 端点 | - | ❌ | 15-C.6 | 代码编译 + API测试 |

### 阶段15-E：健康检查与系统信息

| 编号 | 任务名称 | 文件路径 | 产出物 | 复用自 | 是否为复用功能 | 前置依赖 | 验证方式 |
|------|---------|---------|-------|------|-------------|---------|---------|
| **15-E.1** | 创建 HealthEndpoints | `Api/Endpoints/HealthEndpoints.cs` | /health、/health/live、/health/ready | - | ❌ | 阶段一 | 代码编译 + 健康检查验证 |
| **15-E.2** | 创建 SystemEndpoints | `Api/Endpoints/SystemEndpoints.cs` | /system/info 端点 | - | ❌ | 阶段一 | 代码编译 + 返回值验证 |

### 阶段15-F：集成与验证

| 编号 | 任务名称 | 文件路径 | 产出物 | 复用自 | 是否为复用功能 | 前置依赖 | 验证方式 |
|------|---------|---------|-------|------|-------------|---------|---------|
| **15-F.1** | 注册配置服务到 DI | `Api/Program.cs` | 依赖注入配置 | - | ❌ | 15-C.2 | 代码编译 |
| **15-F.2** | 用户创建时触发初始化 | `Application/Services/UserService.cs` | 新用户自动初始化默认配置 | - | ❌ | 15-C.6 | 集成测试 |
| **15-F.3** | 验证统一配置 CRUD | - | 全局/用户/沙盒配置读写正常 | - | ❌ | 15-D.2 | API测试 |
| **15-F.4** | 验证配置历史与回滚 | - | 配置变更自动记录、支持回滚 | - | ❌ | 15-D.3 | API测试 |
| **15-F.5** | 验证导入导出功能 | - | JSON格式导入导出正常 | - | ❌ | 15-D.3 | API测试 |
| **15-F.6** | 验证敏感配置加密 | - | API密钥等敏感字段加密存储 | - | ❌ | 15-C.1 | 数据库验证 |
| **15-F.7** | 验证默认配置初始化 | - | 新用户登录后配置自动初始化 | - | ❌ | 15-F.2 | 集成测试 |
| **15-F.8** | 验证配置重置功能 | - | 恢复默认配置正常 | - | ❌ | 15-D.4 | API测试 |

---

## 配置类型枚举设计

```csharp
public enum ConfigType
{
    Global,          // 全局配置（管理员）
    User,            // 用户配置（个人）
    Preferences,     // 偏好设置（语言、时区、主题）
    Repository,      // 仓库集成配置
    Knowledge,       // 知识库配置
    Skill,           // 技能配置
    Schedule,        // 定时任务配置
    DeepWiki,        // 向量索引配置
    Review,          // 代码评审配置
    Integration,     // 第三方集成配置
    Notification,    // 通知策略配置
    Sandbox,         // 沙盒执行配置
    Workflow,        // 工作流配置
    ApiKey,          // API密钥配置
    Model,           // AI模型配置
    System           // 系统配置（旧版兼容）
}
```

---

## 配置 Key 命名规范

```
{config_type}.{module}.{name}

示例：
- global.system.version
- global.security.max_login_attempts
- user.preferences.theme
- user.preferences.locale
- sandbox.execution.timeout
- workflow.pipeline.default_branch
- knowledge.source.refresh_interval
- model.minimax.api_key
- apikey.github.token
```

---

## 权限控制矩阵

| 配置类型 | Admin | Developer | Viewer |
|----------|-------|-----------|--------|
| Global | ✅ CRUD | ❌ | ❌ |
| User | ✅ CRUD | ✅ CRUD(own) | ❌ |
| Sandbox | ✅ CRUD | ✅ R/W(own) | ❌ |
| Preferences | ✅ CRUD | ✅ CRUD(own) | ✅ R |
| Workflow | ✅ CRUD | ✅ R/W | ✅ R |
| Knowledge | ✅ CRUD | ✅ R/W | ✅ R |
| ApiKey | ✅ CRUD | ❌ | ❌ |
| Model | ✅ CRUD | ✅ R/W | ✅ R |
| History | ✅ R | ❌ | ❌ |

---

## API 端点汇总

| HTTP方法 | 端点 | 业务描述 | 权限 |
|----------|------|----------|------|
| GET | `/api/v1/configs/{configType}` | 获取指定类型配置列表 | 按类型控制 |
| GET | `/api/v1/configs/{configType}/{configKey}` | 获取单个配置 | 按类型控制 |
| POST | `/api/v1/configs/{configType}` | 创建配置 | 按类型控制 |
| PUT | `/api/v1/configs/{configType}/{configKey}` | 更新配置 | 按类型控制 |
| DELETE | `/api/v1/configs/{configType}/{configKey}` | 删除配置 | 按类型控制 |
| POST | `/api/v1/configs/{configType}/init` | 初始化指定类型配置 | 用户自身 |
| POST | `/api/v1/configs/{configType}/reset` | 重置指定类型配置为默认 | 用户自身 |
| GET | `/api/v1/configs/{configType}/defaults` | 获取指定类型的默认配置模板 | Public |
| GET | `/api/v1/configs/history` | 获取配置变更历史 | Admin |
| POST | `/api/v1/configs/history/{id}/rollback` | 回滚到历史版本 | Admin |
| GET | `/api/v1/configs/{configType}/export` | 导出指定类型配置 | Admin |
| POST | `/api/v1/configs/{configType}/import` | 导入配置 | Admin |
| POST | `/api/v1/configs/batch` | 批量操作配置 | Admin |
| GET | `/health` | 健康检查 | Public |
| GET | `/health/live` | 存活检查 | Public |
| GET | `/health/ready` | 就绪检查 | Public |
| GET | `/system/info` | 系统信息 | Public |

---

## 默认配置数据模板

根据前端 15 个配置模块，系统提供以下默认配置：

### 1. Preferences（偏好设置）

```json
{
  "configKey": "user.preferences.main",
  "configType": "Preferences",
  "configValue": {
    "locale": "zh-CN",
    "timezone": "Asia/Shanghai",
    "theme": "light",
    "landingPage": "/",
    "enableInAppNotice": true,
    "enableEmailNotice": false
  }
}
```

### 2. Sandbox（沙盒执行）

```json
{
  "configKey": "user.sandbox.default",
  "configType": "Sandbox",
  "configValue": {
    "profileName": "default-sandbox",
    "workspaceRootPath": "C:/gitrepos",
    "artifactOutputPath": ".sandbox-artifacts",
    "allowedWritePaths": "src/**\ndocs/**",
    "ignoredPaths": "node_modules/**\ndist/**\n.git/**",
    "executionMode": "dry-run",
    "approvalMode": "strict",
    "maxParallelTasks": 3,
    "commandTimeoutSec": 300,
    "allowWriteOps": false,
    "allowNetworkAccess": false,
    "storeTerminalLogs": true,
    "maskSecretsInLogs": true,
    "defaultModel": "gpt-4",
    "fallbackModel": "gpt-3.5-turbo",
    "promptGuardrail": "先分析风险，再执行最小改动。"
  }
}
```

### 3. Workflow（流程编排）

```json
{
  "configKey": "user.workflow.team-default",
  "configType": "Workflow",
  "configValue": {
    "profileName": "team-default",
    "commandTimeoutSec": 300,
    "maxParallelTasks": 3,
    "requireApprovalForWrite": true,
    "autoCreateTodo": true,
    "outputStyle": "standard",
    "stages": {
      "initialization": {
        "mode": "default",
        "mustAskClarifying": true,
        "useRepoSearch": true,
        "runValidation": true,
        "preloadInstructionFiles": true,
        "preloadRepoMemory": true,
        "generateChecklist": true
      },
      "question": {
        "mode": "default",
        "mustAskClarifying": false,
        "alwaysCiteFiles": true,
        "includeAlternatives": false
      },
      "bugfix": {
        "mode": "default",
        "requireReproSteps": true,
        "maxFixAttempts": 3,
        "autoRunUnitTests": true
      },
      "newfeature": {
        "mode": "default",
        "requireAcceptanceCriteria": true,
        "createImplementationPlan": true,
        "includeRollbackPlan": true
      }
    }
  }
}
```

### 4. Notifications（通知策略）

```json
{
  "configKey": "user.notification.default",
  "configType": "Notification",
  "configValue": {
    "enableInApp": true,
    "enableEmail": false,
    "digestMode": "hourly",
    "focusMode": "balanced",
    "onlyMentioned": false,
    "notifyOnBuildFailed": true,
    "notifyOnReviewRequested": true,
    "notifyOnDeploymentFinished": false,
    "notifyOnSecurityAlert": true,
    "emailProvider": "smtp",
    "deliveryWindow": {
      "enabled": false,
      "start": "09:00",
      "end": "20:00",
      "timezone": "Asia/Shanghai"
    }
  }
}
```

### 5. Knowledge（知识库管理）

```json
{
  "configKey": "user.knowledge.default",
  "configType": "Knowledge",
  "configValue": {
    "sources": [],
    "defaultChunkSize": 1000,
    "defaultChunkOverlap": 200,
    "defaultRefreshPolicy": "daily",
    "defaultAccessLevel": "internal"
  }
}
```

### 6. Schedules（定时任务）

```json
{
  "configKey": "user.schedule.default",
  "configType": "Schedule",
  "configValue": {
    "tasks": [
      {
        "scheduleName": "daily-knowledge-sync",
        "cron": "0 2 * * *",
        "timezone": "Asia/Shanghai",
        "retryLimit": 2,
        "enabled": true,
        "alertChannel": "in-app"
      }
    ]
  }
}
```

### 7. Review（代码评审）

```json
{
  "configKey": "user.review.default",
  "configType": "Review",
  "configValue": {
    "minApprovals": 1,
    "blockOnFailingChecks": true,
    "enforceCodeOwners": true,
    "requiredChecks": ["ci", "lint"],
    "firstResponseSlaHours": 8,
    "exceptionApproverRole": "Tech Lead"
  }
}
```

### 8. DeepWiki（向量索引）

```json
{
  "configKey": "user.deepwiki.default",
  "configType": "DeepWiki",
  "configValue": {
    "workspace": "AutoCodeForge",
    "indexName": "autocodeforge-main-index",
    "embeddingModel": "text-embedding-3-large",
    "topK": 8,
    "metric": "cosine",
    "retentionDays": 90,
    "autoReindex": true
  }
}
```

### 9. Integration（第三方集成）

```json
{
  "configKey": "user.integration.default",
  "configType": "Integration",
  "configValue": {
    "integrations": []
  }
}
```

### 10. Skill（技能配置）

```json
{
  "configKey": "user.skill.default",
  "configType": "Skill",
  "configValue": {
    "skills": [],
    "defaultPriority": 50
  }
}
```

### 11. Repository（仓库集成）

```json
{
  "configKey": "user.repository.default",
  "configType": "Repository",
  "configValue": {
    "provider": "github",
    "owner": "",
    "repositoryName": "",
    "defaultBranch": "main",
    "authMode": "token",
    "mergeStrategies": ["squash", "merge"],
    "requireChecks": true
  }
}
```

### 12-15. ApiKey / Model / System（全局配置）

```json
{
  "configKey": "global.api.default",
  "configType": "ApiKey",
  "configValue": {
    "keys": []
  }
}
```

```json
{
  "configKey": "global.model.default",
  "configType": "Model",
  "configValue": {
    "defaultModel": "gpt-4",
    "fallbackModel": "gpt-3.5-turbo",
    "availableModels": ["gpt-4", "gpt-3.5-turbo", "claude-3-opus"]
  }
}
```

```json
{
  "configKey": "global.system.version",
  "configType": "System",
  "configValue": {
    "version": "1.0.0",
    "environment": "production",
    "maxLoginAttempts": 5,
    "sessionTimeoutMinutes": 60
  }
}
```

---

## 数据库表设计

### configuration_entries（统一配置表）

| 字段名 | 类型 | 约束 | 说明 |
|--------|------|------|------|
| Id | UUID | PRIMARY KEY | 主键 |
| ConfigType | VARCHAR(32) | NOT NULL | 配置类型枚举值 |
| ConfigKey | VARCHAR(128) | NOT NULL, UNIQUE | 配置键 |
| ConfigValue | TEXT | NOT NULL | JSON格式配置值 |
| NtId | VARCHAR(64) | NULLABLE | NTID（NULL=全局配置） |
| IsEncrypted | BOOLEAN | DEFAULT false | 是否加密存储 |
| IsEnabled | BOOLEAN | DEFAULT true | 是否启用 |
| Description | VARCHAR(512) | NULLABLE | 配置说明 |
| Group | VARCHAR(64) | DEFAULT 'default' | 逻辑分组 |
| CreatedAt | TIMESTAMP | DEFAULT CURRENT_TIMESTAMP | 创建时间 |
| UpdatedAt | TIMESTAMP | DEFAULT CURRENT_TIMESTAMP ON UPDATE | 更新时间 |
| CreatedBy | VARCHAR(64) | NULLABLE | 创建人 |
| UpdatedBy | VARCHAR(64) | NULLABLE | 更新人 |

**索引**:
- `idx_config_type` (ConfigType)
- `idx_nt_id` (NtId)
- `idx_config_key` (ConfigKey) UNIQUE

### configuration_history（配置历史表）

| 字段名 | 类型 | 约束 | 说明 |
|--------|------|------|------|
| Id | UUID | PRIMARY KEY | 主键 |
| ConfigId | UUID | FOREIGN KEY | 关联配置ID |
| ConfigType | VARCHAR(32) | NOT NULL | 配置类型 |
| ConfigKey | VARCHAR(128) | NOT NULL | 配置键 |
| PreviousValue | TEXT | NULLABLE | 修改前的值 |
| NewValue | TEXT | NOT NULL | 修改后的值 |
| Operation | VARCHAR(16) | NOT NULL | Created/Updated/Deleted |
| ChangedBy | VARCHAR(64) | NOT NULL | 操作人 |
| ChangedAt | TIMESTAMP | DEFAULT CURRENT_TIMESTAMP | 操作时间 |

---

## 注意事项

⚠️ **重要提醒**

1. **敏感配置加密** - API密钥、密码等敏感字段必须加密存储（IsEncrypted=true）
2. **配置缓存策略** - 全局配置可放入 Redis 缓存，设置合理过期时间
3. **配置验证** - 修改配置时验证 JSON 格式和业务规则
4. **配置历史保留** - 保留最近 100 条历史记录，超出自动清理
5. **日志级别** - 生产环境使用 Warning 或 Error 级别
6. **安全过滤** - /system/info 不要暴露敏感信息

✅ **验收标准**

- 所有配置类型共用一张表，扩展性好
- 全局配置、用户配置、沙盒配置 CRUD 正常
- 配置变更自动记录历史
- 支持回滚到历史版本
- 支持导入导出 JSON 格式配置
- 敏感配置加密存储
- **新用户首次登录时自动初始化 15 个模块的默认配置**
- **支持按模块懒加载初始化**
- **支持手动重置单个模块配置为默认值**
- **支持获取配置模板列表**
- 健康检查和系统信息端点正常工作

---

## 阶段完成总结

### 复用收益

本阶段通过复用阶段一、二的基础设施，预计可以**避免 500+ 行重复代码**：
- ConfigurationEntry 继承 AuditableEntity（4 个属性）
- 2 个 Repository 继承 BaseRepository（100+ 行 CRUD代码）
- 15+ API 端点使用 ApiResponse（避免重复响应格式化）
- 全局异常处理复用 ExceptionHandlingMiddleware

### 本阶段新增复用

本阶段创建了 10 个可复用组件：
- `ConfigType` (enum) - 配置类型枚举（15个模块）
- `ConfigurationEntry` - 统一配置实体
- `ConfigRepository` - 统一配置仓储
- `ConfigService` - 统一配置服务
- `ConfigHistoryEntity` - 配置历史实体
- `ConfigHistoryRepository` - 配置历史仓储
- `ConfigHistoryService` - 配置历史服务
- `EncryptionService` - 加密服务
- `ConfigInitializationService` - 配置初始化服务
- `ConfigDefaultsProvider` - 默认配置提供器

### 架构优势

| 维度 | 原方案 | 优化方案 |
|------|--------|----------|
| 表数量 | 每配置类型一张表 | **统一一张表** |
| 扩展性 | 新增类型需建表 | **仅需添加枚举值** |
| 代码重复 | 大量重复代码 | **单一服务处理所有类型** |
| 查询效率 | 多表 JOIN | **单表查询，索引优化** |
| 维护成本 | 高 | **低** |

### 下一步

本阶段完成后，配置管理后端核心功能已完善。专项业务配置（如 Repository、Knowledge、Workflow）的具体业务逻辑可在对应业务阶段中通过统一的 ConfigService 扩展实现，无需创建新的配置实体或表。
