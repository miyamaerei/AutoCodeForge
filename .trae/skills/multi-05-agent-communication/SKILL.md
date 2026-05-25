---
name: "multi-05-agent-communication"
description: "实现Agent间间接通信协议、事件发布接口、上下文传递协议和产出物标准化格式。Invoke when user asks for 'Agent间通信'、'产出物标准格式'或'上下文传递'。"
---

# multi-05-agent-communication

## 1. Skill名称与描述

- **Skill名称**: `multi-05-agent-communication`
- **一句话描述**: 实现Agent间间接通信模型、事件发布接口、上下文链式传递协议和产出物标准化契约。
- **使用场景触发条件**: 
  - 用户说"实现Agent间通信"
  - 用户说"定义产出物标准格式"
  - 用户说"上下文怎么传递"

## 2. 前置条件与输入要求

- **前置条件**:
  - 已阅读需求来源文档：`e:\git\AutoFrog\AutoCodeForge\docs\多Agent分层协作系统-MVP实施研究报告-INDEX.md`
  - 已阅读需求来源文档：`e:\git\AutoFrog\AutoCodeForge\docs\多Agent分层协作系统-MVP实施研究报告-下篇.md`（§12、§15.4、§15.5）
  - TaskStepEntity已存在（由multi-01-task-pipeline生成）
  - Agent实体已存在（由entity-scaffolder生成）
  - ITaskEventPublisher接口已定义

- **输入要求**:
  - 上下文传递格式定义
  - 产出物标准化契约字段
  - 事件类型定义
  - 存储配置（数据库/文件系统）

## 3. 核心业务规则

### 3.1 间接通信模型

```
Agent A ──(发布事件)──→ EventBus ──(订阅)──→ Agent B
                              │
                              ──(订阅)──→ Agent C
```

**设计原则**:
- Agent之间不直接调用
- 通过事件/消息中介进行通信
- 支持进程内Event → MassTransit → Kafka的演进预留

### 3.2 上下文链式传递

**传递协议**:
```
Step.Output + GlobalContext → NextStep.Input
```

**上下文结构**:
| 字段 | 类型 | 说明 |
|------|------|------|
| stepId | Guid | 当前步骤ID |
| agentId | Guid | 执行Agent ID |
| outputs | object | Step产出物 |
| globalContext | object | 全局上下文 |
| metadata | object | 元数据（时间戳、重试次数等） |

### 3.3 产出物标准化契约

**统一JSON格式**:
```json
{
    "step": "string",
    "agent_id": "guid",
    "artifacts": [
        {
            "type": "code|test|document|other",
            "name": "string",
            "content": "string",
            "format": "json|text|binary"
        }
    ],
    "summary": "string",
    "issues": ["string"],
    "metrics": {
        "execution_time_ms": number,
        "token_usage": number
    }
}
```

### 3.4 事件类型定义

| 事件类型 | 说明 | 触发时机 |
|----------|------|---------|
| TaskCreatedEvent | 任务创建 | Task创建时 |
| TaskCompletedEvent | 任务完成 | Task完成时 |
| StepTransitionEvent | 步骤流转 | Step状态变更时 |
| ArtifactCreatedEvent | 产出物创建 | 产出物存储时 |
| FailureEvent | 失败事件 | 发生失败时 |

## 4. 执行步骤（业务逻辑实现）

### Step 1: 创建 ITaskEventPublisher 接口

**业务动作**: 在 `server/src/Application/Services/` 创建接口
- 方法：`PublishTaskCreated()`、`PublishTaskCompleted()`、`PublishStepTransition()`、`PublishArtifactCreated()`
- 预留MassTransit/Kafka演进扩展点

**验收检查点**: 接口文件存在，方法签名符合设计

### Step 2: 实现 InMemoryTaskEventPublisher

**业务动作**: 创建进程内事件发布实现
- 使用 `IObservable<T>` 或 `IAsyncEnumerable<T>`
- 支持订阅和发布

**验收检查点**: 能正确发布和订阅事件

### Step 3: 创建 IArtifactStore 接口

**业务动作**: 创建产出物存储接口
- 方法：`StoreArtifact()`、`GetArtifact()`、`ListArtifacts()`、`DeleteArtifact()`
- 预留文件系统/Blob存储演进扩展点

**验收检查点**: 接口定义完整

### Step 4: 实现产出物标准化契约

**业务动作**: 定义 `ArtifactContract` 类
- 统一JSON格式：step/agent_id/artifacts/summary/issues/metrics
- 支持序列化和反序列化

**验收检查点**: 契约定义完整，字段齐全

