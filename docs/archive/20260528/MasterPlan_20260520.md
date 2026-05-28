# MASTER_PLAN_20260520.md

## 基本信息
- 项目名称：AutoCodeForge
- 当前执行轮次：第8轮
- 最新更新时间：2026年05月21日 01:42
- 规划负责人：Strategic Planner（AI）
- 执行流水线：DevOps Orchestrator（@Worker + @Auditor）

## 一、全局任务池（P0-P3分级）
| 任务优先级 | 任务描述 | 负责人 | 预计工时 | 已用工时 | 完成度 | 当前状态（进行中/已完成/暂停） | 备注（组件复用要求/依赖项） |
|------------|----------|--------|----------|----------|--------|--------------------------------|------------------------------|
| P0（核心） | 阶段一基础设施初始化（server 解决方案 + 四层项目） | @Worker | 2h | 1.5h | 100% | 已完成 | 作为阶段二至十的基础 |
| P0（核心） | 可复用基类与基础仓储（AuditableEntity/UserOwnedEntity/BaseRepository） | @Worker | 4h | 3h | 100% | 已完成 | 需强制复用已创建基座 |
| P1（规范） | 全局异常中间件与统一响应模型 | @Worker/@Auditor | 1h | 0.8h | 100% | 已完成 | 对齐 PROJECT_SPEC 模板要求 |
| P1（规范） | 阶段一公共类型 XML 注释补齐与合规复核 | @Worker/@Auditor | 0.5h | 0.3h | 100% | 已完成 | 补齐 Core/Infrastructure/Api 基础设施公开成员注释并复构建验证 |
| P2（质量） | 构建与可运行性验证（Swagger） | @Worker | 1h | 0.6h | 100% | 已完成 | dotnet build 已通过 |
| P3（优化） | SqlSugar 查询过滤器增强与例外查询策略 | @Worker | 1h | 0.6h | 80% | 进行中 | 已完成可审计实体过滤与兼容 UserEntity/GlobalConfigEntity 的仓储基类扩展 |
| P0（核心） | 阶段二数据层与认证系统（16 实体 + JWT + Auth API + Seed） | @Worker/@Auditor | 6h | 5.6h | 100% | 已完成 | 已补 AuthService 单元测试 + AuthEndpoints 集成测试并通过验证 |
| P0（核心） | 阶段三 AI 核心模块首版（LLM 网关 + Agent/Chat/SSE） | @Worker/@Auditor | 6h | 3.7h | 90% | 进行中 | 已完成 DTO/接口/仓储/服务/端点与 DI 注册，构建测试通过 |
| P1（重要） | 阶段四任务中心模块（Task DTO/Repository/Service/Queue/Executor/Endpoints） | @Worker/@Auditor | 3h | 2.8h | 100% | 已完成 | 已复用 AgentExecutor 与 BaseRepository，支持异步执行、状态流转、任务日志 |
| P1（重要） | 阶段五定时任务调度（ScheduledTask DTO/Repository/Service/CronScheduler/Endpoints） | @Worker/@Auditor | 2h | 2h | 100% | 已完成 | 复用 BaseRepository/TaskQueueService 链路，Cronos 解析 Cron 表达式，防重复触发 |
| P2（一般） | 阶段七流水线模块（Pipeline DTO/Repository/Service/Sync/Endpoints） | @Worker/@Auditor | 2h | 1.8h | 100% | 已完成 | 复用 RepositoryRepository + BaseRepository，新增 Build 历史与后台状态同步 |
| P2（一般） | 阶段八 Wiki 模块（Wiki DTO/Repository/Service/Endpoints） | @Worker/@Auditor | 1.2h | 1.2h | 100% | 已完成 | 复用 BaseRepository/RepositoryRepository，支持 Wiki CRUD、搜索、仓库关联 |
| P2（完善） | 阶段九系统配置与健康检查（Config DTO/Service/Endpoints + Health/System 端点 + 日志） | @Worker/@Auditor | 2.6h | 2.6h | 100% | 已完成 | 复用 BaseRepository/ApiResponse，新增 Config/Health/System 端点与结构化日志 |
| P0（核心） | 阶段十三 Repo 与审查模块管理（Review Entities/Engine/Repository/Service/Handler/Endpoints） | @Worker/@Auditor | 6h | 4.4h | 85% | 进行中 | 已补齐规则集 CRUD、仓库默认规则集配置、Review 重跑/取消；剩余审计与独立拉取执行链增强 |

## 二、实时进度汇总
1. 本轮（第9轮）核心成果：完成阶段九系统配置与健康检查，新增 Config DTO、ConfigService、Config/Health/System Endpoints，并完成 Program.cs 的 DI 注册和日志配置。
2. 累计完成任务：阶段一、二、四、五、七、八、九 100%；阶段三 90%。
3. 未完成任务：管理员跨租户边界策略细化；Microsoft Agent Framework 真实执行链路替换当前占位实现；PipelineService/PipelineSyncService 单元测试补齐。
4. 工时平衡情况：本轮新增投入约 2.6h，重点完成系统配置和健康检查模块的完整实现。
5. 补充验证结果：2026-05-20 执行 dotnet build 通过；所有端点功能验证通过；日志输出正常（详见 ROUND_REPORT_20260520_DEV_015.md）。

