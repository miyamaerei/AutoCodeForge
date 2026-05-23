---
name: "multi-02-agent-lifecycle"
description: "Agent四状态生命周期管理：Idle/Handling/Learning/Dormant状态机实现、角色差异化空闲超时配置、Learning触发与执行机制、Dormant休眠/唤醒。当用户说'实现Agent状态机'、'Agent休眠'、'空闲超时Learning'时触发。"
argument-hint: "AgentId（Guid）和操作类型（assign/complete/learn/dormant/wake）"
---

# Multi-Agent Agent Lifecycle Skill

Agent四状态生命周期管理核心业务逻辑实现，负责状态机流转、空闲监控、学习触发和休眠管理。

## 1. Skill名称与描述

- **Skill名称**：`multi-02-agent-lifecycle`
- **一句话描述**：Agent四状态（Idle/Handling/Learning/Dormant）状态机实现、角色差异化空闲超时、Learning机制、Dormant休眠与唤醒。
- **使用场景触发条件**：
  - 用户说"实现Agent状态机"或"Agent状态怎么转换"
  - 用户说"空闲超时自动Learning"或"空闲30秒后学习"
  - 用户说"Agent休眠"或"Dormant状态"
  - 用户说"实现AgentIdleMonitor"或"空闲监控服务"

## 2. 前置条件与输入要求

### 前置条件
- AgentEntity已存在且包含State、Role、StateChangedAtUtc字段
- AgentRepository已实现状态查询方法
- GlobalConfig中有IdleTimeoutSeconds和LearningDurationSeconds配置
- 已阅读研究报告上篇§1.3（三状态模型）、§3.3.2（Agent状态机设计）
- 已阅读研究报告下篇§16.1（角色差异化空闲超时）、§16.6（Dormant休眠状态）、§13（Learning机制）

### 输入要求
| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
| agentId | Guid | 是 | Agent ID |
| operation | string | 是 | assign/complete/fail/learn/dormant/wake |
| context | object | 否 | 操作上下文（如任务ID、失败原因等） |

## 3. 核心业务规则

### 3.1 Agent四状态定义

```
AgentState枚举：
- Idle = 0       // 空闲 -- 可接单
- Handling = 1   // 处理问题 -- 锁定资源
- Learning = 2   // 学习 -- 暂停接单
- Dormant = 3   // 休眠 -- 不学习、不接单、等待人工介入
```

**状态转换图**：
```
                    ┌──────────────────────────────────────────┐
                    │                                          │
                    ▼                                          │
Idle ──(分配任务)──→ Handling ──(任务完成)──→ Idle             │
 │                     ▲                    │                   │
 │                     │                    │                   │
 │         ┌───────────┘                    │                   │
 │         │ (中断学习)                     │                   │
 │         ▼                                │                   │
 │    Learning ──(学习完成/高优先级中断)──→ ┘                   │
 │         │                                                       │
 │         │ (空闲超时触发)                                        │
 └─────────┴──────────────────────────────────────────────────────┘
                    ▲                    │
                    │                    │
           (人工介入)│          (连续低效/异常)
                    │                    │
                    └────── Dormant ◄─────┘
                              │
                              │ (人工唤醒)
                              ▼
                           Idle
```

### 3.2 角色差异化空闲超时配置

**IdleTimeoutSeconds按角色差异化**：

| 角色 | IdleTimeoutSeconds | 说明 |
|------|-------------------|------|
| Secretary | 60秒 | 秘书响应快，60秒无任务即触发学习 |
| Manager | 120秒 | 老大决策慢，可多等一会 |
| Worker | 300秒 | 小弟执行快，5分钟无任务才学习 |

**配置来源**：GlobalConfig.AgentIdleTimeoutSeconds（按Role读取）

### 3.3 状态转换规则

