# 阶段十三：Repo 与审查模块管理

**日期**: 2026-05-20  
**预估时间**: 6-8 天  
**优先级**: 🔴 P0 - 核心治理能力  
**前置依赖**: 阶段二（用户与认证）、阶段四（任务中心）、阶段六（Git 集成）、阶段十二（Sandbox 继承与 Repo 同步）

---

## 我是如何考虑的

### 设计思路

本阶段目标是把“仓库管理”与“代码审查”打通为可运营闭环：

1. 用户在 Repositories 配置仓库与分支策略。
2. 用户发起 Review 任务（手动/定时/流水线前置）。
3. 后端基于任务快照拉取指定代码到用户隔离 Sandbox。
4. 审查引擎执行规则并生成结构化报告。
5. 结果回写任务中心，支持追踪、复跑、统计。

核心原则：

1. 快照优先：审查执行只依赖任务创建时快照，避免配置漂移。
2. 结果可追溯：每条问题定位到规则、文件、行号和证据。
3. 规则可分层：全局默认 + 仓库覆盖 + 任务临时覆盖。
4. 安全最小化：凭据只引用密文，不落日志明文。

### 复用设计

本阶段尽量复用现有能力，避免重复建设：

| 复用组件 | 复用自 | 本阶段复用方式 |
|---------|-------|--------------|
| JwtAuthMiddleware | 阶段二 | 获取当前用户身份与权限边界 |
| TaskService/TaskQueueService/TaskExecutor | 阶段四 | 复用任务状态机、排队与执行框架 |
| RepositoryService/GitProviderFactory | 阶段六 | 复用仓库配置与多平台 Git 抽象 |
| SandboxPathResolver/RepoSyncTaskHandler | 阶段十二 | 复用路径隔离与代码落盘能力 |
| ApiResponse<T> | 阶段一 | 统一接口响应格式 |
| ExceptionHandlingMiddleware | 阶段一 | 统一异常处理与追踪 |

---

## 本阶段复用的功能清单（来自其他阶段）

| 复用组件 | 文件路径 | 复用方式 | 避免重复代码 |
|---------|---------|---------|-----------|
| TaskService | server/src/AutoCodeForge.Application/Services/TaskService.cs | 创建与流转审查任务 | 避免重复任务状态机逻辑 |
| TaskQueueService | server/src/AutoCodeForge.Infrastructure/BackgroundServices/TaskQueueService.cs | 异步执行审查任务 | 避免重复队列调度逻辑 |
| RepositoryService | server/src/AutoCodeForge.Application/Services/RepositoryService.cs | 读取仓库配置与分支信息 | 避免重复仓库查询逻辑 |
| GitProviderFactory | server/src/AutoCodeForge.Infrastructure/Git/GitProviderFactory.cs | 审查拉取前的 Git Provider 选择 | 避免重复平台判断逻辑 |
| SandboxPathResolver | server/src/AutoCodeForge.Infrastructure/Sandbox/SandboxPathResolver.cs | 审查执行目录规范化与安全校验 | 避免重复路径拼接与防越权逻辑 |
| ApiResponse<T> | server/src/AutoCodeForge.Core/Models/ApiResponse.cs | 审查接口标准化返回 | 避免重复响应包装代码 |

---

## 本阶段新增的可复用功能清单

| 复用组件 | 文件路径 | 说明 | 被复用次数（预估） |
|---------|---------|------|------------------|
| ReviewRuleSetEntity | server/src/AutoCodeForge.Core/Entities/ReviewRuleSetEntity.cs | 审查规则集定义（全局/仓库级） | 3+ |
| ReviewTaskEntity | server/src/AutoCodeForge.Core/Entities/ReviewTaskEntity.cs | 审查任务运行态模型 | 3+ |
| ReviewFindingEntity | server/src/AutoCodeForge.Core/Entities/ReviewFindingEntity.cs | 审查问题明细（规则/文件/行号） | 4+ |
| IReviewEngine | server/src/AutoCodeForge.Core/Interfaces/IReviewEngine.cs | 审查引擎抽象接口 | 2+ |
| RuleBasedReviewEngine | server/src/AutoCodeForge.Infrastructure/Review/RuleBasedReviewEngine.cs | 规则驱动审查引擎实现 | 2+ |
| ReviewTaskHandler | server/src/AutoCodeForge.Infrastructure/BackgroundServices/Handlers/ReviewTaskHandler.cs | 审查任务执行器 | 2+ |
| ReviewService | server/src/AutoCodeForge.Application/Services/ReviewService.cs | 审查任务编排与查询服务 | 3+ |
| ReviewEndpoints | server/src/AutoCodeForge.Api/Endpoints/ReviewEndpoints.cs | 审查任务 API | 3+ |

