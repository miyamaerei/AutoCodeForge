---
name: "multi-03-human-gate"
description: "人类介入门控机制：7种门控类型(HumanGateType)、门控策略(Policy)、贯穿式介入API(Pause/Resume/Terminate)、前端审批界面、需求变更。当用户说'实现HumanGate'、'审批门控'、'暂停恢复任务'时触发。"
argument-hint: "任务ID（Guid）和门控操作类型（create/approve/reject/pause/resume/terminate）"
---

# Multi-Agent Human Gate Skill

人类介入门控机制核心业务逻辑实现，负责7种门控类型、门控策略判定、贯穿式介入和需求变更。

## 1. Skill名称与描述

- **Skill名称**：`multi-03-human-gate`
- **一句话描述**：HumanGate门控机制，处理7种人类介入场景（需求确认/方案审批/代码Review/测试验收/合并审批/最终签收/紧急介入）、门控策略、贯穿式介入API。
- **使用场景触发条件**：
  - 用户说"实现HumanGate门控"或"审批门控"
  - 用户说"需求确认审批"或"方案审批"
  - 用户说"暂停恢复任务"或"紧急终止"
  - 用户说"HumanGateEntity"或"门控策略"
  - 用户说"实现需求变更"或"执行中更新需求"

## 2. 前置条件与输入要求

### 前置条件
- TaskStepEntity已存在（由multi-01-task-pipeline实现）
- HumanGateEntity实体已存在
- HumanGateRepository已实现
- 已阅读研究报告下篇§17全章（HumanGate机制）
- 已了解ITaskEventPublisher事件发布机制

### 输入要求
| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
| taskId | Guid | 是 | 任务ID |
| operation | string | 是 | create/approve/reject/modify-approve/pause/resume/terminate/update-requirement |
| gateId | Guid | 条件必填 | approve/reject/modify-approve时需要 |
| gateType | string | 条件必填 | create时需要，取值见下 |
| stepId | Guid | 条件必填 | create时需要 |
| reason | string | 否 | 暂停/终止/驳回原因 |
| modifications | object | 条件必填 | modify-approve时需要 |
| newInput | string | 条件必填 | update-requirement时需要 |

### HumanGateType枚举值
| 值 | 枚举名 | 中文名 | 默认策略 |
|------|--------|--------|----------|
| 1 | RequirementConfirm | 需求确认 | always |
| 2 | PlanApproval | 方案审批 | always |
| 3 | CodeReview | 代码审核 | conditional |
| 4 | TestAcceptance | 测试验收 | conditional |
| 5 | MergeApproval | 合并审批 | always |
| 6 | FinalSignoff | 最终签收 | always |
| 7 | Emergency | 紧急介入 | manual |

## 3. 核心业务规则

### 3.1 7种门控类型详解

| GateType | 触发时机 | 人类动作 | 系统行为 |
|----------|---------|---------|----------|
| RequirementConfirm | Step1完成后 | 确认/修正/补充需求 | 确认→Step2；修正→Step1重做 |
| PlanApproval | Step3完成后 | 批准/修改/否决方案 | 批准→Step4；修改→Step3重做；否决→Task终止 |
| CodeReview | Step4完成后（条件触发） | 审查代码 | 通过→Step5；有问题→驳回Step4 |
| TestAcceptance | Step5完成后（条件触发） | 验收测试 | 通过→Step6；不通过→Step4重做 |
| MergeApproval | Step6完成后 | 批准/延迟合并 | 批准→Step7；延迟→等待 |
| FinalSignoff | Step7完成后 | 签收/部分签收/退回 | 签收→Task完成；退回→指定Step |
| Emergency | 任意时刻 | 暂停/接管/终止 | 暂停→等人工决策 |

### 3.2 门控状态机

```
HumanGateStatus状态转换：

Pending ──(人类批准)──→ Approved
    │
    ├──(人类驳回)──→ Rejected
    │
    ├──(人类修改后批准)──→ Modified
    │
    ├──(超时)──→ Timeout
    │
    └──(任务取消)──→ Cancelled
```

### 3.3 门控策略判定规则

**策略类型**：
| 策略 | 说明 | 实现 |
|------|------|------|
| always | 始终触发 | 无条件创建HumanGate |
| conditional | 满足条件触发 | 评估条件表达式 |
| escalation | Agent不确定时升级 | Agent调用Escalate接口 |
| manual | 仅人工主动介入 | 不自动创建，仅记录 |

**Conditional触发条件示例**：
```json
{
    "CodeReview": "changed_files > 5 OR affects_core_module OR contains_database_changes",
    "TestAcceptance": "test_pass_rate < 0.9 OR new_bugs > 0"
}
```

