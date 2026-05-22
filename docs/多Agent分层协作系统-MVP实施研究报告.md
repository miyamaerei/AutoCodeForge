# 多Agent分层协作系统 MVP实施研究报告 v2.0

**报告日期**: 2026-05-22
**报告版本**: v3.1（补充MVP设计短板与改进方案：阈值差异化、负载均衡、审核约束、重试归类、学习闭环、休眠状态、上下文截断、应急解绑）
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

**为什么不用直接通信**：
- Agent可能在不同服务器，直接通信需要服务发现
- 间接通信天然支持：离线重试、审计追踪、状态恢复
- 数据库即"消息板"，事件总线即"通知器"

**MVP通信方式**：
| 通信场景 | 方式 | 实现 |
|----------|------|------|
| 秘书→小弟(分配任务) | 数据库写入+进程内事件 | TaskStepEntity + ITaskEventPublisher |
| 小弟→老大(提交审核) | 数据库写入+进程内事件 | TaskReviewEntity + ITaskEventPublisher |
| 老大→小弟(驳回重试) | 数据库更新+进程内事件 | TaskStepEntity.Status=Handling + ITaskEventPublisher |
| 老大→秘书(终审通过) | 数据库更新+进程内事件 | TaskEntity.Status=Completed + ITaskEventPublisher |
| 任意Agent→调度器(状态变更) | 数据库更新+进程内事件 | AgentEntity.State + ITaskEventPublisher |

**V2通信方式**（分布式）：
| 通信场景 | 方式 | 实现 |
|----------|------|------|
| 跨服务器通信 | HTTP/gRPC | AgentEndpoint + IAgentRemoteCaller |
| 异步消息 | MassTransit + RabbitMQ | ITaskEventPublisher替换为MassTransit实现 |

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

**上下文组装规则**：

```pseudo
function BuildStepInput(task, currentStep):
    input = {
        "original_requirement": task.Input,         // 原始需求(始终携带)
        "previous_step_output": {},                 // 前序工序产出
        "all_step_summaries": [],                   // 所有已完成工序的摘要
        "task_metadata": {                          // 任务元数据
            "task_id": task.Id,
            "current_step": currentStep.Step,
            "priority": task.Priority
        }
    }
    
    // 收集前序工序产出
    for step in task.Steps where step.Step < currentStep.Step:
        if step.Status == Completed:
            input.previous_step_output[step.Step] = step.Output
            input.all_step_summaries.push({
                "step": step.Step,
                "summary": ExtractSummary(step.Output),  // 提取摘要避免过长
                "review_result": step.ReviewStatus
            })
    
    return JSON.stringify(input)
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

**扩展示例**：新增"QA专员"角色

```pseudo
new QaSpecialistRole : IAgentRoleDefinition:
    RoleId = "qa-specialist"
    DisplayName = "QA专员"
    AllowedTransitions = { Idle: [Handling, Learning], Handling: [Idle], Learning: [Idle] }
    DefaultSkillTags = ["测试", "质量", "回归"]
    LearningStrategy = "qa-oriented"
    CanInterruptLearning = true
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

**扩展示例**：
- 轻量修复流程：需求→开发→测试→提交 (4步)
- 安全审计流程：需求→安全扫描→开发→安全复测→提交→终审 (6步)
- 按任务类型自动匹配模板

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

```pseudo
interface IArtifactStore:
    SaveAsync(taskId, stepId, artifactName, content) → string  // 返回引用路径
    LoadAsync(referencePath) → content
    DeleteAsync(referencePath)
```

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

**问题**：当前 TaskOrchestrationService.ClaimTaskAsync 仅靠乐观锁（Version字段）防止两个秘书同时接同一单，但无任务负载均分逻辑。后果：
- 抢单积极的秘书积压大量任务，成为瓶颈
- 其他秘书长期空闲，资源浪费
- 秘书间无协作感知，无法自动平衡

**影响**：单秘书过载时整体流水线吞吐量下降，且过载秘书的工序跟进质量可能滑坡。

**改进方案**：引入最小负载优先（Least-Loaded-First）分配策略

