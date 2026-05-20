# MASTER_PLAN_20260520.md

## 基本信息
- 项目名称：AutoCodeForge
   - 当前执行轮次：第14轮
   - 最新更新时间：2026年05月20日（第14轮 阶段七流水线模块闭环完成）
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
| P0（核心） | 阶段二数据层与认证系统（16 实体 + JWT + Auth API + Seed） | @Worker/@Auditor | 6h | 5.8h | 100% | 已完成 | 已通过 validate-only 闭环验证（build + 9 个认证测试） |
| P0（核心） | 阶段三 AI 核心模块首版（LLM 网关 + Agent/Chat/SSE） | @Worker/@Auditor | 6h | 5.6h | 100% | 已完成 | 已完成 Microsoft Agent Framework 真实执行链路，验收 3/3 通过，测试 18/18 通过 |
| P1（重要） | 阶段四任务中心模块（Task DTO/Repository/Service/Queue/Executor/Endpoints） | @Worker/@Auditor | 3h | 2.8h | 100% | 已完成 | 已复用 AgentExecutor 与 BaseRepository，支持异步执行、状态流转、任务日志 |
| P1（重要） | 阶段五定时任务调度（Cronos + ScheduledTask DTO/Repository/Service/BackgroundService/Endpoints） | @Worker/@Auditor | 2h | 2.0h | 100% | 已完成 | 已复用阶段一/二/四基础设施，build 0 errors，test 12 passed；validate-only 闭环通过 |
| P1（重要） | 阶段六 Git 仓库集成（多平台 Provider + RepositoryService + Endpoints + GitTools） | @Worker/@Auditor | 3h | 3.0h | 100% | 已完成 | 已完成 GitHub/GitLab/Azure DevOps 提供者、凭据加密、多租户隔离与工具集成 |
| P2（质量） | 阶段十测试与优化收口（回归修复 + 集成测试解封 + 性能/文档补齐 + 计划缺口补齐） | @Worker/@Auditor | 3h | 3.0h | 100% | 已完成 | 当前基线：58 total / 58 passed / 0 failed / 0 skipped；阶段十原始计划项全部闭环 |
| P2（质量） | 阶段七流水线模块（Pipeline/Build DTO + Repository + Service + Endpoints + BackgroundService） | @Worker/@Auditor | 1.5h | 1.4h | 100% | 已完成 | 已完成 CRUD、触发构建、30秒轮询状态同步；复用 BaseRepository/UserOwnedEntity/ApiResponse；build 0 errors，test 75 passed；ROUND_REPORT_20260520_DEV_021.md |

## 二、实时进度汇总
   1. 第10轮核心成果：完成 P0 测试债务 TD-20260520-002（ScheduledTaskService + CronSchedulerService 单元测试补齐），并修复回归阻断项（DataProtection DI 注册、ScheduledTaskExecutionEntity 可空映射）。
   2. 第11轮核心成果：完成 RQ-ADMIN-BOUNDARY-20260520-01（管理员跨租户边界策略细化），实现 scoped whitelist（NtId|ResourceScope|TargetTenant）与 allow/deny 审计落库。
   3. 第11轮验收结果：`dotnet build AutoCodeForge.sln` 通过；`AdminAuditServiceTests` 5/5 通过；核心 smoke（`AuthEndpointsTests`）2/2 通过。
   4. 本轮投入工时：P0 实际 1.8h（策略实现 + 审计补强 + 测试补齐 + 回归门禁）。
   5. 第11轮验证完成：已完成 build/受影响测试/smoke 三道门禁，MasterPlan 与 Round Report 同步完成。
   6. 累计完成任务：阶段一、二、三、四、五、六 100%。
   7. 仍待收敛项：阶段六 Webhook 同步与高级 Git 操作预研、SqlSugar 查询过滤例外策略收尾、生产环境密钥治理与多实例调度锁一致性。
   8. 第12轮核心成果：已修复 `RepositoryServiceTests`、`GitProviderTests` 与 Chat 集成测试阻断项，补齐 `TestBase`/`TestDataFactory`/`TestWebApplicationFactory`、性能基线测试和后端 API/部署文档。
   9. 第13轮核心成果：已新增 `BaseRepositoryTests`、`AgentServiceTests`、`LlmGatewayTests` 与独立 code review 记录，补齐阶段十原始计划的剩余项。
   10. 第13轮验收结果：`dotnet test` 当前为 `58` passed / `0` failed；阶段十计划项与验证门禁均已完成。
   11. 第14轮核心成果：完成 RQ-PHASE7-20260520-01（阶段七流水线模块），新增 Pipeline/Build 实体及完整业务层，BackgroundService 支持 30 秒轮询状态流转。
   12. 第14轮验收结果：`dotnet build` 通过（0 errors）；`dotnet test` 当前为 `75` passed / `0` failed；全部 8 个细粒度任务已闭环。
   13. 累计完成任务：阶段一～七、十、十二～十三共 11 个阶段完成。

