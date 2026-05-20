# 前端 System Config 模块业务分析与字段文档

## 概述

本文档详细分析了 AutoCodeForge 前端项目中所有 System Config（系统配置）模块的业务含义、字段结构和功能特性。System Config 模块位于 `client/src/modules/system-config/`，包含 15 个配置页面，覆盖了从基础偏好设置到高级工作流编排的完整配置体系。

---

## 模块总览

| 序号 | 模块名称 | 路由路径 | 业务领域 | 数据持久化 |
|------|----------|----------|----------|------------|
| 1 | Preferences | `/settings/preferences` | 用户偏好 | localStorage |
| 2 | Repositories | `/settings/repositories` | 仓库集成 | localStorage |
| 3 | Knowledge | `/settings/knowledge` | 知识库管理 | localStorage |
| 4 | Skill | `/settings/skill` | 技能配置 | localStorage |
| 5 | Schedules | `/settings/schedules` | 定时任务 | localStorage |
| 6 | DeepWiki | `/settings/deepwiki` | 向量索引 | localStorage |
| 7 | Review | `/settings/review` | 代码评审 | localStorage |
| 8 | Integrations | `/settings/integrations` | 第三方集成 | localStorage |
| 9 | Notifications | `/settings/notifications` | 通知策略 | localStorage |
| 10 | Sandbox | `/settings/sandbox` | 沙盒执行 | localStorage |
| 11 | Workflow | `/settings/workflow` | 流程编排 | localStorage |
| 12 | API | `/system-config/api` | API 密钥 | 内存（Mock） |
| 13 | Models | `/system-config/models` | 模型选择 | 内存（Mock） |
| 14 | Users | `/system-config/users` | 用户管理 | 内存（Mock） |
| 15 | 系统配置（旧） | `/system-config` | 模型与密钥 | 无 |

---

## 模块详细分析

### 1. Preferences（偏好设置）

**业务描述**：管理用户的基础偏好配置，影响全局工作区体验。

**路由**：`/settings/preferences`

**字段结构**：

| 字段名 | 类型 | 必填 | 默认值 | 说明 |
|--------|------|------|--------|------|
| `locale` | `string` | 是 | `'zh-CN'` | 界面语言，可选 `zh-CN`（简体中文）、`en-US`（English） |
| `timezone` | `string` | 是 | `'Asia/Shanghai'` | 时区设置，长度 3-64 字符 |
| `theme` | `'light' \| 'dark' \| 'auto'` | 是 | `'light'` | 主题模式 |
| `landingPage` | `string` | 是 | `'/'` | 默认入口页，可选 `/`（控制台首页）、`/task-center`（任务中心）、`/repo-management`（仓库管理） |
| `enableInAppNotice` | `boolean` | 否 | `true` | 启用站内通知 |
| `enableEmailNotice` | `boolean` | 否 | `false` | 启用邮件通知 |

**特殊功能**：
- 重置引导按钮：调用 `useOnboarding().reset()` 重置用户引导状态

**数据持久化**：当前为 Mock 实现，保存时仅模拟延迟

---

### 2. Repositories（仓库集成）

**业务描述**：配置代码仓库接入、分支策略与权限设置。

**路由**：`/settings/repositories`

**字段结构**：

| 字段名 | 类型 | 必填 | 默认值 | 说明 |
|--------|------|------|--------|------|
| `provider` | `string` | 是 | `'github'` | 代码托管平台，可选 `github`、`azure-devops`、`gitlab` |
| `owner` | `string` | 是 | `'miyamaerei'` | 仓库所有者 |
| `repositoryName` | `string` | 是 | `'AutoCodeForge'` | 仓库名称 |
| `defaultBranch` | `string` | 是 | `'main'` | 默认分支 |
| `authMode` | `string` | 是 | `'token'` | 鉴权方式，可选 `token`、`app`、`oauth` |
| `mergeStrategies` | `string[]` | 是 | `['squash', 'merge']` | 合并策略，可选 `merge`、`squash`、`rebase` |
| `requireChecks` | `boolean` | 否 | `true` | 启用检查门禁 |

