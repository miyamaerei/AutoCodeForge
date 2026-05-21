# 仓库拉取（Repo Sync）功能完整流程指南

## 概述

本文档详细描述 AutoCodeForge 系统中仓库拉取功能的完整闭环流程，包括前端配置、后端处理、数据存储和任务状态管理。

---

## 一、用户操作前置配置

### 1.1 配置沙箱（前端）

用户在 **系统配置 → Sandbox 配置** 页面设置：

| 配置项 | 作用 | 必填 | 默认值 |
|-------|------|------|--------|
| **Workspace Root Path** | 工作区根目录（绝对路径），仓库将被克隆到该目录下 | ✅ | - |
| **Allowed Write Paths** | 允许写入的路径列表（glob 模式） | - | [] |
| **Command Timeout (s)** | 命令执行超时时间 | - | 600 |
| **User Isolation Enabled** | 是否启用用户隔离 | - | true |

**配置页面路径**：`client/src/modules/system-config/views/SystemConfigSandboxView.vue`

### 1.2 添加仓库（前端）

用户在 **仓库管理 → 仓库列表** 页面添加仓库：

#### 1.2.1 Token 管理机制

系统提供**两级 Token 管理机制**，让用户可以灵活选择或新建 Token：

| 层级 | 位置 | 用途 | 优先级 |
|-----|------|------|--------|
| **全局 Token** | 系统配置 → Repository / ApiKey / Integration | 作为默认值，可在下拉列表中选择 | 中（可覆盖） |
| **仓库级 Token** | 仓库管理 → 添加仓库 | 每个仓库独立的认证凭据 | 高 |

#### 1.2.2 Token 选择器功能

在 **添加仓库** 对话框中，Token 选择器提供以下功能：

1. **从配置中选择 Token**
   - 系统自动从以下配置类型中提取 Git Token：
     - `ApiKey` 配置
     - `Repository` 配置
     - `Integration` 配置
   - 提取规则：匹配 GitHub Token 格式（以 `ghp_`、`github_pat_`、`gho_` 等开头）
   - 自动去重，避免重复显示相同的 Token

2. **使用自定义 Token**
   - 选择 "+ 使用自定义 Token" 选项
   - 输入新的 GitHub Token
   - 可选：为此 Token 起个名称
   - 点击 "保存到配置" 将新 Token 保存到 `ApiKey` 配置中，下次可直接选择

3. **Token 优先级**
   ```
   自定义输入的 Token > 选中的配置 Token > 自动读取的第一个 Token
   ```

#### 1.2.3 添加仓库表单字段

| 字段 | 说明 |
|-----|------|
| **仓库名称** | 自定义名称，用于显示 |
| **仓库地址** | Git 远程仓库 URL（如 `https://github.com/owner/repo`） |
| **访问令牌** | 通过 Token 选择器选择或新建 |
| **远程仓库** | 使用 Token 拉取后可从下拉列表中选择 |

**页面路径**：`client/src/modules/repo-management/views/RepoManagementListView.vue`

**核心代码位置**：
- Token 选择器实现：`loadTokenOptions()`, `handleTokenSelect()`, `saveTokenToConfig()`
- Token 提取逻辑：`findTokenFromConfigValue()`, `mayBeGitHubToken()`
- 最终 Token 计算：`finalToken` computed 属性

---

## 二、后端处理流程

### 2.1 流程架构图

