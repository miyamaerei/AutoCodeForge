# 多Agent分层协作系统 — 领域Skill使用指南

**文档用途**：当9个 `multi-0x-*` 领域Skill已建好后，本文档指导开发者如何按正确顺序调用它们完成MVP开发。

---

## 一、动工前确认清单

在调用任何Skill前，确保以下前置条件就绪：

- [ ] 已阅读研究报告：`docs/多Agent分层协作系统-MVP实施研究报告-INDEX.md`（及上下篇）
- [ ] 已引入 NuGet 包 `Stateless`（用于Agent状态机）
- [ ] 已有现有代码盘点：26个Entity、22个Service（见报告§二）
- [ ] 后端项目可正常编译：`server/src/`
- [ ] 前端项目可正常运行：`client/src/`
- [ ] 已有数据库迁移基础设施（EF Core Migration）

---

## 二、9个Skill速查表

| Skill名称 | 一句话职责 | 优先级 |
|-----------|-----------|--------|
| `multi-01-agent-lifecycle` | Agent四状态机（Idle/Handling/Learning/Dormant）+ IdleMonitor | P0 - 第一批 |
| `multi-02-task-pipeline` | 7步工序流水线定义、TaskStepEntity、Step间数据传递 | P0 - 第一批 |
| `multi-04-human-gate` | HumanGate门控（7种类型）、贯穿式介入、前端审批界面 | P0 - 第一批 |
| `multi-03-task-orchestration` | Agent选择策略、多秘书负载均衡、Manager审核约束 | P0 - 第二批 |
| `multi-05-agent-communication` | Agent间间接通信协议、上下文链式传递、产出物标准格式 | P1 - 第二批 |
| `multi-06-failure-recovery` | 失败分类体系（6种）、差异化重试策略、Step卡死解绑 | P1 - 第二批 |
| `multi-07-agent-registration` | Agent注册表、心跳续约、跨服务器上下线感知 | P1 - 第三批 |
| `multi-08-notification-integration` | 通知服务接口、门控触发通知、通知模板 | P1 - 第三批 |
| `multi-09-agent-pipeline-test` | Agent流水线专属测试场景、状态机/门控测试策略 | P1 - 第三批 |

---

## 三、推荐执行顺序（分三批）

### 第一批：系统骨架（必须最先完成）

这三个Skill构成系统的核心骨骼，必须串行完成。

```
Step 1 → multi-02-task-pipeline   （先定义流水线骨架）
Step 2 → multi-01-agent-lifecycle  （再给Agent装上状态机心脏）
Step 3 → multi-04-human-gate       （最后接入人类介入门控）
```

**为什么这个顺序？**
- `multi-02` 的7步工序枚举是所有Skill的共同依赖，必须最先建立
- `multi-01` 的状态机接口会被调度、门控、恢复等Skill直接调用
- `multi-04` 是工作量最大且最复杂的，趁前两个还未被更多依赖时优先拿下

---

### 第二批：调度与协作

第一批稳定后，接入调度大脑和通信协议。

```
Step 4 → multi-03-task-orchestration  （任务分配调度大脑）
Step 5 → multi-05-agent-communication （Agent间通信与产出物协议）
Step 6 → multi-06-failure-recovery    （异常重试与卡死解绑）
```

**为什么这个顺序？**
- `multi-03` 需要依赖 `multi-01` 的状态枚举和 `multi-02` 的Step定义
- `multi-05` 的上下文传递协议依赖 `multi-02` 的Step间数据结构
- `multi-06` 的Step卡死解绑依赖 `multi-02` 的超时检测基础

---

### 第三批：增强与测试

系统核心跑通后，补充注册、通知和测试覆盖。

```
Step 7 → multi-07-agent-registration        （多服务器支持）
Step 8 → multi-08-notification-integration  （门控通知增强）
Step 9 → multi-09-agent-pipeline-test       （测试覆盖收尾）
```

---

## 四、每个Skill的调用方式

### 调用语法（在Qoder中）

直接在对话框输入斜杠命令：

```
/multi-01-agent-lifecycle
/multi-02-task-pipeline
/multi-04-human-gate
...
```

或者描述意图触发（Skill会自动匹配）：

