---
name: "config-file-guide"
description: "配置文件新增工作流。用于指导在 AutoCodeForge 中新增配置类型时的后端枚举、服务、API、初始化、前端模块接入与验收检查。Use when: 添加新配置, 配置初始化, 配置页面接入, 系统配置扩展。"
argument-hint: "你要新增的配置名称/用途是什么？"
user-invocable: true
disable-model-invocation: false
---

# 配置文件新增工作流

## 本 Skill 产出
- 给出一套可执行的新增配置实施步骤（后端 + 前端 + 初始化 + 验收）。
- 明确关键决策点（是否加密、权限范围、运行时加载还是启动绑定）。
- 生成可勾选的完成清单，避免漏改。

## 何时使用
- 需要新增一个系统配置项或配置分组。
- 需要把后端配置能力暴露到前端配置页面。
- 需要在启动时写入默认配置。
- 需要补齐配置读写验收和回归检查。

## 输入信息（执行前先确认）
1. 配置类型名称（例如：`YourNewConfigType`）。
2. 配置存储 key（例如：`your-config-key`）。
3. DTO 字段结构与默认值。
4. 是否敏感信息（决定 `isEncrypted`）。
5. 权限范围（全局管理员 / 用户级）。
6. 前端是否需要独立页面与菜单入口。

## 决策分支

### 分支 A：是否加密
- 敏感信息（API Key、Token、密钥）: `isEncrypted: true`。
- 非敏感信息（UI 偏好、功能开关）: `isEncrypted: false`。

### 分支 B：权限范围
- 全局配置：仅管理员可维护。
- 用户配置：按 NTID 或用户上下文隔离。

### 分支 C：加载方式
- 运行时动态读取：通过 `ConfigService` 获取，适合经常变更。
- 启动时绑定：通过 `IOptions<T>` 绑定，适合基础稳定配置。

## 标准实施流程

### 步骤 1：后端枚举扩展
文件：`server/src/AutoCodeForge.Core/Enums/ConfigType.cs`

操作：在 `ConfigType` 增加新枚举值并分配编号。

### 步骤 2：后端服务方法
文件：`server/src/AutoCodeForge.Application/Services/ConfigService.cs`

操作：新增成对方法。
- `GetYourConfigAsync`：按 `ConfigType + key` 读取并反序列化 DTO。
- `UpsertYourConfigAsync`：序列化 DTO 并 `UpsertAsync`（带 `isEncrypted` 和描述）。

### 步骤 3：API 端点暴露
文件：`server/src/AutoCodeForge.Api/Endpoints/ConfigEndpoints.cs`

操作：新增端点。
- `GET /api/v1/config/your-config`
- `PUT /api/v1/config/your-config`

要求：返回结构与前端 DTO 对齐。

### 步骤 4：默认配置初始化
文件：`server/src/AutoCodeForge.Application/Services/ConfigInitializationService.cs`

操作：在初始化流程中检查是否存在该配置，不存在则写入默认值。

启动接线：在 `Program.cs` 启动流程中确保执行初始化逻辑。

### 步骤 5：数据库迁移
命令：
```bash
dotnet ef migrations add AddYourConfigType
dotnet ef database update
```

### 步骤 6：前端类型定义
文件：`client/src/modules/system-config/api/config.types.ts`

操作：新增 DTO 类型与必要的配置类型联合定义。

### 步骤 7：前端 API 层
文件：`client/src/modules/system-config/api/config.api.ts`

操作：新增 API 方法。
- `getYourConfig()`
- `saveYourConfig(config)`

### 步骤 8：前端 Store 层
文件：`client/src/modules/system-config/store/useSystemConfigStore.ts`

操作：新增状态与动作。
- 状态：`yourConfig`
- 动作：`loadYourConfig`、`saveYourConfig`

### 步骤 9：前端页面与交互
文件：`client/src/modules/system-config/views/YourConfigView.vue`

操作：
- 页面挂载时加载配置。
- 表单编辑后可保存并提示成功。

### 步骤 10：路由与菜单接入
文件：`client/src/modules/system-config/routes.ts`

操作：新增路由并设置 `meta.requiresAuth: true`。

补充：在侧边栏菜单配置处增加入口。

## 完成标准（DoD）
- 后端可通过 API 正确读取与更新目标配置。
- 敏感字段加密策略与权限策略正确。
- 初始化逻辑可在空库场景创建默认配置。
- 前端页面可加载、编辑、保存并回显。
- 路由、菜单、权限均生效。
- 至少完成 1 轮端到端读写验证。

## 验收检查清单

| 序号 | 检查项 | 状态 |
|------|--------|------|
| 1 | 后端：`ConfigType` 已新增枚举值 | ▢ |
| 2 | 后端：`ConfigService` 已新增 Get/Upsert 方法 | ▢ |
| 3 | 后端：`ConfigEndpoints` 已新增 GET/PUT 端点 | ▢ |
| 4 | 后端：`ConfigInitializationService` 已写默认配置逻辑 | ▢ |
| 5 | 后端：数据库迁移已执行成功 | ▢ |
| 6 | 前端：DTO 与类型定义已新增 | ▢ |
| 7 | 前端：API 调用方法已新增 | ▢ |
| 8 | 前端：Store 状态与动作已新增 | ▢ |
| 9 | 前端：配置页面已完成并可保存 | ▢ |
| 10 | 前端：路由和菜单入口已接入 | ▢ |
| 11 | 测试：配置读写回归已通过 | ▢ |

## 常见风险
- DTO 字段和后端 JSON 不一致导致反序列化失败。
- 漏配 `meta.requiresAuth` 导致权限策略失效。
- `isEncrypted` 配置错误导致敏感数据明文存储。
- 初始化逻辑未接入启动流程导致默认配置缺失。

## 快速示例（LLM 配置场景）
当新增 GitHub Copilot CLI 配置时，可按该流程落地：
1. 扩展提供商枚举与配置实体字段。
2. 在网关层增加 CLI 调用能力。
3. 在配置 API 中暴露读取与保存。
4. 在前端系统配置页新增 CLI 配置面板。
