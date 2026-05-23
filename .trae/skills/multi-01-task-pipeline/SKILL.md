---
name: "multi-01-task-pipeline"
description: "7步工序流水线核心业务逻辑：工序流转引擎、上下文链式传递与Token截断策略、Step超时检测与应急解绑机制。当用户说'实现7步工序'、'工序流转'、'Step卡死解绑'时触发。"
argument-hint: "任务ID（Guid）和操作类型（init/advance/skip/unbind）"
---

# Multi-Agent Task Pipeline Skill

7步工序流水线核心业务逻辑实现，负责工序流转控制、上下文管理和异常处理。

## 1. Skill名称与描述

- **Skill名称**：`multi-01-task-pipeline`
- **一句话描述**：7步工序流水线引擎，处理工序初始化、顺序流转、上下文截断、超时检测与应急解绑。
- **使用场景触发条件**：
  - 用户说"实现7步工序流水线"或"工序怎么流转"
  - 用户说"Step卡住自动解绑"或"工序超时释放"
  - 用户说"上下文怎么传递"或"Token截断策略"
  - 用户说"实现TaskStepFlowService"或"工序服务"

## 2. 前置条件与输入要求

### 前置条件
- TaskStepEntity实体已存在（由entity-scaffolder生成）
- TaskStepRepository已存在（由entity-scaffolder生成）
- TaskEntity已包含CurrentStep和CurrentStepId字段
- 已阅读研究报告上篇§3.2.2（7步工序枚举）、§3.3.1（TaskStepEntity设计）
- 已阅读研究报告下篇§12.3（上下文链式传递）、§16.7（上下文截断）、§16.8（Step卡死解绑）

### 输入要求
| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
| taskId | Guid | 是 | 任务ID |
| operation | string | 是 | init/advance/skip/unbind |
| stepId | Guid | 条件必填 | advance/skip时需要 |
| reason | string | 条件必填 | skip/unbind时需要 |
| output | string | 否 | advance时的产出物 |

## 3. 核心业务规则

### 3.1 7步工序定义

| Step值 | 枚举名 | 中文名 | 执行角色 | 产出说明 |
|--------|--------|--------|----------|----------|
| 1 | DemandAnalyse | 需求梳理 | Worker | 需求说明书、任务拆解清单 |
| 2 | QueryCurrent | 查询当前信息 | Worker | 现状报告、数据比对表 |
| 3 | MakePlan | 方案计划 | Worker | 方案文档、排期表、风险清单 |
| 4 | Development | 代码开发 | Worker | 代码变更diff、注释文档 |
| 5 | TestVerify | 测试校验 | Worker | 测试报告、Bug清单 |
| 6 | CommitPr | 版本提交 | Worker | PR信息、冲突报告 |
| 7 | FinalAudit | 最终审核 | Manager | 验收结论、交付物清单 |

### 3.2 工序状态机

```
TaskStepStatus状态转换规则：

Pending ──(Worker领取)──→ Handling ──(完成产出)──→ Completed
    │                          │
    │                          ├──(重试超限/超时)──→ Failed ──(重置)──→ Pending
    │                          │
    │                          └──(人工跳过)──→ Skipped
```

**转换约束**：
- Pending → Handling：仅当该Step是当前活跃Step时才能领取
- Handling → Completed：必须提供有效产出物（output）
- Handling → Failed：重试次数超限或超时触发
- Skipped：仅能跳过当前Step的下一个Step，不能跳跃跳过

### 3.3 工序流转顺序规则

```
初始化顺序：Step1→Step2→Step3→Step4→Step5→Step6→Step7（严格顺序）

推进规则：
1. 只有当前Step状态为Completed/Failed/Skipped时，才能推进到下一步
2. 推进时自动将当前Step的Output作为下一步的Input
3. 最后一步完成后，Task.Status自动变为Completed

跳过规则：
1. 只能跳过当前活跃Step的下一个Step
2. 跳过需要提供reason，记录到SkipReason字段
3. 跳过后自动激活再下一个Step
```

### 3.4 上下文链式传递协议

