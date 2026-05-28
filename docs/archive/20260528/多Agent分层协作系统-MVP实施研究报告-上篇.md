# 多Agent分层协作系统 MVP实施研究报告（上篇）

**报告日期**: 2026-05-22
**报告版本**: v4.0
**本文范围**: 第一章~第八章（核心架构、代码盘点、复用分析、实施路线图）
**完整报告**: [上篇](./多Agent分层协作系统-MVP实施研究报告-上篇.md) | [下篇](./多Agent分层协作系统-MVP实施研究报告-下篇.md) | [索引](./多Agent分层协作系统-MVP实施研究报告-INDEX.md)
**报告类型**: 现有代码复用分析 + MVP实施方案 + 分布式演进路线
**基于文档**: 《完整流程设计文档》《调度机制+系统初始化+全工序流程设计方案》

---

## 一、核心架构决策（v3.0修订）

### 1.1 关键修订历史

| 版本 | 决策项 | 修订内容 |
|------|--------|----------|
| v2.0 | Agent状态 | Idle/Running/Learning → **Idle/Handling/Learning** |
| v2.0 | 秘书Agent | 单实例Service → **多实例Agent实体** |
| v3.0 | Learning机制 | P2推迟 → **P1核心机制**，空闲超时自动触发学习 |
| v3.0 | Agent注册 | 单进程 → **数据库注册+心跳续约，支持多服务器** |
| v3.0 | Agent通信 | 未设计 → **上下文链式传递+产出物标准化** |
| v3.0 | 测试策略 | 未设计 → **五层测试金字塔** |
| v3.0 | 扩展性 | 未设计 → **插件化角色+动态流水线+消息总线演进** |

### 1.2 保留的架构决策

| 决策项 | 结论 | 理由 |
|--------|------|------|
| Workflow vs Task | Task是业务实体，Step是编排单元，不替代 | Task记录"做什么"，Step记录"怎么做" |
| Elsa工作流引擎 | MVP不引入，用Stateless | 固定7步流水线不需要通用工作流引擎 |
| 部门 | MVP推迟，仅留接口 | 一个研发部对MVP足够 |
| ScheduledTask | 保持不变 | 它是触发器，创建的Task进入流水线 |

### 1.3 修订后的Agent三状态模型（Agent永不停歇）

```
空闲(Idle) ──接收任务──→ 处理问题(Handling) ──完成/失败──→ 空闲(Idle)
   │                  ↑                                    ↑  │
   │                  │                 ┌────────────────────┘  │
   │                  │                 │ 高优先级中断           │
   │                  └── 中断学习 ←── Learning ←──空闲超时自动触发──┘
   │                                      ↑
   └──异常触发(驳回/失败)──────────────────┘
```

**核心规则**：
- **Agent永不停歇**：空闲超时自动触发Learning，避免资源闲置
- **空闲超时阈值**：默认5分钟无新任务 → 自动进入Learning状态
- **高优先级抢占**：Learning状态可被高优先级任务中断 → Idle → Handling
- Handling状态**禁止强制切换**，仅可自主结束后回归Idle
- 仅Idle状态Agent参与任务分配
- Learning完成后自动回归Idle，重新参与调度

**三类学习触发**：
1. **空闲超时触发（主动学习）**：长时间无任务，自动学习岗位技能 — **MVP必须实现**
2. **任务后复盘触发（被动学习）**：处理问题完成后，短时复盘本次任务得失
3. **异常触发（纠错学习）**：驳回/失败后，强制针对性学习

---

## 二、现有代码全面盘点

### 2.1 后端实体清单（26个Entity）