**预览功能**：实时显示仓库预览信息（fullName、provider、defaultBranch、authMode）

---

### 3. Knowledge（知识库管理）

**业务描述**：管理知识源配置，支持多种知识来源类型和索引策略。

**路由**：`/settings/knowledge`

**知识源类型**：
- `markdown`：本地 Markdown 文件
- `remote-wiki`：远程 Wiki
- `repository`：代码仓库
- `url`：URL 地址

**知识源字段结构**：

| 字段名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `id` | `string` | 自动生成 | 知识源唯一标识 |
| `name` | `string` | 是 | 知识源名称 |
| `type` | `KnowledgeSourceType` | 是 | 来源类型 |
| `location` | `string` | 是 | 来源位置（路径或 URL） |
| `contentFormat` | `string` | 是 | 内容格式（`markdown`、`docs`、`wiki`、`code`） |
| `refreshPolicy` | `RefreshPolicy` | 是 | 刷新策略（`manual`、`hourly`、`daily`、`weekly`） |
| `chunkSize` | `number` | 是 | 分块大小（100-4000） |
| `chunkOverlap` | `number` | 是 | 分块重叠（0-1000） |
| `indexingScope` | `string[]` | 否 | 索引范围（glob 模式数组） |
| `accessLevel` | `AccessLevel` | 是 | 访问级别（`public`、`internal`、`restricted`） |
| `enabled` | `boolean` | 是 | 是否启用 |
| `lastSyncAt` | `string \| null` | 自动 | 上次同步时间 |
| `syncStatus` | `'synced' \| 'syncing' \| 'failed'` | 自动 | 同步状态 |
| `freshnessSla` | `string` | 自动 | 新鲜度 SLA |
| `repositoryIds` | `string[]` | 否 | 关联仓库 ID 列表 |

**刷新策略与新鲜度 SLA 映射**：
| 刷新策略 | 新鲜度 SLA |
|----------|------------|
| `hourly` | `1h` |
| `daily` | `24h` |
| `weekly` | `7d` |
| `manual` | `-` |

**关联功能**：支持关联多个仓库，知识源只在关联仓库中生效

---

### 4. Skill（技能配置）

**业务描述**：管理 AI 技能配置，定义技能的使用场景、参数提示和输出目标。

**路由**：`/settings/skill`

**技能状态**：
- `active`：已激活
- `inactive`：未激活
- `beta`：测试版

**技能字段结构**：

| 字段名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `id` | `string` | 自动生成 | 技能唯一标识 |
| `name` | `string` | 是 | 技能名称 |
| `description` | `string` | 是 | 技能描述 |
| `argumentHint` | `string` | 否 | 参数提示 |
| `enabled` | `boolean` | 是 | 是否启用 |
| `status` | `SkillStatus` | 是 | 技能状态 |
| `tags` | `string[]` | 否 | 标签列表 |
| `whenToUse` | `string[]` | 否 | 使用场景列表 |
| `outputTargets` | `string[]` | 否 | 输出目标路径列表 |
| `repositoryIds` | `string[]` | 否 | 关联仓库 ID 列表 |

**技能配置表单**：
| 字段名 | 类型 | 说明 |
|--------|------|------|
| `enabled` | `boolean` | 是否启用 |
| `priority` | `number` | 优先级（0-100） |

**特殊操作**：
- 复制技能：创建技能副本，状态设为 `beta`
- 删除技能：仅允许删除非 `active` 状态的技能

---

### 5. Schedules（定时任务）

**业务描述**：配置定时任务计划与执行策略。

**路由**：`/settings/schedules`

**字段结构**：