```
┌──────────────────────────────────────────────────────────────────────────────┐
│                              用户操作层                                      │
├──────────────────────────────────────────────────────────────────────────────┤
│  前端配置沙箱        │  前端添加仓库         │  前端触发同步任务               │
│  (SystemConfig)     │  (RepoManagement)    │  (TaskCenter)                  │
└───────────┬─────────────────────┬──────────────────────────┬────────────────┘
            │                     │                          │
            ▼                     ▼                          ▼
┌──────────────────────────────────────────────────────────────────────────────┐
│                              API 接入层                                      │
├──────────────────────────────────────────────────────────────────────────────┤
│  ConfigEndpoints    │  RepositoryEndpoints    │  RepoSyncEndpoints          │
│  (保存沙箱配置)      │  (创建仓库实体)          │  (创建同步任务)              │
└───────────┬─────────────────────┬───────────────────┬────────────────────────┘
            │                     │                   │
            ▼                     ▼                   ▼
┌──────────────────────────────────────────────────────────────────────────────┐
│                              应用服务层                                      │
├──────────────────────────────────────────────────────────────────────────────┤
│  ConfigService      │  RepositoryService      │  RepoSyncService             │
│  (获取/保存配置)     │  (验证凭据+保存仓库)     │  (创建同步任务+快照)          │
└───────────┬─────────────────────┬───────────────────┬────────────────────────┘
            │                     │                   │
            │                     │                   ▼
            │                     │          ┌──────────────────────────┐
            │                     │          │    TaskEntity            │
            │                     │          │  - SandboxSnapshotJson   │
            │                     │          │  - RepositorySnapshotJson│
            │                     │          └───────────┬──────────────┘
            │                     │                      │
            │                     │                      ▼
            │                     │          ┌──────────────────────────┐
            │                     │          │  RepoSyncTaskHandler     │
            │                     │          │  (执行同步任务)          │
            │                     │          └───────────┬──────────────┘
            │                     │                      │
            │                     │    ┌─────────────────┴─────────────────┐
            │                     │    ▼                                   ▼
            │                     │ ┌──────────────┐                  ┌──────────────────┐
            │                     │ │SandboxPathResolver│            │ GitCloneService  │
            │                     │ │  (解析本地路径)      │            │  (执行Git克隆)   │
            │                     │ └──────────────┘                  └────────┬─────────┘
            │                     │                                           │
            ▼                     ▼                                           ▼
┌──────────────────────────────────────────────────────────────────────────────┐
│                              数据存储层                                      │
├──────────────────────────────────────────────────────────────────────────────┤
│  GlobalConfigEntity  │  RepositoryEntity     │  RepoSandboxWorkspaceEntity │
│  (沙箱配置)           │  (仓库配置+加密Token) │  (工作区记录+状态)           │
└──────────────────────────────────────────────────────────────────────────────┘
```

### 2.2 关键步骤详解

#### 步骤 1：创建仓库（RepositoryService.CreateAsync）

1. 验证 URL 格式
2. 检查仓库是否已存在
3. 通过 `GitProviderFactory` 创建对应 Provider（GitHub/GitLab/AzureDevOps）
4. 调用 `provider.VerifyCredentialsAsync()` 验证 Token
5. 创建 `RepositoryEntity`，Token 加密存储（`DataProtectionService.Encrypt`）
6. 保存到数据库

#### 步骤 2：创建同步任务（RepoSyncService.CreateTaskAsync）

1. 获取仓库配置（`RepositoryEntity`）
2. 获取沙箱配置（`ConfigService.GetSandboxConfigAsync`）
3. 创建快照对象：
   - `SandboxSnapshot`: 包含工作区路径、超时等配置
   - `RepositorySnapshot`: 包含 URL、Provider、加密 Token、分支
4. 创建 `TaskEntity`（`TaskType = RepoSyncToSandbox`）
5. 保存任务和任务日志

#### 步骤 3：执行同步任务（RepoSyncTaskHandler.ExecuteAsync）

1. 从 `TaskEntity` 反序列化两个快照
2. 使用 `SandboxPathResolver` 解析本地路径
3. 创建/更新 `RepoSandboxWorkspaceEntity`
4. 解密 Token（`DataProtectionService.Decrypt`）
5. 调用 `GitCloneService.CloneOrPullAsync()` 执行 Git 操作
6. 更新工作区状态和提交 SHA
7. 更新任务状态为 `Completed`/`Failed`/`Canceled`

#### 步骤 4：路径解析（SandboxPathResolver.Resolve）

**最终路径格式**：
```
{WorkspaceRootPath}/users/{ntId}/tasks/{taskId}/repo/{provider}_{owner}_{repoName}
```

**示例**：
```
C:/gitrepos/AutoCodeForge/users/user123/tasks/a1b2c3d4/repo/GitHub_Microsoft_vscode
```

---

## 三、仓库存储位置

| 层级 | 路径片段 | 说明 |
|-----|---------|------|
| 根目录 | `{WorkspaceRootPath}` | 沙箱配置中指定的工作区根目录 |
| 用户隔离 | `users/{ntId}` | 每个用户独立目录（启用隔离时） |
| 任务隔离 | `tasks/{taskId}` | 每个任务独立目录 |
| 仓库目录 | `repo/{provider}_{owner}_{repo}` | 实际仓库代码存储位置 |

---

## 四、任务完结标志

### 4.1 任务状态流转

```
Pending → Ready → Pulled/Failed/Canceled
          ↑         ↑
          │         └── 任务完成标志
          └── 工作区准备就绪
```

### 4.2 完结条件