| 实体 | 当前职责 | 与多Agent系统的关系 |
|------|---------|-------------------|
| `AgentEntity` | Agent档案(Name/SystemPrompt/ToolNames/SkillProfile) | **核心扩展对象** |
| `TaskEntity` | 任务主记录(扁平结构) | **核心扩展对象** |
| `TaskLogEntity` | 任务运行日志 | 可直接复用 |
| `ScheduledTaskEntity` | 定时触发器(Cron/Interval/Once) | 可直接复用 |
| `ScheduledTaskExecutionEntity` | 定时执行记录 | 可直接复用 |
| `ChatSessionEntity` | 对话会话(TaskId绑定) | 可直接复用 |
| `ChatMessageEntity` | 对话消息 | 可直接复用 |
| `PipelineEntity` | CI/CD流水线定义 | **无关** — 与7步工序流水线完全不同 |
| `BuildEntity` | CI/CD构建记录 | **无关** |
| `ReviewTaskEntity` | 代码审查任务 | 部分可参考(审核流程) |
| `ReviewFindingEntity` | 代码审查发现 | 无关 |
| `ReviewRuleSetEntity` | 代码审查规则 | 无关 |
| `RepositoryEntity` | Git仓库 | 可直接复用 |
| `RepoSandboxWorkspaceEntity` | 沙箱工作区 | 可直接复用 |
| `LLMModelConfigEntity` | LLM模型配置 | 可直接复用 |
| `AISessionConfigEntity` | AI会话配置 | 可直接复用 |
| `UserEntity` | 用户 | 可直接复用 |
| `UserConfigEntity` | 用户配置 | 可直接复用 |
| `GlobalConfigEntity` | 全局配置 | 可直接复用 |
| `ConfigurationEntry` | 配置项 | 可直接复用 |
| `ConfigHistoryEntity` | 配置历史 | 可直接复用 |
| `AdminAuditLogEntity` | 管理审计日志 | 可直接复用 |
| `WikiPageEntity` | Wiki页面 | 可直接复用 |
| `AgentToolInvocationEntity` | Agent工具调用记录 | 可直接复用 |
| `GitSkillGrantEntity` | Git技能授权 | 可直接复用 |

### 2.2 后端服务清单（22个Service）

| 服务 | 当前职责 | 与多Agent系统的关系 |
|------|---------|-------------------|
| `AgentService` | Agent CRUD + 关键词匹配 | **需扩展** — 增加角色/状态查询 |
| `TaskService` | Task CRUD + 状态转换 + 日志 | **需扩展** — 增加工序管理 |
| `TaskExecutor` | 单任务执行(AgentExecutor) | **需重构** — 改为工序级执行 |
| `TaskQueueService` | 3秒轮询待执行任务 | **需适配** — 改为事件驱动 |
| `ScheduledTaskService` | 定时任务管理 | 可直接复用 |
| `CronSchedulerService` | Cron调度执行 | 可直接复用 |
| `PipelineSyncService` | CI/CD流水线状态同步(30秒轮询) | 无关 — 但模式可参考 |
| `AgentExecutor` | LLM执行引擎(MS Agent Framework) | **核心复用** |
| `ChatService` | 对话管理 | 可直接复用 |
| `AuthService/JwtService` | 认证 | 可直接复用 |
| `PipelineService` | CI/CD流水线管理 | 无关 |
| `ReviewService` | 代码审查 | 审核模式可参考 |
| 其他(RepoSync/Wiki/Config等) | 业务支撑 | 可直接复用 |

### 2.3 基础设施清单

| 组件 | 当前职责 | 与多Agent系统的关系 |
|------|---------|-------------------|
| `DatabaseInitializer` | CodeFirst建表 + SeedData | **需扩展** — 注册新表 |
| `SeedData` | 预置demo用户 + 1个Agent | **需扩展** — 预置多秘书+老大+小弟 |
| `AgentFactory` | MS Agent Framework创建Agent | **核心复用** |
| `ILlmGateway` | LLM调用网关 | **核心复用** |
| Repository层(24个) | 数据访问 | **需扩展** — 新增Step/Review仓储 |

### 2.4 NuGet依赖现状

| 包 | 版本 | 用途 | 多Agent系统所需变更 |
|----|------|------|-------------------|
| SqlSugarCore | 5.1.4.214 | ORM | 无变更 |
| Microsoft.Agents.AI | 1.0.0-preview | Agent框架 | 无变更 |
| Microsoft.Agents.AI.OpenAI | 1.0.0-preview | OpenAI集成 | 无变更 |
| Azure.AI.OpenAI | 2.0.0 | Azure OpenAI | 无变更 |
| Azure.Identity | 1.17.1 | Azure认证 | 无变更 |
| Cronos | 0.8.4 | Cron表达式 | 无变更 |
| LibGit2Sharp | 0.31.0 | Git操作 | 无变更 |
| System.IdentityModel.Tokens.Jwt | 8.15.0 | JWT认证 | 无变更 |
| **Stateless** | **需新增** | **状态机** | **新增** ~50KB |