**合法转换**：
| 当前状态 | 触发条件 | 目标状态 | 说明 |
|----------|---------|----------|------|
| Idle | AssignTask | Handling | 接收任务开始处理 |
| Idle | StartLearning | Learning | 空闲超时触发学习 |
| Handling | CompleteTask | Idle | 任务正常完成 |
| Handling | FailTask | Idle | 任务失败 |
| Handling | TimeoutTask | Idle | 任务超时 |
| Learning | CompleteLearning | Idle | 学习正常结束 |
| Learning | InterruptLearning | Idle | 高优先级任务中断 |
| Learning | TimeoutLearning | Idle | 学习超时强制结束 |
| Idle/Dormant | EnterDormant | Dormant | 人工或自动进入休眠 |
| Dormant | WakeUp | Idle | 人工唤醒 |

**非法转换（抛出InvalidOperationException）**：
| 当前状态 | 非法触发 | 原因 |
|----------|---------|------|
| Handling | StartLearning | 处理中不能学习 |
| Handling | AssignTask | 已在处理中 |
| Learning | AssignTask（非高优先级） | 学习中被低优先级任务打断 |
| Dormant | AssignTask | 休眠中不能接单 |

### 3.4 Learning触发条件与执行流程

**三种学习触发方式**：

| 触发类型 | 条件 | 优先级 | MVP实现 |
|----------|------|--------|---------|
| 空闲超时触发 | Idle时间 > IdleTimeoutSeconds | P1 | 必须实现 |
| 任务后复盘触发 | Handling完成任务后 | P2 | 简化实现 |
| 异常触发 | 任务被驳回/失败 | P1 | 必须实现 |

**Learning执行流程**：
```
1. 状态变更：Idle → Learning
2. 更新StateChangedAtUtc
3. 构造学习Prompt（基于role加载学习素材）
   - Secretary: 分析历史调度卡点、异常场景
   - Manager: 复盘驳回案例、审核标准SOP
   - Worker: 复盘被驳回任务、优秀历史案例
4. 调用LLM分析
5. 提取新SkillTags，更新LearningProgress
6. 状态变更：Learning → Idle
```

**Learning时长约束**：
| 参数 | 默认值 | 说明 |
|------|--------|------|
| LearningDurationSeconds | 1800秒（30分钟） | 单次学习最长时长 |
| HighPriorityInterruptEnabled | true | 高优先级任务可中断 |

### 3.5 Dormant休眠机制

**触发Dormant的条件**（满足任一）：
| 条件 | 说明 |
|------|------|
| 人工介入 | 管理员手动将Agent设为Dormant |
| 连续学习效果差 | 连续2次学习效果评分低于阈值 |
| 学习产出有害 | LLM返回有害或无效内容 |

**Dormant状态规则**：
| 规则 | 说明 |
|------|------|
| 不参与任务分配 | Dormant Agent不查询、不接单 |
| 不参与空闲扫描 | IdleMonitor跳过Dormant Agent |
| 仅可通过人工恢复 | 必须管理员调用Wake API |
| 保留全部上下文 | 唤醒后无缝恢复 |

**唤醒后恢复**：
- Agent.State = Idle
- 立即参与调度
- 不重新初始化任何状态

### 3.6 状态持久化与恢复

**状态持久化要求**：
- 每次状态变更必须同时更新：
  - AgentEntity.State
  - AgentEntity.StateChangedAtUtc
  - AgentEntity.Version（乐观锁）
- 状态变更是原子操作

**故障恢复**：
| 场景 | 处理 |
|------|------|
| Handling状态Agent超时30分钟 | BackgroundService强制释放 → Idle |
| 数据库连接断开 | 抛出DatabaseException，状态不变 |
| 状态变更乐观锁冲突 | 抛出ConcurrencyException，调用方重试 |

## 4. 执行步骤（业务逻辑实现）

### 步骤1：分配任务（AssignTask）

**业务动作**：
1. 验证Agent存在、状态为Idle、不是Dormant
2. 验证任务存在且未被取消
3. 使用事务原子执行：
   - Agent.State = Handling
   - Agent.StateChangedAtUtc = now
   - Agent.CurrentTaskId = taskId
4. 发布AgentAssignedEvent

**业务约束**：
- 仅Idle状态能接收任务
- Dormant状态不能接收任务
- Handling状态不能再次分配

### 步骤2：完成任务（CompleteTask）