**传递结构**：
```
TaskGlobalContext {
    OriginalInput: string      // 原始需求，永不截断
    ProcessLog: string[]       // 流程摘要，历史记录
}

StepContext {
    StepNumber: int            // 工序序号
    Input: string             // 当前输入（上个Step的Output或TaskGlobalContext）
    Output: string            // 当前产出物
    AgentId: Guid?            // 执行Agent
    StartedAt: DateTime       // 开始时间
    CompletedAt: DateTime?    // 完成时间
}
```

**传递规则**：
1. TaskGlobalContext.OriginalInput始终保留
2. 每个Step完成后，其Output截断摘要后成为NextStep的Input
3. ProcessLog记录每个Step的一句话summary

### 3.5 上下文Token截断策略

**截断参数**（来自GlobalConfig）：
| 参数 | 默认值 | 说明 |
|------|--------|------|
| MaxStepContextTokens | 8000 | 单Step上下文上限 |
| MaxGlobalContextTokens | 2000 | 全局上下文上限 |

**截断优先级**：
| 优先级 | 保留内容 | 截断方式 |
|--------|----------|-----------|
| P1（必保） | Task.Input原始需求 | 不截断 |
| P2（高优） | 最近2个Step的完整产出 | 完整保留 |
| P3（中优） | 更早Step的摘要 | 按距离递减截断 |
| P4（低优） | 审核评论 | 仅保留驳回关键信息 |

**截断算法**：
```
BuildContext(taskId, currentStepId, maxTokens):
    context = ""
    context += "# Original Input:\n" + task.Input + "\n"
    
    completedSteps = GetCompletedSteps(taskId, currentStepId)
    recentSteps = completedSteps.TakeLast(2)  // P2
    for step in recentSteps:
        context += step.Output + "\n"
    
    earlierSteps = completedSteps.Take(completedSteps.Count - 2)  // P3
    for step in earlierSteps:
        context += TruncateToNTokens(step.Output, 500) + "\n"
    
    if TokenCount(context) > maxTokens:
        context = TruncateByPriority(context, maxTokens)
    
    return context
```

### 3.6 Step卡死检测与应急解绑

**卡死判定条件**（满足任一即触发）：
| 条件 | 阈值 | 说明 |
|------|------|------|
| Handling超时 | 30分钟 | Step.Handling持续超过30分钟无状态变更 |
| 重试超限 | 3次 | Step.RetryCount >= 3 |
| LLM异常 | 1次 | FailureCategory=LlmException时立即触发 |

**解绑后处理策略**：
| 场景 | Step处理 | Agent处理 | Task处理 |
|------|----------|-----------|----------|
| 前置工序(1-3)失败 | Step重置为Failed | Agent→Idle | Task→Failed |
| 后置工序(4-7)失败 | Step重置为Pending，换Worker | 旧Agent→Idle，新Agent→Handling | Task继续 |
| Worker卡死超时 | Step重置为Pending | Agent→Idle | Task继续 |
| LLM异常 | Step保持原状态 | Agent→Idle（非Worker责任） | Task继续 |

**解绑操作原子性要求**：
```
UnbindStep(stepId, reason):
    BEGIN TRANSACTION
        step.Status = Pending
        step.WorkerAgentId = NULL
        step.RetryCount = 0
        step.StartedAtUtc = NULL
        Update(step)
        
        agent = GetAgent(step.WorkerAgentId)
        agent.Status = Idle
        agent.StateChangedAtUtc = now
        Update(agent)
        
        Log(new StepUnboundEvent(stepId, reason))
    COMMIT
```

## 4. 执行步骤（业务逻辑实现）

### 步骤1：初始化7步工序

**业务动作**：
1. 验证Task存在且状态为Pending
2. 创建7条TaskStep记录（Step1-7），Status均为Pending
3. Step1的Input=Task.Input（原始需求）
4. 更新Task.CurrentStep=Step1，Task.CurrentStepId=Step1.Id
5. 记录ProcessLog：{"action": "init_steps", "step": 1}

**业务约束**：
- 7步必须按顺序创建，不能跳步
- 每个Step的Step值必须与枚举对应（1-7）
- Step1创建后即为当前活跃Step

### 步骤2：推进工序（Advance）