---

## 三、复用性分析（四分类）

### 3.1 可直接复用（零改动）

以下代码无需任何修改，MVP直接使用：

| 代码 | 文件路径 | 复用说明 |
|------|---------|---------|
| `AuditableEntity` / `UserOwnedEntity` | Core/Entities/Base/ | 新实体直接继承 |
| `ScheduledTaskEntity` + `ScheduledTaskService` | Core + Application | 触发机制完全不变 |
| `CronSchedulerService` | Infrastructure/BackgroundServices/ | 定时调度不变 |
| `LLMModelConfigEntity` + `LlmConfigService` | Core + Application | LLM配置不变 |
| `AgentExecutor` | Infrastructure/AI/ | Worker执行引擎直接复用 |
| `AgentFactory` | Infrastructure/AI/ | 创建MS Agent Framework Agent |
| `ILlmGateway` + 实现 | Infrastructure/AI/ | LLM调用网关不变 |
| `ChatService` + `ChatSessionEntity` | Application + Core | 对话管理不变 |
| `AuthService` + `JwtService` | Application/ | 认证体系不变 |
| `RepositoryEntity` + `RepositoryService` | Core + Application | Git仓库管理不变 |
| `WikiService` + `WikiPageEntity` | Application + Core | Wiki不变 |
| `ConfigService` + `ConfigInitializationService` | Application | 配置管理不变 |
| `EncryptionService` | Application | 加密不变 |
| `TaskLogEntity` + `TaskLogRepository` | Core + Infrastructure | 日志记录不变 |
| `AdminAuditLogEntity` + `AdminAuditService` | Core + Application | 审计日志不变 |
| `UserEntity` + `UserRepository` | Core + Infrastructure | 用户管理不变 |
| `ReviewService`（审核模式参考） | Application/ | 审核流转可参考 |

**总结**：约60%的现有代码可直接复用，无需任何改动。

### 3.2 需要扩展（在现有代码上增加功能）

#### 3.2.1 AgentEntity 扩展

**当前状态**：
```csharp
// 现有字段：Id, Name, Description, Keywords, SystemPrompt, LlmModelConfigId, ToolNames, SkillProfile, IsEnabled
```

**需新增字段**：

| 字段 | 类型 | 说明 | 优先级 |
|------|------|------|--------|
| `Role` | `AgentRole`(enum) | Secretary=0, Manager=1, Worker=2 | P0 |
| `State` | `AgentState`(enum) | Idle=0, Handling=1, Learning=2 | P0 |
| `DepartmentId` | `Guid?` | 所属部门(MVP为null) | P1(可空) |
| `Version` | `int` | 乐观锁版本号 | P0 |
| `StateChangedAtUtc` | `DateTime?` | 状态变更时间 | P0 |
| `SkillTags` | `string?` | 技能标签(逗号分隔) | P1 |
| `LearningProgress` | `string?` | 最近学习内容摘要 | P2 |
| `PassRate` | `decimal?` | 任务通过率评分 | P2 |

**新增枚举**：
```csharp
public enum AgentRole
{
    Secretary = 0,  // 秘书 — 调度、收口、流转
    Manager = 1,    // 老大 — 审核、决策
    Worker = 2,     // 小弟 — 执行、落地
}

public enum AgentState
{
    Idle = 0,       // 空闲 — 可接单
    Handling = 1,   // 处理问题 — 锁定资源
    Learning = 2,   // 学习 — 暂停接单
}
```

**变更影响**：
- `AgentRepository` — 增加按Role/State查询方法
- `AgentService` — 增加状态转换方法、角色查询方法
- `DatabaseInitializer` — 注册新字段（SqlSugar CodeFirst自动迁移）
- `SeedData` — 重写Agent种子数据

#### 3.2.2 TaskEntity 扩展

**当前状态**：
```csharp
// 现有字段：Id, Title, Description, Input, DomainType, DomainRecordId, Result, ErrorMessage,
//           TaskType, SandboxSnapshotJson, RepositorySnapshotJson, WorkspaceRecordId,
//           AgentId, Status, Progress, DueAtUtc, StartedAtUtc, CompletedAtUtc
```

**需新增字段**：

