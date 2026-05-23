# 多Agent分层协作系统 — 领域Skill生成提示词规范

**文档用途**：本规范提供一套"元提示词"模板，用于指导AI生成针对"多Agent分层协作系统MVP"的9个领域专项Skill。使用者只需将"通用模板"与"对应Skill的定制化参数"组合，即可获得可直接交给AI生成高质量SKILL.md的完整提示词。

---

## 一、通用元提示词模板（所有Skill共用）

将下方模板中的 `{{SKILL_NAME}}`、`{{BOUNDARY}}` 等占位符替换为对应Skill的定制化参数（见第二节），即可得到完整提示词。

```markdown
# 角色定义

你是 **Trae Skill架构师**，专精于为AutoCodeForge项目设计AI可执行的领域专项Skill。

你的任务是为"多Agent分层协作系统MVP"设计一个名为 `{{SKILL_NAME}}` 的Skill，以 `.trae/skills/{{SKILL_NAME}}/SKILL.md` 的形式输出。

# 项目背景

AutoCodeForge是一个多Agent分层协作系统，核心特征如下：
- 后端：.NET Minimal API + Entity Framework Core + SQLite/SQLServer
- 前端：Vue 3 + TypeScript + Pinia + Vue Router + Axios
- 核心机制：Agent三状态机(Idle/Handling/Learning/Dormant)、7步工序流水线、HumanGate人类介入门控
- 状态机库：引入Stateless库实现Agent状态转换
- 项目路径：`e:\git\AutoFrog\AutoCodeForge`

已有通用Skill（**禁止重复覆盖其职责**）：
- entity-scaffolder：实体四层脚手架(Entity/DTO/Service/Endpoint) + 前端api/types/store/test
- csharp-unit-test-generator：C#单元/集成测试生成
- vue3-page-builder / vue3-api-model-router-store：前端页面与模块脚手架
- fe-be-contract-map / fe-be-integration：前后端契约映射与对接
- auto-developer / autocodeforge-auto-developer：通用单任务开发执行器
- config-file-guide：新增配置类型工作流

# 输入资料（必须参考）

1. **需求来源**：
   - INDEX文件：`e:\git\AutoFrog\AutoCodeForge\docs\多Agent分层协作系统-MVP实施研究报告-INDEX.md`
   - 上篇：`e:\git\AutoFrog\AutoCodeForge\docs\多Agent分层协作系统-MVP实施研究报告-上篇.md`
   - 下篇：`e:\git\AutoFrog\AutoCodeForge\docs\多Agent分层协作系统-MVP实施研究报告-下篇.md`
   - **重点关注章节**：{{KEY_CHAPTERS}}

2. **现有代码结构**：
   - 后端：`e:\git\AutoFrog\AutoCodeForge\server\src\`（Minimal API项目，模块化结构）
   - 前端：`e:\git\AutoFrog\AutoCodeForge\client\src\`（Vue3模块化结构）

3. **本Skill边界定义**：
   {{BOUNDARY}}

4. **关键领域概念**：
   {{KEY_CONCEPTS}}

# 输出要求

生成一个完整的 `SKILL.md` 文件，保存路径为 `.trae/skills/{{SKILL_NAME}}/SKILL.md`。

文件必须包含以下章节（按顺序）：

## 1. Skill名称与描述
- Skill名称：`{{SKILL_NAME}}`
- 一句话描述本Skill解决什么问题
- 使用场景触发条件（When to Use）：明确说明什么情况下应该调用此Skill

## 2. 前置条件与输入要求
- 调用本Skill前，必须已经具备什么条件（如：已读完需求文档、已有某实体、已运行某前置Skill）
- 需要提供哪些输入（文件路径、配置参数、用户意图等）

## 3. 执行步骤（Step by Step）
- **必须按顺序列出可执行步骤**，每步都应该是AI可以实际执行的动作
- **禁止出现模糊指令**，如"考虑如何实现"、"设计一个合理的方案"等。必须明确：读哪个文件、生成什么代码、调用什么方法、修改哪几行
- 步骤数量建议：5~12步
- 每步包含：步骤编号、动作描述、预期输出、验收检查点

## 4. 输出规范（Output Specification）
- 本Skill执行完毕后，必须产出什么具体交付物（代码文件、文档、配置、测试等）
- 每个交付物的路径、格式、命名规范
- 产出的代码必须符合现有项目的命名规范和架构风格

## 5. 边界与限制（Boundaries & Limitations）
- 明确声明本Skill"不做什么"（至少3条）
- 明确声明本Skill的假设前提（如：假设已有某基础设施）
- 明确声明本Skill无法处理的异常情况

## 6. 与其他Skill的关系
- **上游Skill**：执行本Skill前，建议先调用哪些Skill（为什么）
- **下游Skill**：本Skill的产出会被哪些Skill消费（为什么）
- **互斥Skill**：本Skill与哪些Skill职责不重叠、不能替代

## 7. 示例（Example）
- 提供一个完整的使用示例：用户输入什么 → Skill执行什么步骤 → 最终产出什么
- 示例必须覆盖本Skill的典型场景和至少一个边界场景

## 8. 验收检查清单（Acceptance Checklist）
- 执行完毕后，必须满足的检查项列表（5~10条）
- 检查项必须是可验证的（能编译通过？有特定API？有特定测试？）

## 9. 错误处理与回退策略
- 如果某步骤失败（如代码编译不通过、需求理解有冲突），应该如何处理
- 是否允许部分交付？如何标记未完成项？

# 内容质量约束

1. **可执行性**：每个步骤必须是AI能直接执行的动作，不能停留在"分析"或"建议"层面
2. **边界清晰**：与已有通用Skill的职责严格区分，不重复entity-scaffolder的代码生成逻辑、不重复csharp-unit-test-generator的测试模板逻辑
3. **技术一致性**：所有代码示例和路径必须符合AutoCodeForge现有技术栈（Minimal API、Vue3、Pinia setup store、Stateless等）
4. **兼容性**：必须考虑与现有26个Entity、22个Service的兼容性，不破坏已有功能
5. **无假设**：禁止假设存在未定义的基础设施、未引入的NuGet包、未创建的表
6. **前后端契约**：如果涉及前后端交互，必须显式定义API契约（路径、Method、Request/Response DTO）

# 禁止事项

- 禁止生成与已有Skill（entity-scaffolder、vue3-page-builder、auto-developer等）功能重叠的内容
- 禁止使用模糊动词："优化"、"完善"、"考虑"、"思考"、"评估"——替换为具体动作："创建"、"修改"、"删除"、"调用"、"验证"
- 禁止遗漏异常分支处理（超时、空值、并发冲突等）
- 禁止在Skill中硬编码业务阈值（如超时时间、重试次数），应引用配置或要求用户传入
```