**业务动作**：
1. 验证Agent存在、状态为Handling
2. 使用事务原子执行：
   - Agent.State = Idle
   - Agent.StateChangedAtUtc = now
   - Agent.CurrentTaskId = null
3. 记录任务完成日志
4. 发布AgentCompletedEvent
5. 检查是否触发任务后复盘学习

**业务约束**：
- 仅Handling状态能完成任务
- 必须提供任务产出物

### 步骤3：任务失败（FailTask）

**业务动作**：
1. 验证Agent存在、状态为Handling
2. 使用事务原子执行：
   - Agent.State = Idle
   - Agent.StateChangedAtUtc = now
   - Agent.CurrentTaskId = null
3. 记录失败日志（包含FailureCategory）
4. 发布AgentFailedEvent
5. 触发异常学习（如果FailureCategory为可学习类型）

**业务约束**：
- 仅Handling状态能标记失败
- 必须提供失败原因和分类

### 步骤4：触发学习（StartLearning）

**业务动作**：
1. 验证Agent存在、状态为Idle、不是Dormant
2. 验证无高优先级任务等待
3. 使用事务执行：
   - Agent.State = Learning
   - Agent.StateChangedAtUtc = now
4. 加载学习素材（按Agent.Role）
5. 构造学习Prompt并调用LLM
6. 解析LLM输出，更新SkillTags和LearningProgress
7. 状态变更：Learning → Idle
8. 记录学习完成日志

**业务约束**：
- 仅Idle状态能进入Learning
- Dormant状态不能进入Learning
- 学习完成后自动回归Idle

### 步骤5：进入休眠（EnterDormant）

**业务动作**：
1. 验证Agent存在
2. 验证Agent不在Handling状态
3. 使用事务执行：
   - Agent.State = Dormant
   - Agent.StateChangedAtUtc = now
   - 记录DormantReason
4. 发布AgentEnteredDormantEvent

**业务约束**：
- Handling状态不能直接进入Dormant（必须先释放任务）
- 进入Dormant时必须记录原因

### 步骤6：唤醒Agent（WakeUp）

**业务动作**：
1. 验证Agent存在、状态为Dormant
2. 验证调用者有管理员权限
3. 使用事务执行：
   - Agent.State = Idle
   - Agent.StateChangedAtUtc = now
   - 清除DormantReason
4. 发布AgentWokenEvent

**业务约束**：
- 仅Dormant状态能唤醒
- 必须有管理员权限

### 步骤7：IdleMonitor定时扫描（后台）

**扫描逻辑**：
```
每60秒扫描一次：
1. 查询所有状态为Idle且非Dormant的Agent
2. 对每个Agent计算Idle时长 = now - StateChangedAtUtc
3. 如果Idle时长 >= IdleTimeoutSeconds（按Role配置）：
   - 如果有高优先级任务(priority >= 4)等待：跳过，继续等待
   - 否则：触发StartLearning
```

**扫描约束**：
- Handling和Learning状态不扫描
- Dormant状态不扫描
- 高优先级任务存在时跳过学习

## 5. API契约

### 5.1 Agent状态查询

| Method | Path | 说明 |
|--------|------|------|
| GET | /api/agents/{id}/state | 获取Agent当前状态 |
| GET | /api/agents?state=Idle | 按状态查询Agent列表 |

**Response: AgentStateResponse**
```json
{
    "id": "guid",
    "name": "string",
    "role": "Secretary|Manager|Worker",
    "state": "Idle|Handling|Learning|Dormant",
    "stateChangedAtUtc": "datetime",
    "currentTaskId": "guid|null",
    "dormantReason": "string|null"
}
```

### 5.2 Agent状态操作

| Method | Path | 说明 |
|--------|------|------|
| POST | /api/agents/{id}/assign | 分配任务给Agent |
| POST | /api/agents/{id}/complete | 标记任务完成 |
| POST | /api/agents/{id}/fail | 标记任务失败 |
| POST | /api/agents/{id}/learn | 触发Learning |
| POST | /api/agents/{id}/dormant | 将Agent设为休眠 |
| POST | /api/agents/{id}/wake | 唤醒Agent |
| GET | /api/agents/dormant | 获取所有休眠Agent |