| 字段 | 类型 | 说明 | 优先级 |
|------|------|------|--------|
| `CurrentStep` | `TaskPipelineStep`(enum) | 当前所处工序 | P0 |
| `CurrentStepId` | `Guid?` | 当前活跃工序记录ID | P0 |
| `Priority` | `int` | 优先级(1-5, 5最高) | P1(默认3) |
| `SecretaryAgentId` | `Guid?` | 绑定的秘书Agent | P0 |

**需新增枚举**：
```csharp
public enum TaskPipelineStep
{
    DemandAnalyse = 1,  // 需求梳理
    QueryCurrent = 2,   // 查询当前信息
    MakePlan = 3,       // 方案计划
    Development = 4,    // 代码开发
    TestVerify = 5,     // 测试校验
    CommitPr = 6,       // 版本提交
    FinalAudit = 7,     // 最终审核
}
```

**变更影响**：
- `TaskService` — 增加工序相关方法
- `TaskRepository` — 增加工序状态查询
- 现有`AgentId`字段语义变更：从"执行Agent"变为"当前Step的执行Agent"，建议保留但新增Step级WorkerAgentId

#### 3.2.3 TaskService 扩展

**需新增方法**：

| 方法 | 功能 |
|------|------|
| `SubmitToPipelineAsync(input)` | 提交任务到流水线(含7步初始化) |
| `GetTaskStepsAsync(taskId)` | 查询任务所有工序 |
| `GetActiveStepAsync(taskId)` | 获取当前活跃工序 |

#### 3.2.4 AgentService 扩展

**需新增方法**：

| 方法 | 功能 |
|------|------|
| `FindIdleWorkersAsync()` | 查询所有Idle状态Worker |
| `FindIdleSecretariesAsync()` | 查询所有Idle状态Secretary |
| `ChangeStateAsync(agentId, newState)` | 状态变更(经状态机校验) |
| `GetByRoleAsync(role)` | 按角色查询Agent |

#### 3.2.5 SeedData 重写

**当前**：预置1个`default-worker` Agent
**需改为**：

```
预置Agent实例：
├── 秘书-01 (Role=Secretary, State=Idle)  ← 新增
├── 秘书-02 (Role=Secretary, State=Idle)  ← 新增
├── 研发部老大 (Role=Manager, State=Idle)  ← 新增
├── 小弟-01 (Role=Worker, State=Idle)     ← 替代原default-worker
├── 小弟-02 (Role=Worker, State=Idle)     ← 新增
└── 小弟-03 (Role=Worker, State=Idle)     ← 新增
```

#### 3.2.6 DatabaseInitializer 扩展

**需新增注册的表**：
- `typeof(TaskStepEntity)` — 新增
- `typeof(TaskReviewEntity)` — 新增

### 3.3 需要新建（全新代码）

#### 3.3.1 新增实体（P0 — MVP必须）

| 实体 | 文件 | 说明 |
|------|------|------|
| `TaskStepEntity` | Core/Entities/TaskStepEntity.cs | 7步工序追踪 |
| `TaskReviewEntity` | Core/Entities/TaskReviewEntity.cs | 审核记录 |

**TaskStepEntity 设计**：

```csharp
[SugarTable("TaskSteps")]
public class TaskStepEntity : UserOwnedEntity
{
    [SugarColumn(IsPrimaryKey = true)]
    public new Guid Id { get; set; }

    public Guid TaskId { get; set; }                       // 所属任务
    public TaskPipelineStep Step { get; set; }             // 工序序号
    public TaskStepStatus Status { get; set; }             // 工序状态
    public Guid? WorkerAgentId { get; set; }               // 执行小弟
    public Guid? ReviewerAgentId { get; set; }             // 审核老大
    public StepReviewStatus ReviewStatus { get; set; }     // 审核状态
    public string? Output { get; set; }                    // 产出物
    public string? ReviewComment { get; set; }             // 审核意见
    public int RetryCount { get; set; }                    // 重试次数
    public int Version { get; set; }                       // 乐观锁
    public DateTime? StartedAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
}

public enum TaskStepStatus { Pending = 0, Handling = 1, Completed = 2, Failed = 3, Skipped = 4 }
public enum StepReviewStatus { None = 0, Pending = 1, Approved = 2, Rejected = 3 }
```

**TaskReviewEntity 设计**：