```pseudo
function ClaimTaskAsync(secretaryId):
    idle_secretaries = FindAllIdleSecretaries()
    // 计算每个秘书当前绑定任务数(负载指标)
    for sec in idle_secretaries:
        sec.current_load = COUNT(TaskEntity WHERE SecretaryAgentId=sec.Id AND Status!=Completed)
    // 选择负载最低的秘书优先接单
    best_secretary = idle_secretaries.OrderBy(s => s.current_load).First()
    if secretaryId == best_secretary.Id:
        return TryClaimWithOptimisticLock(taskId, secretaryId)
    else:
        return null  // 让负载更低的秘书接单
```

**扩展点**：IAgentSelectionStrategy接口（已有设计）可承载此逻辑，MVP实现 LeastLoadSelectionStrategy 替代简单轮询。

**验收标准**：多秘书负载偏差不超过20%（最忙/最闲比值 <= 1.2:1），无单个秘书积压超过70%任务。

### 16.3 Manager审核权责无边界约束

**问题**：当前设计中Manager审核无任何边界约束：
- 未限定单个Manager同时审核上限，海量TaskStep涌入会造成审核阻塞
- 无审核转交机制，Manager临时不可用时审核链断裂
- 无越级审核兜底规则，审核卡死无法升级处理

**影响**：审核环节成为整个7步流水线的瓶颈，上下游工序全部等待。

**改进方案**：三层审核边界约束

| 约束层 | 规则 | 实现 |
|--------|------|------|
| 并发审核上限 | 单个Manager同时处理审核不超过 MaxConcurrentReviews=5 | AgentEntity新增字段；TaskReviewService按上限排队 |
| 审核转交 | Manager可主动将审核转交给其他Idle Manager | TaskReviewEntity新增 TransferredFromManagerId 字段；新增API /api/reviews/{id}/transfer |
| 超时越级兜底 | 审核排队超过 ReviewQueueTimeoutSeconds=600(10分钟) 则自动升级给Secretary终裁 | TaskStepBackgroundService新增审核超时扫描；Secretary拥有越级终裁权 |

**新增字段**：
- AgentEntity: `MaxConcurrentReviews`（默认Secretary=0, Manager=5, Worker=0）
- TaskReviewEntity: `TransferredFromManagerId`(Guid?), `ReviewQueueEntryAtUtc`(DateTime)

### 16.4 TaskStep重试无失败原因归类

**问题**：当前 TaskStepEntity.RetryCount 仅是全局计数器，重试超限（>=3）直接终止任务为Failed。但无法区分失败根因类别：
- 代码错误（Worker产出物有Bug） → 应重试并要求修正
- LLM异常（模型返回错误/超时/格式不符） → 应重试但不计入Worker质量扣分
- 需求问题（需求本身不清晰/矛盾） → 应退回需求梳理而非继续执行

**影响**：后续复盘无依据，无法针对性改进；LLM偶发异常被等同于Worker能力不足，不公平扣分。

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
|----------|---------|-------------------|---------|
| CodeError | 重新执行，提示修正方向 | 计入 | RetryCount>=3 则终止 |
| LlmException | 重新调用LLM，换备用模型 | 不计入（LLM问题非Worker责任） | LLM连续3次失败则终止并标记LlmException |
| RequirementIssue | 退回Step1(DemandAnalyse)重新梳理 | 计入但归零Step后续重试 | 通知Secretary重新调度 |
| ReviewRejection | Worker修正后重新提交 | 计入 | RetryCount>=3 则终止 |
| Timeout | 强制释放Agent，重新分配 | 不计入（超时可能是资源问题） | 换一个Worker重新执行 |

### 16.5 Learning学习机制隐性缺陷

#### 16.5.1 学习效果无量化闭环

**问题**：当前Learning机制仅更新 SkillTags 与 LearningProgress，没有学习结果落地验证。无法判定学习是否真正优化执行质量——学习后Worker的通过率是否提升？Secretary的调度效率是否改善？Manager的审核准确率是否提高？

**影响**：学习可能沦为"自嗨式"标签堆砌，无实际效果佐证，长期无法迭代学习策略。

**改进方案**：学习效果验证闭环