**Request: AssignRequest**
```json
{
    "taskId": "guid"
}
```

**Request: FailRequest**
```json
{
    "reason": "string",
    "failureCategory": "CodeError|LlmException|RequirementIssue|ReviewRejection|Timeout|Unknown"
}
```

**Request: DormantRequest**
```json
{
    "reason": "string"
}
```

## 6. 边界与限制

### 明确声明本Skill"不做什么"
1. **不分配任务**：任务选择和分配策略由multi-04-task-orchestration负责
2. **不处理HumanGate**：门控审批由multi-03-human-gate负责
3. **不生成产出物**：任务执行产出由multi-05-agent-communication负责
4. **不实现LLM调用**：LLM网关由公共基础设施提供
5. **不管理工序Step**：Step流转由multi-01-task-pipeline负责

### 假设前提
1. AgentEntity已包含State、Role、Version字段
2. 数据库支持乐观锁（Version字段）
3. GlobalConfig有角色对应的IdleTimeoutSeconds配置
4. 有LLM网关服务可供调用

### 无法处理的异常
1. Handling状态Agent的数据库连接断开（状态可能不一致）
2. 高并发分配冲突（Version冲突，需重试）
3. Learning过程中LLM服务不可用（状态保持Learning，需超时机制）
4. Dormant唤醒时Agent正在被其他操作修改

## 7. 与其他Skill的关系

| 关系 | Skill | 说明 |
|------|-------|------|
| 上游依赖 | entity-scaffolder | 生成AgentEntity骨架 |
| 上游依赖 | multi-01-task-pipeline | 提供任务状态用于判断是否完成 |
| 下游消费者 | multi-04-task-orchestration | 消费Agent状态来选择可分配的Agent |
| 下游消费者 | multi-06-failure-recovery | 调用FailTask处理失败 |
| 下游消费者 | multi-03-human-gate | 查询Agent状态用于门控判断 |

## 8. 示例

### 示例1：正常任务分配和完成

```
场景：Worker空闲，收到任务分配

输入：agentId=xxx, operation=assign, context={taskId: "yyy"}
处理：
  1. 验证agent.State=Idle
  2. agent.State=Handling, agent.CurrentTaskId="yyy"
  3. 发布AgentAssignedEvent
输出：{success: true, state: "Handling"}

完成后：
输入：agentId=xxx, operation=complete
处理：
  1. agent.State=Idle, agent.CurrentTaskId=null
  2. 发布AgentCompletedEvent
  3. 检查是否触发Learning
输出：{success: true, state: "Idle"}
```

### 示例2：空闲超时触发Learning

```
场景：Worker空闲超过5分钟

Monitor扫描：
  1. 发现worker空闲时间 = 6分钟 > 300秒
  2. 检查高优先级任务：无
  3. 触发StartLearning

StartLearning：
  1. agent.State=Learning
  2. 加载该Worker最近被驳回的任务
  3. 构造学习Prompt
  4. 调用LLM分析
  5. 更新SkillTags
  6. agent.State=Idle
输出：Learning完成，Agent回归Idle
```

### 示例3：Agent进入Dormant

```
场景：Worker连续2次学习效果评分低于阈值

触发Dormant：
  1. 验证agent.State=Idle（非Handling）
  2. agent.State=Dormant, dormantReason="连续学习效果差"
  3. 发布AgentEnteredDormantEvent

唤醒：
  1. 管理员调用wake API
  2. agent.State=Idle, dormantReason=null
  3. 发布AgentWokenEvent
输出：Agent已唤醒，可参与调度
```

## 9. 验收检查清单

- [ ] Agent四状态（Idle/Handling/Learning/Dormant）定义正确
- [ ] 状态转换遵循转换图，无非法转换
- [ ] IdleMonitor按角色差异化超时配置扫描
- [ ] Learning触发后能正确加载学习素材
- [ ] Dormant状态Agent不参与任务分配
- [ ] 状态变更使用乐观锁，冲突返回409
- [ ] Handling超时30分钟能自动释放
- [ ] 所有状态变更记录StateChangedAtUtc
- [ ] 高优先级任务能中断Learning
- [ ] API返回正确的HTTP状态码