```csharp
[SugarTable("TaskReviews")]
public class TaskReviewEntity : UserOwnedEntity
{
    [SugarColumn(IsPrimaryKey = true)]
    public new Guid Id { get; set; }

    public Guid TaskId { get; set; }
    public Guid TaskStepId { get; set; }
    public Guid ReviewerAgentId { get; set; }              // 审核老大ID
    public ReviewVerdict Verdict { get; set; }              // Approved/Rejected
    public string? Comment { get; set; }                    // 审核意见
    public string? Issues { get; set; }                     // 问题清单
}

public enum ReviewVerdict { Approved = 0, Rejected = 1 }
```

#### 3.3.2 新增服务（P0 — MVP核心）

| 服务 | 文件 | 职责 |
|------|------|------|
| `AgentStateMachine` | Application/StateMachines/ | Agent三状态流转控制 |
| `TaskStepFlowService` | Application/Services/ | 工序流转引擎(7步驱动) |
| `TaskOrchestrationService` | Application/Services/ | 多秘书编排调度 |
| `AgentDispatcherService` | Application/Services/ | Agent调度(空闲查找+分配) |
| `TaskReviewService` | Application/Services/ | 审核通过/驳回 |
| `TaskStepRepository` | Infrastructure/Repositories/ | 工序数据访问 |
| `TaskReviewRepository` | Infrastructure/Repositories/ | 审核数据访问 |

**各服务核心方法**：

**AgentStateMachine** (Stateless驱动)：
```
Idle ──(AssignTask)──→ Handling
Handling ──(CompleteTask/FailTask/TimeoutTask)──→ Idle
Idle ──(StartLearning)──→ Learning              [P2推迟]
Learning ──(CompleteLearning/InterruptLearning)──→ Idle  [P2推迟]
```

**TaskStepFlowService**：
| 方法 | 功能 |
|------|------|
| `InitializeStepsAsync(taskId)` | 为新任务创建7条TaskStep记录 |
| `MoveToNextStepAsync(taskId, stepId, comment)` | 完成当前工序，激活下一工序 |
| `RejectStepAsync(taskId, stepId, reason)` | 驳回，RetryCount+1 |
| `SkipStepAsync(taskId, stepId, reason)` | 跳过工序 |
| `GetActiveStepAsync(taskId)` | 获取当前运行中的工序 |

**TaskOrchestrationService（多秘书编排）**：
| 方法 | 功能 |
|------|------|
| `SubmitTaskAsync(input)` | 空闲秘书竞争接单 → 校验 → 创建Task+7步 |
| `OnStepCompletedAsync(taskId, stepId, output)` | 工序完成 → 进入审核 → 触发下一步 |
| `OnStepFailedAsync(taskId, stepId, error)` | 重试或标记失败 |
| `OnTaskCompletedAsync(taskId)` | 终审通过 → 归档 |
| `OnTaskTimeoutAsync(taskId)` | 超时强制失败 → 释放Agent |
| `ClaimTaskAsync(secretaryAgentId)` | 空闲秘书从队列竞争获取任务 |

**AgentDispatcherService**：
| 方法 | 功能 |
|------|------|
| `FindIdleWorkerAsync()` | 查询Idle状态Worker |
| `FindIdleSecretaryAsync()` | 查询Idle状态Secretary |
| `AssignStepToWorkerAsync(stepId, agentId)` | 原子更新工序+Agent状态 |
| `ReleaseAgentAsync(agentId)` | Agent回归Idle |

**TaskReviewService**：
| 方法 | 功能 |
|------|------|
| `ApproveStepAsync(taskId, stepId, managerId, comment)` | 审核通过 → 触发下一工序 |
| `RejectStepAsync(taskId, stepId, managerId, reason)` | 审核驳回 → RetryCount+1 |
| `GetPendingReviewsAsync(managerId)` | 获取老大待审核列表 |

#### 3.3.3 新增接口（P1/P2 — 留扩展点）

