# 阶段十二：Sandbox 继承与 Repo 同步（按用户隔离）

**日期**: 2026-05-20  
**预估时间**: 2-3 天  
**优先级**: 🔴 P0 - 核心能力  
**前置依赖**: 阶段二（用户与认证）、阶段四（任务中心）、阶段六（Git 集成）、阶段九（配置管理）

---

## 我是如何考虑的

### 设计思路

本阶段目标是打通一条完整链路：

1. 前端在 Settings/Sandbox 填写本地沙箱根路径（本机目录）。
2. 前端在 Repositories 完成仓库配置。
3. 创建任务后，后端读取 Sandbox 与 Repo 配置快照。
4. 后端按用户隔离规则生成目标目录。
5. 使用 C# Git 库将仓库拉取到对应 Sandbox。

核心原则：

1. 本地路径优先：Sandbox 是本地地址，不是云存储路径。
2. 用户隔离：每个用户独立目录，避免互相覆盖与越权访问。
3. 任务隔离：同一用户不同任务独立目录，便于回滚与审计。
4. 配置快照：任务执行使用创建时快照，避免执行中配置漂移。

### 是否需要重新设计 Sandbox（结论）

结论：不需要推翻重做，需要做结构化增强。

1. 现有 SandboxConfig 作为用户级策略配置继续保留（本地路径、超时、写权限等）。
2. 新增运行态模型用于记录“本次任务实际落地到哪个本地目录”。
3. 配置层与运行层分离：配置长期有效，运行记录按任务生成。

这样可以避免把一次性执行数据硬塞进配置表，同时满足审计与回溯。

### Repo 与 Sandbox 是否需要数据库关联（结论）

结论：需要关联，但不建议在 Repository 表上做固定强绑定。

建议采用两层关联：

1. 静态关联：TaskEntity 已有 RepositoryId，表示任务目标仓库。
2. 运行态关联：新增 RepoSandboxWorkspaceEntity（或 TaskWorkspaceEntity）记录本次执行工作区。

推荐字段（运行态表）：

1. Id
2. NtId
3. TaskId（唯一约束，1:1 对应任务执行实例）
4. RepositoryId
5. WorkspaceRootPath（快照）
6. EffectiveSandboxPath（实际目录，含 users/{ntId}/tasks/{taskId}）
7. Branch
8. CommitSha（可空）
9. Status（Ready/Cloned/Pulled/Failed/Cleaned）
10. ErrorMessage（可空）
11. CreatedAt/UpdatedAt

不建议给 RepositoryEntity 增加固定 SandboxPath 字段，原因：

1. 同一仓库可被多个用户使用，且每个用户本地路径不同。
2. 同一用户同一仓库可在多个任务目录并存（避免覆盖）。
3. Sandbox 是执行上下文，不是仓库元数据。

### 目录隔离约定（新增）

Sandbox 目标目录统一采用以下结构：

- 根目录：SandboxConfig.workspaceRootPath（本地绝对路径）
- 用户目录：{workspaceRootPath}/users/{ntId}/
- 任务目录：{workspaceRootPath}/users/{ntId}/tasks/{taskId}/
- 仓库目录：{workspaceRootPath}/users/{ntId}/tasks/{taskId}/repo/{provider}_{owner}_{repo}

说明：

1. ntId 使用登录态解析得到的当前用户标识。
2. owner/repo 需要做路径安全清洗（非法字符替换、长度限制）。
3. 所有路径在执行前做规范化并校验必须位于 workspaceRootPath 下。

---

## 复用设计

本阶段复用已有能力，最小化改动范围：

| 复用组件 | 复用自 | 本阶段复用方式 |
|---------|-------|--------------|
| JwtAuthMiddleware | 阶段二 | 获取当前用户身份（ntId） |
| UserRepository | 阶段二 | 用户存在性和状态校验 |
| TaskService/TaskQueueService/TaskExecutor | 阶段四 | 任务创建、排队、异步执行 |
| RepositoryService/RepositoryRepository | 阶段六 | 读取仓库配置与认证信息 |
| ConfigService | 阶段九 | 读取 Sandbox 配置 |
| ApiResponse<T> | 阶段一 | API 统一响应 |
| ExceptionHandlingMiddleware | 阶段一 | 错误统一处理与追踪 |