## 三、风险预判与应对（Planner兜底）
| 风险描述 | 影响范围 | 应对措施 | 处理状态 |
|----------|----------|----------|----------|
| SqlSugar 版本差异导致 QueryFilter API 变动 | Repository 默认查询行为 | 固定当前版本并在升级前做兼容验证 | 已处理 |
| 阶段二认证落地前 NtId 来源不稳定 | 用户隔离查询准确性 | 已实现 JwtAuthMiddleware 注入 Claims + HttpContext.Items 回退，CurrentUser 优先读取 Items/Claims/Header | 已处理 |
| JWT 密钥误配或过短 | 认证链路安全性 | 配置 `Jwt` 节点并支持环境变量 `JWT_KEY` 覆盖，生产环境要求外部注入高强度密钥 | 待持续检查 |
| 阶段三真实 LLM 接入环境未就绪 | Chat 响应真实性与效果 | 下一轮优先完成 Microsoft Agent Framework 真实执行链路与配置校验 | 进行中 |
| 阶段四任务异步执行并发竞争 | 任务状态一致性 | 已通过 Pending->Running 条件更新实现防重复执行，后续可补充分布式锁与超时回收 | 进行中 |

## 四、排期调整记录
| 调整时间 | 原排期 | 调整后排期 | 调整原因 | 调整人 |
|----------|--------|------------|----------|--------|
| 2026-05-20 12:15 | 阶段一 1-2 天 | 阶段一 当日完成 | 初始化执行顺利，基础设施一次通过编译 | Strategic Planner |

## 五、下一轮（第10轮）重点任务
1. 核心任务：阶段十测试与优化（单元测试补齐、性能调优）。
2. 次要任务：补充 PipelineService 与 PipelineSyncService 单元测试（状态同步与构建日志断言）。
3. 次要任务：实现管理员例外查询策略（跨 NtId 查询白名单与审计日志）。
4. 次要任务：补充 Config/Health 端点的单元测试与集成测试。
5. 需重点关注：后台服务并发访问 Sqlite 连接稳定性；生产环境 Config/Health 端点的性能和可靠性。

## 六、补充记录（2026-05-20 阶段十状态核对）
1. 已补做阶段十基线核验：执行 `dotnet test AutoCodeForge.sln --nologo`，结果为 `47` 总数、`38` 通过、`7` 失败、`2` 跳过。
2. 结论：阶段十已启动但未完成，当前主要阻断为 `RepositoryServiceTests` 的 mock 设计问题、`GitProviderTests` 的 Azure DevOps 断言失败，以及 Chat smoke 仍处于 skip。
3. 文档同步：`docs/plans/10-phase-ten-testing.md` 已改为状态快照；本次核对结果已归档到 `docs/reports/ROUND_REPORT_20260520_DOCS_001.md`。

## 七、补充记录（2026-05-20 阶段十回归修复完成）
1. 已完成阶段十本轮收口：修复 `RepositoryServiceTests`、`GitProviderTests`、Chat 集成测试 skip/失败，并补齐 `TestBase`、`TestDataFactory`、`TestWebApplicationFactory`。
2. 已新增轻量性能基线测试 `server/tests/AutoCodeForge.Tests/Performance/LlmGatewayPerformanceTests.cs`，并新增后端文档 `server/docs/API.md` 与 `server/docs/DEPLOYMENT.md`。
3. 最终验证结果：`dotnet test AutoCodeForge.sln --nologo` 现为 `48` 总数、`48` 通过、`0` 失败、`0` 跳过。
4. 阶段判断：`10.14` 已完成，但原计划中的 `10.3`、`10.5`、`10.6`、`10.10` 仍建议后续单独补齐。

## 八、补充记录（2026-05-20 阶段十原始计划闭环）
1. 已新增 `server/tests/AutoCodeForge.Tests/BaseRepositoryTests.cs`、`server/tests/AutoCodeForge.Tests/AgentServiceTests.cs`、`server/tests/AutoCodeForge.Tests/LlmGatewayTests.cs`。
2. 已新增独立 review 记录：`docs/reports/CODE_REVIEW_20260520_PHASE10.md`。
3. 全量回归结果已更新为 `58` 总数、`58` 通过、`0` 失败、`0` 跳过。
4. 结论：阶段十原始计划项和验证门禁均已完成。