1. **学习前快照**：记录Agent当前 PassRate、AvgStepDuration、RejectRate
2. **执行学习**：更新 SkillTags、LearningProgress
3. **学习后验证窗口（7天）**：
   - 收集学习后该Agent处理的N个任务数据
   - 对比学习前快照：PassRate_delta = PostPassRate - PrePassRate
   - AvgStepDuration_delta = PostAvg - PreAvg
   - RejectRate_delta = PostRejectRate - PreRejectRate
4. **效果判定**：
   - PassRate_delta > 0 → 学习有效，保留新标签
   - PassRate_delta <= 0 → 学习无效，回滚标签至学习前版本
   - 标记 LearningEffectiveness = Effective / Ineffective / Rollback

**AgentEntity新增字段**：
- PreLearningPassRate(decimal?) — 学习前通过率快照
- PreLearningRejectRate(decimal?) — 学习前驳回率快照
- LastLearningVerifiedAtUtc(DateTime?) — 学习效果验证时间
- LearningEffectiveness(string?) — Effective/Ineffective/Rollback

#### 16.5.2 学习占用LLM资源无配额限制

**问题**：业务任务与学习任务共用LLM模型通道，无配额隔离。学习高峰期（多个Agent同时空闲超时触发）可能抢占业务调用资源，导致正常任务执行延迟或排队。

**影响**：业务优先级被打乱，客户体验下降；LLM API成本激增但无法按业务价值分配。

**改进方案**：学习任务排队排序 + LLM资源配额隔离

**LLM资源配额隔离**：

| 资源池 | 配额占比 | 说明 |
|--------|---------|------|
| 业务任务池 | 80% | 保障业务任务优先获得LLM资源 |
| 学习任务池 | 20% | 学习任务使用独立配额，不抢占业务资源 |

**学习任务排队排序**：

| 学习优先级 | 角色/触发类型 | 排队策略 |
|----------|---------|----------|
| 3（最高） | Worker实操复盘 / 异常触发纠错学习 | 配额优先保障，立即执行 |
| 2（中等） | Manager审核复盘 | 空隙执行，配额次优先 |
| 1（最低） | Secretary调度复盘 / 空闲超时触发 | 可延迟执行，配额最后保障 |

**实现方式**：
- AgentLearningService调用LLM时，通过ILlmGateway的配额管理接口指定quotaPool = "learning"，与业务任务隔离
- 新增 LearningQueueService（Application/Services/）负责学习任务排队与优先级调度
- 新增 LlmQuotaManager（Application/Services/）负责LLM配额池管理与配额借用
- 业务配额耗尽时，可临时借用学习配额（反之不可）

### 16.6 人工介入休眠状态（Dormant）

**问题**：当前Agent三状态模型（Idle/Handling/Learning）假设Agent永不停歇。但实际运维中存在需要人工介入冻结Agent的场景：
- Agent反复学习无效，需要人工分析原因后再决定是否恢复
- Agent质量评分持续下降，需要人工审查后调整配置
- 系统调试/维护期间，需要暂停特定Agent避免干扰
- LLM资源配额耗尽时，部分Agent应进入节能状态

当前只能通过 IsEnabled=false 禁用Agent，但这会丢失Agent状态上下文，恢复时需重新初始化。

**改进方案**：新增 Dormant（休眠）状态，Agent四状态模型

**修订后的四状态流转图**：

```
Idle ──接收任务──> Handling ──完成/失败──> Idle
   |                  ^                       ^  |
   |                  |       +----------------+  |
   |                  |       | 高优先级中断      |
   |                  +-- 中断学习 <-- Learning <--空闲超时自动触发--+
   |                                      ^
   |                  +--人工唤醒----------+|
   |                  |                     |
   +--人工介入/学习无效/质量预警/运维冻结--> Dormant(休眠)
                                       不学习、不接单、等待人工决策
```

**AgentState枚举修订**：

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
| LLM配额耗尽/服务器资源不足触发Dormant | 批量休眠，释放资源给业务任务 |
| DormantAgent保留全部状态上下文 | 唤醒后无缝恢复（SkillTags/LearningProgress/PassRate等） |

**AgentStateMachine新增流转**：