---

## 本阶段复用的功能清单（来自其他阶段）

| 复用组件 | 文件路径 | 复用方式 | 避免重复代码 |
|---------|---------|---------|-----------|
| JwtAuthMiddleware | server/src/AutoCodeForge.Api/Middleware/JwtAuthMiddleware.cs | 当前用户解析 | 避免重复身份解析逻辑 |
| TaskService | server/src/AutoCodeForge.Application/Services/TaskService.cs | 创建 Repo 同步任务 | 避免重复任务状态机逻辑 |
| TaskQueueService | server/src/AutoCodeForge.Infrastructure/BackgroundServices/TaskQueueService.cs | 后台异步拉取执行 | 避免重复队列逻辑 |
| RepositoryService | server/src/AutoCodeForge.Application/Services/RepositoryService.cs | 查询仓库配置 | 避免重复仓库查询逻辑 |
| ConfigService | server/src/AutoCodeForge.Application/Services/ConfigService.cs | 读取 SandboxConfig | 避免重复配置读取逻辑 |
| ApiResponse<T> | server/src/AutoCodeForge.Core/Models/ApiResponse.cs | 标准接口返回 | 避免重复响应包装 |

---

## 本阶段新增的可复用功能清单

| 复用组件 | 文件路径 | 说明 | 被复用次数（预估） |
|---------|---------|------|------------------|
| SandboxPathResolver | server/src/AutoCodeForge.Infrastructure/Sandbox/SandboxPathResolver.cs | 按用户+任务生成本地沙箱路径并校验安全 | 3+ |
| GitCloneService | server/src/AutoCodeForge.Application/Services/GitCloneService.cs | Git 拉取应用服务接口 | 2+ |
| LibGit2SharpProvider | server/src/AutoCodeForge.Infrastructure/Git/LibGit2SharpProvider.cs | C# Git 库实现（clone/pull/checkout） | 2+ |
| RepoSyncTaskHandler | server/src/AutoCodeForge.Infrastructure/BackgroundServices/Handlers/RepoSyncTaskHandler.cs | Repo 同步任务执行器 | 2+ |
| SandboxConfigValidator | server/src/AutoCodeForge.Application/Validators/SandboxConfigValidator.cs | Sandbox 本地路径与策略校验 | 2+ |
| RepoSyncEndpoints | server/src/AutoCodeForge.Api/Endpoints/RepoSyncEndpoints.cs | 发起任务与查询执行结果 | 2+ |

---

## 数据表变更清单（字段 + 索引 + 唯一约束）

### 变更原则

1. 配置数据与运行数据分离：UserConfig 保留策略，运行态落独立表。
2. 最小入侵：优先新增字段和新增表，不破坏既有 Repository 主数据模型。
3. 可回溯：所有执行态路径、分支、提交和错误可查询。

### A. 现有表增量变更

| 表名 | 变更类型 | 字段 | 类型 | 约束/默认值 | 说明 |
|------|---------|------|------|------------|------|
| Tasks | 新增 | TaskType | TEXT | NOT NULL, DEFAULT 'General' | 区分 RepoSyncToSandbox 与其他任务 |
| Tasks | 新增 | SandboxSnapshotJson | TEXT | NULL | 任务创建时的沙箱快照（根路径、策略） |
| Tasks | 新增 | RepositorySnapshotJson | TEXT | NULL | 任务创建时的仓库快照（provider/owner/repo/branch） |
| Tasks | 新增 | WorkspaceRecordId | TEXT/Guid | NULL | 关联运行态工作区记录 |

### B. 新增运行态关联表

建议表名：RepoSandboxWorkspaces