---

## 数据表变更清单（字段 + 索引 + 唯一约束）

### 变更原则

1. 任务主表只保留通用状态，审查细节落独立表。
2. 找到一条问题可反查任务、仓库、用户与代码位置。
3. 优先增量迁移，避免破坏既有任务与仓库数据。

### A. 现有表增量变更

| 表名 | 变更类型 | 字段 | 类型 | 约束/默认值 | 说明 |
|------|---------|------|------|------------|------|
| Tasks | 新增 | DomainType | TEXT | NOT NULL, DEFAULT 'General' | 标记是否为 Review 任务 |
| Tasks | 新增 | DomainRecordId | TEXT/Guid | NULL | 关联 ReviewTask 主键 |
| Repositories | 新增 | DefaultReviewRuleSetId | TEXT/Guid | NULL | 仓库默认审查规则集 |

### B. 新增审查域表

建议新增以下表：

1. ReviewRuleSets：规则集元信息（名称、级别、启用状态、版本）。
2. ReviewTasks：审查任务运行态（TaskId、RepositoryId、Snapshot、Summary）。
3. ReviewFindings：审查问题明细（Severity、RuleCode、FilePath、Line、Message、Suggestion）。

### C. 索引设计

| 表名 | 索引名 | 字段 | 类型 | 目的 |
|------|-------|------|------|------|
| ReviewTasks | IX_ReviewTasks_TaskId | TaskId | 唯一索引 | 保证任务与审查记录 1:1 |
| ReviewTasks | IX_ReviewTasks_Repo_UpdatedAt | RepositoryId, UpdatedAt | 普通索引 | 查看仓库最近审查记录 |
| ReviewFindings | IX_ReviewFindings_ReviewTask_Severity | ReviewTaskId, Severity | 普通索引 | 按严重级筛选问题 |
| Tasks | IX_Tasks_DomainType_Status | DomainType, Status | 普通索引 | 任务中心按域和状态筛选 |

### D. 唯一约束与一致性约束

| 约束 | 作用对象 | 规则 |
|------|---------|------|
| UQ_ReviewTasks_TaskId | ReviewTasks.TaskId | 一个任务仅对应一条审查运行记录 |
| CK_ReviewFindings_Severity | ReviewFindings.Severity | 仅允许 Critical/High/Medium/Low/Info |
| FK_ReviewTasks_Task | ReviewTasks.TaskId -> Tasks.Id | 审查运行记录必须关联任务 |
| FK_ReviewTasks_Repository | ReviewTasks.RepositoryId -> Repositories.Id | 审查运行记录必须关联仓库 |

### E. 迁移与回滚建议

1. 先发布表结构与索引，再上线 Handler 与 API。
2. 回滚优先停用 Review 任务入口，不删除历史审查记录。
3. 历史任务 DomainType 回填为 General。

---

## 任务清单