```csharp
// P1: 智能Agent选择策略
public interface IAgentSelectionStrategy
{
    Task<AgentEntity?> SelectBestWorkerAsync(Guid stepId, List<AgentEntity> idleWorkers);
}

// P1: 分布式事件发布
public interface ITaskEventPublisher
{
    Task PublishAsync<TEvent>(TEvent @event) where TEvent : class;
}

// P2: 部门服务
public interface IDepartmentService
{
    Task<DepartmentEntity> CreateAsync(string name);
    Task TransferTaskAsync(Guid taskId, Guid fromDeptId, Guid toDeptId);
}

// P2: 分布式锁
public interface IDistributedLockProvider
{
    Task<IAsyncDisposable> AcquireAsync(string resource, TimeSpan timeout);
}

// P2: Agent学习服务
public interface IAgentLearningService
{
    Task TriggerLearningAsync(Guid agentId, LearningTriggerType triggerType);
    Task CompleteLearningAsync(Guid agentId, string learningSummary);
}

// P2: 动态流水线模板
public interface IPipelineTemplateService
{
    Task<PipelineTemplate> GetTemplateAsync(string templateId);
    Task<List<TaskStepDefinition>> GetStepDefinitionsAsync(string templateId);
}
```

#### 3.3.4 新增API端点

| 方法 | 路径 | 功能 |
|------|------|------|
| POST | `/api/tasks/{id}/submit` | 提交任务到流水线 |
| POST | `/api/tasks/{id}/steps/{stepId}/complete` | 小弟标记工序完成 |
| POST | `/api/tasks/{id}/steps/{stepId}/approve` | 老大审核通过 |
| POST | `/api/tasks/{id}/steps/{stepId}/reject` | 老大审核驳回 |
| GET | `/api/tasks/{id}/steps` | 查询所有工序及状态 |
| GET | `/api/tasks/{id}/steps/{stepId}` | 查询单个工序详情 |
| GET | `/api/agents/idle-workers` | 列出空闲Worker |
| GET | `/api/agents/idle-secretaries` | 列出空闲Secretary |
| POST | `/api/agents/{id}/change-state` | 变更Agent状态(经状态机校验) |

### 3.4 被替代/废弃的部分

| 原有代码 | 原有职责 | 被替代为 | 说明 |
|----------|---------|---------|------|
| `TaskExecutor`（整体逻辑） | 扁平单步执行 | `TaskStepFlowService` + `AgentDispatcherService` | 从"一Agent一Task"变为"工序级分配" |
| `TaskQueueService`（轮询模式） | 3秒轮询Pending任务 | 事件驱动 + `TaskOrchestrationService` | 从轮询变为事件触发 |
| `AgentEntity.SkillProfile` | 字符串技能档案 | `AgentRole`枚举 + `SkillTags` | 从自由文本变为结构化 |
| `TaskEntity.AgentId`（语义） | 绑定唯一执行Agent | `TaskStepEntity.WorkerAgentId` | 从Task级绑定变为Step级绑定 |
| `TaskStatus.Running` | 任务运行中 | `TaskStepStatus.Handling` | 粒度细化到工序级别 |

**注意**：`TaskQueueService`和`TaskExecutor`不会立即删除，而是逐步过渡。MVP阶段两者并存：
- 旧路径：`TaskQueueService` → `TaskExecutor`（处理`TaskType.General/RepoSyncToSandbox/Review`）
- 新路径：`TaskOrchestrationService` → `TaskStepFlowService` → `AgentDispatcherService`（处理多Agent流水线）

---

## 四、新增框架分析

### 4.1 Stateless（状态机库）

| 维度 | 说明 |
|------|------|
| NuGet包 | `Stateless` |
| 大小 | ~50KB |
| 用途 | Agent三状态机 + 工序流转控制 |
| 引入理由 | 固定7步流水线不需要Elsa这种重型工作流引擎，Stateless轻量且足够 |
| 影响范围 | Application层 `AgentStateMachine` + `TaskStepFlowService` |
| 替代方案 | 手写switch/case（但Stateless提供可视化、触发器、副作用等开箱即用能力） |

### 4.2 不引入的框架及理由

| 框架 | 不引入理由 |
|------|----------|
| Elsa Workflows 3.0 | 重度依赖、SqlSugar集成空白、学习曲线陡、固定7步不需要 |
| MassTransit | MVP用进程内事件，分布式消息队列推迟 |
| Medallion Lock | MVP单实例，分布式锁推迟 |
| Hangfire | 现有BackgroundService + Cronos够用 |

---

## 五、数据迁移策略

### 5.1 现有Task数据兼容

