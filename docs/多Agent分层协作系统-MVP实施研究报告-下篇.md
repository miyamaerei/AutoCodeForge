# 多Agent分层协作系统 MVP实施研究报告（下篇）

**报告日期**: 2026-05-22
**报告版本**: v4.0
**本文范围**: 第九章~第十七章（进阶设计与改进方案）
**完整报告**: [上篇](./多Agent分层协作系统-MVP实施研究报告-上篇.md) | [下篇](./多Agent分层协作系统-MVP实施研究报告-下篇.md) | [索引](./多Agent分层协作系统-MVP实施研究报告-INDEX.md)

---

## 九、工作量估算

| 阶段 | 后端 | 前端 | 测试 |
|------|------|------|------|
| Phase 1 数据模型 | 2天 | — | 0.5天 |
| Phase 2 状态机+工序引擎 | 3天 | — | 1天 |
| Phase 3 编排服务+多秘书 | 3天 | — | 1天 |
| Phase 4 审核流程 | 2天 | — | 1天 |
| Phase 5 API端点 | 2天 | — | 0.5天 |
| Phase 6 前端集成 | — | 5天 | 1天 |
| Phase 7 测试加固 | — | — | 3天 |
| **合计** | **12天** | **5天** | **8天** |

**MVP总工期估算：5-7周（含前端+测试）**

---

## 十、关键决策总结（v2.0）

| # | 决策 | 结论 |
|---|------|------|
| 1 | Workflow vs Task | Task是业务实体，Step是编排单元，不替代 |
| 2 | Elsa工作流 | MVP不引入，Stateless足够 |
| 3 | 秘书Agent | **多实例Agent实体**（非服务类），有独立状态与生命周期 |
| 4 | Agent状态 | **Idle/Handling/Learning**（"处理问题"非"执行"） |
| 5 | 部门 | MVP推迟，仅留DepartmentId?可空字段 |
| 6 | 学习状态 | 枚举值存在，MVP不实现Learning逻辑 |
| 7 | ScheduledTask | 保持不变，作为触发器 |
| 8 | 旧TaskExecutor | 保留处理旧TaskType，新流水线走新路径 |
| 9 | PipelineEntity | 明确为CI/CD流水线，与工序流水线无关 |
| 10 | 数据迁移 | 旧任务标记Skipped，新任务走完整7步 |

---

## 十一、多服务器部署与Agent注册（分布式演进）

### 11.1 问题：老大和小弟可能部署在不同服务器

MVP阶段所有Agent在同一进程内运行，但**架构必须从一开始就支持跨服务器部署**。原因：
- LLM推理负载差异大：小弟(Worker)密集调用LLM，可能需要GPU服务器
- 老大(Manager)审核型任务轻量，可在CPU服务器运行
- 秘书(Secretary)调度型任务极轻量，与Web服务器同进程即可
- 未来按角色横向扩容：Worker独立扩容、Manager独立扩容

### 11.2 方案：数据库注册 + 心跳续约

**核心思路**：所有Agent实例在共享数据库中注册，通过心跳续约维持"存活"状态。任何服务器的调度器查询数据库即可发现可用Agent。

```
服务器A (Web + 秘书)           服务器B (Manager)           服务器C (Worker集群)
┌─────────────────┐         ┌─────────────────┐         ┌──────────────────────┐
│ Secretary-01    │         │ Manager-01      │         │ Worker-01  Worker-02 │
│ Secretary-02    │         │                 │         │ Worker-03  Worker-04 │
│                 │         │                 │         │                      │
│ 心跳→DB 每30s   │         │ 心跳→DB 每30s   │         │ 心跳→DB 每30s        │
└────────┬────────┘         └────────┬────────┘         └──────────┬───────────┘
         │                           │                             │
         └───────────┬───────────────┴─────────────┬───────────────┘
                     │                             │
              ┌──────▼──────┐              ┌──────▼──────┐
              │  共享数据库   │              │  共享存储    │
              │  AgentEntity │              │  产出物存储  │
              │  TaskStep    │              │  上下文快照  │
              │  TaskReview  │              │             │
              └─────────────┘              └─────────────┘
```

### 11.3 AgentEntity新增分布式字段

| 字段 | 类型 | 说明 | 优先级 |
|------|------|------|--------|
| `ServerNode` | `string?` | 注册的服务器标识(IP+进程ID) | P1 |
| `LastHeartbeatAtUtc` | `DateTime?` | 最后心跳时间 | P1 |
| `AgentEndpoint` | `string?` | Agent的HTTP通信端点(跨服务器调用) | P2 |

### 11.4 心跳续约机制

```pseudo
// 每个服务器启动时运行的心跳后台服务
class AgentHeartbeatService :
    loop every 30 seconds:
        for each local_agent in this_server:
            UPDATE AgentEntity 
            SET LastHeartbeatAtUtc = now, State = compute_current_state()
            WHERE Id = local_agent.Id AND Version = current_version
            
            if update_failed (version mismatch):
                // 被其他服务器修改了状态，重新加载
                reload agent state from database
```

**判定规则**：
- `LastHeartbeatAtUtc`超过90秒未更新 → 标记Agent为**离线(Lost)**
- 离线Agent的任务自动重新分配（异常兜底调度器处理）
- 同一Agent不能同时在两台服务器注册（启动时校验ServerNode）

### 11.5 MVP → 分布式演进路线

| 阶段 | Agent注册 | Agent通信 | 消息机制 | 锁机制 |
|------|----------|----------|---------|--------|
| MVP | 数据库注册(同进程) | 进程内方法调用 | 进程内事件 | lock() |
| V2 | 数据库注册+心跳(跨进程) | HTTP/gRPC调用 | MassTransit | Medallion Lock |
| V3 | 服务发现(Consul) | gRPC双向流 | RabbitMQ/Kafka | Redis分布式锁 |

### 11.6 跨服务器任务分配流程

```
1. 秘书提交任务 → 写入TaskStep(Status=Pending)
2. AgentDispatcherService查询：
   SELECT * FROM Agents 
   WHERE Role='Worker' AND State='Idle' 
   AND LastHeartbeatAtUtc > now-90s
   ORDER BY LastHeartbeatAtUtc DESC
3. 原子更新Agent.State=Handling + TaskStep.WorkerAgentId (乐观锁)
4. 如果Agent在本地 → 直接调用
5. 如果Agent在远程 → HTTP POST AgentEndpoint/execute-step
```

---

## 十二、Agent间通信、上下文与产出

### 12.1 核心问题

三类Agent协作时，必须解决三个关键问题：
1. **通信**：Agent之间如何传递指令和结果？
2. **上下文**：后续Agent如何知道前面Agent做了什么？
3. **产出**：每个Agent的产出物如何标准化、如何流转？

### 12.2 通信模型：间接通信（通过数据库+事件）

**设计原则**：Agent之间不直接通信，通过**共享数据库+事件总线**间接通信。

```
秘书 ──写入Task+Step──→ 数据库 ←──查询待处理Step──→ 老大
秘书 ──发布事件──→ 事件总线 ←──订阅事件──→ 老大/小弟
老大 ──写入Review──→ 数据库 ←──查询待审核──→ 调度器
小弟 ──写入Step.Output──→ 数据库 ←──查询下一步Input──→ 调度器
```