| 字段名 | 类型 | 必填 | 默认值 | 说明 |
|--------|------|------|--------|------|
| `scheduleName` | `string` | 是 | `'daily-knowledge-sync'` | 任务名称 |
| `cron` | `string` | 是 | `'0 2 * * *'` | Cron 表达式（5 字段格式） |
| `timezone` | `string` | 是 | `'Asia/Shanghai'` | 时区 |
| `retryLimit` | `number` | 是 | `2` | 失败重试次数（0-10） |
| `enabled` | `boolean` | 否 | `true` | 启用状态 |
| `alertChannel` | `string` | 是 | `'in-app'` | 告警通道（`in-app`、`email`、`webhook`） |

**Cron 验证规则**：必须符合 5 字段格式（`分 时 日 月 周`）

---

### 6. DeepWiki（向量索引）

**业务描述**：配置知识库向量索引策略、向量模型和保留规则。

**路由**：`/settings/deepwiki`

**字段结构**：

| 字段名 | 类型 | 必填 | 默认值 | 说明 |
|--------|------|------|--------|------|
| `workspace` | `string` | 是 | `'AutoCodeForge'` | 工作区名称 |
| `indexName` | `string` | 是 | `'autocodeforge-main-index'` | 索引名称 |
| `embeddingModel` | `string` | 是 | `'text-embedding-3-large'` | 向量模型，可选 `text-embedding-3-large`、`text-embedding-3-small` |
| `topK` | `number` | 是 | `8` | 检索 Top K（1-50） |
| `metric` | `string` | 是 | `'cosine'` | 距离度量，可选 `cosine`、`dot`、`euclidean` |
| `retentionDays` | `number` | 是 | `90` | 保留天数（7-365） |
| `autoReindex` | `boolean` | 否 | `true` | 自动重建索引 |

---

### 7. Review（代码评审）

**业务描述**：配置代码评审门禁和质量阈值。

**路由**：`/settings/review`

**字段结构**：

| 字段名 | 类型 | 必填 | 默认值 | 说明 |
|--------|------|------|--------|------|
| `minApprovals` | `number` | 是 | `1` | 最小审批数（0-5） |
| `blockOnFailingChecks` | `boolean` | 否 | `true` | 失败检查阻止合并 |
| `enforceCodeOwners` | `boolean` | 否 | `true` | 强制 Code Owner |
| `requiredChecks` | `string[]` | 是 | `['ci', 'lint']` | 必需检查项（`ci`、`lint`、`unit-test`、`security`） |
| `firstResponseSlaHours` | `number` | 是 | `8` | 首响 SLA（1-72 小时） |
| `exceptionApproverRole` | `string` | 是 | `'Tech Lead'` | 异常审批角色 |

---

### 8. Integrations（第三方集成）

**业务描述**：配置第三方服务集成，包括 Azure、GitHub Copilot、GitLab 等。

**路由**：`/settings/integrations`

#### 8.1 Azure PWT 配置

| 字段名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `authMode` | `'entra' \| 'azure-devops-pat'` | 是 | 认证模式 |
| `tenantId` | `string` | 否 | Azure Tenant ID |
| `subscriptionId` | `string` | 否 | Azure Subscription ID |
| `projectName` | `string` | 否 | 项目名称 |
| `commandHint` | `string` | 否 | 命令提示（如 `az login`） |
| `devOpsOrganizationUrl` | `string` | PAT 模式必填 | Azure DevOps 组织 URL |
| `patScopeHint` | `string` | 否 | PAT 权限提示 |
| `patRotationDays` | `number` | 否 | PAT 轮换天数（7-90） |
| `patSecretRef` | `string` | 否 | PAT 密钥引用变量名 |

#### 8.2 GitHub Copilot CLI 配置

| 字段名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `organization` | `string` | 否 | 组织名称 |
| `executable` | `string` | 否 | CLI 可执行文件名（默认 `copilot`） |
| `installChannel` | `'winget' \| 'npm'` | 是 | 安装渠道 |
| `authMode` | `'interactive' \| 'pat'` | 是 | 认证模式 |
| `patEnvVar` | `'GH_TOKEN' \| 'GITHUB_TOKEN'` | PAT 模式必填 | PAT 环境变量名 |
| `workspacePolicy` | `'required' \| 'optional' \| 'disabled'` | 是 | 工作区策略 |
| `authState` | `'unauthenticated' \| 'pending' \| 'connected'` | 自动 | 认证状态 |

