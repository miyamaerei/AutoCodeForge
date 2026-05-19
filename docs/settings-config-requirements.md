# Settings Configuration Requirements

This document defines what should be filled in each setting section for the console configuration pages, including data structure definitions.

## Preferences

Purpose: Define workspace-level defaults used by UI and agent runtime.

**Data Structure:**
```typescript
interface Preferences {
  locale: string              // 字段: 语言区域，如 zh-CN, en-US
  timezone: string            // 字段: IANA时区格式，如 Asia/Shanghai
  themeMode: 'light' | 'dark' | 'auto'  // 字段: UI主题模式
  dateFormat: string          // 字段: 日期显示格式
  timeFormat: string          // 字段: 时间显示格式
  numberFormat: {             // 对象: 数字格式配置
    separator: string         // 字段: 千分位分隔符
    decimal: string           // 字段: 小数点符号
  }
  defaultLandingPage: string  // 字段: 默认工作区首页路径
  notifications: {            // 对象: 通知默认配置
    email: boolean            // 字段: 启用邮件通知
    inApp: boolean            // 字段: 启用站内通知
    webhook: boolean          // 字段: 启用Webhook通知
  }
  safeMode: boolean           // 字段: 安全模式标志
}
```

Required fields:
- Language and locale (example: zh-CN, en-US)
- Timezone (IANA format, example: Asia/Shanghai)
- UI theme mode (light, dark, auto)
- Date and time display format
- Number format and separator rules
- Default workspace landing page
- Notification defaults (email, in-app, webhook)
- Safe mode flags (confirm before destructive operations)

Validation rules:
- Locale must be from supported locale list.
- Timezone must be valid IANA timezone.
- Theme mode must be one of light, dark, auto.

## Repositories

Purpose: Manage source code repository integrations and governance defaults.

**Data Structure:**
```typescript
interface Repository {
  id: string                  // 字段: 仓库唯一标识
  owner: string               // 字段: 仓库所有者
  name: string                // 字段: 仓库名称
  provider: 'github' | 'azure-devops' | 'gitlab'  // 字段: 托管提供商
  authMethod: 'token' | 'app' | 'oauth'  // 字段: 认证方式
  defaultBranch: string       // 字段: 默认分支
  protectedBranches: string[] // 列表: 受保护分支列表
  prTemplate: string          // 字段: PR模板ID
  requiredStatusChecks: string[]  // 列表: 必需状态检查项
  allowedMergeStrategies: ('merge' | 'squash' | 'rebase')[]  // 列表: 允许的合并策略
}
```

Required fields:
- Repository identity (owner, repo name)
- Hosting provider (GitHub, Azure DevOps, GitLab)
- Authentication method (token, app, oauth)
- Default branch (example: main)
- Protected branch policy settings
- Pull request template selection
- Required status checks list
- Allowed merge strategies (merge, squash, rebase)

Validation rules:
- Owner and repo are required and unique per provider.
- Default branch cannot be empty.
- Merge strategies must include at least one option.

## Knowledge

Purpose: Configure knowledge sources used for retrieval and context assembly.

**Data Structure:**
```typescript
interface KnowledgeSource {
  id: string                  // 字段: 知识源唯一标识
  name: string                // 字段: 知识源名称
  type: 'markdown' | 'docs' | 'wiki' | 'code'  // 字段: 内容格式
  location: string            // 字段: 源位置（路径、URL或连接器标识）
  refreshPolicy: 'manual' | 'hourly' | 'daily'  // 字段: 刷新策略
  chunking: {                 // 对象: 分块策略
    size: number              // 字段: 分块大小
    overlap: number           // 字段: 重叠大小
  }
  indexingScope: {            // 对象: 索引范围
    branches: string[]        // 列表: 分支列表
    folders: string[]         // 列表: 文件夹列表
    tags: string[]            // 列表: 标签列表
  }
  accessLevel: 'public' | 'internal' | 'restricted'  // 字段: 访问控制级别
  freshnessSla: string        // 字段: 新鲜度SLA
  lastSyncAt: string          // 字段: 最后同步时间戳
}
```

Required fields:
- Knowledge source name and type
- Source location (path, URL, or connector identifier)
- Content format (markdown, docs, wiki, code)
- Refresh policy (manual, hourly, daily)
- Chunking strategy (size, overlap)
- Indexing scope (branches, folders, tags)
- Access control level (public, internal, restricted)
- Freshness SLA and last sync timestamp