**MVP通信方式**：
| 通信场景 | 方式 | 实现 |
|----------|------|------|
| 秘书→小弟(分配任务) | 数据库写入+进程内事件 | TaskStepEntity + ITaskEventPublisher |
| 小弟→老大(提交审核) | 数据库写入+进程内事件 | TaskReviewEntity + ITaskEventPublisher |
| 老大→小弟(驳回重试) | 数据库更新+进程内事件 | TaskStepEntity.Status=Handling + ITaskEventPublisher |
| 老大→秘书(终审通过) | 数据库更新+进程内事件 | TaskEntity.Status=Completed + ITaskEventPublisher |
| 任意Agent→调度器(状态变更) | 数据库更新+进程内事件 | AgentEntity.State + ITaskEventPublisher |

### 12.3 上下文模型：链式传递

**核心设计**：每个工序的产出物(Output)自动成为下一个工序的输入(Input)上下文。同时，任务全局上下文贯穿全流程。

```
┌──────────────────────────────────────────────────────┐
│                   Task全局上下文                       │
│  TaskEntity.Input (原始需求)                           │
│  + 累积的ProcessLog (所有流转记录)                       │
└──────────┬───────────────────────────────────────────┘
           │
           ▼
┌─────────────────┐    产出     ┌─────────────────┐    产出     ┌─────────────────┐
│ Step1 需求梳理    │──────────→│ Step2 查询信息    │──────────→│ Step3 方案计划    │
│ Input: 原始需求   │            │ Input: Step1产出  │            │ Input: Step2产出  │
│ + Task全局上下文  │            │ + Task全局上下文  │            │ + Task全局上下文  │
│ Output: 需求文档  │            │ Output: 现状报告  │            │ Output: 方案文档  │
└─────────────────┘            └─────────────────┘            └─────────────────┘
       ...后续工序同理...
```

**关键约束**：
- 全局上下文(原始需求+流程摘要)始终携带，避免信息丢失
- 前序产出物**提取摘要**后传递，避免上下文无限膨胀
- 产出物存储在`TaskStepEntity.Output`字段(TEXT类型)
- 超长产出物(>100KB)存文件，Output字段仅存引用路径

### 12.4 产出物标准化

**每个工序的产出物必须遵循统一契约**：

```json
{
  "step": "DemandAnalyse",
  "agent_id": "uuid",
  "agent_role": "Worker",
  "produced_at": "2026-05-22T10:30:00Z",
  "status": "completed",
  "artifacts": [
    {
      "type": "document",
      "title": "需求说明书",
      "content": "...",
      "format": "markdown"
    },
    {
      "type": "checklist",
      "title": "任务拆解清单",
      "items": ["子任务1", "子任务2"]
    }
  ],
  "summary": "一句话摘要，供后续工序快速理解",
  "issues": ["发现的风险点1", "风险点2"],
  "metrics": {
    "duration_seconds": 120,
    "token_usage": 3500
  }
}
```

**各工序产出物规范**：

| 工序 | 必选artifacts | 可选artifacts | summary模板 |
|------|-------------|-------------|------------|
| 需求梳理 | 需求说明书(document) | 任务拆解清单(checklist) | "需求[范围]，涉及[N]个功能点" |
| 查询信息 | 现状报告(document) | 数据比对表(table) | "查询了[N]项信息，发现[M]个关键点" |
| 方案计划 | 方案文档(document) | 排期表(table)、风险清单(checklist) | "方案包含[N]个阶段，预计[M]人天" |
| 代码开发 | 代码变更(diff) | 注释文档(document) | "修改了[N]个文件，新增[M]行代码" |
| 测试校验 | 测试报告(document) | Bug清单(checklist) | "通过率[N]%，发现[M]个Bug" |
| 版本提交 | PR信息(document) | 冲突报告(document) | "提交PR #[N]，合并至[M]分支" |
| 最终审核 | 验收结论(document) | 完整交付物清单(checklist) | "验收[通过/不通过]，[N]项交付物" |

---

## 十三、Learning机制：Agent永不停歇

### 13.1 核心理念：空闲即学习

Agent不应空闲等待。当Agent空闲超过阈值时间，自动进入Learning状态，利用闲置算力迭代自身能力。高优先级任务到来时可中断学习，回归工作。

**目标**：让Agent **24/7运转**，要么处理问题(Handling)，要么学习(Learning)，只有短暂的Idle过渡态。

### 13.2 空闲超时自动触发机制

```
Agent状态时间线：

Handling ──完成──→ Idle ──5min──→ Learning ──30min/中断──→ Idle ──5min──→ Learning ...
                         ↑                                              │
                         └──────── 新任务到来，立即接单 ←──────────────────┘
```

**触发参数**：

| 参数 | 默认值 | 说明 |
|------|--------|------|
| `IdleTimeoutSeconds` | 300 (5分钟) | 空闲超时触发学习的阈值 |
| `LearningDurationSeconds` | 1800 (30分钟) | 单次学习最长时长 |
| `HighPriorityInterruptEnabled` | true | 高优先级任务可中断学习 |
| `PostTaskLearningSeconds` | 60 (1分钟) | 完成任务后短时复盘学习 |

### 13.3 AgentIdleMonitor后台服务

```pseudo
class AgentIdleMonitorService (BackgroundService):
    loop every 30 seconds:
        idle_agents = SELECT * FROM Agents 
                       WHERE State = 'Idle' 
                       AND StateChangedAtUtc < now - IdleTimeoutSeconds
                       AND IsEnabled = true
                       AND IsDeleted = false
        
        for each agent in idle_agents:
            // 检查是否有高优先级待分配任务
            pending_high_priority = SELECT COUNT(*) FROM TaskSteps 
                                    WHERE Status = 'Pending' 
                                    AND Priority >= 4
            
            if pending_high_priority > 0:
                continue  // 有高优先级任务，保持Idle等分配
            
            // 触发学习
            agent.State = Learning
            agent.StateChangedAtUtc = now
            UPDATE Agents SET State=Learning, Version=Version+1
            WHERE Id=agent.Id AND Version=agent.Version
            
            // 发布学习事件
            Publish(new AgentLearningStartedEvent(agent.Id, agent.Role))
```

### 13.4 角色差异化学习内容

不同角色学习不同内容，**学习与岗位强绑定**：

| 角色 | 学习内容来源 | 学习动作 | 产出 |
|------|------------|---------|------|
| Secretary | 历史调度卡点、异常场景、分发均衡性 | 分析失败任务日志 → 提取调度优化规则 | 更新SkillTags、记录LearningProgress |
| Manager | 历史驳回案例、审核标准SOP、高频缺陷 | 复盘驳回记录 → 沉淀审核标准 | 更新SkillTags、记录LearningProgress |
| Worker | 被驳回的自身任务、优秀历史案例 | 复盘自身失败案例 → 修正执行偏差 | 更新SkillTags、记录LearningProgress |