| 编号 | 任务名称 | 文件路径 | 产出物 | 验证方式 |
|------|---------|---------|-------|---------|
| 13.1 | 新增审查域实体与迁移 | server/src/AutoCodeForge.Core/Entities/, server/src/AutoCodeForge.Infrastructure/Persistence/Migrations/ | ReviewRuleSet/ReviewTask/ReviewFinding 实体与数据库迁移 | dotnet build 通过；迁移可成功 apply |
| 13.2 | 定义审查 DTO 与状态模型 | server/src/AutoCodeForge.Core/DTOs/Review/ | 创建审查请求、摘要、问题详情 DTO | DTO 编译通过；Swagger 可见模型 |
| 13.3 | 实现 IReviewEngine 与规则执行器 | server/src/AutoCodeForge.Core/Interfaces/IReviewEngine.cs, server/src/AutoCodeForge.Infrastructure/Review/RuleBasedReviewEngine.cs | 可执行规则并返回结构化 findings | 单元测试覆盖规则命中与空结果 |
| 13.4 | 实现 ReviewRepository 与查询聚合 | server/src/AutoCodeForge.Infrastructure/Repositories/ReviewRepository.cs | 审查记录查询（按任务/仓库/用户） | 集成测试验证分页与筛选 |
| 13.5 | 实现 ReviewService 编排层 | server/src/AutoCodeForge.Application/Services/ReviewService.cs | 发起审查、重跑审查、汇总统计 | 服务层测试通过 |
| 13.6 | 新增 ReviewTaskHandler | server/src/AutoCodeForge.Infrastructure/BackgroundServices/Handlers/ReviewTaskHandler.cs | 审查任务后台执行与状态回写 | 任务状态流转：Pending->Running->Completed/Failed |
| 13.7 | 新增 Review API Endpoints | server/src/AutoCodeForge.Api/Endpoints/ReviewEndpoints.cs | 审查创建、详情、报告、重跑接口 | Swagger 联调通过；响应统一 |
| 13.8 | 仓库默认规则集配置能力 | server/src/AutoCodeForge.Application/Services/RepositoryService.cs, server/src/AutoCodeForge.Api/Endpoints/RepositoryEndpoints.cs | Repo 维度绑定默认规则集 | 更新仓库配置后可生效于新任务 |
| 13.9 | 日志脱敏与审计事件 | server/src/AutoCodeForge.Infrastructure/Logging/LogSanitizer.cs, server/src/AutoCodeForge.Infrastructure/Review/ | 审查日志与安全审计落地 | 日志扫描无 token/密钥明文 |
| 13.10 | DI 注册与端到端联调 | server/src/AutoCodeForge.Api/Program.cs | 注册 ReviewEngine/Service/Handler/Repository | 从仓库选择到审查结果查询全链路通过 |

### 执行状态快照（2026-05-20）

1. 已完成阶段十三的主要后端能力：`13.1`、`13.2`、`13.3`、`13.4`、`13.5`、`13.6`、`13.7`、`13.8`、`13.10` 已具备可运行实现。
2. 当前支持：ReviewRuleSet CRUD、仓库默认规则集配置、创建 Review 任务、基于最新 RepoSync 工作区执行规则扫描、持久化 findings、任务取消/重跑、查询任务摘要与 findings、按仓库分页查看审查历史。
3. 当前未完全覆盖：`13.9` 的独立审计事件和更细粒度日志脱敏仍可继续增强；Review 执行仍依赖已有 RepoSync 工作区，而不是在 Handler 内独立完成代码拉取。
4. 本轮验证：`dotnet test AutoCodeForge.sln --filter ReviewServiceTests --nologo` 共 `5` 项全部通过。

---

## 注意事项

1. 审查执行必须使用任务快照，不允许读取“当前最新配置”替代。
2. 审查结果文件路径必须是相对路径，禁止暴露宿主机绝对路径。
3. Review 任务超时与取消必须可用，防止占满队列。
4. 严重级定义要固定，避免前后端语义不一致。
5. 审查规则执行失败不应导致整个系统崩溃，应降级并记录错误。

---

## 阶段完成总结

### 复用收益

1. 复用任务中心执行框架，避免重复构建调度系统。
2. 复用仓库与沙箱能力，避免重复实现拉取与隔离逻辑。
3. 复用统一响应与异常中间件，减少接口层样板代码。

### 本阶段验收口径

1. 可从仓库发起审查任务并异步执行。
2. 可按任务查看审查报告与问题明细。
3. 可按仓库查看审查历史与趋势统计。
4. 敏感信息不落日志，失败可定位到规则与文件。

### 下一步

完成本阶段后，进入阶段十四，将 Git 技能接入 Agent 执行链路，实现“可读可控可审计”的智能 Git 操作能力。
