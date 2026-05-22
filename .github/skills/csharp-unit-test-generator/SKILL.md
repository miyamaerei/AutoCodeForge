---
name: csharp-unit-test-generator
description: "Generates C# unit tests for AutoCodeForge using IntegrationTestContext. Invoke when user asks to create unit tests, generate tests from requirements, or add test coverage for services/repositories."
---

# C# Unit Test Generator

基于 AutoCodeForge 测试基础设施生成 C# 单元测试。

## 测试基础设施

### IntegrationTestContext 核心辅助方法

```csharp
// 1. 创建并保存沙箱配置
await _context.CreateAndSaveTestSandboxConfigAsync(
    workspaceRootPath = @"C:\sandbox\workspace",
    timeoutSeconds = 300,
    userIsolationEnabled = true);

// 2. 直接在数据库创建测试仓库（绕过Token验证）
await _context.CreateTestRepositoryDirectlyAsync(
    name: "test-repo",
    url: "https://dev.azure.com/org/proj/_git/repo",
    provider: GitProvider.AzureDevOps);

// 3. 创建RepoSync任务
await _context.CreateRepoSyncTaskAsync(
    repositoryId: repoId,
    branch: "main",
    title: "Test Sync Task",
    description: "Test description");
```

### 可用 Services

| Service | 用途 |
|---------|------|
| `_context.ConfigService` | 配置管理（UpsertSandboxConfig, GetSandboxConfig等） |
| `_context.RepositoryService` | 仓库管理（Create, GetPaged等） |
| `_context.RepoSyncService` | 同步任务（CreateTask, GetById等） |
| `_context.AgentService` | Agent管理 |

### 可用 Repositories

| Repository | 实体类型 |
|------------|----------|
| `_context.ConfigRepository` | `UserConfigEntity` |
| `_context.RepositoryRepository` | `RepositoryEntity` |
| `_context.TaskRepository` | `TaskEntity` |
| `_context.TaskLogRepository` | `TaskLogEntity` |
| `_context.WorkspaceRepository` | `RepoSandboxWorkspaceEntity` |
| `_context.AgentRepository` | `AgentEntity` |
| `_context.LLMModelConfigRepository` | `LLMModelConfigEntity` |

## 测试文件结构

```
server/tests/AutoCodeForge.Tests/
├── IntegrationTestContext.cs          # 测试基础设施（已存在）
├── RepoSyncFullIntegrationTests.cs    # RepoSync完整测试（参考）
├── RealEndToEndTaskTests.cs            # E2E测试（参考）
└── [NewTestFile].cs                    # 新增测试文件
```

## 使用流程

1. **分析业务需求**：理解需要测试的功能
2. **确定测试类型**：Service层测试 / Repository层测试 / 集成测试
3. **生成测试代码**：复用 IntegrationTestContext
4. **输出完整测试文件**

## 模板

### 基本测试类模板

```csharp
/**
 * [功能名称] 测试
 *
 * 测试覆盖：
 * 1. [测试点1]
 * 2. [测试点2]
 */

using AutoCodeForge.Core.DTOs.[对应DTO命名空间];
using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Models;

namespace AutoCodeForge.Tests;

/// <summary>
/// [功能名称] 测试
/// </summary>
public sealed class [Feature]Tests : IDisposable
{
    private readonly IntegrationTestContext _context;

    public [Feature]Tests()
    {
        _context = new IntegrationTestContext("test-user");
    }

    #region [测试组1]

    /// <summary>
    /// 测试 [场景描述]
    /// </summary>
    [Fact]
    public async Task [Method]_[Scenario]_Should[ExpectedResult]()
    {
        // Arrange
        // Act
        // Assert
    }

    #endregion

    #region Cleanup

    public void Dispose()
    {
        _context.Dispose();
    }

    #endregion
}
```

### Service层测试模板