| 你说的话 | 自动触发的Skill |
|---------|----------------|
| "实现Agent状态机" / "给Agent加Dormant休眠" | `multi-01-agent-lifecycle` |
| "实现7步工序流水线" / "Step卡住自动解绑" | `multi-02-task-pipeline` |
| "实现HumanGate门控" / "流程暂停恢复" | `multi-04-human-gate` |
| "多秘书负载均衡" / "任务分配策略" | `multi-03-task-orchestration` |
| "定义产出物标准格式" / "上下文怎么传递" | `multi-05-agent-communication` |
| "实现失败重试机制" / "不同错误不同处理" | `multi-06-failure-recovery` |
| "实现Agent注册" / "多服务器心跳" | `multi-07-agent-registration` |
| "接入通知服务" / "门控触发时通知" | `multi-08-notification-integration` |
| "给Agent流水线写测试" / "怎么测HumanGate" | `multi-09-agent-pipeline-test` |

---

## 五、Skill之间的协作关系图

```
multi-02-task-pipeline
    ↓ 提供: 7步工序枚举、Step数据结构
    ├──→ multi-01-agent-lifecycle（依赖Step.Status驱动状态转换）
    ├──→ multi-03-task-orchestration（依赖Step状态做分配决策）
    ├──→ multi-04-human-gate（门控插入在特定Step节点）
    ├──→ multi-05-agent-communication（Step.Output→NextStep.Input协议）
    └──→ multi-06-failure-recovery（Step超时检测）

multi-01-agent-lifecycle
    ↓ 提供: AgentState枚举、状态转换API
    ├──→ multi-03-task-orchestration（选择Idle状态的Agent）
    ├──→ multi-04-human-gate（Dormant状态由门控触发）
    └──→ multi-07-agent-registration（心跳与状态同步）

multi-04-human-gate
    ↓ 触发通知
    └──→ multi-08-notification-integration（发送审批通知）

multi-09-agent-pipeline-test
    ↑ 消费所有上游Skill的接口
    └── 依赖 csharp-unit-test-generator（底层测试生成）
```

---

## 六、与现有通用Skill的边界

开发时注意，以下通用Skill**不要**被 `multi-0x` Skill重复覆盖：

| 通用Skill | 已覆盖的职责 | multi-0x Skill只调用，不重做 |
|-----------|------------|---------------------------|
| `entity-scaffolder` | 实体四层脚手架生成 | multi-0x Skill产生的新Entity通过它生成 |
| `csharp-unit-test-generator` | C#单元/集成测试代码 | `multi-09` 负责策略，测试代码交给它生成 |
| `vue3-page-builder` | 前端页面脚手架 | `multi-04` 的审批界面页面交给它生成 |
| `fe-be-integration` | 前后端接口对接 | 接口对齐工作交给它处理 |
| `auto-developer` | 通用单任务开发执行 | 当某步骤无专属Skill时兜底 |

---

## 七、典型开发场景走法

### 场景A：从零开始实现MVP核心

```
1. /multi-02-task-pipeline     → 建立7步工序基础设施
2. /multi-01-agent-lifecycle   → 实现Agent四状态机
3. /multi-04-human-gate        → 接入HumanGate门控
4. /multi-03-task-orchestration → 实现任务分配调度
5. /multi-05-agent-communication → 定义通信与产出物协议
6. /multi-06-failure-recovery  → 加上失败重试
7. /multi-09-agent-pipeline-test → 补测试
```

### 场景B：只做P0需求（最小可运行版本）

```
1. /multi-02-task-pipeline
2. /multi-01-agent-lifecycle
3. /multi-04-human-gate        → 只做P0门控（需求确认/方案审批/合并审批/最终签收）
```

### 场景C：已有部分代码，只补特定能力

- 只缺门控 → 直接 `/multi-04-human-gate`
- 只缺失败重试 → 直接 `/multi-06-failure-recovery`
- 只缺通知 → 直接 `/multi-08-notification-integration`

---

## 八、执行后验收

每个Skill执行完毕后，运行以下通用检查：

```
□ 后端项目编译通过（dotnet build）
□ 新增Entity已有对应Migration
□ API端点可通过Swagger访问
□ 前端页面路由正常加载
□ 对应单元测试通过
□ 与前一批Skill的接口无冲突
```

---

## 九、遇到冲突时

如果Skill执行中发现与现有代码冲突，调用：

```
/spec-conflict-triage
```

它会帮你记录冲突、分析影响、起草规格变更请求（SPEC_CHANGE_REQUEST）。

---

*文档版本：v1.0 | 生成日期：2026-05-23*