| 字段 | 类型 | 约束 | 说明 |
|------|------|------|------|
| Id | Guid | PK | 主键 |
| NtId | TEXT | NOT NULL | 用户归属 |
| TaskId | Guid | NOT NULL | 关联任务 |
| RepositoryId | Guid | NOT NULL | 关联仓库 |
| WorkspaceRootPath | TEXT | NOT NULL | 执行时沙箱根路径快照 |
| EffectiveSandboxPath | TEXT | NOT NULL | 实际执行目录（含 users/{ntId}/tasks/{taskId}） |
| RelativeRepoPath | TEXT | NOT NULL | 相对路径（便于迁移） |
| Branch | TEXT | NULL | 执行分支 |
| CommitSha | TEXT | NULL | 拉取后的提交 SHA |
| Status | TEXT | NOT NULL, DEFAULT 'Ready' | Ready/Cloned/Pulled/Failed/Cleaned |
| ErrorMessage | TEXT | NULL | 错误信息 |
| StartedAt | DATETIME | NULL | 开始时间 |
| FinishedAt | DATETIME | NULL | 完成时间 |
| CreatedAt | DATETIME | NOT NULL | 创建时间 |
| UpdatedAt | DATETIME | NOT NULL | 更新时间 |

### C. 索引设计

| 表名 | 索引名 | 字段 | 类型 | 目的 |
|------|-------|------|------|------|
| RepoSandboxWorkspaces | IX_RSW_NtId_Status | NtId, Status | 普通索引 | 用户维度查询执行状态 |
| RepoSandboxWorkspaces | IX_RSW_RepositoryId_UpdatedAt | RepositoryId, UpdatedAt | 普通索引 | 查看仓库最近同步记录 |
| RepoSandboxWorkspaces | IX_RSW_TaskId | TaskId | 唯一索引 | 保证每个任务仅一个工作区记录 |
| Tasks | IX_Tasks_TaskType_Status | TaskType, Status | 普通索引 | 任务中心按类型/状态筛选 |
| Tasks | IX_Tasks_WorkspaceRecordId | WorkspaceRecordId | 普通索引 | 快速关联工作区记录 |

### D. 唯一约束与一致性约束

| 约束 | 作用对象 | 规则 |
|------|---------|------|
| UQ_RSW_TaskId | RepoSandboxWorkspaces.TaskId | 一个任务最多一个运行态工作区 |
| CK_RSW_PathNotEmpty | RepoSandboxWorkspaces.WorkspaceRootPath, EffectiveSandboxPath | 路径不能为空 |
| CK_RSW_StatusEnum | RepoSandboxWorkspaces.Status | 状态值必须在 Ready/Cloned/Pulled/Failed/Cleaned |
| FK_RSW_Task | RepoSandboxWorkspaces.TaskId -> Tasks.Id | 工作区记录必须关联任务 |
| FK_RSW_Repository | RepoSandboxWorkspaces.RepositoryId -> Repositories.Id | 工作区记录必须关联仓库 |

### E. 迁移与回滚建议

1. 先加字段和新表，再发布任务处理器逻辑，避免运行期空引用。
2. 回滚时仅停用 RepoSyncToSandbox 任务类型，不删除历史运行态数据。
3. 历史 Tasks 的 TaskType 可一次性回填为 General。

---

## 任务清单

