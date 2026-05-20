# 阶段十四：Agent 增强 Git 技能（可控调用）

**日期**: 2026-05-20  
**预估时间**: 4-6 天  
**优先级**: 🔴 P0 - 智能执行核心能力  
**前置依赖**: 阶段三（AI 核心模块）、阶段六（Git 集成）、阶段十三（Repo 与审查模块管理）

---

## 我是如何考虑的

### 设计思路

本阶段目标是让 Agent 具备“懂仓库、会查询、能操作、可审计”的 Git 能力：

1. 先接入只读技能（仓库/分支/提交/PR 查询），保证稳定性。
2. 再接入变更技能（建分支、提交、推送、建 PR），保证可控性。
3. 将 Git 调用统一通过 Tool 接口进入 AgentExecutor。
4. 增加权限网关与风险确认，避免 Agent 误操作。
5. 全链路记录调用意图、参数摘要、结果与错误，满足审计要求。

核心原则：

1. 默认最小权限：默认只读，写操作需显式授权。
2. 任务上下文绑定：Git 操作必须绑定当前任务仓库快照。
3. 操作可回放：每一步工具调用可追踪与复盘。
4. 故障可恢复：网络失败、权限失败、冲突失败均可给出下一步建议。

### 复用设计

本阶段复用现有 AI 与 Git 能力，新增最少必要组件：

| 复用组件 | 复用自 | 本阶段复用方式 |
|---------|-------|--------------|
| ILlmGateway/LlmGateway | 阶段三 | 保持统一 LLM 调用入口 |
| AgentExecutor/IAgentTool | 阶段三 | 通过 Tool 机制扩展 Git 能力 |
| AgentMatcher | 阶段三 | 按意图匹配“Git 协作助手/审查助手” |
| IGitProvider/GitProviderFactory | 阶段六 | 复用多平台 Git 能力抽象 |
| RepositoryService | 阶段六 | 读取仓库与分支配置 |
| ReviewService | 阶段十三 | 在审查场景中复用审查结果查询能力 |

---

## 本阶段复用的功能清单（来自其他阶段）

| 复用组件 | 文件路径 | 复用方式 | 避免重复代码 |
|---------|---------|---------|-----------|
| IAgentTool | server/src/AutoCodeForge.Core/Interfaces/IAgentTool.cs | Git 技能统一遵循工具接口 | 避免重复定义工具协议 |
| AgentExecutor | server/src/AutoCodeForge.Infrastructure/AI/AgentExecutor.cs | 注册并调度 Git 技能调用 | 避免重复执行框架 |
| AgentMatcher | server/src/AutoCodeForge.Infrastructure/AI/AgentMatcher.cs | 自动匹配合适 Agent | 避免重复意图匹配逻辑 |
| IGitProvider | server/src/AutoCodeForge.Core/Interfaces/IGitProvider.cs | 承载跨平台 Git 操作 | 避免重复平台实现 |
| GitProviderFactory | server/src/AutoCodeForge.Infrastructure/Git/GitProviderFactory.cs | 按仓库类型路由 Provider | 避免重复工厂逻辑 |
| RepositoryService | server/src/AutoCodeForge.Application/Services/RepositoryService.cs | 读取仓库元数据与认证引用 | 避免重复仓库读取逻辑 |

---

## 本阶段新增的可复用功能清单

| 复用组件 | 文件路径 | 说明 | 被复用次数（预估） |
|---------|---------|------|------------------|
| GitReadToolset | server/src/AutoCodeForge.Infrastructure/AI/Tools/GitReadToolset.cs | 只读 Git 技能集合 | 3+ |
| GitWriteToolset | server/src/AutoCodeForge.Infrastructure/AI/Tools/GitWriteToolset.cs | 变更类 Git 技能集合 | 3+ |
| GitSkillPermissionGuard | server/src/AutoCodeForge.Application/Security/GitSkillPermissionGuard.cs | Git 技能权限网关 | 4+ |
| GitOperationPolicy | server/src/AutoCodeForge.Core/Models/Security/GitOperationPolicy.cs | Git 操作策略模型（只读/可写/高风险） | 3+ |
| AgentToolAuditLogger | server/src/AutoCodeForge.Infrastructure/Logging/AgentToolAuditLogger.cs | 工具调用审计日志 | 4+ |
| GitContextHydrator | server/src/AutoCodeForge.Infrastructure/AI/GitContextHydrator.cs | 将任务快照注入 Agent 上下文 | 3+ |
| GitSkillErrorMapper | server/src/AutoCodeForge.Application/AI/GitSkillErrorMapper.cs | Git 异常映射为用户可执行建议 | 3+ |

---

## 数据表变更清单（字段 + 索引 + 唯一约束）

### 变更原则

1. 工具调用审计独立存储，不污染会话主表。
2. 权限策略可配置，默认安全且可追溯。
3. 支持按任务与会话回放工具调用链。

### A. 现有表增量变更

| 表名 | 变更类型 | 字段 | 类型 | 约束/默认值 | 说明 |
|------|---------|------|------|------------|------|
| ChatSessions | 新增 | TaskId | TEXT/Guid | NULL | 绑定会话与任务上下文 |
| Agents | 新增 | SkillProfile | TEXT | NULL | Agent 技能画像（ReadOnly/Collaborator/Reviewer） |
| UserConfigs | 新增 | GitSkillPolicyJson | TEXT | NULL | 用户级 Git 技能授权策略 |