### 13.5 学习实现方式（MVP）

MVP阶段的学习**不涉及模型微调**，而是通过LLM分析历史数据来优化Prompt和技能标签：

```pseudo
async function ExecuteLearning(agentId, role):
    // 1. 收集学习素材
    if role == Secretary:
        materials = LoadFailedTasksLast7Days()
    elif role == Manager:
        materials = LoadRejectedReviewsLast7Days()
    else:  // Worker
        materials = LoadMyRejectedTasksLast7Days(agentId)
    
    // 2. 构造学习Prompt
    learning_prompt = BuildLearningPrompt(role, materials)
    
    // 3. 调用LLM分析
    analysis = await LlmGateway.ChatAsync(learning_prompt)
    
    // 4. 更新Agent技能标签
    agent.SkillTags = ExtractNewSkillTags(analysis)
    agent.LearningProgress = analysis.Summary
    agent.PassRate = RecalculatePassRate(agent)
    
    // 5. 记录学习日志
    CreateTaskLog("Learning completed: " + analysis.Summary)
    
    // 6. 回归Idle
    agent.State = Idle
    agent.StateChangedAtUtc = now
```

### 13.6 学习状态与任务调度的优先级冲突

| 场景 | 处理策略 |
|------|---------|
| Learning中 + 高优先级任务(4-5) | 立即中断Learning → Idle → Handling |
| Learning中 + 普通任务(1-3) | 完成当前学习周期(最多等5分钟) → Idle → Handling |
| Learning超时(30分钟) | 强制完成Learning → Idle |
| 所有Agent都在Learning | 降级中断：优先中断最早进入Learning的Agent |

### 13.7 新增服务与组件

| 组件 | 文件 | 优先级 |
|------|------|--------|
| `AgentIdleMonitorService` | Infrastructure/BackgroundServices/AgentIdleMonitorService.cs | P1 |
| `AgentLearningService` | Application/Services/AgentLearningService.cs | P1 |
| `IAgentLearningService` | Core/Interfaces/IAgentLearningService.cs | P1 |
| Learning相关DTO | Core/DTOs/Agent/ | P1 |

---

## 十四、单元测试策略

### 14.1 测试金字塔（五层）

```
            ╱╲
           ╱  ╲         E2E测试 (少量)
          ╱    ╲        完整7步流水线端到端
         ╱──────╲
        ╱        ╲       集成测试 (适量)
       ╱          ╲      服务间协作：编排→调度→审核
      ╱────────────╲
     ╱              ╲     状态机测试 (适量)
    ╱                ╲    AgentStateMachine + StepFlow
   ╱──────────────────╲
  ╱                    ╲   服务单元测试 (大量)
 ╱                      ╲  每个Service方法：正常/异常/边界
╱────────────────────────╲
                            数据模型测试 (大量)
                           Entity字段、枚举值、映射
```

### 14.2 各层测试步骤（不含代码，仅测试设计）

#### 第1层：数据模型测试

| 测试场景 | 测试步骤 | 预期结果 |
|----------|---------|---------|
| AgentEntity新增字段 | 创建AgentEntity，设置Role=Secretary/State=Handling | 枚举值正确存储 |
| AgentState枚举完整性 | 遍历所有枚举值 | Idle=0, Handling=1, Learning=2 |
| AgentRole枚举完整性 | 遍历所有枚举值 | Secretary=0, Manager=1, Worker=2 |
| TaskStepEntity创建 | 创建7条Step，验证Step序号连续 | 1-7连续 |
| TaskStepStatus枚举 | 遍历所有枚举值 | Pending/Handling/Completed/Failed/Skipped |
| StepReviewStatus枚举 | 遍历所有枚举值 | None/Pending/Approved/Rejected |
| TaskReviewEntity创建 | 创建Review，设置Verdict=Rejected | 审核记录正确存储 |
| 乐观锁Version | 两人同时更新同一Agent | 后者失败，抛出并发冲突异常 |

#### 第2层：服务单元测试

**TaskStepFlowService测试**：

| 测试场景 | 测试步骤 | 预期结果 |
|----------|---------|---------|
| 初始化7步 | 调用InitializeStepsAsync(taskId) | 创建7条TaskStep，Status均为Pending，Step=1为Handling |
| 前进到下一步 | 完成Step1 + 审核通过 → MoveToNextStep | Step1=Completed，Step2=Handling |
| 驳回当前步 | Step3审核驳回 → RejectStep | Step3.RetryCount+1，Step3.Status=Handling |
| 驳回超限 | 同一Step驳回3次 → RejectStep | Step3.Status=Failed，Task.Status=Failed |
| 跳过工序 | SkipStep(Step2, reason) | Step2=Skipped，Step3=Handling |
| 获取活跃Step | 多个Step存在时GetActiveStep | 仅返回Status=Handling的那个 |
| 7步全完成 | 逐步推进到Step7完成 | Task.Status=Completed |

**AgentDispatcherService测试**：

| 测试场景 | 测试步骤 | 预期结果 |
|----------|---------|---------|
| 查找空闲Worker | 3个Worker，2个Handling，1个Idle | 返回Idle的那个 |
| 无空闲Worker | 所有Worker都在Handling | 返回null |
| 分配Step给Worker | AssignStepToWorker(stepId, agentId) | Agent.State=Handling，Step.WorkerAgentId=agentId |
| 释放Worker | ReleaseAgentAsync(agentId) | Agent.State=Idle |
| 多秘书竞争 | 2个Idle Secretary同时接单 | 仅一个成功(乐观锁) |

**TaskOrchestrationService测试**：

| 测试场景 | 测试步骤 | 预期结果 |
|----------|---------|---------|
| 提交任务 | SubmitTaskAsync(validInput) | 创建Task+7个Step+绑定Secretary |
| 空闲秘书接单 | 2个Idle Secretary → ClaimTaskAsync | 1个成功变Handling，1个仍Idle |
| 工序完成回调 | OnStepCompletedAsync → 自动触发审核 | Step.ReviewStatus=Pending |
| 任务超时 | OnTaskTimeoutAsync | Task=Failed，Agent回归Idle |

**TaskReviewService测试**：

| 测试场景 | 测试步骤 | 预期结果 |
|----------|---------|---------|
| 审核通过 | ApproveStepAsync → 触发下一工序 | Step.ReviewStatus=Approved，NextStep=Handling |
| 审核驳回 | RejectStepAsync → 重试 | Step.ReviewStatus=Rejected，RetryCount+1 |
| 终审通过 | Step7审核通过 → 秘书归档 | Task.Status=Completed |

**AgentLearningService测试**：

| 测试场景 | 测试步骤 | 预期结果 |
|----------|---------|---------|
| 空闲超时触发 | Agent空闲5分钟 → Monitor触发 | Agent.State=Learning |
| 学习完成 | 学习结束 → 自动回归Idle | Agent.State=Idle，LearningProgress已更新 |
| 高优先级中断 | Learning中 + 高优先级任务 | Agent.State=Idle(中断学习)，立即接单 |
| 无学习素材 | 新Agent无历史数据 → 短时学习即结束 | LearningProgress="暂无复盘数据" |

