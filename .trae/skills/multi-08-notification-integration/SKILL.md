---
name: "multi-08-notification-integration"
description: "实现通知服务接口、通知渠道抽象、门控触发通知派发和通知模板管理。Invoke when user asks for '接入通知服务'、'门控触发时通知人类'或'添加通知模板'。"
---

# multi-08-notification-integration

## 1. Skill名称与描述

- **Skill名称**: `multi-08-notification-integration`
- **一句话描述**: 实现通知服务接口设计、通知渠道抽象、门控触发时的通知派发、通知模板管理。
- **使用场景触发条件**: 
  - 用户说"接入通知服务"
  - 用户说"门控触发时通知人类"
  - 用户说"添加通知模板"

## 2. 前置条件与输入要求

- **前置条件**:
  - 已阅读需求来源文档：`e:\git\AutoFrog\AutoCodeForge\docs\多Agent分层协作系统-MVP实施研究报告-INDEX.md`
  - 已阅读需求来源文档：`e:\git\AutoFrog\AutoCodeForge\docs\多Agent分层协作系统-MVP实施研究报告-下篇.md`（§17.8）
  - HumanGateEntity已存在（由entity-scaffolder生成）
  - ITaskEventPublisher接口已定义

- **输入要求**:
  - 通知渠道配置（站内信/邮件/Webhook/IM）
  - 通知模板定义
  - 通知频率控制配置（防抖/合并）
  - 通知优先级配置

## 3. 核心业务规则

### 3.1 通知渠道抽象

| 渠道类型 | 说明 | 实现方式 |
|----------|------|---------|
| InApp | 站内信 | 数据库存储，前端轮询 |
| Email | 邮件通知 | SMTP客户端 |
| Webhook | Webhook推送 | HTTP POST |
| IM | 即时消息 | 第三方SDK（预留） |

### 3.2 通知模板管理

**模板结构**:
| 字段 | 类型 | 说明 |
|------|------|------|
| TemplateId | string | 模板ID |
| Name | string | 模板名称 |
| Subject | string | 标题（支持变量） |
| Content | string | 内容（支持变量） |
| Channel | enum | 适用渠道 |

**变量替换**:
- `{{TaskId}}` - 任务ID
- `{{GateType}}` - 门控类型
- `{{UserName}}` - 用户名称
- `{{ActionUrl}}` - 操作链接

### 3.3 通知频率控制

**防抖策略**:
- 相同类型通知在指定时间内合并
- 默认：30秒内相同任务的通知合并

**优先级排序**:
- High > Medium > Low
- 高优先级通知立即发送

### 3.4 门控触发通知派发

**触发流程**:
```
HumanGate创建 → 发布HumanGateCreatedEvent → NotificationService订阅 → 发送通知
```

**通知规则**:
- RequirementConfirm: 通知项目经理
- PlanApproval: 通知技术负责人
- CodeReview: 通知代码审核员
- TestAcceptance: 通知测试人员
- MergeApproval: 通知运维人员
- FinalSignoff: 通知项目负责人
- Emergency: 通知所有相关人员

## 4. 执行步骤（业务逻辑实现）

### Step 1: 创建 INotificationService 接口

**业务动作**: 在 `server/src/Application/Services/` 创建接口
- 方法：`SendNotification()`、`SendBatchNotifications()`、`RegisterChannel()`

**验收检查点**: 接口定义完整，预留扩展点

### Step 2: 创建 NotificationChannel 枚举

**业务动作**: 创建枚举，包含 InApp、Email、Webhook、IM 等渠道

**验收检查点**: 枚举包含必要渠道类型

### Step 3: 创建 NotificationTemplate 模板管理

**业务动作**: 定义模板类，支持变量替换
- 从GlobalConfig读取模板配置

**验收检查点**: 模板类能正确解析和替换变量

### Step 4: 实现 InAppNotificationChannel

**业务动作**: 创建站内信渠道实现
- 使用数据库存储通知
- 支持查询和标记已读