Validation rules:
- Source location must be reachable or testable.
- Refresh policy must define cadence if not manual.
- Chunk size and overlap must be positive integers.

## Skill

Purpose: Configure skill behavior, safety bounds, and execution controls.

**Data Structure:**
```typescript
interface SkillConfig {
  id: string                  // 字段: 技能唯一标识
  name: string                // 字段: 技能名称
  version: string             // 字段: 技能版本
  enabled: boolean            // 字段: 启用标志
  runtimeMode: 'strict' | 'balanced' | 'exploratory'  // 字段: 运行模式
  timeoutSec: number          // 字段: 超时时间（秒）
  maxRetries: number          // 字段: 最大重试次数
  backoffPolicy: {            // 对象: 退避策略
    initialDelay: number      // 字段: 初始延迟（毫秒）
    multiplier: number        // 字段: 延迟倍数
    maxDelay: number          // 字段: 最大延迟（毫秒）
  }
  allowedTools: string[]      // 列表: 允许的工具列表
  forbiddenOperations: string[]  // 列表: 禁止的操作列表
  promptTemplateRef: string   // 字段: 提示模板引用
}
```

Required fields:
- Skill name and version
- Enable flag per skill
- Runtime mode (strict, balanced, exploratory)
- Timeout in seconds
- Max retries and backoff policy
- Allowed tools list
- Forbidden operations list
- Prompt template or instruction reference

Validation rules:
- Timeout must be within approved range (for example 10-300 seconds).
- Retry count must be non-negative.
- Allowed tools must be explicit for restricted mode.

## Schedules

Purpose: Define recurring jobs for sync, reports, and maintenance automation.

**Data Structure:**
```typescript
interface Schedule {
  id: string                  // 字段: 调度唯一标识
  name: string                // 字段: 调度名称
  cronExpression: string      // 字段: Cron表达式
  timezone: string            // 字段: 时区
  targetType: 'workflow' | 'pipeline' | 'sync'  // 字段: 任务类型
  targetId: string            // 字段: 目标任务ID
  enabled: boolean            // 字段: 启用标志
  retryPolicy: {              // 对象: 失败重试策略
    maxRetries: number        // 字段: 最大重试次数
    delaySec: number          // 字段: 重试间隔（秒）
  }
  alertChannel: 'email' | 'webhook' | 'slack'  // 字段: 告警渠道
  lastRunAt: string           // 字段: 上次运行时间
  nextRunAt: string           // 字段: 下次运行时间
}
```

Required fields:
- Schedule name
- Cron expression
- Timezone
- Job target (workflow, pipeline, sync task)
- Enable flag
- Failure retry policy
- Alert channel on failure
- Last run and next run timestamps

Validation rules:
- Cron expression must parse correctly.
- Timezone must be valid.
- Job target must map to an existing executable task.

## DeepWiki

Purpose: Configure DeepWiki indexing and retrieval behavior.

**Data Structure:**
```typescript
interface DeepWikiConfig {
  workspaceId: string         // 字段: 工作区或项目标识
  indexName: string           // 字段: 索引名称
  embeddingModel: string      // 字段: 嵌入模型名称
  vectorDimension: number     // 字段: 向量维度
  distanceMetric: 'cosine' | 'euclidean' | 'dot'  // 字段: 距离度量
  retrievalTopK: number       // 字段: 默认检索数量
  includePatterns: string[]   // 列表: 包含模式列表
  excludePatterns: string[]   // 列表: 排除模式列表
  reindexTrigger: 'manual' | 'auto' | 'schedule'  // 字段: 重索引触发策略
  retentionDays: number       // 字段: 保留天数
  cleanupPolicy: string       // 字段: 清理策略
}
```

Required fields:
- Workspace or project identifier
- Index name
- Embedding model
- Vector dimension and distance metric
- Retrieval top-k default
- Source include and exclude patterns
- Re-index trigger policy
- Retention period and cleanup policy

Validation rules:
- Index name must be unique within workspace.
- Embedding model must be available in current environment.
- Top-k must be positive integer and within system limits.

## Review

Purpose: Define code review quality gates and pull request guardrails.