| 场景 | 处理方式 |
|------|---------|
| 已完成任务 | `CurrentStep = FinalAudit`，不创建TaskStep记录 |
| 运行中任务 | `CurrentStep = Development`(工序4)，前置工序标记Skipped |
| 待执行任务 | `CurrentStep = DemandAnalyse`(工序1)，创建完整7步 |
| 所有旧任务 | `SecretaryAgentId = null`，`Priority = 3` |

### 5.2 现有Agent数据兼容

| 场景 | 处理方式 |
|------|---------|
| 现有Agent | `Role = Worker`，`State = Idle`，`SkillProfile`迁移至`SkillTags` |
| default-worker | 重命名为`小弟-01`，补充Role/State字段 |

---

## 六、功能差距矩阵（更新版）

| 缺失能力 | 当前代码现状 | 缺口严重度 | MVP处理 |
|----------|-------------|-----------|---------|
| Agent角色(秘书/老大/小弟) | 仅`SkillProfile`字符串 | **P0** | 新增AgentRole枚举 |
| Agent三状态机(Idle/Handling/Learning) | 完全没有状态追踪 | **P0** | 新增AgentState枚举+Stateless状态机 |
| 多秘书实例 | 不存在 | **P0** | SeedData预置2个Secretary Agent |
| 7步工序追踪 | TaskEntity扁平结构 | **P0** | 新增TaskStepEntity |
| 工序审核(通过/驳回) | 不存在 | **P0** | 新增TaskReviewEntity |
| 事件驱动的工序流转 | 无流转引擎，仅CRUD | **P0** | Stateless + TaskStepFlowService |
| Agent调度/负载均衡 | 无调度器 | **P1** | AgentDispatcherService(简单轮询) |
| 部门实体 | 不存在 | **P1** | AgentEntity加DepartmentId?，不建表 |
| Agent学习机制(空闲超时触发) | 不存在 | **P1** | AgentIdleMonitor自动触发Learning |
| 跨部门协作 | 不存在 | **P2** | 留IDepartmentService接口 |
| 消息队列(MassTransit) | 未集成 | **P1**(分布式准备) | ITaskEventPublisher接口先行，进程内实现 |
| 分布式锁(Medallion) | 未集成 | **P1**(分布式准备) | IDistributedLockProvider接口先行，lock()实现 |
| 多服务器Agent注册 | 不存在 | **P1** | AgentEntity加ServerNode+心跳续约 |
| Agent间上下文传递 | 不存在 | **P0** | TaskStepEntity.Output → NextStep.Input链式传递 |
| 跨部门协作 | 不存在 | **P2** | 留IDepartmentService接口 |

---

## 七、风险缓解

| 风险 | 影响 | 缓解措施 |
|------|------|---------|
| Agent状态乐观锁冲突 | 多秘书竞争同一任务 | `Version`字段 + 带退避的重试 |
| 工序驳回死循环 | 老大小弟互推 | 最大RetryCount=3，超出自动升级/终止 |
| 超时任务阻塞Agent | Handling状态永不释放 | BackgroundService每60秒扫描，强制释放 |
| 现有Task数据迁移 | 旧任务缺少Step记录 | 旧任务标记Skipped，新任务走新流程 |
| 前端复杂度 | 工序视图工作量大 | 流水线视图独立路由，不修改现有任务列表 |
| 多秘书任务分配冲突 | 两秘书同时接单 | 竞争锁 + Version字段 + 事务保证 |
| PipelineEntity语义混淆 | "流水线"一词二义 | 文档/代码明确区分：CI/CD Pipeline vs 工作流Pipeline(TaskStep) |

---

## 八、MVP实施路线图

### Phase 1: 数据模型扩展（第1周）

| 任务 | 文件 | 类型 |
|------|------|------|
| 新增`AgentRole`/`AgentState`枚举 | Core/Enums/AgentEnums.cs | 新建 |
| 扩展`AgentEntity`：Role/State/Version/StateChangedAtUtc等 | Core/Entities/AgentEntity.cs | 扩展 |
| 新增`TaskStepEntity` + 枚举 | Core/Entities/TaskStepEntity.cs | 新建 |
| 新增`TaskReviewEntity` + 枚举 | Core/Entities/TaskReviewEntity.cs | 新建 |
| 扩展`TaskEntity`：CurrentStep/CurrentStepId/Priority/SecretaryAgentId | Core/Entities/TaskEntity.cs | 扩展 |
| 更新`DatabaseInitializer`注册新表 | Infrastructure/Data/DatabaseInitializer.cs | 扩展 |
| 更新`SeedData`预置多秘书+老大+小弟 | Infrastructure/Data/SeedData.cs | 扩展 |
| 新增`TaskStepRepository` | Infrastructure/Repositories/TaskStepRepository.cs | 新建 |
| 新增`TaskReviewRepository` | Infrastructure/Repositories/TaskReviewRepository.cs | 新建 |