**验收检查点**: 能正确发送站内信通知

### Step 5: 创建 NotificationService

**业务动作**: 实现通知派发逻辑
- 订阅HumanGate事件
- 根据渠道类型分发通知
- 实现防抖和合并逻辑

**验收检查点**: 服务能正确派发通知

### Step 6: 创建通知API Endpoint

**业务动作**: 创建 `NotificationEndpoints.cs`
- POST `/api/notifications/send`
- GET `/api/notifications/templates`
- GET `/api/notifications/user/{userId}`

**验收检查点**: API能正确响应

## 5. API契约

### 5.1 通知发送

| Method | Path | 说明 |
|--------|------|------|
| POST | /api/notifications/send | 发送通知 |
| GET | /api/notifications/user/{userId} | 获取用户通知列表 |
| PUT | /api/notifications/{id}/read | 标记已读 |
| GET | /api/notifications/templates | 获取通知模板列表 |

**Request: SendNotificationRequest**
```json
{
    "channel": "InApp|Email|Webhook",
    "userId": "guid",
    "templateId": "string",
    "variables": {
        "TaskId": "guid",
        "GateType": "string"
    },
    "priority": "High|Medium|Low"
}
```

## 6. 输出规范（Output Specification）

| 交付物 | 路径 | 格式 | 说明 |
|--------|------|------|------|
| INotificationService 接口 | `server/src/Application/Services/INotificationService.cs` | C# Interface | 通知服务接口 |
| NotificationChannel 枚举 | `server/src/Application/Enums/NotificationChannel.cs` | C# Enum | 通知渠道枚举 |
| NotificationTemplate 类 | `server/src/Application/Models/NotificationTemplate.cs` | C# Class | 通知模板模型 |
| InAppNotificationChannel | `server/src/Infrastructure/Notification/Channels/InAppNotificationChannel.cs` | C# Class | 站内信渠道实现 |
| NotificationService | `server/src/Application/Services/NotificationService.cs` | C# Class | 通知服务实现 |
| API端点 | `server/src/Presentation/Endpoints/NotificationEndpoints.cs` | C# Class | 通知API |
| 前端类型 | `client/src/types/notification.ts` | TypeScript | 类型定义 |
| 前端API | `client/src/api/notification.ts` | TypeScript | API调用层 |

## 7. 边界与限制

- **不负责**: 具体邮件服务器配置
- **不负责**: IM机器人SDK实现（只留扩展点）
- **不负责**: 业务门控判断（由multi-03-human-gate负责）
- **不负责**: 实体创建（由entity-scaffolder负责）
- **假设前提**: HumanGate机制已存在，数据库连接已配置
- **无法处理**: 外部通知服务不可用的场景（需配合失败恢复机制）

## 8. 与其他Skill的关系

| 关系 | Skill | 说明 |
|------|-------|------|
| 上游依赖 | multi-03-human-gate | 依赖门控事件触发通知 |
| 上游依赖 | multi-05-agent-communication | 依赖事件发布机制 |
| 下游消费者 | multi-06-failure-recovery | 消费失败通知 |

## 9. 示例

**典型场景**: 门控触发通知

```
场景：PlanApproval门控创建，通知技术负责人

通知流程：
  1. HumanGate创建，发布HumanGateCreatedEvent
  2. NotificationService订阅事件
  3. 获取PlanApproval模板
  4. 替换变量：{{TaskId}}、{{ActionUrl}}
  5. 发送站内信和邮件通知技术负责人
```

## 10. 验收检查清单

- [ ] `INotificationService` 接口已创建，包含必要方法
- [ ] `NotificationChannel` 枚举包含必要渠道类型
- [ ] `InAppNotificationChannel` 实现站内信发送
- [ ] `NotificationService` 能正确派发通知
- [ ] 门控触发时能自动发送通知
- [ ] 通知频率控制（防抖/合并）逻辑正确
- [ ] POST `/api/notifications/send` API 正常工作