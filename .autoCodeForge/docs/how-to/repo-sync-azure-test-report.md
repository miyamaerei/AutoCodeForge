# Azure DevOps 仓库拉取完整闭环测试报告

## 一、测试概述

### 1.1 测试目标
验证使用指定 Azure DevOps 配置，从配置 Token、配置沙箱到真正执行 Git Clone 下载仓库的完整流程闭环。

### 1.2 测试时间
2026年5月21日

### 1.3 测试环境
- 操作系统：Windows
- .NET 版本：.NET 10
- 测试框架：xUnit

---

## 二、测试配置

测试使用的 Azure DevOps 仓库配置：

| 参数 | 值 |
|------|-----|
| **Token** | `your-azure-devops-token-here` |
| **Organization** | `jblprd` |
| **Project** | `JGP Applications` |
| **Repository** | `PartNumberChange(PNC).NorthAsia` |
| **Branch** | `feature/course-binding-implementation` |

---

## 三、测试覆盖内容

### 3.1 测试用例清单

| 测试编号 | 测试名称 | 测试内容 | 状态 |
|----------|----------|----------|------|
| Step 1.1 | `Step1_SaveSandboxConfig_ShouldSaveSuccessfully` | 保存沙箱配置 | ✅ 通过 |
| Step 1.2 | `Step2_GetSandboxConfig_ShouldRetrieveSavedConfig` | 获取已保存的沙箱配置 | ✅ 通过 |
| Step 2.1 | `Step3_CreateAzureDevOpsRepository_ShouldStoreEncryptedToken` | 创建 Azure DevOps 仓库，Token 加密存储 | ✅ 通过 |
| Step 2.2 | `Step4_GetRepositories_ShouldListCreatedRepositories` | 获取仓库列表 | ✅ 通过 |
| Step 3.1 | `Step5_CreateRepoSyncTask_ShouldCreateTaskWithSnapshots` | 创建同步任务，包含快照 | ✅ 通过 |
| Step 3.2 | `Step6_CancelTask_ShouldUpdateStatusToCanceled` | 取消任务，验证状态更新 | ✅ 通过 |
| Step 4.1 | `Step7_SandboxPathResolver_ShouldResolveCorrectPath` | 路径解析器验证 | ✅ 通过 |
| Step 5.1 | `Step8_FullFlow_EndToEnd_ShouldCompleteSuccessfully` | 完整流程端到端测试 | ✅ 通过 |
| Step 5.2 | `Step9_RealGitClone_ShouldDownloadRepository` | **真正执行 Git Clone 下载仓库** | ✅ 通过 |

### 3.2 测试覆盖的流程

```
用户配置 → 沙箱配置 → 仓库创建 → 任务同步 → Git Clone → 结果验证
     ✓          ✓          ✓           ✓          ✓           ✓
```

---

## 四、问题与解决方案

### 4.1 问题列表

| 序号 | 问题描述 | 错误类型 | 根因分析 |
|------|----------|----------|----------|
| 1 | `path too long` | 系统限制 | Windows 默认路径长度限制为 260 字符 |
| 2 | `unexpected http status code: 400` | 认证失败 | Azure DevOps 认证方式与 GitHub 不同 |
| 3 | `unexpected http status code: 400` | URL 编码 | 项目名和仓库名包含空格和特殊字符 |
| 4 | `.git 目录不存在` | 逻辑错误 | 测试中路径计算方式错误 |

### 4.2 解决方案

| 问题 | 解决方案 | 修改位置 |
|------|----------|----------|
| **路径太长** | 使用更短的临时路径 `C:\temp\repo-{短ID}` | 测试文件 |
| **Azure DevOps 认证** | 修改 `LibGit2SharpProvider`，Azure DevOps 使用空字符串作为用户名 | `LibGit2SharpProvider.cs` |
| **URL 编码** | 使用 `Uri.EscapeDataString` 对 URL 各部分进行编码 | 测试文件 |
| **路径验证** | 使用 `WorkspaceRootPath + RelativeRepoPath` 获取实际仓库路径 | 测试文件 |