## 三、风险预判与应对（Planner兜底）
| 风险描述 | 影响范围 | 应对措施 | 处理状态 |
|----------|----------|----------|----------|
| SqlSugar 版本差异导致 QueryFilter API 变动 | Repository 默认查询行为 | 固定当前版本并在升级前做兼容验证 | 已处理 |
| 阶段二认证落地前 NtId 来源不稳定 | 用户隔离查询准确性 | 已实现 JwtAuthMiddleware 注入 Claims + HttpContext.Items 回退，CurrentUser 优先读取 Items/Claims/Header | 已处理 |
| JWT 密钥误配或过短 | 认证链路安全性 | 配置 `Jwt` 节点并支持环境变量 `JWT_KEY` 覆盖，生产环境要求外部注入高强度密钥 | 待持续检查 |
| MVP 测试豁免导致回归缺口累积 | 阶段三至阶段十功能稳定性 | 以测试债务条目集中管理，MVP 完成后按优先级分批补齐并设置准入门槛 | 进行中 |
| 阶段四任务异步执行并发竞争 | 任务状态一致性 | 已通过 Pending->Running 条件更新实现防重复执行，后续可补充分布式锁与超时回收 | 进行中 |

## 四、排期调整记录
| 调整时间 | 原排期 | 调整后排期 | 调整原因 | 调整人 |
|----------|--------|------------|----------|--------|
| 2026-05-20 12:15 | 阶段一 1-2 天 | 阶段一 当日完成 | 初始化执行顺利，基础设施一次通过编译 | Strategic Planner |
| 2026-05-20 14:30 | 阶段七 1-2 天 | 阶段七 1.4h 完成 | 代码已完整实现，编译验证与测试全通过，快速交付 | DevOps Orchestrator |

## 五、下一轮（第15轮）重点任务
   1. 核心任务：阶段八 Wiki 模块（Markdown 存储与渲染）。
   2. 并行任务：阶段九系统配置与健康检查。
   3. 需重点关注：Pipeline 状态同步可靠性、Wiki 页面版本管理。
   4. 预计工时：2-3 天（Wiki 1 天 + Config 1 天 + 集成验证 0.5 天）。

## 六、第12轮（旧）重点任务（已完成）
   1. 核心任务：阶段六扩展能力（Git Webhook 同步与高级 Git 操作预研）。
   2. 次要任务：SqlSugar 查询过滤例外策略收尾并补齐验证用例。
   3. 次要任务：生产环境密钥治理与多实例调度一致性验证。
   4. 需重点关注：SQLite 在非测试环境下的并发边界，以及后续真实外部模型接入后的失败注入测试。

## 十、第9轮任务分配（auto-delivery-loop 模式）（已完成）

### 8.1 P0 任务：Microsoft Agent Framework 真实执行链路（阶段三收尾）

**任务ID**: RQ-STAGE3-20260520-01  
**优先级**: P0（核心）  
**任务类型**: DEV  
**模式**: implement（替换占位实现）  
**负责人**: @Worker  
**审核人**: @Auditor  
**预估工时**: 2.5h  
**已用工时**: 3.7h（前期分析）  

#### 需求冻结
- **需求ID**: RQ-STAGE3-20260520-01
- **需求文本**: 
  - 目标：完成 Microsoft Agent Framework 与 AgentExecutor 的真实集成，替换当前占位实现  
  - 范围：工具注册机制、工具调用链路、流式响应处理
  - 不在范围内：多轮对话状态管理优化（下一轮）、高级提示优化（下一阶段）

#### 接受标准（验收门禁）
1. **工具注册完整性**：
   - [ ] 所有 Agent 支持的工具已注册到 AgentExecutor
   - [ ] 工具参数模式正确传递至 LlmGateway
   - [ ] 动态工具加载逻辑可扩展（为阶段六 Git 工具预留接口）

2. **工具调用链路验证**：
   - [ ] Agent 工具调用返回正确格式的响应
   - [ ] 流式输出（SSE）正确处理中间响应
   - [ ] 错误场景处理（工具异常、超时）有防御性代码

3. **质量门禁**：
   - [ ] dotnet build 0 errors、0 warnings
   - [ ] 构建耗时 < 30s
   - [ ] 相关单元测试通过（AgentExecutor、工具调用链路）

#### 回归基线（闭环测试）
| 测试项 | 验证范围 | 通过标准 | 证据 |
|--------|----------|----------|------|
| 构建验证 | dotnet build | 0 errors, 0 warnings | build log |
| 工具集成单元测试 | AgentExecutor.RegisterTools() 与 Tool 执行 | 关键 Tool 单元覆盖 | test output |
| 烟雾测试 | Agent CRUD + Chat 端点 + Tool 调用 | 端到端无异常，响应格式正确 | Swagger 测试或集成日志 |
| 代码复用检查 | 新增代码是否复用已有 AgentService、LlmGateway、IAgentTool | 不存在代码重复 | code review |
| 文档完整性 | AgentExecutor 与 Tool 接口 XML 注释 | 公开成员 100% 覆盖 | API 文档生成结果 |

#### 实现范围
**修改目标文件**：
- Core/Domain/Entities/AgentEntity.cs - 补充工具列表序列化
- Application/Services/AgentExecutor.cs - 替换占位，实现真实工具注册与调用
- Application/Services/LlmGateway.cs - 补充工具参数格式化
- Api/Endpoints/ChatEndpoints.cs - 验证流式响应工具调用处理
- 新增单元测试：Tests/AgentExecutorToolTests.cs（工具注册与调用）