### Step 5: 实现上下文链式传递机制

**业务动作**: 创建 `ContextChainService`
- 支持 Step.Output + 全局Context → NextStep.Input
- 处理上下文截断（超过MaxStepContextTokens时）

**验收检查点**: 上下文能正确在步骤间传递

### Step 6: 创建通信相关API Endpoint

**业务动作**: 创建 `AgentCommunicationEndpoints.cs`
- POST `/api/communication/events`
- GET `/api/artifacts/{id}`
- GET `/api/artifacts/task/{taskId}`

**验收检查点**: API能正确响应

## 5. API契约

### 5.1 事件发布

| Method | Path | 说明 |
|--------|------|------|
| POST | /api/communication/events | 发布事件 |

**Request**:
```json
{
    "eventType": "string",
    "taskId": "guid",
    "stepId": "guid",
    "payload": "object"
}
```

### 5.2 产出物管理

| Method | Path | 说明 |
|--------|------|------|
| POST | /api/artifacts | 存储产出物 |
| GET | /api/artifacts/{id} | 获取产出物 |
| GET | /api/artifacts/task/{taskId} | 获取任务的所有产出物 |
| DELETE | /api/artifacts/{id} | 删除产出物 |

**Request: StoreArtifactRequest**
```json
{
    "taskId": "guid",
    "stepId": "guid",
    "agentId": "guid",
    "artifacts": [
        {
            "type": "string",
            "name": "string",
            "content": "string",
            "format": "string"
        }
    ],
    "summary": "string",
    "issues": ["string"],
    "metrics": "object"
}
```

## 6. 输出规范（Output Specification）

| 交付物 | 路径 | 格式 | 说明 |
|--------|------|------|------|
| ITaskEventPublisher 接口 | `server/src/Application/Services/ITaskEventPublisher.cs` | C# Interface | 事件发布接口 |
| InMemoryTaskEventPublisher | `server/src/Application/Services/InMemoryTaskEventPublisher.cs` | C# Class | 进程内事件发布器 |
| IArtifactStore 接口 | `server/src/Application/Services/IArtifactStore.cs` | C# Interface | 产出物存储接口 |
| ArtifactContract 类 | `server/src/Application/Contracts/ArtifactContract.cs` | C# Class | 标准化产出物格式 |
| ContextChainService | `server/src/Application/Services/ContextChainService.cs` | C# Class | 上下文传递服务 |
| API端点 | `server/src/Presentation/Endpoints/AgentCommunicationEndpoints.cs` | C# Class | 通信API |
| 前端类型 | `client/src/types/agent-communication.ts` | TypeScript | 类型定义 |
| 前端API | `client/src/api/agent-communication.ts` | TypeScript | API调用层 |

## 7. 边界与限制

- **不负责**: 具体消息队列实现（只留接口，不实现MassTransit/Kafka）
- **不负责**: 门控逻辑（由multi-03-human-gate负责）
- **不负责**: 状态机实现（由multi-02-agent-lifecycle负责）
- **不负责**: 实体创建（由entity-scaffolder负责）
- **假设前提**: TaskStepEntity已存在，数据库连接已配置
- **无法处理**: 大规模分布式场景下的事件一致性（需后续演进）

## 8. 与其他Skill的关系

| 关系 | Skill | 说明 |
|------|-------|------|
| 上游依赖 | multi-01-task-pipeline | 提供TaskStepEntity定义 |
| 上游依赖 | multi-02-agent-lifecycle | 依赖Agent状态机 |
| 下游消费者 | multi-03-human-gate | 消费事件触发门控 |
| 下游消费者 | multi-04-task-orchestration | 使用事件进行任务状态同步 |

## 9. 示例

**典型场景**: 产出物传递

```
场景：Step4完成后，产出物传递给Step5

传递流程：
  1. Step4完成，调用 StoreArtifact() 存储产出物
  2. 发布 ArtifactCreatedEvent
  3. ContextChainService 获取产出物和全局上下文
  4. 构建 NextStep.Input
  5. 触发 Step5 执行
```

## 10. 验收检查清单

- [ ] `ITaskEventPublisher` 接口已创建，包含必要方法
- [ ] `IArtifactStore` 接口已创建，预留扩展点
- [ ] `InMemoryTaskEventPublisher` 实现进程内事件发布
- [ ] `ArtifactContract` 定义标准化产出物格式
- [ ] `ContextChainService` 实现上下文链式传递
- [ ] POST `/api/communication/events` API 正常工作
- [ ] GET `/api/artifacts/{id}` API 正常工作
- [ ] 上下文截断策略正确执行