#### 第3层：状态机测试

| 测试场景 | 测试步骤 | 预期结果 |
|----------|---------|---------|
| Idle→Handling合法 | Idle状态 + AssignTask触发 | 状态变更为Handling |
| Idle→Learning合法 | Idle状态 + StartLearning触发 | 状态变更为Learning |
| Handling→Idle合法 | Handling状态 + CompleteTask触发 | 状态变更为Idle |
| Learning→Idle合法 | Learning状态 + CompleteLearning触发 | 状态变更为Idle |
| Handling→Learning非法 | Handling状态 + StartLearning触发 | 抛出InvalidOperationException |
| Learning→Handling非法 | Learning状态 + AssignTask触发 | 抛出InvalidOperationException(非高优先级) |
| 高优先级中断Learning | Learning状态 + HighPriorityAssign触发 | 先切Idle，再切Handling |
| 未知触发 | Idle状态 + 未知触发器 | 抛出InvalidOperationException |

#### 第4层：集成测试

| 测试场景 | 测试步骤 | 预期结果 |
|----------|---------|---------|
| 完整调度链路 | 提交任务→秘书接单→分配小弟→完成Step→老大审核→下一步 | 7步依次推进 |
| 驳回重试链路 | Step3被驳回→小弟重做→再次提交→老大审核 | RetryCount+1，重做后通过 |
| 超时兜底链路 | Step执行超过阈值→超时监控触发 | Step=Failed，Agent=Idle，任务重分配 |
| 多秘书竞争链路 | 同时提交2个任务→2个Idle秘书 | 各接1单，互不冲突 |
| 学习触发链路 | Agent空闲5分钟→自动Learning→学习完成→Idle | 全流程状态变更记录完整 |
| 学习中断链路 | Agent在学习中→高优先级任务到来 | 中断学习，立即接单处理 |

#### 第5层：E2E测试

| 测试场景 | 测试步骤 | 预期结果 |
|----------|---------|---------|
| 7步完整流水线 | POST /api/tasks → 逐步complete → 逐步approve → 任务完成 | 全流程200，所有Step状态正确 |
| API驳回场景 | 完成Step → POST reject → 重新complete → approve | RetryCount正确，最终通过 |
| API错误场景 | POST无效taskId → POST已完成的Step | 返回404/400，状态不变 |

### 14.3 测试基础设施

| 组件 | 用途 | 现有 |
|------|------|------|
| `IntegrationTestContext` | SqlSugar内存数据库+真实仓储 | ✅ 已有 |
| Mock LLM Gateway | 模拟LLM返回，避免真实调用 | 需新建 |
| Mock AgentExecutor | 模拟Agent执行结果 | 需新建 |
| 固定时间源 | 测试超时逻辑时可控时间 | 需新建(ITimeProvider) |
| 并发测试工具 | 多线程竞争场景 | 需新建(基于Task.WhenAll) |

---

## 十五、扩展性架构设计

### 15.1 扩展点全景图

```
┌─────────────────────────────────────────────────────────────────┐
│                        扩展层设计                                 │
├─────────────┬───────────────┬───────────────┬───────────────────┤
│  角色扩展     │  流水线扩展     │  通信扩展      │  存储扩展          │
│             │               │               │                   │
│ IAgentRole  │ IPipeline     │ ITaskEvent    │ IArtifactStore    │
│ .Definition │ .Template     │ .Publisher    │                   │
│     ↓       │     ↓         │     ↓         │       ↓           │
│ Secretary   │ Fixed7Step    │ InProcess     │ DatabaseStore     │
│ Manager     │ Dynamic       │ MassTransit   │ FileSystemStore   │
│ Worker      │ Elsa          │ gRPC          │ BlobStorageStore  │
│ [自定义角色] │ [自定义模板]   │ [自定义通信]   │ [自定义存储]       │
└─────────────┴───────────────┴───────────────┴───────────────────┘
```

### 15.2 角色扩展：插件化Agent角色

**当前**：AgentRole硬编码为Secretary/Manager/Worker三个枚举值
**演进**：引入`IAgentRoleDefinition`接口，角色可插拔

```pseudo
interface IAgentRoleDefinition:
    RoleId: string               // "secretary" | "manager" | "worker" | "custom"
    DisplayName: string          // "秘书" | "老大" | "小弟"
    AllowedTransitions: map      // { Idle: [Handling, Learning], Handling: [Idle], ... }
    DefaultSkillTags: string[]   // 角色默认技能标签
    LearningStrategy: string     // 学习策略标识
    CanInterruptLearning: bool   // 是否可被中断学习
```

**数据库变更**：`AgentEntity.Role`从enum改为string，兼容新旧

### 15.3 流水线扩展：从固定7步到动态模板

**当前**：硬编码`TaskPipelineStep`枚举(1-7)
**演进**：引入`IPipelineTemplateService`

```pseudo
interface IPipelineTemplateService:
    GetTemplateAsync(templateId) → PipelineTemplate
    GetStepDefinitionsAsync(templateId) → List<StepDefinition>

PipelineTemplate:
    Id: string
    Name: string               // "标准研发流程" | "轻量修复流程" | "安全审计流程"
    Steps: List<StepDefinition>
    IsDefault: bool

StepDefinition:
    Order: int
    Name: string               // "需求梳理" | "安全扫描"
    ExecutorRole: string       // "worker" | "manager"
    ReviewerRole: string       // "manager" | "secretary"
    IsRequired: bool           // 是否可跳过
    MaxRetryCount: int         // 最大重试次数
    TimeoutSeconds: int        // 超时阈值
```

### 15.4 通信扩展：从进程内到分布式

| 阶段 | 实现 | 适用场景 |
|------|------|---------|
| MVP | `InProcessEventPublisher`(进程内) | 单服务器 |
| V2 | `MassTransitEventPublisher`(RabbitMQ) | 多服务器 |
| V3 | `KafkaEventPublisher`(Kafka) | 大规模集群 |

**接口不变**：`ITaskEventPublisher.PublishAsync<TEvent>(TEvent)`，仅替换实现

### 15.5 存储扩展：产出物存储

| 阶段 | 实现 | 适用场景 |
|------|------|---------|
| MVP | `TaskStepEntity.Output`(数据库TEXT字段) | 小型产出物 |
| V2 | `FileSystemArtifactStore`(本地文件系统) | 大型产出物(代码/文档) |
| V3 | `BlobStorageArtifactStore`(Azure Blob/S3) | 云端部署 |

### 15.6 调度扩展：从轮询到智能

| 阶段 | 实现 | 适用场景 |
|------|------|---------|
| MVP | `RoundRobinSelectionStrategy` | 简单轮询 |
| V2 | `ScoreBasedSelectionStrategy` | 按通过率/经验评分择优 |
| V3 | `MLBasedSelectionStrategy` | 机器学习预测最优分配 |

### 15.7 演进路线总览