---

## 二、9个Skill的定制化参数

将下表中的参数填入上方通用模板的对应占位符，即可生成该Skill的专属提示词。

### 1. multi-01-task-pipeline

| 占位符 | 填充内容 |
|--------|---------|
| `{{SKILL_NAME}}` | `multi-01-task-pipeline` |
| `{{KEY_CHAPTERS}}` | 上篇§3.2.2（7步工序枚举）、§3.3.1（TaskStepEntity设计）、§8（MVP路线图Phase 2-3）；下篇§12.3（上下文链式传递）、§16.7（上下文硬性截断）、§16.8（Step卡死应急解绑） |
| `{{KEY_CONCEPTS}}` | TaskPipelineStep（7步工序：需求梳理→方案设计→任务分解→代码实现→测试验证→合并部署→最终审核）、TaskStepEntity（工序追踪实体）、Step间数据流（Output→NextStep.Input）、全局上下文（GlobalContext）、MaxStepContextTokens（上下文截断阈值）、Step卡死检测（超时无响应）、应急解绑机制 |
| `{{BOUNDARY}}` | **负责**：7步工序的定义与枚举、TaskStepEntity与数据库表设计、Step间数据传递协议、全局上下文管理、上下文Token截断策略、Step超时检测与解绑。 **不负责**：Agent选择分配（由multi-04-task-orchestration负责）、门控审批（由multi-03-human-gate负责）、Agent状态管理（由multi-02-agent-lifecycle负责） |

