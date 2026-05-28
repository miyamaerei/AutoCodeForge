# 阶段16：Agent 可配置 Skill 与知识库集成

**日期**: 2026-05-21
**项目**: AutoCodeForge - Agent 能力增强

---

## 概要

目标是让 Agent 在被调用时，可从配置中动态加载“skill（技能）”和“知识库（Knowledge Base）”，并把这些配置纳入调用语境（context）以影响 Agent 行为与决策。方案需支持：

- 多技能组合（chain-of-skills）、按权重/优先级选择技能
- 外部知识库接入（向量检索、本地文档、数据库）并将检索结果注入调用上下文
- 安全/权限控制、版本管理、回滚、审计
- 低延迟路径（缓存、批量检索）与 fallback 策略


## 小合同（Contract）

- 输入：Agent 调用请求（payload） + 配置（skills 列表、knowledge 指向）
- 输出：Agent 响应，其中显式包含使用的 skills、knowledge hits、trace id
- 错误模式：配置错误 -> 返回可辨识的 4xx；知识库不可用 -> 进入降级策略并记录告警
- 成功准则：在受控负载下，平均额外延迟 < 200ms（检索和注入开销），并且用户可通过配置显式改变 Agent 行为


## 关键边界情况

- 配置缺失或格式不合法
- 多个知识源冲突（版本/优先级）
- 知识库检索超时或返回空结果
- 并发高时缓存失效或热点
- 权限不足访问某些知识源/技能


## 16 个阶段总览（高层）

1. 需求澄清与用例收集
2. 配置模型设计（schema + validation）
3. Skill 插件化接口设计（SDK/契约）
4. 知识库抽象层（接口）与适配器框架
5. 向量检索与索引策略定义
6. 配置存储与版本控制（DB / GitOps）
7. 运行时加载器与热更新
8. 注入策略（prompt template / context window 管理）
9. 权限与审计（RBAC / 审计事件）
10. 缓存与性能优化（批量、并行、缓存）
11. 错误处理与降级策略
12. 测试框架（单元、集成、负载）
13. CLI / 管理界面与可视化（查看/编辑配置）
14. 灰度发布与回滚机制
15. 监控与策略迭代（SLO / A/B）
16. 文档、示例、交付与迁移指南


---

## 每个阶段的详细任务、产出和验收（按阶段展开）

### 阶段 1：需求澄清与用例收集
- 任务：召集业务、产品、后端、SRE 收集典型用例（例如：代码辅助、测试生成、PR 审查、基线知识检索）
- 产出：用例矩阵 + 优先级清单
- 验收：用例矩阵被 Stakeholder 批准
- 相关文件：`docs/plans/16-phase-sixteen-agent-skill-knowledge.md`（本文件）


### 阶段 2：配置模型设计（schema + validation）
- 任务：定义 `skill` 与 `knowledge` 的 JSON Schema（字段：id, version, priority, params, selector, allowedRoles）和全局 `agentInvocationConfig` 模型
- 产出：`src/config/agent-config.schema.json`, 类型（TypeScript）`src/config/types.ts`
- 验收：通过 schema 验证示例并生成类型（tsc 无错误）


### 阶段 3：Skill 插件化接口设计（SDK/契约）
- 任务：设计 skill 接口（load/execute/init/teardown/health），定义 lifecycle 与异步返回值契约
- 产出：`src/modules/skills/` 的接口文件与示例 skill（`code-assist`, `lint-suggest`）
- 验收：示例 skill 能在本地被加载并在单元测试中被调用


### 阶段 4：知识库抽象层与适配器框架
- 任务：实现统一的 KnowledgeSource 接口（search, fetch, embeddingsUpsert, metadata），并提供适配器：LocalFS、VectorStore（Pinecone/Weaviate兼容）、SQL
- 产出：`src/lib/knowledge/` 目录，包含适配器与 mock 实现
- 验收：通过抽象接口能无缝切换不同适配器的单元测试


### 阶段 5：向量检索与索引策略定义
- 任务：定义 embedding pipeline（文本清洗、chunk、embed）、索引策略（分片、metadata）、检索召回/重排序策略
- 产出：`docs/architecture/embeddings-indexing.md`, `src/lib/embeddings/*`
- 验收：索引并检索示例文档，召回率满足用例要求（手工评估）


### 阶段 6：配置存储与版本控制（DB / GitOps）
- 任务：确定配置持久化方式（数据库 + 可选 GitOps 同步），实现 CRUD API 与审计字段
- 产出：API: `POST /api/agent/configs`, `GET /api/agent/configs/{id}`，DB schema
- 验收：配置能被创建、回滚、有变更记录