**Data Structure:**
```typescript
interface ReviewConfig {
  minApprovals: number        // 字段: 最小审批数量
  requiredReviewerRoles: string[]  // 列表: 必需评审者角色
  blockOnFailingChecks: boolean    // 字段: 检查失败时阻止合并
  requiredChecks: string[]    // 列表: 必需检查项（CI, test, lint, security）
  codeOwnerEnforcement: boolean    // 字段: 代码所有者强制
  slaTargetMinutes: number    // 字段: 首次评审响应SLA（分钟）
  autoAssignmentRules: {      // 对象: 自动分配规则
    enabled: boolean          // 字段: 是否启用
    roundRobin: boolean       // 字段: 是否轮询分配
    excludeRoles: string[]    // 列表: 排除的角色
  }
  exceptionProcess: {         // 对象: 异常流程
    enabled: boolean          // 字段: 是否启用
    approverRoles: string[]   // 列表: 审批者角色
  }
}
```

Required fields:
- Minimum required approvals
- Required reviewer roles
- Block merge on failing checks (true or false)
- Required checks list (CI, test, lint, security)
- Code owner enforcement (true or false)
- SLA target for first review response
- Auto-assignment rules
- Exception process and approver roles

Validation rules:
- Minimum approvals must be integer >= 0.
- If blocking on checks is true, required checks list cannot be empty.
- Exception process must identify at least one approver role.

## API

Purpose: Manage API key configurations for external service integrations.

**Data Structure:**
```typescript
interface ApiKeyConfig {
  id: string                  // 字段: API配置唯一标识
  name: string                // 字段: API名称
  key: string                 // 字段: API密钥值
  secret: string | null       // 字段: Secret（可选）
  status: 'active' | 'inactive'  // 字段: 状态
  providerType: string        // 字段: 提供商类型（OpenAI, GitHub, Azure等）
}
```

Required fields:
- API name and identifier
- API key value
- Secret (optional for some providers)
- Status (active, inactive)
- Provider type (OpenAI, GitHub, Azure, etc.)

Validation rules:
- API name must be unique.
- API key cannot be empty for active status.
- Status must be one of active or inactive.

## Integrations

Purpose: Configure external service integrations like Azure PWT and GitHub Copilot.

**Data Structure:**
```typescript
interface IntegrationConfig {
  id: string                  // 字段: 集成唯一标识
  name: string                // 字段: 集成名称
  status: 'configured' | 'beta' | 'available'  // 字段: 状态
  authMode: string            // 字段: 认证模式
  providerConfig: {           // 对象: 提供商特定配置
    tenantId?: string         // 字段: Azure租户ID
    subscriptionId?: string   // 字段: Azure订阅ID
    organization?: string     // 字段: GitHub组织
    devOpsOrgUrl?: string     // 字段: Azure DevOps组织URL
    patSecretRef?: string     // 字段: PAT密钥引用
    patScopeHint?: string     // 字段: PAT权限提示
    patRotationDays?: number  // 字段: PAT轮换天数
  }
  cliConfig: {                // 对象: CLI配置
    executable: string        // 字段: CLI可执行文件路径
    installChannel: 'winget' | 'npm'  // 字段: 安装渠道
    authMode: 'interactive' | 'pat'  // 字段: CLI认证模式
    patEnvVar?: string        // 字段: PAT环境变量名
  }
  workspacePolicy: 'required' | 'optional' | 'disabled'  // 字段: 工作区策略
}
```

Required fields:
- Integration ID and name
- Integration status (configured, beta, available)
- Auth mode selection
- Provider-specific configuration (tenant ID, subscription ID, organization)
- CLI executable path and install channel
- Workspace policy (required, optional, disabled)

Validation rules:
- Auth mode must match supported options for each integration.
- Required fields depend on selected auth mode.
- CLI status must be verifiable.

## Models

Purpose: Configure AI models for task execution and select default model.

**Data Structure:**
```typescript
interface ModelConfig {
  id: string                  // 字段: 模型唯一标识
  name: string                // 字段: 模型名称
  provider: string            // 字段: 提供商（OpenAI, Anthropic, Meta等）
  version: string             // 字段: 版本标识
  status: 'active' | 'inactive'  // 字段: 状态
  maxTokens: number           // 字段: 最大Token限制
  isDefault: boolean          // 字段: 是否为默认模型
}
```

Required fields:
- Model ID and name
- Provider (OpenAI, Anthropic, Meta, etc.)
- Version identifier
- Status (active, inactive)
- Max tokens limit
- Default model flag