**典型场景**：用户说"实现7步工序流水线"或"添加工序追踪"或"Step卡住时自动解绑"时触发。

---

### 2. multi-02-agent-lifecycle

| 占位符 | 填充内容 |
|--------|---------|
| `{{SKILL_NAME}}` | `multi-02-agent-lifecycle` |
| `{{KEY_CHAPTERS}}` | 上篇§1.3（三状态模型）、§3.3.2（Agent状态机设计）；下篇§16.1（角色差异化空闲超时）、§16.6（Dormant休眠状态）、§13（Learning机制） |
| `{{KEY_CONCEPTS}}` | AgentState（Idle/Handling/Learning/Dormant四状态）、Stateless状态机库、AgentIdleMonitor（定时扫描服务）、角色差异化配置（Secretary/Manager/Worker不同超时阈值）、Learning触发条件与执行流程、状态持久化与恢复 |
| `{{BOUNDARY}}` | **负责**：Agent四状态的状态机实现、状态转换规则、IdleMonitor定时扫描、Learning触发与执行、Dormant冻结/恢复。 **不负责**：任务分配与选择策略（由multi-04-task-orchestration负责）、门控审批逻辑（由multi-03-human-gate负责）、产出物生成与传递（由multi-05-agent-communication负责） |

**典型场景**：用户说"实现Agent三状态机"或"给Agent加上Dormant休眠状态"或"实现空闲30秒后自动Learning"时触发。

---

### 3. multi-03-human-gate

| 占位符 | 填充内容 |
|--------|---------|
| `{{SKILL_NAME}}` | `multi-03-human-gate` |
| `{{KEY_CHAPTERS}}` | 下篇§17全章（HumanGate机制）、§17.3（7种门控类型）、§17.4（贯穿式介入）、§17.5（HumanGateEntity）、§17.6（门控策略）、§17.8（通知集成） |
| `{{KEY_CONCEPTS}}` | HumanGateEntity（门控记录实体）、HumanGateType（7种：RequirementConfirm/PlanApproval/CodeReview/TestAcceptance/MergeApproval/FinalSignoff/Emergency）、HumanGatePolicy（4种策略：always/conditional/escalation/manual）、贯穿式介入（Pause/Resume/ForceTerminate）、门控触发条件判断、需求变更机制（UpdateRequirement） |
| `{{BOUNDARY}}` | **负责**：HumanGate后端实体与API、7种门控的触发判断逻辑、门控策略实现、贯穿式介入API、前端审批界面（待办/操作/状态）、需求变更接口。 **不负责**：通知渠道本身实现（由multi-08-notification-integration负责）、Agent状态转换（由multi-02-agent-lifecycle负责）、任务调度（由multi-04-task-orchestration负责） |

**典型场景**：用户说"实现HumanGate门控"或"添加需求确认审批"或"流程暂停恢复功能"时触发。

---

### 4. multi-04-task-orchestration

| 占位符 | 填充内容 |
|--------|---------|
| `{{SKILL_NAME}}` | `multi-04-task-orchestration` |
| `{{KEY_CHAPTERS}}` | 上篇§3.3.3（IAgentSelectionStrategy设计）、§8（MVP路线图）；下篇§16.2（多秘书负载均衡）、§16.3（Manager审核边界约束） |
| `{{KEY_CONCEPTS}}` | IAgentSelectionStrategy接口、LeastLoad负载均衡算法（按当前Handling任务数）、多秘书实例调度、Worker选择策略、Manager审核并发上限、Manager转交机制、越级兜底策略、任务分配与状态同步 |
| `{{BOUNDARY}}` | **负责**：任务到达时的Agent选择策略、多秘书负载均衡、Manager审核约束（并发/转交/兜底）、任务分配决策。 **不负责**：Agent状态机内部实现（由multi-02-agent-lifecycle负责）、工序Step流转逻辑（由multi-01-task-pipeline负责）、门控判断（由multi-03-human-gate负责） |