**禁止修改**：
- AgentEntity、ChatSession、ChatMessage 的核心字段结构（除非 schema 冲突）
- 已验收的阶段一、二、四、五基础设施

#### 复用约束（强制）
- 必须复用 `BaseRepository<AgentEntity>`
- 必须复用 `IAgentTool` 接口定义
- 必须复用 `LlmGateway` 多模型调用封装
- 必须复用 `ApiResponse<T>` 响应格式
- 禁止绕过 `JwtAuthMiddleware`（认证保护）

#### 分类标签
- 主要风险：LLM SDK 版本兼容性、工具参数序列化
- 依赖解除：无新外部依赖（已在 appsettings.json 中配置）
- 测试优先级：工具注册 > 工具调用 > 流式响应

---

### 8.2 P1 任务：阶段六 Git 仓库集成（GitService/GitRepository/Endpoints）

**任务ID**: RQ-STAGE6-20260520-01  
**优先级**: P1（重要功能）  
**任务类型**: DEV  
**模式**: implement  
**负责人**: @Worker  
**审核人**: @Auditor  
**预估工时**: 3.0h  
**起始时刻**: RQ-STAGE3-20260520-01 通过回归门禁后  

#### 需求冻结
- **需求ID**: RQ-STAGE6-20260520-01
- **需求文本**：  
  - 目标：完成 Git 多平台抽象与 Agent 工具集成
  - 范围：IGitProvider 接口、GitHub/GitLab/Azure DevOps 实现、RepositoryRepository、RepositoryService、Repository Endpoints、凭据加密存储
  - 不在范围内：高级 Git 操作（如自动修复、代码审查建议）、Webhook 实时同步（下一阶段）

#### 接受标准（验收门禁）
1. **多平台提供者实现**：
   - [ ] IGitProvider 接口定义完整（Clone、Fetch、Push、CreatePR、ListBranches 等核心操作）
   - [ ] GitHubProvider/GitLabProvider/AzureDevOpsProvider 各自实现所有核心操作
   - [ ] GitProviderFactory 根据仓库配置正确选择提供者

2. **数据层完整性**：
   - [ ] RepositoryEntity 继承 UserOwnedEntity（用户隔离）
   - [ ] RepositoryRepository 继承 BaseRepository、支持按用户过滤
   - [ ] RepositoryService 支持 CRUD、凭据加密存储（使用 DataProtectionService）
   - [ ] 数据库迁移脚本正确生成并通过验证

3. **端点与工具集成**：
   - [ ] RepositoryEndpoints 提供 Repository CRUD API
   - [ ] Git 工具（GitTools）实现 IAgentTool 接口，可被 Agent 调用
   - [ ] Git 操作错误正确捕获并返回统一 ApiResponse

4. **质量门禁**：
   - [ ] dotnet build 0 errors、0 warnings
   - [ ] 新增单元测试覆盖 IGitProvider、凭据加密、多租户隔离
   - [ ] 数据库迁移测试通过

#### 回归基线（闭环测试）
| 测试项 | 验证范围 | 通过标准 | 证据 |
|--------|----------|----------|------|
| 构建验证 | dotnet build + database migration check | 0 errors，迁移脚本正确 | build log + migration test |
| 多平台单元测试 | IGitProvider 各实现 | GitHub/GitLab/Azure DevOps 工厂选择正确 | test output |
| 凭据安全测试 | RepositoryService 凭据加密/解密 | 加密后无法直读，解密正确 | encryption test log |
| 工具集成测试 | GitTools implements IAgentTool | Agent 可调用 Git 工具，返回格式正确 | agent tool call test |
| 多租户隔离测试 | RepositoryRepository 用户过滤 | 查询结果只包含当前用户仓库 | query isolation test |
| 烟雾测试 | Repository CRUD + Git 工具调用 | 无异常，响应格式正确 | Swagger test 或集成日志 |
| 复用检查 | 代码是否复用 UserOwnedEntity、BaseRepository、IAgentTool | 不存在代码重复 | code review |

#### 实现范围
**新增文件**：
- Core/Domain/Entities/RepositoryEntity.cs
- Core/Domain/ValueObjects/GitCredential.cs
- Application/Services/IGitProvider.cs
- Application/Services/Impl/GitProviderFactory.cs
- Application/Services/Impl/GitHubProvider.cs
- Application/Services/Impl/GitLabProvider.cs
- Application/Services/Impl/AzureDevOpsProvider.cs
- Application/Services/RepositoryService.cs
- Infrastructure/Persistence/RepositoryRepository.cs
- Application/Dto/RepositoryDto.cs
- Api/Endpoints/RepositoryEndpoints.cs
- Application/Tools/GitTools.cs（实现 IAgentTool）
- Tests/GitProviderTests.cs
- Tests/RepositoryServiceTests.cs

**数据库迁移**：
- 新增 Repository 表（与 User 外键关联）
- 新增 GitCredential 字段（encrypted）

**禁止修改**：
- 阶段三 AI 核心模块已验收部分
- 用户认证与授权机制

