# MASTER_PLAN_20260520.md

## 基本信息
- 项目名称：AutoCodeForge
- 当前执行轮次：第5轮
- 最新更新时间：2026年05月20日 10:24
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

## 二、实时进度汇总
1. 本轮（第5轮）核心成果：执行阶段二 validate-only 闭环验证，未触发重开发，重点核验验收标准与回归门禁一致性。
2. 累计完成任务：P0 3/3（阶段一+阶段二闭环完成）；P1 2/2；P2 1/1；P3 0.8/1。
3. 未完成任务：管理员跨租户边界策略细化。
4. 工时平衡情况：本轮新增投入约 0.2h，用于回归验证与计划同步。
5. 补充验证结果：2026-05-20 执行 dotnet build 与 dotnet test 均通过，结果 9 passed / 0 failed。
6. 闭环结论：阶段二需求文本与验收项已由自动化证据覆盖，当前状态可确认 closure。

## 三、风险预判与应对（Planner兜底）
| 风险描述 | 影响范围 | 应对措施 | 处理状态 |
|----------|----------|----------|----------|
| SqlSugar 版本差异导致 QueryFilter API 变动 | Repository 默认查询行为 | 固定当前版本并在升级前做兼容验证 | 已处理 |
| 阶段二认证落地前 NtId 来源不稳定 | 用户隔离查询准确性 | 已实现 JwtAuthMiddleware 注入 Claims + HttpContext.Items 回退，CurrentUser 优先读取 Items/Claims/Header | 已处理 |
| JWT 密钥误配或过短 | 认证链路安全性 | 配置 `Jwt` 节点并支持环境变量 `JWT_KEY` 覆盖，生产环境要求外部注入高强度密钥 | 待持续检查 |
| MVP 测试豁免导致回归缺口累积 | 阶段三至阶段十功能稳定性 | 以测试债务条目集中管理，MVP 完成后按优先级分批补齐并设置准入门槛 | 进行中 |

## 四、排期调整记录
| 调整时间 | 原排期 | 调整后排期 | 调整原因 | 调整人 |
|----------|--------|------------|----------|--------|
| 2026-05-20 12:15 | 阶段一 1-2 天 | 阶段一 当日完成 | 初始化执行顺利，基础设施一次通过编译 | Strategic Planner |

## 五、下一轮（第6轮）重点任务
1. 核心任务：实现管理员例外查询策略（跨 NtId 查询白名单与审计日志）。
2. 次要任务：进入阶段三 AI 核心模块的实体与服务接口初始化。
3. 需重点关注：生产环境 JWT 配置安全与种子数据仅开发环境执行策略。

## 六、MVP 测试债务追踪（新增）

### 6.1 阶段性策略
1. MVP 阶段：以 `dotnet build` 与关键链路 smoke 为最低门禁，单元/集成测试不作为当前轮次阻断条件。
2. MVP 完成后：进入测试补齐阶段，未完成测试债务的模块不得进入长期稳定维护状态。

### 6.2 测试债务条目
| 记录时间 | 债务ID | 范围 | 当前状态 | Owner | 触发条件 | 清偿截止 | 验收标准 | 证据链接 |
|----------|--------|------|----------|-------|----------|----------|----------|----------|
| 2026-05-20 13:35 | TD-20260520-001 | AuthService + AuthEndpoints（单元/集成） | closed | @Worker/@Auditor | MVP 功能闭环完成并冻结主链路变更 | MVP 结束后第1轮 | AuthService 关键分支单元覆盖 + AuthEndpoints 登录/注册/当前用户集成通过 | docs/reports/ROUND_REPORT_20260520_DEV_004.md; docs/reports/ROUND_REPORT_20260520_OTHER_001.md |

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
| YYYY-MM-DD HH:mm | RQ-XXXX | implement/validate-only | blocked/failed | code/test-case/environment/requirement/needs-triage | 可多值 | test-id/build-step | 关键报错摘要 | module/endpoint/workflow | always/intermittent/unknown | @Worker/@Auditor/OPS/PO | 明确可验证条件 | BUG/TEST/OPS/REQ/TRIAGE | P0/P1/P2/P3 | report or log path |

### 7.3 归因到动作映射
1. code -> BUG：实现修复，标注疑似引入范围和受影响模块。
2. test-case -> TEST：补用例或修断言，标注缺失场景。
3. environment -> OPS：修环境配置或依赖，标注环境指纹。
4. requirement -> REQ：需求澄清或范围重定义，标注决策截止时间。
5. needs-triage -> TRIAGE：先排查后再回填主归因，最长不超过 1 个执行轮次。