**配置存储**：GlobalConfigEntity，键为HumanGatePolicy

### 3.4 HumanGate与Agent审核的协作

**协作模型**：
```
Worker完成Step → Manager审核 → [HumanGate检查点] → 下一步
                                  │
                          ┌───────┴───────┐
                          │ 是否需要人工？  │
                          └───┬───────┬───┘
                              │       │
                         不需要      需要
                              │       │
                              ↓       ↓
                         直接推进   创建HumanGate
                                      │
                                      ↓
                               等待人类响应
```

**协作规则表**：
| 场景 | Agent审核结果 | HumanGate触发 | 人类可执行操作 |
|------|-------------|-------------|-------------|
| Agent通过 + 无需人工 | Approved | 不创建 | — |
| Agent通过 + 需人工确认 | Approved | 创建 | 批准/修改批准/驳回 |
| Agent驳回 + 需人工确认 | Rejected | 创建 | 确认驳回/推翻Agent决定 |
| Agent不确定 | 待定 | 创建（升级） | 批准/驳回/修改后批准 |

### 3.5 贯穿式介入能力

| 介入类型 | 说明 | API |
|----------|------|-----|
| 暂停任务 | PauseTask | 冻结所有Step，Agent保持当前状态 |
| 恢复任务 | ResumeTask | 解冻，继续执行 |
| 接管任务 | TakeOver | Agent释放，任务转人工 |
| 紧急终止 | ForceTerminate | 所有Step=Cancelled，Agent释放 |
| 需求变更 | UpdateRequirement | 更新Task.Input，触发Step重新执行 |

**暂停规则**：
- 暂停后，当前Handling的Step状态不变
- 其他Step保持Pending
- 暂停的Agent不参与新任务分配
- 恢复时，从暂停点继续

**终止规则**：
- 所有Step状态变更为Cancelled
- 所有绑定的Agent释放回Idle
- Task状态变更为Terminated
- 不可恢复

### 3.6 需求变更机制

**UpdateRequirement流程**：
1. 人类提交新需求文本
2. 系统更新Task.Input
3. 确定需要重做的Step（通常从变更点开始）
4. 重置相关Step为Pending
5. 通知相关Agent重新执行

**重做策略**：
| 变更范围 | 重做起点 | 说明 |
|----------|---------|------|
| 需求方向变更 | Step1 | 从头开始 |
| 方案影响 | Step3 | 重新方案 |
| 开发范围变更 | Step4 | 重新开发 |
| 测试问题 | Step5 | 重新测试 |

## 4. 执行步骤（业务逻辑实现）

### 步骤1：判断是否需要创建HumanGate

**业务动作**（在TaskStepFlowService.MoveToNextStepAsync中调用）：
1. 获取完成的Step和GateType映射
2. 从GlobalConfig读取HumanGatePolicy
3. 根据GateType判断策略：
   - always：直接创建
   - conditional：评估条件表达式
   - 其他：不创建，返回null
4. 如果需要创建，调用CreateGateAsync

**业务约束**：
- 仅在Step完成时检查门控
- Emergency类型仅人工触发

### 步骤2：创建HumanGate

**业务动作**：
1. 验证Task存在、Step完成
2. 创建HumanGateEntity：
   - TaskId, TaskStepId, GateType
   - Status = Pending
   - Reason = Step产出摘要
   - ReviewerUserId = null（待认领）
3. 冻结Task状态（可选，取决于配置）
4. 发布HumanGateCreatedEvent
5. 触发通知（调用INotificationService）

**业务约束**：
- 一个Step只能有一个Pending的HumanGate
- 创建时必须记录Reason供人类参考

### 步骤3：人类审批 - 批准（Approve）

**业务动作**：
1. 验证HumanGate存在、状态为Pending
2. 验证审批人有权限（ReviewerUserId匹配或管理员）
3. 使用事务执行：
   - HumanGate.Status = Approved
   - HumanGate.ReviewerUserId = 审批人ID
   - HumanGate.RespondedAtUtc = now
4. 激活下一Step（调用TaskStepFlowService）
5. 发布HumanGateApprovedEvent
6. 触发通知（通知相关Agent）

**业务约束**：
- 仅Pending状态可批准
- 批准后流程继续，不能撤销

### 步骤4：人类审批 - 驳回（Reject）

**业务动作**：
1. 验证HumanGate存在、状态为Pending
2. 验证审批人有权限
3. 使用事务执行：
   - HumanGate.Status = Rejected
   - HumanGate.HumanResponse = 驳回原因
   - HumanGate.ReviewerUserId = 审批人ID
   - HumanGate.RespondedAtUtc = now