Validation rules:
- Only one model can be set as default.
- Model ID must be unique.
- Max tokens must be positive integer.

## Notifications

Purpose: Configure notification channels and alerting rules.

**Data Structure:**
```typescript
interface NotificationConfig {
  projectName: string         // 字段: 项目名称
  focusMode: 'balanced' | 'quiet' | 'critical-only'  // 字段: 专注模式
  channels: {                 // 对象: 通知渠道配置
    inApp: boolean            // 字段: 启用站内通知
    email: boolean            // 字段: 启用邮件通知
  }
  emailConfig: {              // 对象: 邮件配置
    provider: 'smtp' | 'ses' | 'sendgrid'  // 字段: 邮件服务商
    fromName: string          // 字段: 发件人名称
    fromAddress: string       // 字段: 发件邮箱
    smtpHost: string          // 字段: SMTP主机
    smtpPort: number          // 字段: SMTP端口
    smtpSecure: boolean       // 字段: SSL/TLS安全连接
    smtpUsername: string      // 字段: SMTP用户名
    smtpPassword: string      // 字段: SMTP密码
  }
  digestMode: 'immediate' | 'hourly' | 'daily'  // 字段: 汇总频率
  eventSubscriptions: {       // 对象: 事件订阅
    buildFailed: boolean      // 字段: 构建失败
    reviewRequested: boolean  // 字段: 收到代码评审请求
    deploymentFinished: boolean  // 字段: 部署完成
    securityAlert: boolean    // 字段: 安全告警
  }
  deliveryWindow: {           // 对象: 投递时间窗口
    enabled: boolean          // 字段: 是否启用
    start: string             // 字段: 开始时间
    end: string               // 字段: 结束时间
    timezone: string          // 字段: 时区
  }
  onlyMentioned: boolean      // 字段: 仅@我时通知
}
```

Required fields:
- Project name
- Focus mode (balanced, quiet, critical-only)
- Enabled channels (in-app, email)
- Email provider (SMTP, SES, SendGrid)
- SMTP configuration (host, port, secure, credentials)
- Digest mode (immediate, hourly, daily)
- Event subscriptions (build failed, review requested, deployment finished, security alert)
- Delivery window (enabled, start/end time, timezone)
- Only mentioned notification flag

Validation rules:
- If email enabled, SMTP host and port are required.
- Time window start must be before end.
- Event list must contain at least one item if notifications enabled.

## Users

Purpose: Manage workspace users and their roles.

**Data Structure:**
```typescript
interface User {
  id: string                  // 字段: 用户唯一标识
  username: string            // 字段: 用户名
  email: string               // 字段: 邮箱地址
  role: 'Admin' | 'Developer' | 'Viewer'  // 字段: 角色
  status: 'active' | 'inactive'  // 字段: 状态
  joinDate: string            // 字段: 加入日期
}
```

Required fields:
- User ID and username
- Email address
- Role (Admin, Developer, Viewer)
- Status (active, inactive)
- Join date

Validation rules:
- Email must be valid format.
- Role must be one of predefined options.
- Username cannot be empty.

## Workflow

Purpose: Configure Agent process orchestration for different task types.

**Data Structure:**
```typescript
interface WorkflowConfig {
  profileName: string         // 字段: 配置文件名称
  outputStyle: 'concise' | 'standard' | 'detailed'  // 字段: 输出风格
  commandTimeoutSec: number   // 字段: 命令超时（秒）
  maxParallelTasks: number    // 字段: 最大并发任务数
  executionControls: {        // 对象: 执行控制
    requireApprovalForWrite: boolean  // 字段: 写操作需审批
    autoCreateTodo: boolean   // 字段: 自动生成Todo计划
  }
  initialization: {           // 对象: 初始化配置
    mode: 'default' | 'explore' | 'mixed'  // 字段: Agent模式
    preloadInstructionFiles: boolean  // 字段: 预加载指令文件
    preloadRepoMemory: boolean        // 字段: 读取仓库记忆
    generateChecklist: boolean        // 字段: 生成执行清单
    notesTemplate: string     // 字段: 笔记模板
  }
  question: {                 // 对象: 提问配置
    mode: 'default' | 'explore' | 'mixed'  // 字段: Agent模式
    mustAskClarifying: boolean       // 字段: 必须先澄清
    useRepoSearch: boolean           // 字段: 默认仓库检索
    alwaysCiteFiles: boolean         // 字段: 必须引用文件
    includeAlternatives: boolean     // 字段: 给出备选方案
    notesTemplate: string     // 字段: 笔记模板
  }
  bugfix: {                   // 对象: Bug修复配置
    mode: 'default' | 'explore' | 'mixed'  // 字段: Agent模式
    maxFixAttempts: number    // 字段: 最大修复轮次
    requireReproSteps: boolean        // 字段: 要求复现步骤
    autoRunUnitTests: boolean         // 字段: 自动跑单测
    runValidation: boolean            // 字段: 修复后验证
    notesTemplate: string     // 字段: 笔记模板
  }
  feature: {                  // 对象: 新需求配置
    mode: 'default' | 'explore' | 'mixed'  // 字段: Agent模式
    requireAcceptanceCriteria: boolean   // 字段: 要求验收标准
    createImplementationPlan: boolean    // 字段: 生成实施计划
    includeRollbackPlan: boolean         // 字段: 包含回滚方案
    runValidation: boolean               // 字段: 实施后验证
    notesTemplate: string     // 字段: 笔记模板
  }
}
```