```
MVP ───────────────────→ V2 ───────────────────→ V3

单服务器               多服务器                 集群
SqlSugar单库           主从数据库               分库分表
进程内事件              MassTransit+RabbitMQ     Kafka
lock()                 Medallion分布式锁        Redis分布式锁
数据库TEXT产出物        文件系统产出物            Blob存储产出物
固定7步流水线           动态模板                 Elsa工作流
3枚举角色              IAgentRoleDefinition     自定义角色市场
轮询调度               评分调度                 ML调度
手动扩展Worker          自动扩容                 K8s弹性伸缩
```

---

## 十六、MVP设计短板与改进方案

本节梳理当前MVP设计中已识别的隐性缺陷，给出问题分析、影响评估与改进方案。所有改进项标注优先级，供实施阶段按序落地。

### 16.1 空闲超时全局统一阈值不合理

**问题**：当前设计所有角色统一使用 `IdleTimeoutSeconds=300`（5分钟）作为空闲超时触发学习的阈值。但三类角色的负载特征差异极大：

| 角色 | 典型负载 | 空闲频率 | 5分钟统一阈值的影响 |
|------|---------|---------|-------------------|
| Secretary | 极轻量（调度、流转），高频短时 | 空闲窗口极短，几乎不超5分钟 | 空闲刚满5分钟就被强制学习，可能打断即将到来的调度任务 |
| Manager | 中等（审核、决策），脉冲式 | 审核高峰后空闲，但随时可能再被占用 | 5分钟阈值过短，审核间隙刚结束就被拉去学习 |
| Worker | 重载（LLM调用、代码生成），长耗时 | 任务间空闲窗口较长 | 5分钟阈值合理，但Worker学习应更聚焦实操复盘 |

**影响**：角色资源利用率失衡——Secretary/Manager被过早拉入学习，浪费轻量角色的调度敏捷性。

**改进方案**：按角色差异化配置空闲超时阈值

| 角色 | 建议阈值 | 理由 |
|------|---------|------|
| Secretary | 60秒（1分钟） | 秘书空闲极少，短阈值即可触发，学习内容轻量 |
| Manager | 180秒（3分钟） | 审核脉冲间隙，3分钟足以判断是否真正空闲 |
| Worker | 300秒（5分钟） | Worker任务间空窗较长，5分钟合理 |

**实现方式**：AgentEntity新增 `IdleTimeoutSeconds` 字段（按角色SeedData预置默认值），AgentIdleMonitorService查询时使用角色专属阈值而非全局常量。

### 16.2 多秘书竞争接单无负载均衡

**问题**：当前 TaskOrchestrationService.ClaimTaskAsync 仅靠乐观锁（Version字段）防止两个秘书同时接同一单，但无任务负载均分逻辑。

**改进方案**：引入最小负载优先（Least-Loaded-First）分配策略

```pseudo
function ClaimTaskAsync(secretaryId):
    idle_secretaries = FindAllIdleSecretaries()
    for sec in idle_secretaries:
        sec.current_load = COUNT(TaskEntity WHERE SecretaryAgentId=sec.Id AND Status!=Completed)
    best_secretary = idle_secretaries.OrderBy(s => s.current_load).First()
    if secretaryId == best_secretary.Id:
        return TryClaimWithOptimisticLock(taskId, secretaryId)
    else:
        return null
```

**验收标准**：多秘书负载偏差不超过20%（最忙/最闲比值 <= 1.2:1），无单个秘书积压超过70%任务。

### 16.3 Manager审核权责无边界约束

**问题**：当前设计中Manager审核无任何边界约束：
- 未限定单个Manager同时审核上限，海量TaskStep涌入会造成审核阻塞
- 无审核转交机制，Manager临时不可用时审核链断裂
- 无越级审核兜底规则，审核卡死无法升级处理

**改进方案**：三层审核边界约束

| 约束层 | 规则 | 实现 |
|--------|------|------|
| 并发审核上限 | 单个Manager同时处理审核不超过 MaxConcurrentReviews=5 | AgentEntity新增字段；TaskReviewService按上限排队 |
| 审核转交 | Manager可主动将审核转交给其他Idle Manager | TaskReviewEntity新增 TransferredFromManagerId 字段；新增API /api/reviews/{id}/transfer |
| 超时越级兜底 | 审核排队超过 ReviewQueueTimeoutSeconds=600(10分钟) 则自动升级给Secretary终裁 | TaskStepBackgroundService新增审核超时扫描；Secretary拥有越级终裁权 |

### 16.4 TaskStep重试无失败原因归类

**问题**：当前 TaskStepEntity.RetryCount 仅是全局计数器，重试超限（>=3）直接终止任务为Failed。但无法区分失败根因类别。

**改进方案**：新增 FailureCategory 枚举，每次失败记录根因

```csharp
public enum FailureCategory
{
    CodeError = 0,        // Worker产出物代码/逻辑错误
    LlmException = 1,     // LLM调用异常(超时/格式错误/拒绝响应)
    RequirementIssue = 2, // 需求不清晰/矛盾/缺失
    ReviewRejection = 3,  // Manager审核驳回(常规)
    Timeout = 4,          // 工序超时未完成
    Unknown = 5,          // 未分类
}
```

TaskStepEntity新增字段：`LastFailureCategory`(FailureCategory?), `FailureDetail`(string?)

**差异化重试策略**：

| 失败类别 | 重试策略 | 是否计入RetryCount | 后续动作 |
|----------|---------|-------------------|----------|
| CodeError | 重新执行，提示修正方向 | 计入 | RetryCount>=3 则终止 |
| LlmException | 重新调用LLM，换备用模型 | 不计入 | LLM连续3次失败则终止 |
| RequirementIssue | 退回Step1(DemandAnalyse)重新梳理 | 计入但归零Step后续重试 | 通知Secretary重新调度 |
| ReviewRejection | Worker修正后重新提交 | 计入 | RetryCount>=3 则终止 |
| Timeout | 强制释放Agent，重新分配 | 不计入 | 换一个Worker重新执行 |

### 16.5 Learning学习机制隐性缺陷

#### 16.5.1 学习效果无量化闭环

**问题**：当前Learning机制仅更新 SkillTags 与 LearningProgress，没有学习结果落地验证。

**改进方案**：学习效果验证闭环

1. **学习前快照**：记录Agent当前 PassRate、AvgStepDuration、RejectRate
2. **执行学习**：更新 SkillTags、LearningProgress
3. **学习后验证窗口（7天）**：收集学习后该Agent处理的N个任务数据，对比学习前快照
4. **效果判定**：PassRate_delta > 0 → 有效保留；<= 0 → 回滚标签

**AgentEntity新增字段**：PreLearningPassRate(decimal?), PreLearningRejectRate(decimal?), LastLearningVerifiedAtUtc(DateTime?), LearningEffectiveness(string?)

#### 16.5.2 学习占用LLM资源无配额限制

**问题**：业务任务与学习任务共用LLM模型通道，无配额隔离。

**改进方案**：学习任务排队排序 + LLM资源配额隔离

| 资源池 | 配额占比 | 说明 |
|--------|---------|------|
| 业务任务池 | 80% | 保障业务任务优先获得LLM资源 |
| 学习任务池 | 20% | 学习任务使用独立配额，不抢占业务资源 |