4. 重置对应Step为Pending（重新执行）
5. 发布HumanGateRejectedEvent
6. 触发通知

**业务约束**：
- 驳回后Step重新执行，RetryCount+1
- 连续驳回超限（3次）触发TaskFailed

### 步骤5：人类审批 - 修改后批准（ModifyApprove）

**业务动作**：
1. 验证HumanGate存在、状态为Pending
2. 验证审批人有权限
3. 使用事务执行：
   - HumanGate.Status = Modified
   - HumanGate.HumanResponse = 修改内容
   - HumanGate.Modifications = 人类修改的上下文
   - HumanGate.ReviewerUserId = 审批人ID
   - HumanGate.RespondedAtUtc = now
4. 将修改内容注入NextStep的Input
5. 激活下一Step
6. 发布HumanGateModifiedEvent

**业务约束**：
- 修改内容会作为下一个Step的额外输入
- 记录修改历史用于审计

### 步骤6：暂停任务（PauseTask）

**业务动作**：
1. 验证Task存在、不已是Paused状态
2. 使用事务执行：
   - Task.Status = Paused
   - 记录PauseReason
3. 不释放Agent（保持当前状态）
4. 发布TaskPausedEvent
5. 触发通知（通知相关人员）

**业务约束**：
- Paused状态Task不参与调度
- Agent状态保持不变，仅暂停执行

### 步骤7：恢复任务（ResumeTask）

**业务动作**：
1. 验证Task存在、状态为Paused
2. 使用事务执行：
   - Task.Status = Pending（如果之前是Pending）或 InProgress（如果之前是InProgress）
3. 发布TaskResumedEvent
4. 触发通知

**业务约束**：
- 仅Paused状态能恢复
- 从暂停点继续执行

### 步骤8：紧急终止（ForceTerminate）

**业务动作**：
1. 验证Task存在、不已是Completed/Terminated
2. 使用事务执行：
   - Task.Status = Terminated
   - 所有Pending/Handling的Step.Status = Cancelled
   - 释放所有绑定的Agent → Idle
3. 发布TaskTerminatedEvent
4. 触发通知（所有相关人员）
5. 记录终止原因和操作人

**业务约束**：
- 终止后Task不可恢复
- 必须记录终止原因

### 步骤9：需求变更（UpdateRequirement）

**业务动作**：
1. 验证Task存在、不是Completed/Terminated
2. 获取变更影响的Step范围
3. 使用事务执行：
   - Task.Input = 新需求
   - 相关Step.Status = Pending
   - Step.Input = 新需求（如果从Step1重做）
4. 重置Task.CurrentStep（指向重做起点）
5. 发布RequirementUpdatedEvent
6. 触发通知

**业务约束**：
- 需求变更必须记录变更内容
- 确定重做起点后，被动过的Step都要重做

## 5. API契约

### 5.1 HumanGate查询

| Method | Path | 说明 |
|--------|------|------|
| GET | /api/human-gates/pending | 获取当前用户待处理门控列表 |
| GET | /api/human-gates/{id} | 获取门控详情 |
| GET | /api/tasks/{taskId}/gates | 获取任务的所有门控记录 |

**Response: HumanGateResponse**
```json
{
    "id": "guid",
    "taskId": "guid",
    "taskStepId": "guid",
    "gateType": "RequirementConfirm|PlanApproval|...",
    "gateTypeName": "string",
    "status": "Pending|Approved|Rejected|Modified|Timeout|Cancelled",
    "reason": "string",
    "humanResponse": "string|null",
    "modifications": "object|null",
    "reviewerUserId": "guid|null",
    "reviewerName": "string|null",
    "createdAtUtc": "datetime",
    "respondedAtUtc": "datetime|null"
}
```

### 5.2 HumanGate操作

| Method | Path | 说明 |
|--------|------|------|
| POST | /api/human-gates/{id}/approve | 批准门控 |
| POST | /api/human-gates/{id}/reject | 驳回门控 |
| POST | /api/human-gates/{id}/modify-approve | 修改后批准 |
| POST | /api/human-gates/{id}/cancel | 取消门控（仅管理员） |

**Request: ApproveRequest**
```json
{
    "comment": "string"
}
```

**Request: RejectRequest**
```json
{
    "reason": "string"
}
```

**Request: ModifyApproveRequest**
```json
{
    "modifications": {
        "input": "string",
        "instructions": "string"
    }
}
```

### 5.3 任务贯穿式介入