#### 8.3 通用集成配置（GitLab 等）

| 字段名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `endpoint` | `string` | 是 | 服务端点 URL |
| `workspace` | `string` | 否 | 工作区或项目标识 |
| `authType` | `'oauth' \| 'token' \| 'app'` | 是 | 认证类型 |
| `credentialHint` | `string` | 否 | 密钥说明或变量名 |

**CLI 状态模型**：
| 字段名 | 类型 | 说明 |
|--------|------|------|
| `checking` | `boolean` | 是否正在检测 |
| `installed` | `boolean` | 是否已安装 |
| `version` | `string` | 版本号 |
| `checkOutput` | `string` | 检测输出 |
| `lastCheckedAt` | `string` | 上次检测时间 |

---

### 9. Notifications（通知策略）

**业务描述**：配置通知渠道、事件范围和送达策略。

**路由**：`/settings/notifications`

**专注策略**：
| 策略 ID | 名称 | 说明 |
|---------|------|------|
| `balanced` | Balanced | 推荐模式，关键通知实时，其他按小时汇总 |
| `quiet` | Quiet Hours First | 降低噪音，仅保留高价值通知 |
| `critical-only` | Critical Only | 只保留阻塞交付的关键信号 |

**字段结构**：

| 字段名 | 类型 | 必填 | 默认值 | 说明 |
|--------|------|------|--------|------|
| `projectName` | `string` | 否 | `'AutoCodeForge'` | 项目标识 |
| `enableInApp` | `boolean` | 否 | `true` | 启用站内通知 |
| `enableEmail` | `boolean` | 否 | `true` | 启用邮件通知 |
| `digestMode` | `'immediate' \| 'hourly' \| 'daily'` | 是 | `'hourly'` | 汇总频率 |
| `focusMode` | `'balanced' \| 'quiet' \| 'critical-only'` | 是 | `'balanced'` | 专注策略 |
| `onlyMentioned` | `boolean` | 否 | `false` | 仅 @我 时通知 |
| `notifyOnBuildFailed` | `boolean` | 否 | `true` | 构建失败通知 |
| `notifyOnReviewRequested` | `boolean` | 否 | `true` | 收到代码评审请求通知 |
| `notifyOnDeploymentFinished` | `boolean` | 否 | `false` | 部署完成通知 |
| `notifyOnSecurityAlert` | `boolean` | 否 | `true` | 安全告警通知 |

**邮件配置（启用邮件时）**：

| 字段名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `emailProvider` | `'smtp' \| 'ses' \| 'sendgrid'` | 是 | 邮件服务商 |
| `emailFromName` | `string` | 否 | 发件人名称 |
| `emailFromAddress` | `string` | 是 | 发件邮箱 |
| `smtpHost` | `string` | 是 | SMTP 服务器地址 |
| `smtpPort` | `number` | 是 | SMTP 端口（1-65535） |
| `smtpSecure` | `boolean` | 否 | 连接安全（SSL/TLS 或 STARTTLS） |
| `smtpUsername` | `string` | 是 | SMTP 用户名 |
| `smtpPasswordPlaceholder` | `string` | 否 | SMTP 密码占位符 |

**投递时间窗口**：

| 字段名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `enabled` | `boolean` | 否 | 是否启用时间窗口 |
| `start` | `string` | 是 | 开始时间（如 `09:00`） |
| `end` | `string` | 是 | 结束时间（如 `20:00`） |
| `timezone` | `string` | 否 | 时区 |

**噪音等级计算**：
- **偏高**：启用渠道 ≥ 3 且启用事件 ≥ 3 且未开启"仅 @我"
- **偏低**：启用渠道 = 0 或启用事件 ≤ 1
- **平衡**：其他情况

---

### 10. Sandbox（沙盒执行）

**业务描述**：配置沙盒执行环境的安全策略、资源配额和文件范围。

