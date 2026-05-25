# API 文档

## API 基础信息

- **基础 URL**: `http://localhost:5000` (开发环境)
- **认证方式**: JWT Bearer Token
- **数据格式**: JSON
- **字符编码**: UTF-8

**Swagger UI**: http://localhost:5000/swagger (仅开发环境)

---

## 认证

### 注册用户

**端点**: `POST /api/auth/register`

**请求体**:
```json
{
  "username": "testuser",
  "password": "Password@123",
  "email": "test@example.com"
}
```

**响应** (200 OK):
```json
{
  "id": 1,
  "username": "testuser",
  "email": "test@example.com",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "refresh_token_string"
}
```

---

### 用户登录

**端点**: `POST /api/auth/login`

**请求体**:
```json
{
  "username": "testuser",
  "password": "Password@123"
}
```

**响应** (200 OK):
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "refresh_token_string",
  "expiresIn": 3600,
  "user": {
    "id": 1,
    "username": "testuser",
    "email": "test@example.com"
  }
}
```

---

### 刷新令牌

**端点**: `POST /api/auth/refresh`

**请求体**:
```json
{
  "refreshToken": "refresh_token_string"
}
```

**响应** (200 OK):
```json
{
  "token": "new_jwt_token",
  "refreshToken": "new_refresh_token",
  "expiresIn": 3600
}
```

---

## 任务管理

### 获取任务列表

**端点**: `GET /api/tasks`

**查询参数**:
| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
| `page` | number | 否 | 页码（默认 1） |
| `pageSize` | number | 否 | 每页数量（默认 10） |
| `status` | string | 否 | 任务状态过滤 |
| `priority` | string | 否 | 优先级过滤 |

**请求头**:
```
Authorization: Bearer <token>
```

**响应** (200 OK):
```json
{
  "data": [
    {
      "id": 1,
      "title": "实现任务优先级功能",
      "description": "为任务中心添加优先级字段",
      "status": "InProgress",
      "priority": "High",
      "assignee": "user1",
      "createdAt": "2026-05-20T10:00:00Z",
      "updatedAt": "2026-05-25T15:30:00Z"
    }
  ],
  "total": 100,
  "page": 1,
  "pageSize": 10
}
```

---

### 获取任务详情

**端点**: `GET /api/tasks/{id}`

**路径参数**:
| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
| `id` | number | 是 | 任务 ID |

**请求头**:
```
Authorization: Bearer <token>
```

**响应** (200 OK):
```json
{
  "id": 1,
  "title": "实现任务优先级功能",
  "description": "为任务中心添加优先级字段",
  "status": "InProgress",
  "priority": "High",
  "assignee": "user1",
  "createdAt": "2026-05-20T10:00:00Z",
  "updatedAt": "2026-05-25T15:30:00Z",
  "steps": [
    {
      "id": 1,
      "title": "设计数据库表结构",
      "status": "Completed",
      "order": 1
    },
    {
      "id": 2,
      "title": "实现后端 API",
      "status": "InProgress",
      "order": 2
    }
  ]
}
```

---

### 创建任务

**端点**: `POST /api/tasks`

**请求头**:
```
Authorization: Bearer <token>
Content-Type: application/json
```

**请求体**:
```json
{
  "title": "新任务",
  "description": "任务描述",
  "priority": "Medium",
  "assignee": "user1"
}
```

**响应** (201 Created):
```json
{
  "id": 2,
  "title": "新任务",
  "description": "任务描述",
  "status": "Pending",
  "priority": "Medium",
  "assignee": "user1",
  "createdAt": "2026-05-25T16:00:00Z",
  "updatedAt": "2026-05-25T16:00:00Z"
}
```

---

### 更新任务

**端点**: `PUT /api/tasks/{id}`

**路径参数**:
| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
| `id` | number | 是 | 任务 ID |

**请求头**:
```
Authorization: Bearer <token>
Content-Type: application/json
```

**请求体**:
```json
{
  "title": "更新后的任务标题",
  "status": "Completed",
  "priority": "High"
}
```

**响应** (200 OK):
```json
{
  "id": 2,
  "title": "更新后的任务标题",
  "description": "任务描述",
  "status": "Completed",
  "priority": "High",
  "assignee": "user1",
  "createdAt": "2026-05-25T16:00:00Z",
  "updatedAt": "2026-05-25T16:30:00Z"
}
```

---

### 删除任务

**端点**: `DELETE /api/tasks/{id}`

**路径参数**:
| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
| `id` | number | 是 | 任务 ID |

**请求头**:
```
Authorization: Bearer <token>
```

**响应** (204 No Content)

---

## Agent 管理

### 获取 Agent 列表

**端点**: `GET /api/agents`

**查询参数**:
| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
| `type` | string | 否 | Agent 类型过滤 |
| `status` | string | 否 | Agent 状态过滤 |

**请求头**:
```
Authorization: Bearer <token>
```

**响应** (200 OK):
```json
{
  "data": [
    {
      "id": 1,
      "name": "CodeReviewAgent",
      "type": "CodeReview",
      "status": "Active",
      "description": "自动代码审查 Agent",
      "skills": ["code-analysis", "quality-check"],
      "createdAt": "2026-05-10T10:00:00Z"
    }
  ],
  "total": 10
}
```

---

### 注册 Agent

**端点**: `POST /api/agent-registrations`

**请求头**:
```
Authorization: Bearer <token>
Content-Type: application/json
```

**请求体**:
```json
{
  "name": "DeploymentAgent",
  "type": "Deployment",
  "skills": ["docker", "kubernetes", "azure"],
  "configuration": {
    "maxConcurrency": 5,
    "timeout": 300
  }
}
```

**响应** (201 Created):
```json
{
  "id": 2,
  "name": "DeploymentAgent",
  "type": "Deployment",
  "status": "Pending",
  "skills": ["docker", "kubernetes", "azure"],
  "registrationToken": "agent_token_string",
  "createdAt": "2026-05-25T17:00:00Z"
}
```

---

## 聊天对话

### 发送聊天消息

**端点**: `POST /api/chat`

**请求头**:
```
Authorization: Bearer <token>
Content-Type: application/json
```

**请求体**:
```json
{
  "message": "帮我分析一下任务执行情况",
  "conversationId": "conv_123",
  "context": {
    "taskId": 1
  }
}
```

**响应** (200 OK):
```json
{
  "conversationId": "conv_123",
  "messageId": "msg_456",
  "response": "根据分析，任务 #1 目前处于进行中状态...",
  "timestamp": "2026-05-25T17:30:00Z"
}
```

---

### 流式聊天

**端点**: `GET /api/chat/stream`

**查询参数**:
| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
| `message` | string | 是 | 聊天消息 |
| `conversationId` | string | 否 | 会话 ID |

**请求头**:
```
Authorization: Bearer <token>
Accept: text/event-stream
```

**响应** (SSE 流):
```
data: {"type":"start","conversationId":"conv_123"}