| 编号 | 任务名称 | 文件路径 | 产出物 | 验证方式 |
|------|---------|---------|-------|---------|
| 12.1 | 扩展 Sandbox 配置模型（本地路径+用户隔离开关） | server/src/AutoCodeForge.Core/DTOs/Config/SandboxConfigDto.cs, server/src/AutoCodeForge.Core/Entities/UserConfigEntity.cs | Sandbox DTO 与存储结构支持本地地址和用户隔离策略 | dotnet build 通过；配置读写接口可保存与读取新字段 |
| 12.2 | 增加 Sandbox 配置校验器 | server/src/AutoCodeForge.Application/Validators/SandboxConfigValidator.cs | 对 workspaceRootPath、allowedWritePaths、timeout、隔离策略做校验 | 单元测试覆盖有效/无效路径；非法路径返回 400 |
| 12.3 | 实现 SandboxPathResolver（按用户分目录） | server/src/AutoCodeForge.Infrastructure/Sandbox/SandboxPathResolver.cs | 输入 ntId/taskId/repo 得到规范化本地路径，防目录穿越 | 单元测试验证路径规范化；越权路径被拒绝 |
| 12.4 | 引入 LibGit2Sharp 并封装 Git Provider | server/src/AutoCodeForge.Infrastructure/AutoCodeForge.Infrastructure.csproj, server/src/AutoCodeForge.Infrastructure/Git/LibGit2SharpProvider.cs | 基于 C# Git 库的 clone/pull/checkout 能力 | dotnet build 通过；使用测试仓库完成 clone |
| 12.5 | 新增 GitCloneService 应用层接口与实现 | server/src/AutoCodeForge.Application/Services/GitCloneService.cs, server/src/AutoCodeForge.Application/Interfaces/IGitCloneService.cs | 统一业务入口：根据 repo 配置拉取到指定 sandbox 目录 | 集成测试验证认证失败、分支不存在、网络错误映射 |
| 12.6 | 新增 RepoSync 任务类型与任务处理器 | server/src/AutoCodeForge.Core/Entities/TaskEntity.cs, server/src/AutoCodeForge.Infrastructure/BackgroundServices/Handlers/RepoSyncTaskHandler.cs | 支持创建 RepoSyncToSandbox 任务并后台执行 | 创建任务后状态流转正确：Pending->Running->Completed/Failed |
| 12.7 | 任务快照机制（防配置漂移） | server/src/AutoCodeForge.Core/Entities/TaskEntity.cs, server/src/AutoCodeForge.Application/Services/TaskService.cs | 将 repo/sandbox 关键字段写入任务快照 | 修改原配置后重跑旧任务，仍按快照执行 |
| 12.8 | 新增 Repo 同步任务 API | server/src/AutoCodeForge.Api/Endpoints/RepoSyncEndpoints.cs | 提供创建同步任务、查询任务详情、查询产物路径接口 | Swagger 联调通过；接口返回 ApiResponse 统一格式 |
| 12.9 | 日志与脱敏（token、凭据） | server/src/AutoCodeForge.Infrastructure/Logging/LogSanitizer.cs, server/src/AutoCodeForge.Infrastructure/BackgroundServices/Handlers/RepoSyncTaskHandler.cs | 同步日志不泄露敏感信息 | 日志扫描无明文 token；失败日志可定位错误原因 |
| 12.10 | DI 注册与端到端联调 | server/src/AutoCodeForge.Api/Program.cs | 注册 GitCloneService、PathResolver、Handler、Validator | 从前端配置到任务执行全链路成功，仓库落盘到用户沙箱目录 |

---

## 注意事项

1. Sandbox 是本地目录，必须限制在允许根路径内。
2. 用户目录隔离必须默认开启，不允许跨用户读写。
3. 所有 Git 认证信息仅通过密钥引用获取，禁止明文日志。
4. 路径拼接必须统一由 SandboxPathResolver 处理，禁止业务层手工拼接。
5. Repo 同步任务应支持取消和超时，防止长时间阻塞队列。

---

## 阶段完成总结

### 复用收益

1. 复用任务中心异步执行框架，避免重复建设队列和状态机。
2. 复用 Git 集成仓储配置能力，避免重复建模。
3. 复用配置中心能力，避免新增独立配置系统。

### 本阶段验收口径

1. 可在前端配置本地 Sandbox 根路径。
2. 同一仓库由不同用户触发任务时，代码下载到各自用户目录。
3. 单个用户多任务互不覆盖，路径可追踪。
4. 异常场景（认证失败、分支不存在、目录不可写）可观测且可定位。

### 下一步

完成本阶段后，可将 RepoSyncToSandbox 能力复用到：

1. 定时任务模块（周期同步仓库）。
2. 流水线模块（构建前自动更新代码）。
3. 审查模块（拉取指定分支后执行静态检查）。

并通过 docs/plans/12-phase-twelve-sandbox-repo-sync-index.md 统一查看阶段12相关文档索引。