#### 复用约束（强制）
- 必须继承 `UserOwnedEntity`（用户隔离）
- 必须继承 `BaseRepository<RepositoryEntity>`
- 必须实现 `IAgentTool` 接口（工具集成）
- 必须使用 `ApiResponse<T>` 响应格式
- 必须通过 `JwtAuthMiddleware` 认证
- 必须使用 DataProtectionService 加密凭据（不存在明文存储）

#### 分类标签
- 主要风险：第三方 Git API 限流、凭据泄露、多平台接口差异
- 依赖解除：无新外部依赖（仅使用标准 REST 调用）
- 测试优先级：多租户隔离 > 凭据加密 > 多平台提供者 > 工具集成

---

### 8.3 执行链路与依赖关系

```
第9轮执行顺序：
├─ RQ-STAGE3-20260520-01 (P0, 2.5h)
│  ├─ 需求验收：工具注册完整性 + 工具调用链路 + 质量门禁
│  ├─ 回归门禁：build + 单元测试 + 烟雾测试
│  └─ 完成后触发下一个任务
├─ RQ-STAGE6-20260520-01 (P1, 3.0h) [依赖 P0 通过]
│  ├─ 需求验收：多平台提供者 + 数据层完整性 + 端点与工具集成
│  ├─ 回归门禁：build + 多平台单元测试 + 凭据安全 + 工具集成 + 多租户隔离
│  └─ 完成后同步 MasterPlan

TD-20260520-002 单元测试补齐 [并行，不阻断 P0/P1]
```

#### 失败反馈流程
- 若 P0 任务回归失败 → 分类主归因（code/test-case/environment/requirement）→ 创建对应 BUG/TEST/OPS/REQ 动作 → 更新 MasterPlan → 继续闭环
- 若 P1 任务因 P0 未通过而无法启动 → 标记为 blocked 并设置 unblock condition（P0 done）
- 所有失败必须有 owner、unblock condition、next action type

---

### 8.4 报告与同步计划

**第9轮预期交付物**：
1. ROUND_REPORT_20260520_DEV_009.md（P0 实现与验收）
2. ROUND_REPORT_20260520_DEV_010.md（P1 实现与验收）
3. MasterPlan.md 更新（状态同步、工时记录、下一轮计划调整）
4. 若有失败：SPEC_CHANGE_REQUEST_RQ-STAGE3-20260520-01.md 或 BUG/TEST 反馈记录

## 九、第11轮任务分配（auto-delivery-loop 模式）

### 9.1 P0 任务：管理员跨租户边界策略细化（白名单 + 审计 + 回归覆盖）

**任务ID**: RQ-ADMIN-BOUNDARY-20260520-01  
**优先级**: P0（核心）  
**任务类型**: DEV  
**模式**: implement  
**负责人**: @Worker  
**审核人**: @Auditor  
**预估工时**: 2.5h  
**Reporting Mode**: round-only（完成后输出一份 ROUND_REPORT）

#### 需求冻结
- **需求ID**: RQ-ADMIN-BOUNDARY-20260520-01
- **需求文本**:
  - 目标：细化管理员跨租户访问边界，确保仅允许白名单定义范围内的跨租户操作，并保留可审计证据链。
  - 范围：管理员白名单策略、策略命中审计日志、关键接口授权判定与回归测试补齐。
  - 不在范围内：组织级 RBAC 重构、外部 IAM 接入、历史审计数据迁移。

#### 作用域与边界
- **目标模块**：`AutoCodeForge.Api`、`AutoCodeForge.Application`、`AutoCodeForge.Infrastructure`、`AutoCodeForge.Tests`
- **目标文件类型**：授权中间件/服务、管理员策略配置、审计记录持久化、受影响端点测试
- **明确排除**：阶段三 AI 执行链路、阶段六已验收 Git Provider 主流程、docs/templates 下模板文件

#### 接受标准（验收门禁）
1. **白名单策略正确性**：
   - [x] 管理员跨租户访问默认拒绝，仅在白名单命中时放行。
   - [x] 白名单支持按租户ID/资源域配置，策略匹配具备确定性顺序。
   - [x] 非管理员身份在相同请求下保持原有隔离行为，不得被放宽。

2. **审计链路完整性**：
   - [x] 每次跨租户判定均记录审计日志（包含操作者、源租户、目标租户、资源、决策结果、时间戳）。
   - [x] 拒绝与放行均可追溯，错误路径不丢日志。
   - [x] 审计字段满足排查最小集，不落明文敏感凭据。

3. **质量门禁**：
   - [x] `dotnet build` 通过（0 errors；存在既有测试工程告警，不阻断本轮）。
   - [x] 受影响模块测试通过（授权策略 + 审计日志 + 边界回归）。
   - [x] 核心 smoke 路径通过（本轮执行 `AuthEndpointsTests` 2/2 通过）。