### B. 新增审计与策略表

建议新增以下表：

1. AgentToolInvocations：记录每次工具调用（ToolName、InputDigest、OutputDigest、Status、Latency）。
2. GitSkillGrants：记录用户/仓库级授权（ReadOnly、Write、Dangerous）。

### C. 索引设计

| 表名 | 索引名 | 字段 | 类型 | 目的 |
|------|-------|------|------|------|
| AgentToolInvocations | IX_ATI_Session_CreatedAt | SessionId, CreatedAt | 普通索引 | 会话内调用链回放 |
| AgentToolInvocations | IX_ATI_Task_Status | TaskId, Status | 普通索引 | 任务维度排查失败调用 |
| GitSkillGrants | IX_GSG_User_Repo | NtId, RepositoryId | 唯一索引 | 防止重复授权记录 |

### D. 唯一约束与一致性约束

| 约束 | 作用对象 | 规则 |
|------|---------|------|
| UQ_GSG_User_Repo | GitSkillGrants(NtId, RepositoryId) | 每用户每仓库仅一条有效授权 |
| CK_GSG_Level | GitSkillGrants.Level | 仅允许 ReadOnly/Write/Dangerous |
| FK_ATI_Session | AgentToolInvocations.SessionId -> ChatSessions.Id | 调用记录必须关联会话 |
| FK_ATI_Task | AgentToolInvocations.TaskId -> Tasks.Id | 调用记录可回溯到任务 |

### E. 迁移与回滚建议

1. 先上审计表与策略表，再放开写操作技能。
2. 回滚时保留审计数据，只关闭写技能入口。
3. 旧会话 TaskId 为空属于正常历史数据。

---

## 任务清单

| 编号 | 任务名称 | 文件路径 | 产出物 | 验证方式 |
|------|---------|---------|-------|---------|
| 14.1 | 定义 Git 技能请求/响应 DTO | server/src/AutoCodeForge.Core/DTOs/AI/GitTools/ | 统一工具输入输出模型 | dotnet build 通过 |
| 14.2 | 实现 GitReadToolset（只读） | server/src/AutoCodeForge.Infrastructure/AI/Tools/GitReadToolset.cs | 分支/提交/PR/差异读取工具 | 单元测试覆盖查询成功与空结果 |
| 14.3 | 实现 GitWriteToolset（变更） | server/src/AutoCodeForge.Infrastructure/AI/Tools/GitWriteToolset.cs | 创建分支、提交、推送、创建 PR 工具 | 集成测试覆盖授权成功/失败场景 |
| 14.4 | 实现 GitSkillPermissionGuard | server/src/AutoCodeForge.Application/Security/GitSkillPermissionGuard.cs | 用户/仓库/操作级权限校验 | 权限矩阵测试通过 |
| 14.5 | 实现 GitContextHydrator | server/src/AutoCodeForge.Infrastructure/AI/GitContextHydrator.cs | 自动注入仓库/分支/任务快照上下文 | 聊天调用无需重复输入仓库信息 |
| 14.6 | 扩展 AgentExecutor 工具注册链路 | server/src/AutoCodeForge.Infrastructure/AI/AgentExecutor.cs | 动态加载读写工具并附加权限校验 | 端到端会话可触发正确工具 |
| 14.7 | 新增工具调用审计日志 | server/src/AutoCodeForge.Infrastructure/Logging/AgentToolAuditLogger.cs | 工具调用全链路可追踪 | 查询审计日志可还原调用链 |
| 14.8 | 新增 Git 技能策略 API | server/src/AutoCodeForge.Api/Endpoints/AgentSkillEndpoints.cs | 配置/查询用户 Git 技能权限策略 | Swagger 联调通过 |
| 14.9 | 异常映射与用户建议 | server/src/AutoCodeForge.Application/AI/GitSkillErrorMapper.cs | 将冲突、权限、网络异常转化为可执行建议 | 人工验证失败场景提示可用 |
| 14.10 | DI 注册与端到端联调 | server/src/AutoCodeForge.Api/Program.cs | 注册 Toolset/Guard/Hydrator/Audit 组件 | 从“查询分支”到“发起 PR”链路通过 |

---

## 注意事项

1. 未授权写操作一律拒绝，且需要清晰错误码。
2. 高风险命令（强推、删除分支）默认禁用。
3. 工具输入输出应做摘要存储，避免审计日志过大。
4. 工具异常必须带可执行建议，不可只返回堆栈。
5. Agent 自动选择命中 Git 场景时，应优先只读技能，降低误操作概率。

---

## 阶段完成总结

### 复用收益

1. 复用阶段三工具体系，避免重建 Agent 调用协议。
2. 复用阶段六 Git Provider，避免重复实现多平台 API。
3. 复用阶段十三审查能力，使“查询代码风险 -> 发起修复 PR”形成闭环。

### 本阶段验收口径

1. Agent 可稳定执行 Git 只读查询技能。
2. 写操作在授权后可执行，未授权时安全拒绝。
3. 工具调用具备完整审计链路与问题定位能力。
4. 用户可在策略页配置 Git 技能权限。

### 下一步

完成本阶段后，可将 Git 技能进一步复用到：

1. 定时任务场景（周期巡检分支与 PR 状态）。
2. 流水线场景（失败后自动分析最近变更）。
3. 审查场景（根据 findings 自动生成修复分支与 PR 草稿）。