**业务动作**：
1. 验证Step存在、属于该Task、状态为Handling
2. 验证Step产出物output不为空（如果Step需要产出）
3. 更新Step.Status=Completed，Step.Output=output，Step.CompletedAtUtc=now
4. 查找下一个Step（按Step值排序）
5. 更新NextStep.Status=Pending（如果存在），NextStep.Input=output
6. 更新Task.CurrentStep=NextStep，Task.CurrentStepId=NextStep.Id
7. 如果是最后一步，更新Task.Status=Completed
8. 记录ProcessLog

**业务约束**：
- 仅Handling状态能推进到Completed
- 产出物为必填（除非Step定义为无需产出）
- 最后一步完成后Task必须标记Completed

### 步骤3：跳过工序（Skip）

**业务动作**：
1. 验证Step存在、属于该Task
2. 验证Step是当前活跃Step的下一个Step（不能跳越多步）
3. 更新Step.Status=Skipped，Step.SkipReason=reason
4. 查找再下一个Step
5. 更新该Step.Status=Pending
6. 更新Task.CurrentStep和CurrentStepId指向该Step
7. 记录ProcessLog：{"action": "skip", "step": x, "reason": reason}

**业务约束**：
- 不能跳过当前活跃Step本身
- 不能跳过当前活跃Step之后的第二个Step以后
- 必须提供跳过原因reason

### 步骤4：应急解绑（Unbind）

**业务动作**：
1. 验证Step存在、状态为Handling
2. 获取该Step绑定的WorkerAgent
3. 使用事务原子性执行：
   - Step.Status = Pending
   - Step.WorkerAgentId = NULL
   - Step.RetryCount = 0
   - Step.StartedAtUtc = NULL
   - Agent.Status = Idle（根据卡死原因决定是否变更）
4. 判断Task是否需要标记Failed（前置工序1-3失败则TaskFailed）
5. 发布StepUnboundEvent供Orchestrator重新分配
6. 记录ProcessLog

**业务约束**：
- 必须使用事务确保原子性
- Agent状态变更必须同步
- 事件发布在事务提交后

### 步骤5：构建上下文（BuildContext）

**业务动作**：
1. 获取Task.GlobalContext（原始需求+流程摘要）
2. 获取所有已完成Step的Output
3. 按截断优先级构建上下文字符串：
   - P1：原始需求（完整）
   - P2：最近2个Step（完整）
   - P3：更早Step（截断至500 tokens）
   - P4：审核意见（仅保留关键驳回信息）
4. 如果总长度超过MaxStepContextTokens，按优先级截断
5. 返回截断后的上下文字符串

**业务约束**：
- 原始需求永远不能被截断
- 必须保留最近2个Step的完整产出
- 截断后的上下文必须仍能还原业务含义

## 5. API契约

### 5.1 工序查询

| Method | Path | 说明 |
|--------|------|------|
| GET | /api/tasks/{taskId}/steps | 获取任务所有工序列表 |
| GET | /api/tasks/{taskId}/steps/active | 获取当前活跃工序 |
| GET | /api/tasks/{taskId}/steps/{stepId} | 获取指定工序详情 |

**Response: TaskStepResponse**
```json
{
    "id": "guid",
    "taskId": "guid",
    "step": 1,
    "stepName": "DemandAnalyse",
    "status": "Pending|Handling|Completed|Failed|Skipped",
    "workerAgentId": "guid|null",
    "reviewerAgentId": "guid|null",
    "input": "string|null",
    "output": "string|null",
    "startedAtUtc": "datetime|null",
    "completedAtUtc": "datetime|null",
    "retryCount": 0
}
```

### 5.2 工序操作

| Method | Path | 说明 |
|--------|------|------|
| POST | /api/tasks/{taskId}/steps/init | 初始化7步工序 |
| POST | /api/tasks/{taskId}/steps/{stepId}/advance | 推进工序到下一步 |
| POST | /api/tasks/{taskId}/steps/{stepId}/skip | 跳过工序 |
| POST | /api/tasks/{taskId}/steps/{stepId}/unbind | 应急解绑 |
| GET | /api/tasks/{taskId}/context | 获取当前工序的上下文 |

**Request: AdvanceRequest**
```json
{
    "output": "string"  // 必填，当前工序产出物
}
```