## 九、补充记录（2026-05-21 阶段十二首版实现）
1. 已完成阶段十二后端首版闭环：新增 RepoSync 任务类型、任务快照字段、运行态表 `RepoSandboxWorkspaces`、路径解析器 `SandboxPathResolver`、执行处理器 `RepoSyncTaskHandler` 与端点 `RepoSyncEndpoints`。
2. 已完成 Sandbox 配置结构化增强：新增 `SandboxConfigDto`、`SandboxConfigValidator`，并在 `ConfigService` 与 `ConfigEndpoints` 提供 typed 的读写接口（`/api/v1/configs/user/sandbox`）。
3. 已引入 `LibGit2Sharp` 并实现 `LibGit2SharpProvider` + `GitCloneService`，RepoSync 执行链可完成 clone/pull 并回写 commit sha 与工作区状态。
4. 本轮验证结果：`dotnet build AutoCodeForge.sln --nologo` 通过；定向测试 `SandboxConfigValidatorTests`、`SandboxPathResolverTests` 共 `6` 项全部通过。
5. 剩余建议：补齐 RepoSync 取消与超时硬中断机制，以及 RepoSync API 端到端集成测试。

## 十、补充记录（2026-05-21 阶段十二补齐项完成）
1. 已补齐 RepoSync 取消能力：新增 `POST /api/v1/repo-sync/tasks/{taskId}/cancel`，并在 `RepoSyncService` 落地状态迁移与审计日志。
2. 已补齐 RepoSync 超时控制：`RepoSyncTaskHandler` 执行时按 `timeoutSeconds` 建立超时令牌，超时后任务标记为失败并记录可观测错误信息。
3. 已补齐 Git 传输取消信号：`LibGit2SharpProvider` 在 clone/fetch/pull 传输回调中响应取消令牌，减少长时间阻塞。
4. 已补测试覆盖：新增 `RepoSyncServiceTests`（创建、取消、非法取消），并完成定向回归。
5. 验证结果：`dotnet build AutoCodeForge.sln --nologo` 通过；`dotnet test --filter RepoSyncServiceTests|SandboxConfigValidatorTests|SandboxPathResolverTests` 共 `9` 项全部通过。

## 十一、补充记录（2026-05-21 阶段十三首轮后端闭环）
1. 已完成阶段十三首轮后端实现：新增 `ReviewRuleSetEntity`、`ReviewTaskEntity`、`ReviewFindingEntity`，并将 Review 表纳入 `DatabaseInitializer` 的 Code First 建表清单。
2. 已完成 Review 核心执行链：新增 `IReviewEngine` + `RuleBasedReviewEngine`、`ReviewRepository`、`ReviewService`、`ReviewTaskHandler` 与 `ReviewEndpoints`，并在 `TaskExecutor` / `Program.cs` 完成分发与 DI 注册。
3. 当前链路已可复用阶段十二 RepoSync 工作区：创建 Review 任务后，后台会扫描已同步仓库目录，输出结构化 findings，按任务和仓库查询摘要与历史。
4. 本轮验证结果：`dotnet test AutoCodeForge.sln --filter "RuleBasedReviewEngineTests|ReviewServiceTests" --nologo` 共 `5` 项全部通过。
5. 剩余建议：补齐规则集 CRUD、Review 重跑/取消、仓库默认规则集 API、审计事件与更强的日志脱敏能力。

## 十二、补充记录（2026-05-21 阶段十三后续功能补齐）
1. 已补齐 `ReviewRuleSet` 管理能力：新增规则集 DTO、`ReviewRuleSetRepository`、`ReviewRuleSetService`，并在 `ReviewEndpoints` 增加 `/api/v1/reviews/rule-sets` CRUD 接口。
2. 已补齐 Review 任务状态补偿：`ReviewService` 新增取消与重跑能力，`ReviewTaskHandler` 执行前后会回查任务状态，支持运行中用户取消的状态回写。
3. 已补齐仓库默认规则集配置：新增 `RepositoryReviewSettingsService` 与 `/api/v1/repositories/{id}/review-settings`，可为仓库绑定默认规则集。
4. 本轮验证结果：`dotnet test AutoCodeForge.sln --filter ReviewServiceTests --nologo` 共 `5` 项全部通过。
5. 剩余建议：继续增强 `13.9` 的审计事件与日志脱敏，并考虑让 Review Handler 在任务快照内自带仓库拉取链路，减少对已有 RepoSync 工作区的前置依赖。

## 十三、补充记录（2026-05-20 阶段十四首轮落地）
1. 已完成阶段十四首轮核心实现：新增 `GitReadToolset`、`GitWriteToolset`、`GitSkillPermissionGuard`、`GitContextHydrator`、`AgentToolAuditLogger`、`GitSkillErrorMapper`，并通过 `IAgentTool` 注入到 `AgentExecutor` 调用链。
2. 已新增策略与审计数据模型：新增 `GitSkillGrantEntity`、`AgentToolInvocationEntity`，并在 `DatabaseInitializer` 纳入建表清单，支持仓库级权限策略和工具调用摘要审计。
3. 已新增策略 API：新增 `AgentSkillEndpoints`，提供 `/api/v1/agent-skills/git/grants/{repositoryId}` 查询与更新能力。
4. 已补齐首轮测试：新增 `GitSkillPermissionGuardTests`、`GitSkillPolicyServiceTests`；验证结果 `12` 通过、`0` 失败、`0` 跳过。
5. 风险与下一步：`create-branch` 与 `commit-changes` 尚需扩展 `IGitProvider` 统一抽象并补全三方 Provider 适配；当前已通过策略网关默认拒绝高风险操作。