### 阶段 7：运行时加载器与热更新
- 任务：实现运行时加载器，支持热加载/热替换 skill 与 knowledge 配置（无重启）
- 产出：`src/host/agent-loader.ts`、WebSocket/事件总线用于配置变更推送
- 验收：更新配置后，后续调用能体现变更（内置测试场景）


### 阶段 8：注入策略（prompt template / context window 管理）
- 任务：定义如何把知识检索结果注入到 prompt（short vs long context）、分段注入策略、token 预算管理
- 产出：`src/lib/prompt-injection/*`、模板示例
- 验收：在 token 预算内成功注入并命中知识点


### 阶段 9：权限与审计（RBAC / 审计事件）
- 任务：为配置操作与知识访问添加权限控制，定义审计事件与存储策略
- 产出：RBAC 映射文档、`src/stores/audit/*`
- 验收：未授权用户无法修改敏感配置，审计日志可回溯


### 阶段 10：缓存与性能优化（批量、并行、缓存）
- 任务：实现知识检索缓存（TTL）、batch retrieval、并行 skill 执行 orchestrator
- 产出：`src/lib/cache/*`, orchestrator 实现
- 验收：在压力测试下平均延迟满足 SLO


### 阶段 11：错误处理与降级策略
- 任务：定义各类错误处理（超时、部分失败、重试），实现降级（跳过知识注入、使用上次成功配置）
- 产出：`src/lib/fallbacks/*`、错误分类表
- 验收：模拟失败场景系统仍能返回合理结果并记录告警


### 阶段 12：测试框架（单元、集成、负载）
- 任务：为 skill、KB 适配器、loader、注入逻辑编写测试（vitest / node），并提供负载脚本
- 产出：`tests/agent-skill/*`，CI task
- 验收：所有主要测试通过，加载测试达到目标并记录基线


### 阶段 13：CLI / 管理界面与可视化（查看/编辑配置）
- 任务：实现管理界面（前端）与 CLI（快速查看/应用配置），显示使用 trace 与执行时间线
- 产出：`client/src/modules/agent-config-ui/*`, `tools/agent-cli`（脚本）
- 验收：管理员能通过 UI 编辑配置并灰度发布


### 阶段 14：灰度发布与回滚机制
- 任务：实现流量分配（基于用户、org 或百分比）、回滚触发器与观察点
- 产出：灰度策略引擎、回滚 API
- 验收：在试验组中成功发布并能回滚


### 阶段 15：监控与策略迭代（SLO / A/B 测试）
- 任务：把关键指标（延迟、召回率、错误率、配置命中）上报到监控平台并设 SLO
- 产出：Grafana 面板、SLO 报表、A/B 案例模板
- 验收：监控面板上线，A/B 测试能收敛


### 阶段 16：文档、示例、交付与迁移指南
- 任务：编写用户文档、API 参考、迁移指南（从旧 agent 调用到新配置化调用）以及示例仓库
- 产出：`docs/agent-config.md`, `examples/agent-skill-samples/`
- 验收：文档经内部 review 并能指导用户完成一次配置化调用


---

## 风险、依赖与替代方案

- 依赖：向量数据库（若使用第三方需签约）、embedding 服务、现有权限系统
- 风险：KB 检索延迟影响体验 -> 采用异步注入/缓存与降级
- 替代：初期仅支持本地文件与 SQL 适配器，后期逐步接入向量服务


## 估时与人员建议

- 估时（粗略）：总计约 8-12 人月
  - 设计与 PoC：1.5 人月
  - 核心实现（adapter + loader + injection）：4 人月
  - 测试/性能/部署/监控：2 人月
  - UI/文档/发布：1.5 人月

- 建议小团队：后端 2 人，前端 1 人，SRE 1 人，产品 0.5 人（兼职）


## 验收清单（验收时逐项检查）

- 配置 Schema 与 UI 已部署
- 至少两个 skill 示例可热加载并正常执行
- 至少一个外部知识适配器（LocalFS 或 VectorStore）可用并被注入
- 权限与审计在管理界面可见并可回溯
- 性能基线文档与监控面板上线
- 使用示例与迁移指南完成


## 下一步（短期）

1. 召开需求澄清会议（约 1 周内）
2. 完成阶段 2/3 的 PoC（schema + skill SDK）并评审
3. 由 SRE 评估向量数据库选型与成本


---

文档作者：AutoCodeForge 团队