| Method | Path | 说明 |
|--------|------|------|
| POST | /api/tasks/{id}/pause | 暂停任务 |
| POST | /api/tasks/{id}/resume | 恢复任务 |
| POST | /api/tasks/{id}/terminate | 紧急终止 |
| POST | /api/tasks/{id}/update-requirement | 需求变更 |
| GET | /api/tasks/{id}/pause-history | 获取暂停历史 |

**Request: PauseRequest**
```json
{
    "reason": "string"
}
```

**Request: TerminateRequest**
```json
{
    "reason": "string"
}
```

**Request: UpdateRequirementRequest**
```json
{
    "newInput": "string",
    "restartFromStep": 1
}
```

## 6. 边界与限制

### 明确声明本Skill"不做什么"
1. **不发送通知**：通知发送由multi-08-notification-integration负责
2. **不管理Agent状态**：Agent状态转换由multi-02-agent-lifecycle负责
3. **不分配任务**：任务调度由multi-04-task-orchestration负责
4. **不管理工序流转**：Step流转由multi-01-task-pipeline负责
5. **不生成代码**：代码生成由其他Skill负责

### 假设前提
1. HumanGateEntity已存在且包含所有必要字段
2. HumanGateRepository已实现CRUD方法
3. INotificationService接口已定义
4. Task状态机支持Paused和Terminated状态
5. GlobalConfig有HumanGatePolicy配置

### 无法处理的异常
1. 门控超时（需外部定时器触发）
2. 门控创建时Step已被其他操作处理
3. 修改批准时NextStep已被取消
4. 并发审批冲突（Version冲突）

## 7. 与其他Skill的关系

| 关系 | Skill | 说明 |
|------|-------|------|
| 上游依赖 | multi-01-task-pipeline | 提供Step完成事件触发门控检查 |
| 上游依赖 | multi-02-agent-lifecycle | Agent状态用于判断是否释放 |
| 下游消费者 | multi-08-notification-integration | 消费门控事件发送通知 |
| 下游消费者 | multi-04-task-orchestration | 门控影响Task状态，触发重新调度 |
| 下游消费者 | multi-06-failure-recovery | 连续驳回触发失败处理 |

## 8. 示例

### 示例1：方案审批门控

```
场景：Step3(方案计划)完成，触发方案审批门控

触发：
  1. TaskStepFlowService.MoveToNextStepAsync检测Step3完成
  2. HumanGatePolicy.PlanApproval=always
  3. 创建HumanGate(Status=Pending, GateType=PlanApproval)

人类审批（批准）：
  1. 管理员查看门控，看到方案文档
  2. 点击批准
  3. HumanGate.Status=Approved
  4. Step4(Development)变为Pending
  5. 通知相关Worker开始开发

人类审批（驳回）：
  1. 管理员发现方案有问题，点击驳回
  2. HumanGate.Status=Rejected, HumanResponse="方案风险过高"
  3. Step3重置为Pending，RetryCount+1
  4. 通知Worker重新做方案
```

### 示例2：紧急暂停和恢复

```
场景：任务执行中，人类发现方向错误

暂停：
  1. 人类点击"暂停任务"
  2. Task.Status=Paused
  3. 当前Handling的Step保持Handling
  4. Agent保持当前状态，不释放
  5. 通知相关人员任务已暂停

人工介入后恢复：
  1. 人类修正了错误方向
  2. 点击"恢复任务"
  3. Task.Status=InProgress
  4. 继续从暂停点执行
```

### 示例3：需求变更

```
场景：Step3执行中，人类决定修改需求

变更：
  1. 人类调用UpdateRequirement，提交新需求
  2. Task.Input=新需求
  3. 确定重做起点=Step3
  4. Step3/4/5/6/7重置为Pending
  5. Task.CurrentStep=Step3
  6. 通知所有相关Agent重新执行
```

## 9. 验收检查清单

- [ ] 7种HumanGateType枚举定义正确
- [ ] HumanGateStatus状态转换正确
- [ ] always策略的门控在触发点自动创建
- [ ] conditional策略的门控条件判断正确
- [ ] 批准后流程能正确推进到下一步
- [ ] 驳回后Step能重置并增加RetryCount
- [ ] 修改批准能将修改内容注入NextStep
- [ ] 暂停任务能冻结Task和Step状态
- [ ] 终止任务能释放所有Agent
- [ ] 需求变更能正确更新上下文并重做相关Step
- [ ] 所有操作记录操作日志
- [ ] API返回正确的HTTP状态码
- [ ] 门控超时能正确触发Timeout状态