**路由**：`/settings/sandbox`

**预设策略**：
| 策略 ID | 名称 | 说明 |
|---------|------|------|
| `balanced` | Balanced | 默认推荐，兼顾执行效率与安全约束 |
| `strict` | Strict Guardrail | 更高安全要求，禁止写操作和网络访问 |
| `debug` | Debug Assist | 适合定位问题，可放宽审批策略 |

**字段结构**：

| 字段名 | 类型 | 必填 | 默认值 | 说明 |
|--------|------|------|--------|------|
| `profileName` | `string` | 否 | `'default-sandbox'` | 配置名称 |
| `workspaceRootPath` | `string` | 否 | `'C:/gitrepos/AutoCodeForge'` | 工作区根路径 |
| `artifactOutputPath` | `string` | 否 | `'C:/gitrepos/AutoCodeForge/.sandbox-artifacts'` | 产物输出路径 |
| `allowedWritePaths` | `string` | 否 | `'src/**\ndocs/**'` | 允许写入路径（每行一个 glob） |
| `ignoredPaths` | `string` | 否 | `'node_modules/**\ndist/**\n.git/**'` | 忽略路径（每行一个 glob） |
| `executionMode` | `'dry-run' \| 'sandbox-live'` | 是 | `'dry-run'` | 执行模式 |
| `approvalMode` | `'strict' \| 'manual' \| 'off'` | 是 | `'strict'` | 审批模式 |
| `maxParallelTasks` | `number` | 是 | `3` | 最大并发任务数（1-12） |
| `commandTimeoutSec` | `number` | 是 | `300` | 命令超时秒数（60-1800） |
| `allowWriteOps` | `boolean` | 否 | `false` | 允许写操作 |
| `allowNetworkAccess` | `boolean` | 否 | `false` | 允许网络访问 |
| `storeTerminalLogs` | `boolean` | 否 | `true` | 保留终端日志 |
| `maskSecretsInLogs` | `boolean` | 否 | `true` | 日志脱敏 |
| `defaultModel` | `string` | 否 | `'gpt-5.3-codex'` | 默认模型 |
| `fallbackModel` | `string` | 否 | `'gpt-4.1'` | 回退模型 |
| `promptGuardrail` | `string` | 否 | `'先分析风险，再执行最小改动。'` | 提示护栏 |

**风险等级计算**：
- **高风险**：允许写操作且允许网络访问
- **中风险**：允许写操作或允许网络访问或审批模式为 off
- **低风险**：其他情况

---

### 11. Workflow（流程编排）

**业务描述**：配置 Agent 流程编排策略，包括初始化、提问、修 Bug、新需求等场景。

**路由**：`/settings/workflow`

**预设策略**：
| 策略 ID | 名称 | 说明 |
|---------|------|------|
| `safe` | Safe Guardrail | 适合高风险仓库，强调澄清、审批与验证 |
| `balanced` | Balanced Team | 平衡速度与稳定性，默认推荐 |
| `delivery` | Delivery First | 强调交付效率，减少不必要阻塞 |

**全局配置**：

| 字段名 | 类型 | 必填 | 默认值 | 说明 |
|--------|------|------|--------|------|
| `profileName` | `string` | 是 | `'team-default'` | 配置名称 |
| `commandTimeoutSec` | `number` | 是 | `300` | 命令超时秒数（60-1200） |
| `maxParallelTasks` | `number` | 是 | `3` | 最大并发任务数（1-8） |
| `requireApprovalForWrite` | `boolean` | 否 | `true` | 写操作需审批 |
| `autoCreateTodo` | `boolean` | 否 | `true` | 自动生成 Todo 计划 |
| `outputStyle` | `'concise' \| 'standard' \| 'detailed'` | 是 | `'standard'` | 响应风格 |

**阶段配置模型**：

| 字段名 | 类型 | 说明 |
|--------|------|------|
| `mode` | `'default' \| 'explore' \| 'mixed'` | Agent 模式 |
| `mustAskClarifying` | `boolean` | 必须先澄清 |
| `useRepoSearch` | `boolean` | 默认仓库检索 |
| `runValidation` | `boolean` | 执行后验证 |
| `notesTemplate` | `string` | 模板说明 |

