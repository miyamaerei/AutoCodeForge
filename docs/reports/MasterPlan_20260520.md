# MASTER_PLAN_20260520.md

## 基本信息
- 项目名称：AutoCodeForge
- 当前执行轮次：第4轮
- 最新更新时间：2026年05月20日 18:30
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

## 二、实时进度汇总
1. 本轮（第4轮）核心成果：完成阶段三首版后端闭环，新增 LlmGateway/AgentExecutor/ChatSessionManager/AgentMatcher、AgentService/ChatService、Agent/Chat/SSE 端点与相关 DTO/Repository。
2. 累计完成任务：P0 新增阶段三首版 90%（阶段一、二已 100%）；P1 2/2；P2 1/1；P3 0.8/1。
3. 未完成任务：管理员跨租户边界策略细化；Microsoft Agent Framework 真实执行链路替换当前占位实现。
4. 工时平衡情况：本轮新增投入约 3.7h，阶段三已完成可运行骨架并进入真实模型能力收口阶段。
5. 补充验证结果：2026-05-20 执行 dotnet build 与 dotnet test 均通过，结果为 9 passed / 0 failed；Microsoft.Agents.AI 包已成功接入并通过 restore/build。

## 三、风险预判与应对（Planner兜底）
| 风险描述 | 影响范围 | 应对措施 | 处理状态 |
|----------|----------|----------|----------|
| SqlSugar 版本差异导致 QueryFilter API 变动 | Repository 默认查询行为 | 固定当前版本并在升级前做兼容验证 | 已处理 |
| 阶段二认证落地前 NtId 来源不稳定 | 用户隔离查询准确性 | 已实现 JwtAuthMiddleware 注入 Claims + HttpContext.Items 回退，CurrentUser 优先读取 Items/Claims/Header | 已处理 |
| JWT 密钥误配或过短 | 认证链路安全性 | 配置 `Jwt` 节点并支持环境变量 `JWT_KEY` 覆盖，生产环境要求外部注入高强度密钥 | 待持续检查 |
| 阶段三真实 LLM 接入环境未就绪 | Chat 响应真实性与效果 | 下一轮优先完成 Microsoft Agent Framework 真实执行链路与配置校验 | 进行中 |

## 四、排期调整记录
| 调整时间 | 原排期 | 调整后排期 | 调整原因 | 调整人 |
|----------|--------|------------|----------|--------|
| 2026-05-20 12:15 | 阶段一 1-2 天 | 阶段一 当日完成 | 初始化执行顺利，基础设施一次通过编译 | Strategic Planner |

## 五、下一轮（第4轮）重点任务
1. 核心任务：实现 Microsoft Agent Framework 真实执行链路（替换占位响应，完成工具注册与调用）。
2. 次要任务：实现管理员例外查询策略（跨 NtId 查询白名单与审计日志）。
3. 需重点关注：生产环境 LLM 密钥配置安全、工具调用权限边界与 SSE 断连处理。