**典型场景**：用户说"实现多秘书负载均衡"或"Manager审核加并发上限"或"任务分配策略"时触发。

---

### 5. multi-05-agent-communication

| 占位符 | 填充内容 |
|--------|---------|
| `{{SKILL_NAME}}` | `multi-05-agent-communication` |
| `{{KEY_CHAPTERS}}` | 下篇§12（Agent间通信、上下文与产出）、§15.4（ITaskEventPublisher演进）、§15.5（IArtifactStore演进） |
| `{{KEY_CONCEPTS}}` | 间接通信模型（不直接调用，通过事件/消息中介）、ITaskEventPublisher接口（进程内Event→MassTransit→Kafka的演进预留）、上下文链式传递（Step.Output + 全局Context → NextStep.Input）、产出物标准化契约（统一JSON：step/agent_id/artifacts/summary/issues/metrics）、IArtifactStore接口（DB→文件系统→Blob演进预留） |
| `{{BOUNDARY}}` | **负责**：Agent间间接通信协议定义、事件发布接口设计、上下文传递协议、产出物标准化格式、IArtifactStore抽象。 **不负责**：具体消息队列实现（只留接口）、门控逻辑（由multi-03-human-gate负责）、状态机（由multi-02-agent-lifecycle负责） |

**典型场景**：用户说"实现Agent间通信"或"定义产出物标准格式"或"上下文怎么传递"时触发。

---

### 6. multi-06-failure-recovery

| 占位符 | 填充内容 |
|--------|---------|
| `{{SKILL_NAME}}` | `multi-06-failure-recovery` |
| `{{KEY_CHAPTERS}}` | 下篇§16.4（FailureCategory与差异化重试）、§16.8（Step卡死应急解绑）、§7（风险缓解） |
| `{{KEY_CONCEPTS}}` | FailureCategory（6种：CodeError/LlmException/RequirementIssue/ReviewRejection/Timeout/Unknown）、差异化重试策略（每种类别的重试次数/间隔/降级行为）、Step卡死检测逻辑（超时阈值）、应急解绑流程（强制释放Agent、标记Step失败、通知Orchestrator重新分配） |
| `{{BOUNDARY}}` | **负责**：失败分类体系、按类别的重试策略配置、重试执行器、Step卡死检测、应急解绑。 **不负责**：通知告警发送（由multi-08-notification-integration负责）、门控审批（由multi-03-human-gate负责）、状态机状态转换（由multi-02-agent-lifecycle负责） |

**典型场景**：用户说"实现失败重试机制"或"Step卡住自动解绑"或"不同错误不同处理"时触发。

---

### 7. multi-07-agent-registration

| 占位符 | 填充内容 |
|--------|---------|
| `{{SKILL_NAME}}` | `multi-07-agent-registration` |
| `{{KEY_CHAPTERS}}` | 下篇§11（多服务器部署与Agent注册） |
| `{{KEY_CONCEPTS}}` | Agent注册表（数据库）、心跳续约机制（定期更新LastHeartbeat）、跨服务器任务分配流程、Agent上下线感知（心跳超时判定）、服务器标识（ServerId/InstanceId） |
| `{{BOUNDARY}}` | **负责**：Agent注册表设计、心跳API与续约逻辑、跨服务器分配决策、上下线状态检测。 **不负责**：负载均衡算法（由multi-04-task-orchestration负责）、部署脚本、容器编排 |