**初始化阶段扩展字段**：

| 字段名 | 类型 | 说明 |
|--------|------|------|
| `preloadInstructionFiles` | `boolean` | 预加载指令文件 |
| `preloadRepoMemory` | `boolean` | 读取仓库记忆 |
| `generateChecklist` | `boolean` | 生成执行清单 |

**提问阶段扩展字段**：

| 字段名 | 类型 | 说明 |
|--------|------|------|
| `alwaysCiteFiles` | `boolean` | 必须引用文件 |
| `includeAlternatives` | `boolean` | 给出备选方案 |

**修 Bug 阶段扩展字段**：

| 字段名 | 类型 | 说明 |
|--------|------|------|
| `requireReproSteps` | `boolean` | 要求复现步骤 |
| `maxFixAttempts` | `number` | 最大修复轮次（1-5） |
| `autoRunUnitTests` | `boolean` | 自动跑单测 |

**新需求阶段扩展字段**：

| 字段名 | 类型 | 说明 |
|--------|------|------|
| `requireAcceptanceCriteria` | `boolean` | 要求验收标准 |
| `createImplementationPlan` | `boolean` | 生成实施计划 |
| `includeRollbackPlan` | `boolean` | 包含回滚方案 |

**自动化评分计算**（满分 100）：
- 写操作无需审批：+25 分
- 自动跑单测：+15 分
- 生成实施计划：+15 分
- 初始化无需澄清：+10 分
- 提问无需澄清：+10 分
- 并发任务 ≥ 4：+25 分

**自动化等级**：
- ≥ 75 分：高自动化
- 45-74 分：平衡自动化
- < 45 分：审慎自动化

---

### 12. API（API 密钥配置）

**业务描述**：管理第三方 API 密钥配置。

**路由**：`/system-config/api`

**字段结构**：

| 字段名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| `name` | `string` | 是 | API 名称 |
| `key` | `string` | 是 | API Key |
| `secret` | `string` | 否 | Secret（可选） |
| `status` | `'active' \| 'inactive'` | 自动 | 状态 |

**预设数据**：
- OpenAI API
- GitHub API
- Azure API

**注意**：当前为 Mock 实现，数据仅存储在内存中

---

### 13. Models（模型选择）

**业务描述**：选择默认 AI 模型。

**路由**：`/system-config/models`

**模型字段结构**：

| 字段名 | 类型 | 说明 |
|--------|------|------|
| `id` | `string` | 模型 ID |
| `name` | `string` | 模型名称 |
| `provider` | `string` | 提供商 |
| `version` | `string` | 版本 |
| `status` | `'active' \| 'inactive'` | 状态 |
| `maxTokens` | `number` | 最大 Token 数 |

**预设模型**：
| ID | 名称 | 提供商 | 最大 Token |
|----|------|--------|------------|
| `gpt-4` | GPT-4 | OpenAI | 128000 |
| `gpt-35` | GPT-3.5 Turbo | OpenAI | 4096 |
| `claude3` | Claude 3 Opus | Anthropic | 200000 |
| `llama2` | Llama 2 | Meta | 4096 |

**注意**：当前为 Mock 实现，数据仅存储在内存中

---

### 14. Users（用户管理）

**业务描述**：管理系统用户。

**路由**：`/system-config/users`

**用户字段结构**：

| 字段名 | 类型 | 说明 |
|--------|------|------|
| `id` | `string` | 用户 ID |
| `username` | `string` | 用户名 |
| `email` | `string` | 邮箱 |
| `role` | `'Admin' \| 'Developer' \| 'Viewer'` | 角色 |
| `status` | `'active' \| 'inactive'` | 状态 |
| `joinDate` | `string` | 加入日期 |

**角色权限**：
- **Admin**：完全管理权限
- **Developer**：开发者权限
- **Viewer**：只读权限