### 16.6 人工介入休眠状态（Dormant）

**问题**：当前Agent三状态模型（Idle/Handling/Learning）假设Agent永不停歇。但实际运维中存在需要人工介入冻结Agent的场景。

**改进方案**：新增 Dormant（休眠）状态，Agent四状态模型

```csharp
public enum AgentState
{
    Idle = 0,       // 空闲 -- 可接单
    Handling = 1,   // 处理问题 -- 锁定资源
    Learning = 2,   // 学习 -- 暂停接单
    Dormant = 3,    // 休眠 -- 不学习、不接单、等待人工介入决策
}
```

**Dormant状态规则**：

| 规则 | 说明 |
|------|------|
| Dormant状态Agent不参与任务分配 | 休眠Agent不查询、不接单、不学习 |
| Dormant状态仅可通过人工恢复回归Idle | 管理员手动恢复，确保评估到位 |
| 连续2次学习效果评分低于阈值触发Dormant | 自动休眠，等待人工评估学习策略 |
| 学习产出有害/无效内容触发Dormant | 自动休眠，等待人工纠正 |
| DormantAgent保留全部状态上下文 | 唤醒后无缝恢复 |

**新增API端点**：

| 方法 | 路径 | 功能 |
|------|------|------|
| POST | /api/agents/{id}/dormant | 管理员将Agent设为休眠 |
| POST | /api/agents/{id}/wake | 管理员手动唤醒恢复Idle |
| GET  | /api/agents/dormant | 列出所有休眠Agent及休眠原因 |

### 16.7 上下文硬性截断上限

**问题**：当前上下文链式传递设计中，没有硬性截断上限。随着工序推进，累积上下文可能超过LLM模型的Token上限。

**改进方案**：引入 MaxStepContextTokens 硬性截断

| 参数 | 默认值 | 说明 |
|------|--------|------|
| MaxStepContextTokens | 8000 | 单步上下文最大Token数 |
| MaxGlobalContextTokens | 2000 | 全局上下文(原始需求+流程摘要)最大Token数 |

**截断优先级**：

| 截断优先级 | 保留内容 | 说明 |
|------------|---------|------|
| 1（必保留） | 原始需求(task.Input) | 业务核心，不可截断 |
| 2（高优先） | 最近2个工序的完整产出 | 保证上下文连贯性 |
| 3（中优先） | 更早工序的摘要 | 按距离递减截断 |
| 4（低优先） | 审核评论摘要 | 仅保留驳回关键信息 |

### 16.8 Step卡死应急解绑

**问题**：当前设计中单工序重试超限后直接标记Task为Failed，但绑定的Agent可能被长时间锁在 Handling 状态。

**改进方案**：单工序重试超限自动释放Agent，不无限锁死执行节点

| 场景 | 处理 | Agent状态 |
|------|------|----------|
| 重试超限 + 前置工序(1-3) | Task=Failed，释放Agent | Agent转为Idle |
| 重试超限 + 后置工序(4-7) | Step重置为Pending，换Worker重试 | 旧Agent转为Idle，新Agent转为Handling |
| Worker卡死超时(Handling>30min) | 强制释放Agent | Agent转为Idle |
| LLM异常导致卡死 | 标记FailureCategory=LlmException | Agent转为Idle（非Worker责任） |

### 16.9 改进项总览与优先级

| # | 设计短板 | 改进方案 | 优先级 | 影响范围 |
|---|---------|---------|--------|----------|
| 16.1 | 空闲超时统一阈值 | 按角色差异化配置IdleTimeoutSeconds | **P0** | AgentEntity + AgentIdleMonitorService |
| 16.2 | 多秘书无负载均衡 | LeastLoadSelectionStrategy负载感知分配 | **P0** | TaskOrchestrationService + IAgentSelectionStrategy |
| 16.3 | Manager审核无边界 | 并发上限+转交+越级兜底三层约束 | **P1** | AgentEntity + TaskReviewService + TaskReviewEntity |
| 16.4 | 重试无失败归类 | FailureCategory枚举+差异化重试策略 | **P1** | TaskStepEntity + TaskStepFlowService |
| 16.5.1 | 学习效果无闭环 | 学习前后对比+效果验证+无效回滚 | **P1** | AgentEntity + AgentLearningService |
| 16.5.2 | 学习抢占LLM资源 | LearningQueue排队+LlmQuota配额隔离 | **P1** | 新增LearningQueueService + LlmQuotaManager |
| 16.6 | 缺少休眠状态 | 新增Dormant状态+人工介入/唤醒机制 | **P0** | AgentState枚举 + AgentStateMachine + API |
| 16.7 | 上下文无截断上限 | MaxStepContextTokens硬截断+优先级保留策略 | **P0** | TaskStepFlowService + GlobalConfig |
| 16.8 | Step卡死不解绑 | 重试超限自动释放Agent+后置工序换Worker | **P0** | TaskStepBackgroundService |

**P0项为MVP必须实现，P1项为MVP后首轮迭代应实现。**

---

## 十七、人类介入机制（HumanGate）

> **v4.0新增章节** — 原报告中7步流水线为封闭的Agent自循环，人类仅在起点提交任务后便完全交给Agent链。本节补充人类在流水线各环节的介入机制，确保关键决策可控、风险可防、结果可信。

### 17.1 核心问题：封闭Agent自循环缺少人类介入

当前7步流水线的审核全部是 **Agent → Agent**：

```
人类提交需求 → [Step1] → [Step2] → [Step3] → [Step4] → [Step5] → [Step6] → [Step7] → 完成
                   ↓         ↓         ↓         ↓         ↓         ↓         ↓
               Worker产出  Worker产出  Worker产出  Worker产出  Worker产出  Worker产出  Manager终审
                   ↓         ↓         ↓         ↓         ↓         ↓         ↓
               Manager审核 Manager审核 Manager审核 Manager审核 Manager审核 Manager审核 Secretary归档
```

**关键缺陷**：没有任何机制让Agent的产出回到人类手中确认。Manager审核的是"格式"，不是"业务正确性"。一旦Agent理解错了需求或方向，后续所有步骤全部白费，且人类无法及时纠偏。

### 17.2 人类介入场景全景图

```
人类 ──提交需求──→ [Step1 需求梳理] → [Step2 查询] → [Step3 方案] → [Step4 开发] → [Step5 测试] → [Step6 提交] → [Step7 终审]
  │                    │                  │               │               │               │               │               │
  │                    ↓                  ↓               ↓               ↓               ↓               ↓               ↓
  │               ❶需求确认          ❷信息补全       ❸方案审批       ❹代码Review    ❺测试验收     ❻合并审批     ❼最终签收
  │                                                                                                               │
  └──────────────────────────────────────── 贯穿式介入 ───────────────────────────────────────────────────────────────┘
   ❽ 随时介入：驳回/暂停/接管/修正方向
   ❾ Dormant唤醒（§16.6已有）
   ❿ Agent配置调优（Prompt/SkillTags）
   ⓫ 紧急刹车：强制终止整条流水线
   ⓬ 需求变更：执行中更新需求上下文
```

### 17.3 各关卡人类介入详细设计

