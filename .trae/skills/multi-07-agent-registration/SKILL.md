---
name: "multi-07-agent-registration"
description: "实现Agent注册、心跳续约、跨服务器任务分配和上下线感知机制。Invoke when user asks for 'Agent注册'、'多服务器心跳'或'跨服务器分配任务'。"
---

# multi-07-agent-registration

## 1. Skill名称与描述

- **Skill名称**: `multi-07-agent-registration`
- **一句话描述**: 实现Agent注册表管理、心跳API与续约逻辑、跨服务器分配决策、上下线状态检测。
- **使用场景触发条件**: 
  - 用户说"实现Agent注册"
  - 用户说"多服务器心跳"
  - 用户说"跨服务器分配任务"

## 2. 前置条件与输入要求

- **前置条件**:
  - 已阅读需求来源文档：`e:\git\AutoFrog\AutoCodeForge\docs\多Agent分层协作系统-MVP实施研究报告-INDEX.md`
  - 已阅读需求来源文档：`e:\git\AutoFrog\AutoCodeForge\docs\多Agent分层协作系统-MVP实施研究报告-下篇.md`（§11）
  - Agent实体已存在（由entity-scaffolder生成）
  - AgentRegistration实体已存在（由entity-scaffolder生成）

- **输入要求**:
  - 心跳续约间隔配置
  - 心跳超时阈值配置
  - 服务器标识配置（ServerId/InstanceId）
  - 跨服务器分配策略配置

## 3. 核心业务规则

### 3.1 Agent注册流程

```
Agent启动 → 调用RegisterAgent → 记录注册信息 → 开始心跳续约
```

**注册信息结构**:
| 字段 | 类型 | 说明 |
|------|------|------|
| AgentId | Guid | Agent唯一标识 |
| ServerId | string | 服务器标识 |
| InstanceId | string | 实例标识 |
| LastHeartbeat | DateTime | 最后心跳时间 |
| Status | enum | Online/Offline/Unknown |
| RegisteredAt | DateTime | 注册时间 |

### 3.2 心跳续约机制

**续约流程**:
1. Agent定期调用心跳API
2. 更新LastHeartbeat时间戳
3. 状态保持Online

**心跳间隔**: 默认30秒（可配置）

### 3.3 上下线状态检测

**检测逻辑**:
1. 定时扫描AgentRegistration
2. 如果LastHeartbeat超过超时阈值，标记为Offline
3. 发布AgentOfflineEvent

**超时阈值**: 默认120秒（可配置）

### 3.4 跨服务器分配决策

**分配策略**:
1. 查询所有Online状态的Agent
2. 根据ServerId过滤（可选）
3. 调用TaskOrchestrator选择合适的Agent

## 4. 执行步骤（业务逻辑实现）

### Step 1: 创建 IAgentRegistryService 接口

**业务动作**: 在 `server/src/Application/Services/` 创建接口
- 方法：`RegisterAgent()`、`RenewHeartbeat()`、`DeregisterAgent()`、`GetAvailableAgents()`

**验收检查点**: 接口定义完整

### Step 2: 实现 AgentRegistryService

**业务动作**: 实现注册、续约、注销和查询逻辑
- 使用AgentRegistrationRepository
- 集成事件发布

**验收检查点**: 服务能正确管理Agent注册状态

### Step 3: 实现心跳续约机制

**业务动作**: 在 AgentRegistryService 中实现 `RenewHeartbeat()` 方法
- 更新LastHeartbeat时间戳
- 保持Agent状态为Online

**验收检查点**: 心跳更新逻辑正确

### Step 4: 实现上下线状态检测

**业务动作**: 创建 `AgentHeartbeatMonitor` 定时扫描服务
- 使用后台任务定期扫描
- 检测超时Agent并标记为Offline

**验收检查点**: 能正确识别离线Agent

### Step 5: 实现跨服务器分配决策

**业务动作**: 在 AgentRegistryService 中添加跨服务器分配逻辑
- 支持从不同服务器选择Agent
- 过滤Online状态的Agent

**验收检查点**: 跨服务器分配功能正常

### Step 6: 创建注册相关API Endpoint

**业务动作**: 创建 `AgentRegistrationEndpoints.cs`
- POST `/api/agents/register`
- PUT `/api/agents/heartbeat`
- DELETE `/api/agents/{id}`
- GET `/api/agents/available`

**验收检查点**: API能正确响应

## 5. API契约

### 5.1 Agent注册

| Method | Path | 说明 |
|--------|------|------|
| POST | /api/agents/register | 注册Agent |
| PUT | /api/agents/heartbeat | 更新心跳 |
| DELETE | /api/agents/{id} | 注销Agent |
| GET | /api/agents/available | 获取可用Agent列表 |
| GET | /api/agents/{id} | 获取Agent信息 |

**Request: RegisterAgentRequest**
```json
{
    "agentId": "guid",
    "serverId": "string",
    "instanceId": "string",
    "agentType": "Secretary|Manager|Worker"
}
```

## 6. 输出规范（Output Specification）

| 交付物 | 路径 | 格式 | 说明 |
|--------|------|------|------|
| IAgentRegistryService 接口 | `server/src/Application/Services/IAgentRegistryService.cs` | C# Interface | 注册服务接口 |
| AgentRegistryService | `server/src/Application/Services/AgentRegistryService.cs` | C# Class | 注册服务实现 |
| AgentHeartbeatMonitor | `server/src/Application/Services/AgentHeartbeatMonitor.cs` | C# Class | 心跳监控服务 |
| API端点 | `server/src/Presentation/Endpoints/AgentRegistrationEndpoints.cs` | C# Class | 注册API |
| 前端类型 | `client/src/types/agent-registration.ts` | TypeScript | 类型定义 |
| 前端API | `client/src/api/agent-registration.ts` | TypeScript | API调用层 |

## 7. 边界与限制

- **不负责**: 负载均衡算法（由multi-04-task-orchestration负责）
- **不负责**: 部署脚本和容器编排
- **不负责**: 实体创建（由entity-scaffolder负责）
- **假设前提**: Agent实体和AgentRegistration实体已存在，数据库连接已配置
- **无法处理**: 网络分区场景下的一致性问题（需后续演进）

## 8. 与其他Skill的关系

| 关系 | Skill | 说明 |
|------|-------|------|
| 上游依赖 | entity-scaffolder | 依赖Agent和AgentRegistration实体 |
| 上游依赖 | multi-02-agent-lifecycle | 依赖Agent状态机接口 |
| 下游消费者 | multi-04-task-orchestration | 使用注册信息进行Agent选择 |

## 9. 示例

**典型场景**: Agent注册和心跳

```
场景：新Agent启动并注册

注册流程：
  1. Agent启动，调用 POST /api/agents/register
  2. AgentRegistryService创建注册记录
  3. Agent开始定期调用 PUT /api/agents/heartbeat
  4. AgentHeartbeatMonitor检测到Online状态
  5. TaskOrchestrator可以分配任务给该Agent
```

## 10. 验收检查清单

- [ ] `IAgentRegistryService` 接口已创建，包含必要方法
- [ ] `AgentRegistryService` 包含 `RegisterAgent()` 和 `RenewHeartbeat()` 方法
- [ ] AgentHeartbeatMonitor 能正确检测超时Agent
- [ ] POST `/api/agents/register` API 正常工作
- [ ] PUT `/api/agents/heartbeat` API 正常工作
- [ ] GET `/api/agents/available` API 正常工作
- [ ] 跨服务器分配功能正常