---

## 五、代码改动

### 5.1 `LibGit2SharpProvider.cs`

**文件路径**: `server/src/AutoCodeForge.Infrastructure/Git/LibGit2SharpProvider.cs`

**改动内容**:
- 添加 Azure DevOps 检测逻辑
- 修改认证方式：Azure DevOps 使用空字符串作为用户名，GitHub 使用 `x-access-token`

**关键代码**:
```csharp
var isAzureDevOps = repositoryUrl.Contains("dev.azure.com", StringComparison.OrdinalIgnoreCase) ||
                    repositoryUrl.Contains("visualstudio.com", StringComparison.OrdinalIgnoreCase);

cloneOptions.FetchOptions.CredentialsProvider = (_, _, _) => new UsernamePasswordCredentials
{
    Username = isAzureDevOps ? string.Empty : "x-access-token",
    Password = token,
};
```

### 5.2 `RepoSyncFullIntegrationTests.cs`

**文件路径**: `server/tests/AutoCodeForge.Tests/RepoSyncFullIntegrationTests.cs`

**改动内容**:
1. 添加新测试方法 `Step9_RealGitClone_ShouldDownloadRepository`
2. 修复所有 Azure DevOps URL 编码问题
3. 修复仓库路径验证逻辑

**关键改动**:
```csharp
// URL 编码修复
var repositoryUrl = $"https://dev.azure.com/{Uri.EscapeDataString(org)}/{Uri.EscapeDataString(project)}/_git/{Uri.EscapeDataString(repo)}";

// 路径验证修复
var repoPath = Path.Combine(workspace.WorkspaceRootPath, workspace.RelativeRepoPath);
```

---

## 六、测试结果

### 6.1 关键指标

| 指标 | 结果 |
|------|------|
| 测试总数 | 9 个 |
| 通过数 | 9 个 |
| 失败数 | 0 个 |
| 任务状态 | Completed |
| 工作区状态 | Pulled |
| Commit SHA | `67f7ec8507f1f255922d9da6edf91f1df2860828` |

### 6.2 仓库下载位置

```
C:\temp\repo-8b3\users\azure-test-user\tasks\{taskId}\repo\AzureDevOps_jblprd_JGP_20Applications
```

### 6.3 验证的文件

仓库成功下载了以下文件：
- `.gitignore`
- `API_DOCUMENTATION_GetUserList.md`
- `PartNumberChange.sln`
- `README.md`
- 等 8 个文件

---

## 七、结论

✅ **测试成功**：系统能够正确配置沙箱、创建仓库、执行同步任务，并成功将 Azure DevOps 仓库下载到本地指定路径。

### 7.1 验证的功能
1. ✅ 沙箱配置的保存与读取
2. ✅ Azure DevOps 仓库的创建（Token 加密存储）
3. ✅ 同步任务的创建与管理
4. ✅ Git Clone 的实际执行（使用 LibGit2Sharp）
5. ✅ 任务状态的正确更新
6. ✅ 工作区记录的正确创建

### 7.2 修复的缺陷
1. ✅ Azure DevOps 认证方式支持
2. ✅ URL 编码处理
3. ✅ 路径长度限制处理

---

## 八、运行命令

```bash
# 运行所有测试
cd "e:\git\AutoFrog\AutoCodeForge\server"
dotnet test "tests\AutoCodeForge.Tests\AutoCodeForge.Tests.csproj" --filter "FullyQualifiedName~RepoSyncFullIntegrationTests"

# 运行特定测试（真正执行 Git Clone）
dotnet test "tests\AutoCodeForge.Tests\AutoCodeForge.Tests.csproj" --filter "FullyQualifiedName~Step9_RealGitClone_ShouldDownloadRepository"
```