```
Idle ──(EnterDormant)──> Dormant           // 人工介入休眠
Handling ──(EnterDormant)──> Dormant    // 资源紧张/异常休眠
Learning ──(EnterDormant)──> Dormant     // 学习无效/内容异常休眠
Dormant ──(ResumeByAdmin)──> Idle              // 管理员手动恢复
```

**新增API端点**：

| 方法 | 路径 | 功能 |
|------|------|------|
| POST | /api/agents/{id}/dormant | 管理员将Agent设为休眠 |
| POST | /api/agents/{id}/wake | 管理员手动唤醒恢复Idle |
| GET  | /api/agents/dormant | 列出所有休眠Agent及休眠原因 |

### 16.7 上下文硬性截断上限

**问题**：当前上下文链式传递设计中，前序工序产出物通过 ExtractSummary 提取摘要后传递，但**没有硬性截断上限**。随着工序推进（Step6/Step7），累积上下文可能超过LLM模型的Token上限，导致：
- LLM调用超限报错
- 请求响应时间过长甚至卡死
- Token成本不可控飙升

**影响**：后期工序（版本提交、最终审核）的执行稳定性下降，可能出现调用失败或返回截断结果。

**改进方案**：引入 MaxStepContextTokens 硬性截断

| 参数 | 默认值 | 说明 |
|------|--------|------|
| MaxStepContextTokens | 8000 | 单步上下文最大Token数 |
| MaxGlobalContextTokens | 2000 | 全局上下文(原始需求+流程摘要)最大Token数 |

**截断优先级**（必须保障的内容优先级从高到低）：

| 截断优先级 | 保留内容 | 说明 |
|------------|---------|------|
| 1（必保留） | 原始需求(task.Input) | 业务核心，不可截断 |
| 2（高优先） | 最近2个工序的完整产出 | 保证上下文连贯性 |
| 3（中优先） | 更早工序的摘要 | 按距离递减截断 |
| 4（低优先） | 审核评论摘要 | 仅保留驳回关键信息 |

**新增配置**：GlobalConfigEntity 新增 MaxStepContextTokens 配置项（默认8K，可根据模型调整）。

**要求每个工序产出物必须包含 summary 字段**（一句话摘要），上下文传递时仅传递摘要而非全文。若无 summary，自动调用LLM提取摘要后再传递。

### 16.8 Step卡死应急解绑

**问题**：当前设计中单工序重试超限后直接标记Task为Failed，但绑定的Agent可能被长时间锁在 Handling 状态（重试循环中），无限占用执行节点。

**影响**：卡死的Agent不释放，其他任务无法分配到该Worker，资源被无效占用。

**改进方案**：单工序重试超限自动释放Agent，不无限锁死执行节点

**解绑规则**：

| 场景 | 处理 | Agent状态 |
|------|------|----------|
| 重试超限 + 前置工序(1-3) | Task=Failed，释放Agent | Agent转为Idle |
| 重试超限 + 后置工序(4-7) | Step重置为Pending，换Worker重试 | 旧Agent转为Idle，新Agent转为Handling |
| Worker卡死超时(Handling>30min) | 强制释放Agent | Agent转为Idle |
| LLM异常导致卡死 | 标记FailureCategory=LlmException | Agent转为Idle（非Worker责任） |

**新增方法**：TaskStepFlowService.OnStepRetryExceeded() — 重试超限时的完整处理逻辑（标记失败+释放Agent+记录原因+触发事件）

### 16.9 改进项总览与优先级

| # | 设计短板 | 改进方案 | 优先级 | 影响范围 |
|---|---------|---------|--------|---------|
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

**报告完成时间**: 2026-05-22
**报告版本**: v3.1
**变更记录**: 
- v2.0: 修订三状态模型(Handling替代Running)、秘书从单实例改为多实例
- v3.0: 补充分布式Agent注册+心跳方案、五层测试策略、Learning空闲超时自动触发机制、Agent间通信/上下文/产出模型、扩展性架构设计
- v3.1: 补充MVP设计短板分析——空闲超时阈值差异化、多秘书负载均衡、Manager审核边界约束、TaskStep失败原因归类、学习效果量化闭环、LLM资源配额、人工介入休眠状态(Dormant)、上下文硬性截断上限、Step卡死应急解绑、学习任务排队排序