#### ❶ 需求确认关（Step1 → Step2 之间）

| 介入点 | 触发条件 | 人类动作 | 系统行为 |
|--------|---------|---------|----------|
| 需求确认 | Step1 产出需求说明书后 | 确认/修正/补充 | 确认后流程继续；修正后Step1重做 |
| 需求澄清 | Agent识别到需求矛盾时 | 回答澄清问题 | 澄清内容追加到Task上下文 |

**当前缺口**：没有任何机制让Step1的产出回到人类手中确认。

#### ❷ 信息补全关（Step2 执行期间）

| 介入点 | 触发条件 | 人类动作 | 系统行为 |
|--------|---------|---------|----------|
| 人工补充 | Agent标记"缺少关键信息" | 提供补充说明/文件路径/权限 | 补充内容注入Step2上下文 |
| 方向纠正 | Agent查询偏离方向 | 指出错误方向 | 重新执行Step2 |

#### ❸ 方案审批关（Step3 → Step4 之间）— **最关键**

**为什么最关键**：方案计划决定了后续所有代码开发的方向。这是**成本分水岭**——方案错了，后面4-7步全部白费。人类必须在这里有否决权。

| 介入点 | 触发条件 | 人类动作 | 系统行为 |
|--------|---------|---------|----------|
| 方案审批 | Step3 产出方案文档后 | 批准/修改/否决 | 批准→Step4；修改→Step3重做；否决→Task终止 |
| 风险确认 | 方案涉及高风险变更 | 确认接受风险 | 风险标记写入Task上下文 |

#### ❹ 代码Review关（Step4 → Step5 之间）

| 介入点 | 触发条件 | 人类动作 | 系统行为 |
|--------|---------|---------|----------|
| 人工Code Review | 变更涉及核心模块/数据库/安全 | 人工审查代码 | 审查意见注入Review记录 |
| 自动通过 | 变更仅涉及文档/配置/样式 | 无需介入 | Manager审核即可 |

**审核分级**：

```csharp
public enum HumanReviewLevel
{
    None = 0,       // 无需人工审核（文档/配置变更）
    Light = 1,      // 轻量审核（确认变更范围即可）
    Full = 2,       // 完整审核（核心逻辑/数据库/安全）
    Mandatory = 3,  // 强制人工审核（生产发布/破坏性变更）
}
```

#### ❺ 测试验收关（Step5 → Step6 之间）

| 介入点 | 触发条件 | 人类动作 | 系统行为 |
|--------|---------|---------|----------|
| 测试验收 | 测试通过但覆盖度存疑 | 人工验收/补充测试 | 验收通过→Step6 |
| 边界确认 | 测试发现边界问题 | 决定是否接受/修复 | 接受→继续；修复→Step4重做 |

#### ❻ 合并审批关（Step6 → Step7 之间）

| 介入点 | 触发条件 | 人类动作 | 系统行为 |
|--------|---------|---------|----------|
| 合并审批 | PR准备合并 | 批准合并/延迟合并 | 批准→Step7；延迟→等待 |
| 冲突裁决 | 合并冲突 | 决定冲突解决策略 | 策略注入Step6上下文 |

#### ❼ 最终签收关（Step7 完成时）

| 介入点 | 触发条件 | 人类动作 | 系统行为 |
|--------|---------|---------|----------|
| 最终签收 | Step7审核通过 | 签收/部分签收/退回 | 签收→Task归档；退回→回到指定Step |
| 评价反馈 | 签收时 | 对质量/效率评分 | 评分写入Task，供Learning使用 |

### 17.4 贯穿式介入能力

| 介入类型 | 说明 | 当前是否有机制 |
|----------|------|--------------|
| ❽ 随时驳回/暂停/接管 | 人类发现Agent走偏，随时可打断 | **无** — 没有"暂停任务"的API |
| ❾ Dormant唤醒 | §16.6 已设计 | ✅ 有 |
| ❿ Agent配置调优 | 人类调整SystemPrompt/SkillTags | 部分有（Agent CRUD API） |
| ⓫ 紧急刹车 | 整条流水线出问题，人类强制终止 | **无** — 没有"紧急终止"机制 |
| ⓬ 需求变更 | 任务执行中人类改了需求 | **无** — 没有"需求变更"的上下文更新机制 |

### 17.5 HumanGate门控机制设计

引入 `HumanGate` 概念——流程在指定节点暂停，等待人类确认后继续。

#### HumanGateEntity 数据模型

```csharp
[SugarTable("HumanGates")]
public class HumanGateEntity : AuditableEntity
{
    [SugarColumn(IsPrimaryKey = true)]
    public new Guid Id { get; set; }

    public Guid TaskId { get; set; }
    public Guid TaskStepId { get; set; }
    public HumanGateType GateType { get; set; }      // 门控类型
    public HumanGateStatus Status { get; set; }      // 门控状态
    public string? Reason { get; set; }               // 暂停原因/待确认内容
    public string? HumanResponse { get; set; }        // 人类回复
    public Guid? ReviewerUserId { get; set; }         // 人类审核者
    public DateTime? RespondedAtUtc { get; set; }
    public int Priority { get; set; }                  // 门控优先级
}

public enum HumanGateType
{
    RequirementConfirm = 1,   // 需求确认
    PlanApproval = 2,         // 方案审批
    CodeReview = 3,           // 代码审核
    TestAcceptance = 4,       // 测试验收
    MergeApproval = 5,        // 合并审批
    FinalSignoff = 6,         // 最终签收
    Emergency = 7,            // 紧急介入
}

public enum HumanGateStatus
{
    Pending = 0,      // 等待人类响应
    Approved = 1,     // 人类批准
    Rejected = 2,     // 人类驳回
    Modified = 3,     // 人类修改后批准
    Timeout = 4,      // 人类超时未响应
    Cancelled = 5,    // 任务取消
}
```

### 17.6 门控触发策略

不是每步都要人类审批，否则Agent系统失去自动化价值。需要**分级触发**：

| 触发策略 | 说明 | 示例 |
|----------|------|------|
| **始终触发** | 某些关卡永远需要人类 | 方案审批(PlanApproval)、最终签收(FinalSignoff) |
| **条件触发** | 满足条件才需人类 | 代码变更>50行 → CodeReview；涉及核心模块 → Full Review |
| **升级触发** | Agent审核不确定时升级给人类 | Manager审核置信度低 → 升级给人类 |
| **人工主动** | 人类随时可介入 | 人类发现走偏 → 暂停+接管 |

**配置化**（存在 GlobalConfigEntity）：

```json
{
  "HumanGatePolicy": {
    "RequirementConfirm": "always",
    "PlanApproval": "always",
    "CodeReview": "conditional: changed_files > 5 OR affects_core_module",
    "TestAcceptance": "conditional: test_pass_rate < 0.9",
    "MergeApproval": "always",
    "FinalSignoff": "always"
  },
  "HumanGateTimeoutSeconds": 3600,
  "HumanGateTimeoutAction": "notify_and_wait"
}
```

### 17.7 HumanGate与Agent审核的协作模型