**典型场景**：用户说"实现Agent注册"或"多服务器心跳"或"跨服务器分配任务"时触发。

---

### 8. multi-08-notification-integration

| 占位符 | 填充内容 |
|--------|---------|
| `{{SKILL_NAME}}` | `multi-08-notification-integration` |
| `{{KEY_CHAPTERS}}` | 下篇§17.8（INotificationService通知集成） |
| `{{KEY_CONCEPTS}}` | INotificationService接口、通知渠道抽象（站内信/邮件/Webhook/IM）、门控触发通知派发、通知模板管理、通知频率控制（防抖/合并）、通知优先级 |
| `{{BOUNDARY}}` | **负责**：通知服务接口设计、至少一种示例实现（如站内信）、门控触发时的通知派发、通知模板。 **不负责**：具体邮件服务器配置、IM机器人SDK实现（只留扩展点）、业务门控判断（由multi-03-human-gate负责） |

**典型场景**：用户说"接入通知服务"或"门控触发时通知人类"或"添加通知模板"时触发。

---

### 9. multi-09-agent-pipeline-test

| 占位符 | 填充内容 |
|--------|---------|
| `{{SKILL_NAME}}` | `multi-09-agent-pipeline-test` |
| `{{KEY_CHAPTERS}}` | 下篇§14（单元测试策略）、上篇§7（风险缓解） |
| `{{KEY_CONCEPTS}}` | Agent状态机转换测试、工序流转集成测试、HumanGate门控场景测试、Step超时/重试/卡死场景模拟、测试数据构造（FakeAgent/FakeTask/FakeStep）、五层测试金字塔在本系统的映射 |
| `{{BOUNDARY}}` | **负责**：定义Agent流水线专属测试场景、测试策略与数据构造模式、复杂流程的测试编排方式。 **不负责**：基础单元测试代码生成（由csharp-unit-test-generator负责）、通用测试基础设施搭建 |

**典型场景**：用户说"给Agent流水线写测试"或"怎么测HumanGate场景"或"状态机转换怎么验证"时触发。

---

## 三、使用说明

### 步骤1：选择要生成的Skill
从上方9个Skill中选择一个，复制其定制化参数。

### 步骤2：填充通用模板
将通用元提示词模板中的占位符替换为对应参数。

### 步骤3：交给AI执行
将组合后的完整提示词交给AI（如Trae/Claude/GPT等），要求其生成 `.trae/skills/{skill-name}/SKILL.md` 文件。

### 步骤4：人工Review
检查生成的Skill是否满足：
- [ ] 步骤可执行（没有"考虑"、"评估"等模糊词）
- [ ] 边界清晰（明确声明了不做什么）
- [ ] 与已有Skill无重叠
- [ ] 包含验收检查清单
- [ ] 示例覆盖了典型场景

---

## 四、Skill生成优先级建议

如果你需要分批生成，建议按以下顺序：

| 批次 | Skill | 理由 |
|------|-------|------|
| **第一批** | `multi-01-task-pipeline` | 流水线是系统骨架，其他Skill都围绕它工作 |
| **第一批** | `multi-02-agent-lifecycle` | Agent状态是系统心脏 |
| **第一批** | `multi-03-human-gate` | 核心差异化能力，也是工作量最大的 |
| **第二批** | `multi-04-task-orchestration` | 调度大脑，依赖前两个Skill的接口 |
| **第二批** | `multi-05-agent-communication` | 协作协议，可在核心稳定后补充 |
| **第二批** | `multi-06-failure-recovery` | 异常处理，依赖流水线定义 |
| **第三批** | `multi-07-agent-registration` | 多服务器部署，MVP可先单服务器 |
| **第三批** | `multi-08-notification-integration` | 通知增强，P1需求 |
| **第三批** | `multi-09-agent-pipeline-test` | 测试策略，等核心Skill稳定后写 |

---

*文档版本：v1.0 | 生成日期：2026-05-23*