```csharp
/**
 * [ServiceName] 业务逻辑测试
 *
 * 测试覆盖：
 * 1. [正常场景]
 * 2. [异常场景]
 * 3. [边界条件]
 */

using AutoCodeForge.Application.Services;
using AutoCodeForge.Core.DTOs.[模块];
using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Models;

namespace AutoCodeForge.Tests;

/// <summary>
/// [ServiceName] 业务逻辑测试
/// </summary>
public sealed class [ServiceName]Tests : IDisposable
{
    private readonly IntegrationTestContext _context;

    public [ServiceName]Tests()
    {
        _context = new IntegrationTestContext("test-user");
    }

    /// <summary>
    /// 测试 [场景1]
    /// </summary>
    [Fact]
    public async Task [Method1]_Should_[Expected]()
    {
        // Arrange - 准备测试数据和依赖
        await _context.CreateAndSaveTestSandboxConfigAsync();

        var request = new [DtoName]Request
        {
            // 属性赋值
        };

        // Act - 执行被测方法
        var result = await _context.[ServiceName].[Method1](request);

        // Assert - 验证结果
        Assert.NotNull(result);
        Assert.Equal(expected, result.Actual);

        Console.WriteLine($"[测试1] [成功/失败]原因: xxx");
    }

    /// <summary>
    /// 测试 [场景2] - 异常处理
    /// </summary>
    [Fact]
    public async Task [Method2]_Should_ThrowException_When[Condition]()
    {
        // Arrange
        // Act & Assert
        await Assert.ThrowsAsync<[ExceptionType]>(
            () => _context.[ServiceName].[Method2](invalidRequest));
    }
}
```

### Repository层测试模板

```csharp
/**
 * [RepositoryName] 数据访问测试
 *
 * 测试覆盖：
 * 1. Create - 创建
 * 2. GetById - 查询
 * 3. Update - 更新
 * 4. Delete - 删除（软删除）
 */

using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Models;
using AutoCodeForge.Infrastructure.Repositories;

namespace AutoCodeForge.Tests;

/// <summary>
/// [RepositoryName] 数据访问测试
/// </summary>
public sealed class [RepositoryName]Tests : IDisposable
{
    private readonly IntegrationTestContext _context;
    private readonly [RepositoryName] _repository;

    public [RepositoryName]Tests()
    {
        _context = new IntegrationTestContext("test-user");
        _repository = _context.[RepositoryName];
    }

    /// <summary>
    /// 测试创建实体
    /// </summary>
    [Fact]
    public async Task CreateAsync_Should_SaveEntity()
    {
        // Arrange
        var entity = new [EntityName]
        {
            Id = Guid.NewGuid(),
            Name = "Test Entity",
            // ... 其他属性
            IsDeleted = false,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
        };

        // Act
        var created = await _repository.CreateAsync(entity);

        // Assert
        Assert.NotNull(created);
        Assert.Equal(entity.Name, created.Name);

        // 验证数据库中确实存在
        var retrieved = await _repository.GetByIdAsync(entity.Id);
        Assert.NotNull(retrieved);
    }
}
```

## 常用断言模式

```csharp
// 基本断言
Assert.NotNull(result);
Assert.Equal(expected, actual);
Assert.True(condition);
Assert.False(condition);

// 异步断言
await Assert.ThrowsAsync<ExceptionType>(() => method());
await Assert.ThrowsAsync<InvalidOperationException>(
    () => _context.ConfigService.GetSandboxConfigAsync());

// 集合断言
Assert.Empty(collection);
Assert.Single(collection);
Assert.Contains(item, collection);

// 异常断言
var exception = await Assert.ThrowsAsync<Exception>(() => method());
Assert.Contains("expected message", exception.Message);
```

## 常用测试数据构造

```csharp
// 沙箱配置
var sandboxConfig = new SandboxConfigDto
{
    WorkspaceRootPath = @"C:\sandbox\workspace",
    AllowedWritePaths = [@"C:\sandbox\workspace\users"],
    TimeoutSeconds = 300,
    UserIsolationEnabled = true,
};

// 仓库
var repoId = await _context.CreateTestRepositoryDirectlyAsync(
    "test-repo",
    "https://dev.azure.com/org/proj/_git/repo",
    GitProvider.AzureDevOps);

// 任务
var task = await _context.CreateRepoSyncTaskAsync(repoId, "main");
```

## 输出格式

生成测试时，输出：

1. **文件路径**：`server/tests/AutoCodeForge.Tests/[Feature]Tests.cs`
2. **完整代码**：包含所有必要的 using 语句和完整实现
3. **测试说明**：简要说明每个测试的目的

## 示例

**输入**：为 ConfigService 的 UpsertSandboxConfigAsync 方法编写测试

**输出**：
- 文件：`server/tests/AutoCodeForge.Tests/ConfigServiceTests.cs`
- 包含测试：
  - `UpsertSandboxConfig_Should_SaveNewConfig`
  - `UpsertSandboxConfig_Should_UpdateExistingConfig`
  - `GetSandboxConfig_Should_ReturnSavedConfig`
  - `GetSandboxConfig_Should_ReturnNull_WhenNoConfigSaved`