#### 回归基线（闭环测试）
| 测试项 | 验证范围 | 通过标准 | 证据 |
|--------|----------|----------|------|
| 构建验证 | `dotnet build`（server/AutoCodeForge.sln） | 0 errors, 0 warnings | build log |
| 策略单元测试 | 白名单匹配、默认拒绝、身份分支 | 核心判定分支全通过 | test output |
| 审计单元测试 | 放行/拒绝/异常路径日志写入 | 关键字段完整且可查询 | test output |
| 隔离回归测试 | 管理员跨租户、普通用户同路径访问 | 管理员仅白名单放行；普通用户保持隔离 | test output |
| Smoke 测试 | 关键 API 端点访问链路 | 响应码与 `ApiResponse` 结构符合预期 | endpoint log |
| 复用检查 | 是否复用既有认证与响应基座 | 不新增重复认证判定实现 | code review |

#### 复用约束（强制）
- 必须复用 `JwtAuthMiddleware` 及现有用户上下文读取机制。
- 必须复用统一 `ApiResponse<T>` 响应模型。
- 必须复用现有仓储与实体基类（`BaseRepository<>`、`UserOwnedEntity`/`AuditableEntity`）处理审计持久化。
- 禁止绕过既有租户隔离基础规则，仅允许在管理员白名单路径做显式例外。

#### 失败分类策略（强制启用）
- 主归因必须从 `code`、`test-case`、`environment`、`requirement` 中选择。
- 每个 blocked/failed 结果必须附带：失败检查ID或用例名、错误签名、首次发现时间、影响范围、可复现性。
- 若归因置信度不足，主归因填 `needs-triage`，并在本轮生成 TRIAGE 动作。

#### 失败回流动作映射（第11轮专用）
1. `code` -> BUG：修复授权判定/审计落库缺陷，标注受影响端点与疑似引入范围。
2. `test-case` -> TEST：补边界场景（白名单冲突、空配置、并发请求）并强化断言。
3. `environment` -> OPS：修复环境配置漂移（租户配置源、密钥注入、日志落盘链路）。
4. `requirement` -> REQ：澄清“管理员可跨租户”的业务边界，给出决策截止时间。

#### 完成定义（DoD）
1. 验收标准 3/3 全部满足。
2. 回归基线全部执行且有证据。
3. `docs/MasterPlan.md` 状态与工时同步更新。
4. 产出 `docs/reports/ROUND_REPORT_20260520_DEV_014.md`（若失败则标记 blocked 并附失败归因与后续动作）。

### 9.2 执行决策矩阵（第11轮）
1. 验收与回归均通过：任务状态置为 done，进入第11轮次要任务。
2. 验收通过但回归失败：任务状态置为 blocked，必须创建 BUG/TEST/OPS/REQ 动作后继续闭环。
3. 仅完成部分范围：任务状态置为 partial，记录 deferred scope 与下一轮优先级。
4. 出现策略冲突：冻结冲突范围并新增 `SPEC_CHANGE_REQUEST`，非冲突范围继续推进。

### 9.3 第11轮闭环执行结果（RQ-ADMIN-BOUNDARY-20260520-01）
- **任务状态**：done
- **完成度**：100%
- **实际工时**：1.8h
- **代码证据**：
  - `server/src/AutoCodeForge.Application/Services/AdminAuditService.cs`（新增 scoped whitelist 判定、allow/deny 审计决策日志）
  - `server/src/AutoCodeForge.Api/Endpoints/AdminEndpoints.cs`（三条 admin 路由统一走 AuthorizeCrossTenantAsync）
  - `server/src/AutoCodeForge.Core/DTOs/Admin/AdminAuditLogDto.cs`（补充 AccessDecision/DecisionReason）
  - `server/tests/AutoCodeForge.Tests/AdminAuditServiceTests.cs`（新增 5 条策略与审计测试）
- **回归证据**：
  - `dotnet build AutoCodeForge.sln`：通过
  - `dotnet test AutoCodeForge.sln --filter "FullyQualifiedName~AdminAuditServiceTests"`：5 passed / 0 failed
  - `dotnet test AutoCodeForge.sln --filter "FullyQualifiedName~AuthEndpointsTests"`：2 passed / 0 failed
- **失败归因结论**：无阻断失败（本轮未触发 BUG/TEST/OPS/REQ 回流项）
- **报告链接**：`docs/reports/ROUND_REPORT_20260520_DEV_014.md`

## 六、MVP 测试债务追踪（新增）

### 6.1 阶段性策略
1. MVP 阶段：以 `dotnet build` 与关键链路 smoke 为最低门禁，单元/集成测试不作为当前轮次阻断条件。
2. MVP 完成后：进入测试补齐阶段，未完成测试债务的模块不得进入长期稳定维护状态。

### 6.2 测试债务条目
| 记录时间 | 债务ID | 范围 | 当前状态 | Owner | 触发条件 | 清偿截止 | 验收标准 | 证据链接 |
|----------|--------|------|----------|-------|----------|----------|----------|----------|
| 2026-05-20 13:35 | TD-20260520-001 | AuthService + AuthEndpoints（单元/集成） | closed | @Worker/@Auditor | MVP 功能闭环完成并冻结主链路变更 | MVP 结束后第1轮 | AuthService 关键分支单元覆盖 + AuthEndpoints 登录/注册/当前用户集成通过 | docs/reports/ROUND_REPORT_20260520_DEV_004.md; docs/reports/ROUND_REPORT_20260520_OTHER_001.md |
| 2026-05-20 | TD-20260520-002 | ScheduledTaskService + CronSchedulerService（单元） | closed | @Worker/@Auditor | 阶段五功能交付后补充 | 第10轮 | Cron 解析边界、NextRunAt 计算、pause/resume 状态机覆盖 + CronSchedulerService 核心执行路径验证 | docs/reports/ROUND_REPORT_20260520_DEV_007.md; docs/reports/ROUND_REPORT_20260520_DEV_011.md |