**注意**：当前为 Mock 实现，数据仅存储在内存中

---

### 15. 系统配置（旧版）

**业务描述**：旧版系统配置页面，包含模型与密钥配置。

**路由**：`/system-config`

**字段结构**：

| 字段名 | 类型 | 说明 |
|--------|------|------|
| API Key | `string` | API 密钥 |
| 模型选择 | `string` | 模型选择（`gpt-4o`、`gpt-4.1`、`o4-mini`） |
| 并发任务上限 | `number` | 并发任务上限（1-20） |

**注意**：此页面为旧版实现，建议迁移到新的模块化配置页面

---

## 数据持久化策略

### localStorage 键名映射

| 模块 | Storage Key |
|------|-------------|
| Knowledge | `system-config.knowledge.v1` |
| Skill | `system-config.skills.v1` |
| Integrations | `system-config.integrations.v1` |
| Notifications | `system-config.notifications.v1` |
| Sandbox | `system-config.sandbox.v1` |
| Workflow | `system-config.workflow.v1` |

### 持久化数据结构

```typescript
interface StorageModel {
  form: FormModel           // 表单数据
  lastSavedAt: string       // 上次保存时间
  selectedScenario?: string // 选中的预设策略
  selectedId?: string       // 选中的项目 ID
}
```

---

## Composable 依赖关系

```
useSystemConfigKnowledge
  └── useRepoManagementStore (获取仓库列表)

useSystemConfigSkills
  └── useRepoManagementStore (获取仓库列表)

useSystemConfigIntegrations
  └── window.__AUTOCODEFORGE_HOST__ (终端桥接)

useSystemConfigNotifications
  └── 无外部依赖

useSystemConfigSandbox
  └── 无外部依赖

useSystemConfigWorkflow
  └── 无外部依赖
```

---

## UI 布局模式

### 三栏布局（知识库、技能、集成、通知、沙盒）

```
┌─────────────────────────────────────────────────────────────┐
│                        Page Header                           │
├──────────────┬──────────────────────────────┬───────────────┤
│   Catalog    │         Detail Panel         │  Status Panel │
│   (320px)    │      (min 720px, 1fr)        │   (320px)     │
│              │                              │               │
│  - 搜索框    │  - 表单区域                  │  - 统计信息   │
│  - 项目列表  │  - 分隔线                    │  - 保存状态   │
│  - 添加按钮  │  - 操作按钮                  │  - 保存按钮   │
└──────────────┴──────────────────────────────┴───────────────┘
```

### 两栏布局（工作流）

```
┌─────────────────────────────────────────────────────────────┐
│                        Page Header                           │
├─────────────────────────────────────────────┬───────────────┤
│              Editor Panel                    │  Status Panel │
│           (min 980px, 1fr)                  │   (320px)     │
│                                             │               │
│  - Tab 导航                                 │  - 统计信息   │
│  - 表单区域                                 │  - 保存状态   │
│  - 分隔线                                   │  - 保存按钮   │
└─────────────────────────────────────────────┴───────────────┘
```

### 单卡片布局（偏好、仓库、定时任务、DeepWiki、评审、API、模型、用户）

```
┌─────────────────────────────────────────────────────────────┐
│                        Page Header                           │
├─────────────────────────────────────────────────────────────┤
│                      Main Card                               │
│                   (max 1180px)                               │
│                                                             │
│  - 表单区域                                                 │
│  - 预览表格（部分页面）                                     │
│  - 操作按钮                                                 │
└─────────────────────────────────────────────────────────────┘
```

---

## 后续优化建议

1. **数据持久化统一**：将 Mock 实现迁移到后端 API，确保数据安全
2. **表单验证增强**：添加更完善的字段验证规则和错误提示
3. **预设策略扩展**：支持自定义预设策略的创建和保存
4. **权限控制**：根据用户角色控制配置项的可见性和可编辑性
5. **配置导入导出**：支持配置的 JSON 导入导出功能
6. **配置版本历史**：记录配置变更历史，支持回滚