Required fields:
- Profile name
- Output style (concise, standard, detailed)
- Command timeout (seconds)
- Max parallel tasks
- Execution controls (require approval for write, auto create todo)

Per-task configurations:
- Initialization: mode (default, explore, mixed), preload instruction files, preload repo memory, generate checklist
- Question: mode, must ask clarifying, use repo search, always cite files, include alternatives
- Bugfix: mode, max fix attempts, require repro steps, auto run unit tests, run validation
- Feature: mode, require acceptance criteria, create implementation plan, include rollback plan, run validation

Validation rules:
- Timeout must be between 60-1200 seconds.
- Max parallel tasks must be between 1-8.
- At least one task type must have mode configured.

## Sandbox

Purpose: Configure sandbox execution environment and safety boundaries.

**Data Structure:**
```typescript
interface SandboxConfig {
  workspaceRootPath: string   // 字段: 工作区根路径
  artifactOutputPath: string  // 字段: 产物输出路径
  profileName: string         // 字段: 配置文件名称
  executionMode: 'dry-run' | 'sandbox-live'  // 字段: 执行模式
  approvalMode: 'strict' | 'manual' | 'off'  // 字段: 审批模式
  maxParallelTasks: number    // 字段: 最大并发任务数
  commandTimeoutSec: number   // 字段: 命令超时（秒）
  defaultModel: string        // 字段: 默认模型
  fallbackModel: string       // 字段: 备用模型
  promptGuardrail: string     // 字段: 提示词护栏
  allowedWritePaths: string[] // 列表: 允许写入路径（glob模式）
  ignoredPaths: string[]      // 列表: 忽略路径（glob模式）
  capabilities: {             // 对象: 能力标志
    allowWriteOps: boolean    // 字段: 允许写操作
    allowNetworkAccess: boolean  // 字段: 允许网络访问
    storeTerminalLogs: boolean   // 字段: 保留终端日志
    maskSecretsInLogs: boolean   // 字段: 日志脱敏
  }
}
```

Required fields:
- Workspace root path
- Artifact output path
- Profile name
- Execution mode (dry-run, sandbox-live)
- Approval mode (strict, manual, off)
- Max parallel tasks
- Command timeout (seconds)
- Default model and fallback model
- Prompt guardrail
- Allowed write paths (glob patterns)
- Ignored paths (glob patterns)
- Capability flags (allow write ops, allow network access, store terminal logs, mask secrets)

Validation rules:
- Workspace path must be valid directory.
- Timeout must be between 60-1800 seconds.
- At least one allowed write path required if write ops enabled.

## Suggested Common Metadata For All Sections

```typescript
interface CommonMetadata {
  environment: 'dev' | 'test' | 'prod'  // 字段: 环境范围
  updatedBy: string           // 字段: 更新人
  updatedAt: string           // 字段: 更新时间
  changeTicket: string        // 字段: 变更工单或审计引用
  rollbackGuidance: string    // 字段: 回滚指导
  dataSensitivity: 'public' | 'internal' | 'confidential'  // 字段: 数据敏感级别
}
```

- Environment scope (dev, test, prod)
- Updated by and updated at
- Change ticket or audit reference
- Rollback guidance
- Data sensitivity classification