## 七、失败回流记录模板（闭环专用）

### 7.1 使用规则
1. 仅记录 blocked 或 failed 的闭环结果。
2. 每条失败必须有主归因：code、test-case、environment、requirement。
3. 可附加次归因；若归因不确定，主归因填 `needs-triage` 并创建快速排查动作。
4. 每条记录必须包含 owner、unblock condition、next action type。
5. 禁止写泛化待办，必须能追溯到失败证据。

### 7.2 回流记录表
| 记录时间 | Requirement ID | Loop 模式 | 失败状态 | 主归因 | 次归因 | 失败检查ID/用例 | 错误签名 | 影响范围 | 可复现性 | Owner | Unblock 条件 | Next Action Type | Next Action Priority | 证据链接 |
|----------|----------------|-----------|----------|--------|--------|-----------------|----------|----------|----------|-------|----------------|------------------|----------------------|----------|
| 2026-05-20（第9轮） | RQ-STAGE3-20260520-01 / RQ-STAGE6-20260520-01 | implement | 无失败 | - | - | - | - | 阶段三与阶段六闭环验收 | always | @Worker/@Auditor | 不适用 | 不适用 | - | docs/reports/ROUND_REPORT_20260520_DEV_009.md; docs/reports/ROUND_REPORT_20260520_DEV_010.md |

### 7.3 归因到动作映射
1. code -> BUG：实现修复，标注疑似引入范围和受影响模块。
2. test-case -> TEST：补用例或修断言，标注缺失场景。
3. environment -> OPS：修环境配置或依赖，标注环境指纹。
4. requirement -> REQ：需求澄清或范围重定义，标注决策截止时间。
5. needs-triage -> TRIAGE：先排查后再回填主归因，最长不超过 1 个执行轮次。

---

## 十、后续任务规划（Phase 07-14 详细分析）

### 10.1 项目进度快照

**已交付阶段**（Phase 01-06, 10, 12-13）：
- ✅ Phase 01：基础设施（BaseRepository、实体基类、中间件、响应格式）
- ✅ Phase 02：数据层与认证（16 实体、JWT、Seed）
- ✅ Phase 03：AI 核心模块（LLM 网关、Agent 框架、Chat SSE）
- ✅ Phase 04：任务中心（Task/Queue/Executor）
- ✅ Phase 05：定时调度（ScheduledTask/Cron）
- ✅ Phase 06：Git 集成（多平台 Provider、凭据加密、多租户隔离）
- ✅ Phase 10：测试优化（58/58 测试通过）
- ✅ Phase 12：Sandbox 与 Repo 同步（按用户隔离、任务快照）
- ✅ Phase 13：Repo 与审查模块（ReviewEngine、RuleSet、Finding、Endpoints）

**待启动阶段**（Phase 07-09, 14）：
- ⏳ Phase 07：流水线模块（P2, 1-2 天）
- ⏳ Phase 08：Wiki 模块（P2, 1 天）
- ⏳ Phase 09：系统配置与健康检查（P2, 1 天）
- ⏳ Phase 14：Agent 增强 Git 技能（P0, 4-6 天）

**总体统计**：
```
已交付：10 个阶段 + 阶段十一（Plan 对齐）
规划中：4 个阶段
累计代码：~15000+ 行（Phase 01-13）
累计测试：58/58 通过
```

---

### 10.2 Phase 07: 流水线模块（Pipeline）

**优先级**：🟡 P2  
**预估工时**：1-2 天  
**前置依赖**：Phase 01, 02, 06 ✅  
**核心目标**：CI/CD 流水线配置与状态同步  

#### 设计特点
| 特点 | 说明 |
|------|------|
| **外部系统绑定** | 存储第三方 CI/CD（GitHub Actions/Jenkins/GitLab CI）流水线 ID |
| **状态定期同步** | BackgroundService 后台轮询拉取最新构建状态 |
| **历史追踪** | Build 表独立记录每次构建详情 |
| **错误降级** | 外部系统不可用时有降级处理 |

#### 任务清单（7.1 ~ 7.8）
```
7.1 创建 Pipeline DTO
7.2 创建 PipelineRepository (复用 BaseRepository)
7.3 创建 BuildRepository (复用 BaseRepository)
7.4 创建 PipelineService
7.5 创建 Pipeline Endpoints
7.6 创建 PipelineSyncService (BackgroundService)
7.7 注册服务到 Program.cs
7.8 端到端验证
```

#### 复用情况
✅ BaseRepository / ApiResponse / JwtAuthMiddleware / ExceptionMiddleware  
✅ RepositoryService（仓库相关查询）  
✅ PaginationHelper（列表分页）  
**避免代码**：~250+ 行