**Request: SkipRequest**
```json
{
    "reason": "string"  // 必填，跳过原因
}
```

**Request: UnbindRequest**
```json
{
    "reason": "string",  // 必填，解绑原因
    "failureCategory": "CodeError|LlmException|Timeout"  // 可选，失败分类
}
```

## 6. 边界与限制

### 明确声明本Skill"不做什么"
1. **不生成代码**：代码生成由entity-scaffolder负责，本Skill只实现业务逻辑
2. **不分配Agent**：Worker的选择和分配由multi-04-task-orchestration负责
3. **不处理门控审批**：HumanGate审批由multi-03-human-gate负责，本Skill仅提供触发点
4. **不管理Agent状态**：Agent状态转换由multi-02-agent-lifecycle负责
5. **不实现前端UI**：前端工序视图由vue3-page-builder生成的模块负责

### 假设前提
1. TaskStepEntity已存在且包含所有必要字段
2. TaskRepository和TaskStepRepository已实现CRUD方法
3. 数据库使用乐观锁（Version字段）处理并发
4. GlobalConfig中有MaxStepContextTokens和MaxGlobalContextTokens配置

### 无法处理的异常
1. 跨Task操作Step（一个Step只能属于一个Task）
2. 对已完成/已跳过/已失败的Step再次推进
3. 非顺序跳步（跳过非相邻Step）
4. 并发冲突（Version不匹配时需前端重试）

## 7. 与其他Skill的关系

| 关系 | Skill | 说明 |
|------|-------|------|
| 上游依赖 | entity-scaffolder | 生成TaskStepEntity和Repository骨架 |
| 上游依赖 | multi-02-agent-lifecycle | 提供Agent状态查询和转换 |
| 下游消费者 | multi-03-human-gate | 消费Step完成事件触发门控 |
| 下游消费者 | multi-04-task-orchestration | 查询ActiveStep进行分配 |
| 下游消费者 | multi-06-failure-recovery | 调用UnbindStep处理卡死 |

## 8. 示例

### 示例1：正常工序流转

```
场景：Worker完成需求梳理，推进到方案计划

输入：taskId=xxx, operation=advance, stepId=step1-id, output="需求完成，涉及3个功能点"
处理：
  1. step1.Status=Completed, step1.Output=output
  2. step2.Status=Pending, step2.Input=output
  3. task.CurrentStep=MakePlan, task.CurrentStepId=step2-id
输出：{nextStepId: step2-id, nextStep: 3}
```

### 示例2：Step卡死解绑

```
场景：Development工序(Step4)超时30分钟，Worker被锁死

输入：taskId=xxx, operation=unbind, stepId=step4-id, reason="Handling超时30分钟"
处理：
  1. 开启事务
  2. step4.Status=Pending, step4.WorkerAgentId=null, step4.RetryCount=0
  3. worker-agent.Status=Idle
  4. 提交事务
  5. 发布StepUnboundEvent
输出：{success: true, message: "Step已解绑，可重新分配"}
```

### 示例3：上下文截断

```
场景：任务进行到Step6，需要构建Step7的输入上下文

输入：taskId=xxx, operation=context, stepId=step7-id
处理：
  1. 获取原始需求（1000 tokens，不截断）
  2. 获取Step4和Step5产出（各3000 tokens，完整）
  3. 获取Step1-3摘要（各200 tokens）
  4. 总计约10200 tokens，超过8000限制
  5. 按优先级截断Step1-3摘要
输出：截断后约7500 tokens的上下文
```

## 9. 验收检查清单

- [ ] 7步工序按定义顺序初始化，不能跳步
- [ ] Advance只能推进Handling状态的Step
- [ ] Skip只能跳过当前活跃Step的下一个Step
- [ ] Unbind使用事务确保Step和Agent状态同时更新
- [ ] BuildContext按优先级截断，原始需求不丢失
- [ ] 最后一步完成后Task.Status自动变为Completed
- [ ] 前置工序(1-3)失败时Task标记Failed
- [ ] 所有操作记录ProcessLog
- [ ] API返回正确的HTTP状态码（200/400/404/409/500）
- [ ] 乐观锁冲突返回409，前端能处理重试