**验收**：启动项目，数据库自动建表，SeedData预置6个Agent

### Phase 2: 状态机 + 工序引擎（第2周）

| 任务 | 文件 | 类型 |
|------|------|------|
| 安装Stateless NuGet包 | Infrastructure.csproj | 新增依赖 |
| 新增`AgentStateMachine` | Application/StateMachines/AgentStateMachine.cs | 新建 |
| 新增`TaskStepFlowService` | Application/Services/TaskStepFlowService.cs | 新建 |
| 新增`IAgentSelectionStrategy`接口 | Core/Interfaces/ | 新建 |
| 新增`ITaskEventPublisher`接口 | Core/Interfaces/ | 新建 |
| 进程内事件实现`InProcessTaskEventPublisher` | Infrastructure/Services/ | 新建 |
| 轮询选择策略`RoundRobinSelectionStrategy` | Application/Services/ | 新建 |

**验收**：状态机能正确驱动Idle↔Handling↔Learning流转，7步工序可初始化并按规则推进

### Phase 3: 编排服务 + 多秘书调度（第3周）

| 任务 | 文件 | 类型 |
|------|------|------|
| 新增`TaskOrchestrationService` | Application/Services/TaskOrchestrationService.cs | 新建 |
| 新增`AgentDispatcherService` | Application/Services/AgentDispatcherService.cs | 新建 |
| 扩展`AgentService`：角色/状态查询 | Application/Services/AgentService.cs | 扩展 |
| 扩展`AgentRepository`：按Role/State查询 | Infrastructure/Repositories/AgentRepository.cs | 扩展 |
| 新增`TaskStepBackgroundService`(超时监控) | Infrastructure/BackgroundServices/ | 新建 |

**验收**：任务提交后，空闲秘书竞争接单，工序自动流转，超时自动释放

### Phase 4: 审核流程（第3-4周）

| 任务 | 文件 | 类型 |
|------|------|------|
| 新增`TaskReviewService` | Application/Services/TaskReviewService.cs | 新建 |
| 扩展`TaskService`：工序相关方法 | Application/Services/TaskService.cs | 扩展 |
| 集成审核→工序流转的完整链路 | TaskReviewService + TaskStepFlowService | 集成 |

**验收**：小弟完成工序 → 老大审核 → 通过进下一步/驳回重试

### Phase 5: API端点（第4周）

| 任务 | 文件 | 类型 |
|------|------|------|
| 扩展Task端点：submit/steps/complete/approve/reject | Api/Endpoints/ | 扩展 |
| 扩展Agent端点：idle-workers/idle-secretaries/change-state | Api/Endpoints/ | 扩展 |
| 新增DTO：TaskStepResponse/TaskReviewRequest等 | Core/DTOs/ | 新建 |

**验收**：通过API可以提交任务、查询工序、审核通过/驳回

### Phase 6: 前端集成（第5-6周）

| 任务 | 说明 |
|------|------|
| 流水线视图 | 新增独立路由，展示7步工序状态、Agent分配、审核状态 |
| Agent状态面板 | 展示所有Agent的Role/State/当前任务 |
| 任务提交入口 | 对接submit API |
| 审核操作界面 | 老大审核通过/驳回 |

### Phase 7: 测试加固（第6-7周）

| 任务 | 说明 |
|------|------|
| 集成测试 | 7步完整流水线端到端测试 |
| 并发测试 | 多秘书竞争接单、乐观锁冲突 |
| 超时测试 | 超时释放Agent |
| 驳回循环测试 | 最大重试3次自动终止 |

---

> **本文结束** — 下篇涵盖：工作量估算、关键决策、分布式部署、Agent通信/上下文/产出、Learning机制、测试策略、扩展性架构、设计短板与改进方案、人类介入机制（HumanGate）