HumanGate不是替代Agent审核，而是在Agent审核链路上叠加人工审核层：

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
                               (可超时/可通知)
```

**协作规则**：

| 场景 | Agent审核结果 | HumanGate触发 | 人类可执行操作 |
|------|-------------|-------------|-------------|
| Agent通过 + 无需人工 | Approved | 不创建 | — |
| Agent通过 + 需人工确认 | Approved | 创建 | 批准/修改批准/驳回 |
| Agent驳回 + 需人工确认 | Rejected | 创建 | 确认驳回/推翻Agent决定 |
| Agent不确定 | 待定 | 创建（升级） | 批准/驳回/修改后批准 |

### 17.8 对现有架构的影响

#### 新增实体与枚举

| 新增项 | 文件 | 优先级 |
|--------|------|--------|
| HumanGateEntity | Core/Entities/HumanGateEntity.cs | P0 |
| HumanGateType枚举 | Core/Enums/HumanGateEnums.cs | P0 |
| HumanGateStatus枚举 | Core/Enums/HumanGateEnums.cs | P0 |
| HumanReviewLevel枚举 | Core/Enums/HumanGateEnums.cs | P1 |
| HumanGateRepository | Infrastructure/Repositories/HumanGateRepository.cs | P0 |

#### 新增服务

| 服务 | 文件 | 职责 | 优先级 |
|------|------|------|--------|
| HumanGateService | Application/Services/HumanGateService.cs | 门控核心：创建/响应/超时 | P0 |
| HumanGatePolicy | Application/Services/HumanGatePolicy.cs | 门控策略判定：是否需人工 | P0 |
| INotificationService | Core/Interfaces/INotificationService.cs | 通知人类（邮件/WebSocket/钉钉） | P1 |

#### TaskStepFlowService 扩展

MoveToNextStepAsync 执行前需检查 HumanGate 策略：

```pseudo
async function MoveToNextStepAsync(taskId, stepId, comment):
    // 1. 完成当前工序
    complete_current_step(stepId)
    
    // 2. 检查HumanGate策略
    gate_type = DetermineGateType(current_step)
    policy = HumanGatePolicy.GetPolicy(gate_type)
    
    if policy == "always" OR (policy == "conditional" AND MeetsCondition()):
        // 创建HumanGate，流程暂停
        await HumanGateService.CreateGateAsync(taskId, stepId, gate_type)
        return  // 等待人类响应
    
    // 3. 无需人工，直接推进
    activate_next_step(taskId)
```

#### TaskOrchestrationService 扩展

新增贯穿式介入方法：

| 方法 | 功能 |
|------|------|
| `PauseTaskAsync(taskId, reason)` | 暂停任务（冻结所有Step） |
| `ResumeTaskAsync(taskId)` | 恢复暂停的任务 |
| `ForceTerminateTaskAsync(taskId, reason)` | 紧急终止 |
| `UpdateRequirementAsync(taskId, newInput)` | 需求变更（更新上下文） |

#### 新增API端点

| 方法 | 路径 | 功能 |
|------|------|------|
| GET | /api/human-gates/pending | 获取当前用户待处理门控列表 |
| POST | /api/human-gates/{id}/approve | 人类批准 |
| POST | /api/human-gates/{id}/reject | 人类驳回 |
| POST | /api/human-gates/{id}/modify-approve | 人类修改后批准 |
| POST | /api/tasks/{id}/pause | 暂停任务 |
| POST | /api/tasks/{id}/resume | 恢复任务 |
| POST | /api/tasks/{id}/terminate | 紧急终止 |
| POST | /api/tasks/{id}/update-requirement | 需求变更 |

### 17.9 HumanGate测试策略补充

#### 单元测试

| 测试场景 | 测试步骤 | 预期结果 |
|----------|---------|----------|
| 创建HumanGate | 工序完成 + 策略为always | 创建HumanGateEntity(Status=Pending) |
| 人类批准 | POST approve | HumanGate.Status=Approved，流程继续 |
| 人类驳回 | POST reject | HumanGate.Status=Rejected，工序重做 |
| 人类修改后批准 | POST modify-approve | HumanGate.Status=Modified，修改内容注入上下文 |
| 超时未响应 | 超过HumanGateTimeoutSeconds | HumanGate.Status=Timeout，按策略处理 |
| 策略判断-不需要 | conditional条件不满足 | 不创建HumanGate，直接推进 |
| 策略判断-需要 | conditional条件满足 | 创建HumanGate |

#### 集成测试

| 测试场景 | 测试步骤 | 预期结果 |
|----------|---------|----------|
| 完整HumanGate链路 | 提交→需求确认→方案审批→开发→代码审核→签收 | 每个门控点暂停等待人类 |
| 紧急刹车 | 任务执行中→强制终止 | 所有Step=Cancelled，Agent释放 |
| 需求变更 | Step3执行中→更新需求 | 上下文更新，当前Step重新执行 |
| Agent审核+人工审核协作 | Manager通过+需人工确认 | 人类可推翻Agent决定 |

### 17.10 改进项总览

| # | 设计短板 | 改进方案 | 优先级 | 影响范围 |
|---|---------|---------|--------|----------|
| 17.1 | 封闭Agent自循环 | HumanGate门控机制 | **P0** | 全流水线 |
| 17.2 | 需求无人确认 | RequirementConfirm门控(always) | **P0** | Step1 |
| 17.3 | 方案无人审批 | PlanApproval门控(always) | **P0** | Step3 — 成本分水岭 |
| 17.4 | 代码无人工Review | CodeReview门控(conditional) | **P1** | Step4 |
| 17.5 | 测试无人验收 | TestAcceptance门控(conditional) | **P1** | Step5 |
| 17.6 | 合并无人类审批 | MergeApproval门控(always) | **P0** | Step6 — 不可逆操作 |
| 17.7 | 最终无人签收 | FinalSignoff门控(always) | **P0** | Step7 |
| 17.8 | 无随时介入能力 | PauseTask/ResumeTask/ForceTerminate | **P0** | 全流水线 |
| 17.9 | 无需求变更机制 | UpdateRequirement API | **P1** | 全流水线 |
| 17.10 | 无紧急刹车 | ForceTerminateTask API | **P0** | 全流水线 |

---

**报告完成时间**: 2026-05-22
**报告版本**: v4.0
**变更记录**: 
- v2.0: 修订三状态模型(Handling替代Running)、秘书从单实例改为多实例
- v3.0: 补充分布式Agent注册+心跳方案、五层测试策略、Learning空闲超时自动触发机制、Agent间通信/上下文/产出模型、扩展性架构设计
- v3.1: 补充MVP设计短板分析——空闲超时阈值差异化、多秘书负载均衡、Manager审核边界约束、TaskStep失败原因归类、学习效果量化闭环、LLM资源配额、人工介入休眠状态(Dormant)、上下文硬性截断上限、Step卡死应急解绑
- v4.0: 报告拆分为上下两篇；新增第十七章人类介入机制(HumanGate)——7大关卡门控、贯穿式介入能力、HumanGateEntity数据模型、分级触发策略、Agent+人工协作模型、9项P0/P1改进项