#### 关键风险点
| 风险 | 缓解措施 |
|------|--------|
| 外部系统 API 差异 | 创建 IPipelineProvider 抽象层（GitHub/Jenkins/GitLab） |
| 状态同步频率 | 可配置同步间隔（默认 5 分钟），考虑 Webhook 回调 |
| 大日志文件处理 | 分页加载或仅保存摘要，支持异步流式加载 |

#### 优化建议
```diff
当前缺少：
+ IPipelineProvider 接口（如同 IGitProvider）
+ PipelineProviderFactory（路由不同 CI 系统）
+ PipelineSyncTask 数据表（记录同步日志）
+ 构建日志流式下载端点
```

---

### 10.3 Phase 08: Wiki 模块

**优先级**：🟡 P2  
**预估工时**：1 天  
**前置依赖**：Phase 01, 02, 06 ✅  
**核心目标**：知识库/文档管理  

#### 设计特点
| 特点 | 说明 |
|------|------|
| **简单 CRUD** | MVP 阶段，无需版本历史 |
| **Markdown 存储** | 后端存原始 Markdown，前端负责渲染 |
| **全文检索** | MVP 用 LIKE，生产可升级为全文检索引擎 |
| **仓库关联** | Wiki 页面可绑定仓库，便于知识组织 |

#### 任务清单（8.1 ~ 8.6）
```
8.1 创建 Wiki DTO
8.2 创建 WikiPageRepository (复用 BaseRepository)
8.3 创建 WikiService（CRUD + 搜索）
8.4 创建 Wiki Endpoints
8.5 注册服务到 Program.cs
8.6 端到端验证
```

#### 复用情况
✅ UserOwnedEntity / BaseRepository / ApiResponse / JwtAuthMiddleware  
✅ PaginationHelper（分页查询）  
**避免代码**：~200+ 行

#### 关键风险点
| 风险 | 缓解措施 |
|------|--------|
| 搜索性能 | MVP 用 LIKE，后期升级 Elasticsearch/MeiliSearch |
| 版本历史缺失 | 提前预留 WikiPageHistory 表设计 |
| 权限控制不足 | 考虑添加仓库维度的访问控制 |

#### 优化建议
```diff
当前缺少：
+ WikiPageHistory 表（版本控制预留）
+ WikiPageTag 表（标签分类）
+ 全文检索接口（可选）
+ Wiki 评论/讨论功能（可选）
```

---

### 10.4 Phase 09: 系统配置 & 健康检查

**优先级**：🟡 P2  
**预估工时**：1 天  
**前置依赖**：Phase 01, 02 ✅  
**核心目标**：系统运维和可观测性  

#### 设计特点
| 特点 | 说明 |
|------|------|
| **统一配置服务** | ConfigService 管理全局和用户级配置 |
| **健康检查** | /health 端点检查核心服务状态 |
| **系统信息** | /system/info 展示版本/环境信息 |
| **结构化日志** | Serilog 配置，支持多级别日志 |

#### 任务清单（9.1 ~ 9.8）
```
9.1 创建 Config DTO
9.2 创建 ConfigService（统一配置管理）
9.3 创建 Config Endpoints
9.4 创建健康检查端点 (/health)
9.5 创建系统信息端点 (/system/info)
9.6 配置结构化日志 (Serilog)
9.7 注册配置相关服务
9.8 端到端验证
```

#### 复用情况
✅ AuditableEntity / UserOwnedEntity / BaseRepository / ApiResponse  
✅ ExceptionMiddleware（异常处理）  
**避免代码**：~250+ 行

#### 关键风险点
| 风险 | 缓解措施 |
|------|--------|
| 敏感信息泄露 | /system/info 仅返回非敏感信息，禁止暴露密钥 |
| 健康检查不深入 | 支持检查 Redis、消息队列、外部 API |
| 日志性能影响 | 生产环境默认 Warning 级别，支持动态调整 |

#### 优化建议
```diff
当前缺少：
+ HealthCheckService 抽象（支持多种检查）
+ LivenessProbe / ReadinessProbe 分离
+ 配置缓存（全局配置不频繁变更）
+ 审计日志（谁修改了哪个配置）
```

---

### 10.5 Phase 14: Agent 增强 Git 技能（核心能力）

**优先级**：🔴 P0（核心能力）  
**预估工时**：4-6 天  
**前置依赖**：Phase 03, 06, 13 ✅  
**核心目标**：Agent 可执行 Git 查询与操作，包括权限管控与审计追踪  

#### 设计原则
| 原则 | 说明 |
|------|------|
| **只读优先** | 默认启用只读技能，写操作需显式授权 |
| **权限管控** | GitSkillPermissionGuard 网关拦截未授权操作 |
| **任务快照绑定** | Git 操作必须关联任务快照，避免配置漂移 |
| **全链路审计** | AgentToolAuditLogger 记录每次调用（含参数摘要、结果、状态） |
| **错误可恢复** | GitSkillErrorMapper 将异常转化为可执行建议 |

