# 后端配置 API 文档

## 概述

本文档详细描述了 AutoCodeForge 后端配置管理系统的 API 接口设计，包括全局配置、用户配置和沙盒配置的完整 CRUD 操作。

---

## 基础信息

| 项目 | 说明 |
|------|------|
| **API 版本** | v1 |
| **基础路径** | `/api/v1/configs` |
| **认证方式** | JWT Token |
| **响应格式** | JSON |

---

## 全局配置 API

全局配置对所有用户生效，仅管理员有权限操作。

### 1. 获取全局配置列表

**请求**
```
GET /api/v1/configs/global?page=1&pageSize=20
```

**参数**

| 参数 | 类型 | 必填 | 默认值 | 说明 |
|------|------|------|--------|------|
| `page` | int | 否 | 1 | 页码（从1开始） |
| `pageSize` | int | 否 | 20 | 每页条数 |

**响应**
```json
{
  "success": true,
  "message": null,
  "data": {
    "items": [
      {
        "id": "guid-string",
        "configKey": "FeatureFlag.EnableAI",
        "configValue": "true",
        "description": "是否启用AI功能",
        "createdAt": "2024-01-01T00:00:00Z",
        "updatedAt": "2024-01-01T00:00:00Z"
      }
    ],
    "totalCount": 10,
    "page": 1,
    "pageSize": 20
  }
}
```

### 2. 获取单个全局配置

**请求**
```
GET /api/v1/configs/global/{configKey}
```

**路径参数**

| 参数 | 类型 | 说明 |
|------|------|------|
| `configKey` | string | 配置键名 |

**响应**
```json
{
  "success": true,
  "message": null,
  "data": {
    "id": "guid-string",
    "configKey": "FeatureFlag.EnableAI",
    "configValue": "true",
    "description": "是否启用AI功能",
    "createdAt": "2024-01-01T00:00:00Z",
    "updatedAt": "2024-01-01T00:00:00Z"
  }
}
```

### 3. 创建/更新全局配置

**请求**
```
POST /api/v1/configs/global
```

**请求体**
```json
{
  "configKey": "FeatureFlag.EnableAI",
  "configValue": "true",
  "description": "是否启用AI功能"
}
```

**字段说明**

| 字段 | 类型 | 必填 | 说明 |
|------|------|------|------|
| `configKey` | string | 是 | 配置键名 |
| `configValue` | string | 是 | 配置值 |
| `description` | string | 否 | 配置描述 |

**响应**
```json
{
  "success": true,
  "message": "Global config saved",
  "data": {
    "id": "guid-string",
    "configKey": "FeatureFlag.EnableAI",
    "configValue": "true",
    "description": "是否启用AI功能",
    "createdAt": "2024-01-01T00:00:00Z",
    "updatedAt": "2024-01-01T00:00:00Z"
  }
}
```

### 4. 更新指定全局配置

**请求**
```
PUT /api/v1/configs/global/{configKey}
```

**请求体**
```json
{
  "configValue": "false",
  "description": "是否启用AI功能"
}
```

### 5. 删除全局配置

**请求**
```
DELETE /api/v1/configs/global/{configKey}
```

**响应**
```json
{
  "success": true,
  "message": "Global config deleted",
  "data": null
}
```

---

## 用户配置 API

用户配置仅对当前用户生效，每个用户有独立的配置空间。

### 1. 获取用户配置列表

**请求**
```
GET /api/v1/configs/user?page=1&pageSize=20
```

**响应**
```json
{
  "success": true,
  "message": null,
  "data": {
    "items": [
      {
        "id": "guid-string",
        "configKey": "Editor.Theme",
        "configValue": "dark",
        "createdAt": "2024-01-01T00:00:00Z",
        "updatedAt": "2024-01-01T00:00:00Z"
      }
    ],
    "totalCount": 5,
    "page": 1,
    "pageSize": 20
  }
}
```

### 2. 获取单个用户配置

**请求**
```
GET /api/v1/configs/user/{configKey}
```

### 3. 创建/更新用户配置

**请求**
```
POST /api/v1/configs/user
```

**请求体**
```json
{
  "configKey": "Editor.Theme",
  "configValue": "dark"
}
```

### 4. 更新指定用户配置

**请求**
```
PUT /api/v1/configs/user/{configKey}
```

### 5. 删除用户配置

**请求**
```
DELETE /api/v1/configs/user/{configKey}
```

---

## 沙盒配置 API

沙盒配置是用户配置的特殊类型，用于管理代码执行沙盒的安全策略。