| 状态 | 标志 | 说明 |
|-----|------|------|
| **Completed** | `Task.Status = Completed` | Git 克隆/拉取成功 |
| **Failed** | `Task.Status = Failed` | 执行失败（超时/网络错误等） |
| **Canceled** | `Task.Status = Canceled` | 用户主动取消 |

### 4.3 完成时更新的字段

**TaskEntity 更新**：
```csharp
task.Status = TaskStatus.Completed;
task.Progress = 100;
task.Result = JsonSerialize({ workspacePath, commitSha, branch });
task.WorkspaceRecordId = workspace.Id;
task.CompletedAtUtc = DateTime.UtcNow;
```

**RepoSandboxWorkspaceEntity 更新**：
```csharp
workspace.Status = RepoSandboxWorkspaceStatus.Pulled;
workspace.CommitSha = sha;
workspace.Branch = branch;
workspace.FinishedAtUtc = DateTime.UtcNow;
```

---

## 五、涉及的核心文件

| 层级 | 文件路径 | 职责 |
|-----|---------|------|
| **前端** | `client/src/modules/system-config/views/SystemConfigSandboxView.vue` | 沙箱配置页面 |
| **前端** | `client/src/modules/repo-management/views/RepoManagementListView.vue` | 仓库管理页面 |
| **后端API** | `server/src/AutoCodeForge.Api/Endpoints/RepositoryEndpoints.cs` | 仓库 CRUD 端点 |
| **后端API** | `server/src/AutoCodeForge.Api/Endpoints/RepoSyncEndpoints.cs` | 同步任务端点 |
| **应用服务** | `server/src/AutoCodeForge.Application/Services/RepositoryService.cs` | 仓库服务 |
| **应用服务** | `server/src/AutoCodeForge.Application/Services/RepoSyncService.cs` | 同步任务服务 |
| **基础设施** | `server/src/AutoCodeForge.Infrastructure/BackgroundServices/Handlers/RepoSyncTaskHandler.cs` | 任务执行处理器 |
| **基础设施** | `server/src/AutoCodeForge.Infrastructure/Services/GitCloneService.cs` | Git 克隆服务 |
| **基础设施** | `server/src/AutoCodeForge.Infrastructure/Sandbox/SandboxPathResolver.cs` | 路径解析器 |
| **实体** | `server/src/AutoCodeForge.Core/Entities/RepositoryEntity.cs` | 仓库实体 |
| **实体** | `server/src/AutoCodeForge.Core/Entities/RepoSandboxWorkspaceEntity.cs` | 工作区实体 |

---

## 六、完整闭环总结

```
用户配置沙箱 → 用户添加仓库 → 用户触发同步 → 后台执行同步 → 更新状态 → 任务完结
     │               │               │               │            │          │
     ▼               ▼               ▼               ▼            ▼          ▼
 GlobalConfig    Repository      TaskEntity     Git克隆      Workspace   Completed
   Entity          Entity                            →        Entity      (Progress=100)
```

1. **用户配置沙箱**：设置工作区根目录等参数，保存到 `GlobalConfigEntity`
2. **用户添加仓库**：输入仓库 URL 和 Token，后端验证并存入 `RepositoryEntity`（Token 加密）
3. **用户触发同步**：创建 `TaskEntity`（类型为 `RepoSyncToSandbox`），保存沙箱和仓库快照
4. **后台执行同步**：`RepoSyncTaskHandler` 读取快照，解析路径，执行 Git 克隆/拉取
5. **更新状态**：成功则更新任务状态为 `Completed`，更新工作区记录的 `CommitSha` 和状态
6. **任务完结**：`Task.Status = Completed` 且 `Progress = 100`，同时 `Workspace.Status = Pulled`

**仓库最终位置**：`{WorkspaceRootPath}/users/{ntId}/tasks/{taskId}/repo/{provider}_{owner}_{repoName}`

---

## 七、数据关联关系

| 实体 | 关联字段 | 说明 |
|-----|---------|------|
| `TaskEntity` | `RepositorySnapshotJson` | 存储仓库快照（URL、Provider、Token等） |
| `TaskEntity` | `SandboxSnapshotJson` | 存储沙箱配置快照 |
| `TaskEntity` | `WorkspaceRecordId` | 关联到工作区记录 |
| `RepoSandboxWorkspaceEntity` | `RepositoryId` | 关联到原始仓库实体 |
| `RepoSandboxWorkspaceEntity` | `TaskId` | 关联到任务实体 |

---

**文档版本**: 1.0  
**创建日期**: 2026-05-21  
**适用系统**: AutoCodeForge