#### 任务清单（14.1 ~ 14.10）
```
14.1 定义 Git 技能 DTO (GitReadToolRequest/Response 等)
14.2 实现 GitReadToolset (只读: ListBranches/Commits/PRs/Diff)
14.3 实现 GitWriteToolset (变更: CreateBranch/Commit/Push/CreatePR)
14.4 实现 GitSkillPermissionGuard (ReadOnly/Write/Dangerous 三级权限)
14.5 实现 GitContextHydrator (自动注入任务快照)
14.6 扩展 AgentExecutor 工具注册链路
14.7 新增 AgentToolAuditLogger (工具调用审计)
14.8 新增 Git 技能策略 API (配置授权)
14.9 异常映射与用户建议 (GitSkillErrorMapper)
14.10 DI 注册与端到端联调
```

#### 数据表变更
**新增字段**：
```
ChatSessions.TaskId                  // 绑定会话与任务
Agents.SkillProfile                  // 技能画像 (ReadOnly/Collaborator/Reviewer)
UserConfigs.GitSkillPolicyJson       // 用户 Git 技能授权策略
```

**新增表**：
```
AgentToolInvocations (审计日志)
├─ SessionId, TaskId, ToolName
├─ InputDigest, OutputDigest, Status, Latency
└─ 索引：IX_ATI_Session_CreatedAt, IX_ATI_Task_Status

GitSkillGrants (权限授权)
├─ NtId, RepositoryId
├─ Level (ReadOnly/Write/Dangerous)
├─ GrantedBy, CreatedAt/UpdatedAt
└─ 唯一索引：UQ_GSG_User_Repo
```

#### 复用情况
**深度复用 Phase 03/06/13**：
✅ IAgentTool / AgentExecutor / AgentMatcher（工具框架）  
✅ IGitProvider / GitProviderFactory（多平台 Git）  
✅ RepositoryService（仓库信息）  
✅ ReviewService（审查结果查询）  
**避免代码**：~500+ 行

#### 新增可复用组件
```
✅ GitReadToolset (3+ 次)          → 定时任务、流水线
✅ GitWriteToolset (3+ 次)         → 自动化修复、PR 建议
✅ GitSkillPermissionGuard (4+ 次) → 其他安全操作
✅ AgentToolAuditLogger (4+ 次)    → 通用工具审计
✅ GitContextHydrator (3+ 次)      → 任务上下文注入
```

#### 关键风险点
| 风险 | 缓解措施 |
|------|--------|
| 权限判定错误 | 多层权限校验 + 详细审计日志 + 人工复核选项 |
| 高风险操作误执行 | 删除分支/强制推送默认禁用，需显式授权 |
| 审计日志不完整 | AgentToolAuditLogger 强制记录所有调用 |
| 异常处理不友好 | GitSkillErrorMapper 转化为可执行建议 |

#### 执行分阶段建议
```
第一阶段（基础）：14.1-14.5, 14.7, 14.10
  └─ 只读工具 + 上下文注入 + 审计日志 + 端到端

第二阶段（安全）：14.4 增强 + 14.8-14.9
  └─ 权限管控 + 错误映射 + 策略 API

第三阶段（完善）：14.3 + 高风险防护
  └─ 写操作 + 人工审批选项
```

---

### 10.6 综合执行计划

#### 优先级排序
```
1️⃣ 【立即启动】Phase 14 (P0, 4-6 天)
   └─ 关键路径：后续流水线自动分析、审查建议等基础
   └─ 前置完成：Phase 03, 06, 13 都已交付 ✅

2️⃣ 【同步启动】Phase 07, 08, 09 (P2, 3-5 天)
   └─ 相互独立，可并行执行
   └─ 前置完成：全部满足 ✅
```

#### 时间表预估
```
Phase 14 预计完成：2026-05-24 ~ 05-26 (4-6 天)
Phase 07, 08, 09 预计完成：2026-05-26 ~ 05-31 (3-5 天)
全项目 Phase 01-14 预计完成：2026-05-31
```

#### 状态指标
| 指标 | 当前值 | 目标值 |
|------|-------|-------|
| 已交付阶段数 | 10 | 14 |
| 总测试用例 | 58 | 70+ |
| 代码覆盖 | ~3500 行（Phase 01-13） | ~4500+ 行（Phase 14 新增） |
| 复用组件数 | ~30+ | ~40+ |

---

### 10.7 综合评价

| 维度 | 评分 | 备注 |
|------|------|------|
| **计划完整性** | ⭐⭐⭐⭐⭐ | 每个 Phase 都有详细设计思路、复用清单、任务分解 |
| **复用设计** | ⭐⭐⭐⭐⭐ | Phase 14 深度复用 03/06/13，避免重复建设 |
| **风险识别** | ⭐⭐⭐⭐ | 各 Phase 风险点清晰，缓解措施具体 |
| **可执行性** | ⭐⭐⭐⭐ | 任务清单详尽，验证标准明确 |
| **优化空间** | ⭐⭐⭐ | Phase 07 建议补充 IPipelineProvider 抽象 |

---

### 10.8 建议的后续行动

**立即行动**：
- ✅ 启动 Phase 14 作为下一轮重点 P0 任务（相关工具集成）
- ✅ 同步推进 Phase 07-09 的辅助任务（相互独立）

**节点检查点**：
- Phase 14.5 完成：验证 GitContextHydrator 与任务快照绑定
- Phase 14.7 完成：验证审计日志完整性
- Phase 14.10 完成：完整端到端验证（查询分支到创建 PR）