data: {"type":"token","content":"根据"}

data: {"type":"token","content":"分析"}

data: {"type":"token","content":"，任务"}

data: {"type":"end","messageId":"msg_456"}
```

---

## 仓库管理

### 获取仓库列表

**端点**: `GET /api/repositories`

**查询参数**:
| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
| `platform` | string | 否 | Git 平台（GitHub/GitLab/AzureDevOps） |

**请求头**:
```
Authorization: Bearer <token>
```

**响应** (200 OK):
```json
{
  "data": [
    {
      "id": 1,
      "name": "AutoCodeForge",
      "platform": "GitHub",
      "owner": "miyamaerei",
      "url": "https://github.com/miyamaerei/AutoCodeForge",
      "defaultBranch": "main",
      "lastSync": "2026-05-25T10:00:00Z",
      "status": "Active"
    }
  ],
  "total": 5
}
```

---

### 同步仓库

**端点**: `POST /api/repo-syncs/{repoId}`

**路径参数**:
| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
| `repoId` | number | 是 | 仓库 ID |

**请求头**:
```
Authorization: Bearer <token>
```

**响应** (202 Accepted):
```json
{
  "syncId": "sync_789",
  "repoId": 1,
  "status": "InProgress",
  "startedAt": "2026-05-25T18:00:00Z"
}
```

---

## 系统配置

### 获取配置列表

**端点**: `GET /api/config`

**查询参数**:
| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
| `category` | string | 否 | 配置类别过滤 |

**请求头**:
```
Authorization: Bearer <token>
```

**响应** (200 OK):
```json
{
  "data": [
    {
      "key": "git.github.enabled",
      "value": "true",
      "category": "Git",
      "description": "启用 GitHub 集成",
      "isEncrypted": false
    },
    {
      "key": "ai.openai.apiKey",
      "value": "***",
      "category": "AI",
      "description": "OpenAI API 密钥",
      "isEncrypted": true
    }
  ]
}
```

---

### 更新配置

**端点**: `PUT /api/config/{key}`

**路径参数**:
| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
| `key` | string | 是 | 配置键 |

**请求头**:
```
Authorization: Bearer <token>
Content-Type: application/json
```

**请求体**:
```json
{
  "value": "new_value"
}
```

**响应** (200 OK):
```json
{
  "key": "git.github.enabled",
  "value": "new_value",
  "updatedAt": "2026-05-25T18:30:00Z"
}
```

---

## 错误响应

### 标准错误格式

**响应** (4xx/5xx):
```json
{
  "error": {
    "code": "NOT_FOUND",
    "message": "Task with ID 999 not found",
    "details": {
      "resource": "Task",
      "id": 999
    },
    "timestamp": "2026-05-25T19:00:00Z"
  }
}
```

### 常见错误码

| HTTP 状态码 | 错误码 | 说明 |
|------------|--------|------|
| 400 | `VALIDATION_ERROR` | 请求参数验证失败 |
| 401 | `UNAUTHORIZED` | 未认证或令牌无效 |
| 403 | `FORBIDDEN` | 无权限访问资源 |
| 404 | `NOT_FOUND` | 资源不存在 |
| 409 | `CONFLICT` | 资源冲突（如重复创建） |
| 429 | `RATE_LIMIT_EXCEEDED` | 请求频率超限 |
| 500 | `INTERNAL_ERROR` | 服务器内部错误 |

---

## 分页

所有列表端点支持分页：

**查询参数**:
| 参数 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `page` | number | 1 | 页码 |
| `pageSize` | number | 10 | 每页数量 |

**响应格式**:
```json
{
  "data": [...],
  "total": 100,
  "page": 1,
  "pageSize": 10,
  "totalPages": 10
}
```

---

## 排序和过滤

### 排序

**查询参数**:
```
?sortBy=createdAt&sortOrder=desc
```

### 过滤

**查询参数**:
```
?status=InProgress&priority=High
```

---

## API 版本

当前 API 版本：**v1**

未来如有 API 不兼容变更，将引入版本号：
```
/api/v2/tasks
```

---

## 下一步

- [部署指南](07-部署指南.md) - 生产环境部署
- [故障排除](08-故障排除.md) - API 调试和问题解决
- [开发指南](05-开发指南.md) - 如何扩展 API