### 1. 获取沙盒配置

**请求**
```
GET /api/v1/configs/user/sandbox
```

**响应**
```json
{
  "success": true,
  "message": null,
  "data": {
    "workspaceRootPath": "C:/gitrepos/AutoCodeForge",
    "artifactOutputPath": "C:/gitrepos/AutoCodeForge/.sandbox-artifacts",
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
    "defaultModel": "gpt-5.3-codex",
    "fallbackModel": "gpt-4.1",
    "promptGuardrail": "先分析风险，再执行最小改动。"
  }
}
```

### 2. 创建/更新沙盒配置

**请求**
```
PUT /api/v1/configs/user/sandbox
```

**请求体**

| 字段 | 类型 | 必填 | 默认值 | 说明 |
|------|------|------|--------|------|
| `workspaceRootPath` | string | 否 | - | 工作区根路径 |
| `artifactOutputPath` | string | 否 | - | 产物输出路径 |
| `allowedWritePaths` | string | 否 | - | 允许写入路径（每行一个 glob） |
| `ignoredPaths` | string | 否 | - | 忽略路径（每行一个 glob） |
| `executionMode` | string | 是 | dry-run | 执行模式：dry-run / sandbox-live |
| `approvalMode` | string | 是 | strict | 审批模式：strict / manual / off |
| `maxParallelTasks` | int | 是 | 3 | 最大并发任务数（1-12） |
| `commandTimeoutSec` | int | 是 | 300 | 命令超时秒数（60-1800） |
| `allowWriteOps` | boolean | 否 | false | 允许写操作 |
| `allowNetworkAccess` | boolean | 否 | false | 允许网络访问 |
| `storeTerminalLogs` | boolean | 否 | true | 保留终端日志 |
| `maskSecretsInLogs` | boolean | 否 | true | 日志脱敏 |
| `defaultModel` | string | 否 | - | 默认模型 |
| `fallbackModel` | string | 否 | - | 回退模型 |
| `promptGuardrail` | string | 否 | - | 提示护栏 |

---

## 健康检查 API

### 1. 综合健康检查

**请求**
```
GET /health
```

**响应（健康）**
```json
{
  "status": "healthy",
  "timestamp": "2024-01-01T00:00:00Z",
  "checks": {
    "database": "ok"
  }
}
```

**响应（不健康）**
```json
{
  "status": "unhealthy",
  "timestamp": "2024-01-01T00:00:00Z",
  "error": "Database connection failed",
  "checks": {
    "database": "failed"
  }
}
```

### 2. 存活检查

**请求**
```
GET /health/live
```

**响应**
```json
{
  "status": "alive",
  "timestamp": "2024-01-01T00:00:00Z"
}
```

### 3. 就绪检查

**请求**
```
GET /health/ready
```

**响应**
```json
{
  "status": "ready",
  "timestamp": "2024-01-01T00:00:00Z"
}
```

---

## 错误响应格式

```json
{
  "success": false,
  "message": "Error message here",
  "data": null
}
```

## 权限说明

| API 路径 | 所需角色 |
|----------|----------|
| `/api/v1/configs/global/*` | Admin |
| `/api/v1/configs/user/*` | 任意登录用户 |
| `/health/*` | 匿名访问 |

---

## 数据传输对象 (DTO)

### UpdateConfigRequest

```csharp
public class UpdateConfigRequest
{
    public string ConfigKey { get; set; }
    public string ConfigValue { get; set; }
    public string? Description { get; set; }
}
```

### ConfigResponse

```csharp
public class ConfigResponse
{
    public Guid Id { get; set; }
    public string ConfigKey { get; set; }
    public string ConfigValue { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

### SandboxConfigDto

```csharp
public class SandboxConfigDto
{
    public string? WorkspaceRootPath { get; set; }
    public string? ArtifactOutputPath { get; set; }
    public string? AllowedWritePaths { get; set; }
    public string? IgnoredPaths { get; set; }
    public string ExecutionMode { get; set; }
    public string ApprovalMode { get; set; }
    public int MaxParallelTasks { get; set; }
    public int CommandTimeoutSec { get; set; }
    public bool AllowWriteOps { get; set; }
    public bool AllowNetworkAccess { get; set; }
    public bool StoreTerminalLogs { get; set; }
    public bool MaskSecretsInLogs { get; set; }
    public string? DefaultModel { get; set; }
    public string? FallbackModel { get; set; }
    public string? PromptGuardrail { get; set; }
}
```