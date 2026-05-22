# 多Agent分层协作系统 MVP实施研究报告 — 索引

**报告版本**: v4.0 | **报告日期**: 2026-05-22

**分篇阅读**: [上篇](./多Agent分层协作系统-MVP实施研究报告-上篇.md) | [下篇](./多Agent分层协作系统-MVP实施研究报告-下篇.md)

---

## 章节索引

### 上篇（基础架构与实施路线）

| 章节 | 标题 | 核心内容 | 所在文件 |
|------|------|---------|---------|
| 一 | 核心架构决策 | 三状态模型(Idle/Handling/Learning)、多秘书实例、修订历史 | 上篇 |
| 二 | 现有代码全面盘点 | 26个Entity、22个Service、基础设施、NuGet依赖 | 上篇 |
| 三 | 复用性分析（四分类） | 可直接复用(60%)、需扩展、需新建、被替代 | 上篇 |
| 四 | 新增框架分析 | Stateless状态机库引入、不引入Elsa/MassTransit等理由 | 上篇 |
| 五 | 数据迁移策略 | 旧Task/Agent数据兼容方案 | 上篇 |
| 六 | 功能差距矩阵 | P0缺6项、P1缺5项、P2缺2项 | 上篇 |
| 七 | 风险缓解 | 乐观锁冲突、驳回死循环、超时阻塞等7项风险 | 上篇 |
| 八 | MVP实施路线图 | Phase 1~7（5-7周）、每Phase验收标准 | 上篇 |

### 下篇（进阶设计与改进方案）

| 章节 | 标题 | 核心内容 | 所在文件 |
|------|------|---------|---------|
| 九 | 工作量估算 | 后端12天+前端5天+测试8天 | 下篇 |
| 十 | 关键决策总结 | 10项核心决策结论 | 下篇 |
| 十一 | 多服务器部署与Agent注册 | 数据库注册+心跳续约、跨服务器分配流程 | 下篇 |
| 十二 | Agent间通信、上下文与产出 | 间接通信模型、链式上下文传递、产出物标准化契约 | 下篇 |
| 十三 | Learning机制 | 空闲超时触发、角色差异化学习、AgentIdleMonitor | 下篇 |
| 十四 | 单元测试策略 | 五层测试金字塔、各层测试设计、测试基础设施 | 下篇 |
| 十五 | 扩展性架构设计 | 角色/流水线/通信/存储/调度五大扩展点、MVP→V2→V3演进 | 下篇 |
| 十六 | MVP设计短板与改进方案 | 9项设计短板分析（阈值/负载/审核/重试/学习/休眠/截断/解绑） | 下篇 |
| **十七** | **人类介入机制（HumanGate）** | **7大关卡门控、贯穿式介入、HumanGateEntity、分级触发策略** | **下篇** |

---

## 关键概念速查

| 概念 | 定义 | 详见章节 |
|------|------|---------|
| AgentRole | Secretary(秘书)/Manager(老大)/Worker(小弟) | §3.2.1 |
| AgentState | Idle/Handling/Learning/Dormant 四状态 | §1.3, §16.6 |
| TaskPipelineStep | 7步工序枚举(需求梳理→最终审核) | §3.2.2 |
| TaskStepEntity | 工序追踪实体，7条记录对应7步 | §3.3.1 |
| TaskReviewEntity | Agent审核记录(Approved/Rejected) | §3.3.1 |
| HumanGateEntity | 人工门控记录，流程暂停等待人类确认 | §17.5 |
| HumanGateType | 7种门控类型(需求确认/方案审批/代码审核/测试验收/合并审批/最终签收/紧急) | §17.5 |
| HumanGatePolicy | 门控触发策略(always/conditional/escalation/manual) | §17.6 |
| FailureCategory | 6种失败类别(CodeError/LlmException/RequirementIssue/ReviewRejection/Timeout/Unknown) | §16.4 |
| IAgentSelectionStrategy | Worker/Secretary选择策略接口 | §3.3.3 |
| ITaskEventPublisher | 事件发布接口(进程内→MassTransit→Kafka演进) | §3.3.3, §15.4 |
| IArtifactStore | 产出物存储接口(DB→文件系统→Blob演进) | §15.5 |
| 产出物标准化契约 | 统一JSON格式(step/agent_id/artifacts/summary/issues/metrics) | §12.4 |
| 上下文链式传递 | Step.Output → NextStep.Input + 全局上下文 | §12.3 |
| Dormant休眠状态 | 人工介入冻结Agent，保留全部状态上下文 | §16.6 |
| AgentIdleMonitor | 30秒扫描空闲Agent，触发Learning | §13.3 |

---

## 改进项优先级总览

### P0（MVP必须实现）

| # | 改进项 | 来源章节 | 影响范围 |
|---|--------|---------|---------|
| 1 | Agent三状态机 + Stateless | §3.3.2 | AgentStateMachine |
| 2 | 7步工序追踪 TaskStepEntity | §3.3.1 | TaskStepFlowService |
| 3 | 按角色差异化配置空闲超时 | §16.1 | AgentEntity + AgentIdleMonitorService |
| 4 | 多秘书负载均衡 LeastLoad | §16.2 | TaskOrchestrationService |
| 5 | Dormant休眠状态 | §16.6 | AgentState枚举 + AgentStateMachine |
| 6 | 上下文硬性截断 MaxStepContextTokens | §16.7 | TaskStepFlowService + GlobalConfig |
| 7 | Step卡死应急解绑 | §16.8 | TaskStepBackgroundService |
| 8 | HumanGate门控机制 | §17.5 | 全流水线 |
| 9 | 需求确认门控(RequirementConfirm) | §17.3-❶ | Step1 |
| 10 | 方案审批门控(PlanApproval) | §17.3-❸ | Step3 |
| 11 | 合并审批门控(MergeApproval) | §17.3-❻ | Step6 |
| 12 | 最终签收门控(FinalSignoff) | §17.3-❼ | Step7 |
| 13 | 贯穿式介入(Pause/Resume/ForceTerminate) | §17.4-❽❾⓫ | 全流水线 |

### P1（MVP后首轮迭代）

| # | 改进项 | 来源章节 |
|---|--------|---------|
| 1 | Manager审核边界约束(并发上限+转交+越级兜底) | §16.3 |
| 2 | FailureCategory差异化重试策略 | §16.4 |
| 3 | 学习效果量化闭环 | §16.5.1 |
| 4 | LLM资源配额隔离 | §16.5.2 |
| 5 | 代码Review门控(Conditional) | §17.3-❹ |
| 6 | 测试验收门控(Conditional) | §17.3-❺ |
| 7 | 需求变更机制(UpdateRequirement) | §17.4-⓬ |
| 8 | INotificationService通知集成 | §17.8 |

---

## 变更记录

| 版本 | 日期 | 变更内容 |
|------|------|---------|
| v2.0 | 2026-05-22 | 修订三状态模型(Handling替代Running)、秘书从单实例改为多实例 |
| v3.0 | 2026-05-22 | 补充分布式Agent注册+心跳、五层测试策略、Learning机制、通信/上下文/产出模型、扩展性架构 |
| v3.1 | 2026-05-22 | 补充MVP设计短板分析（9项改进方案） |
| v4.0 | 2026-05-22 | 报告拆分为上下两篇+索引；**新增第十七章人类介入机制(HumanGate)** |
