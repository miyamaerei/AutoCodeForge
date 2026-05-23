using AutoCodeForge.Core.DTOs.Config;
using AutoCodeForge.Core.Exceptions;
using AutoCodeForge.Infrastructure.Sandbox;

namespace AutoCodeForge.Tests;

public sealed class SandboxPathResolverTests
{
    private readonly SandboxPathResolver _resolver = new();

    [Fact]
    public void Resolve_ShouldBuildUserBasedPath()
    {
        var config = new SandboxConfigDto
        {
            WorkspaceRootPath = @"C:\sandbox-root",
            TimeoutSeconds = 600,
            UserIsolationEnabled = true,
        };

        var taskId = Guid.NewGuid();
        var result = _resolver.Resolve(config, "alice", taskId, "github", "my-org", "repo-a");

        // 新路径结构: {workspaceRoot}/{ntId}/{provider}_{owner}_{repo}
        Assert.StartsWith(@"C:\sandbox-root\alice", result.EffectiveSandboxPath, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("alice", result.EffectiveSandboxPath, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("github_my-org_repo-a", result.RepositoryPath, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Resolve_ShouldShortenLongSegments()
    {
        var config = new SandboxConfigDto
        {
            WorkspaceRootPath = @"C:\sandbox-root",
            TimeoutSeconds = 600,
            UserIsolationEnabled = true,
        };

        var taskId = Guid.NewGuid();
        var result = _resolver.Resolve(config, "alice", taskId, "github", "very-long-owner-name-that-exceeds-limit", "very-long-repo-name-that-exceeds-limit");

        // 验证路径长度不超过限制
        Assert.True(result.RepositoryPath.Length <= 200, $"Path too long: {result.RepositoryPath.Length}");
        Assert.DoesNotContain("..", result.RepositoryPath, StringComparison.Ordinal);
    }

    [Fact]
    public void Resolve_WithUnsafeSegments_ShouldSanitize()
    {
        var config = new SandboxConfigDto
        {
            WorkspaceRootPath = @"C:\sandbox-root",
            TimeoutSeconds = 600,
            UserIsolationEnabled = true,
        };

        var result = _resolver.Resolve(config, "a/b", Guid.NewGuid(), "github", "owner..", "repo??");

        Assert.DoesNotContain("..", result.RepositoryPath, StringComparison.Ordinal);
        Assert.DoesNotContain("?", result.RepositoryPath, StringComparison.Ordinal);
    }

    [Fact]
    public void Resolve_WithEmptyNtId_ShouldThrow()
    {
        var config = new SandboxConfigDto
        {
            WorkspaceRootPath = @"C:\sandbox-root",
            TimeoutSeconds = 600,
            UserIsolationEnabled = true,
        };

        Assert.Throws<ValidationException>(() => _resolver.Resolve(config, string.Empty, Guid.NewGuid(), "github", "owner", "repo"));
    }

    [Fact]
    public void Resolve_PathShouldBeWithinWorkspaceRoot()
    {
        var config = new SandboxConfigDto
        {
            WorkspaceRootPath = @"C:\sandbox-root",
            TimeoutSeconds = 600,
            UserIsolationEnabled = true,
        };

        var taskId = Guid.NewGuid();
        var result = _resolver.Resolve(config, "alice", taskId, "github", "owner", "repo");

        Assert.StartsWith(@"C:\sandbox-root", result.WorkspaceRootPath, StringComparison.OrdinalIgnoreCase);
        Assert.StartsWith(result.WorkspaceRootPath, result.RepositoryPath, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Resolve_ShouldHandleAzureDevOpsPath()
    {
        var config = new SandboxConfigDto
        {
            WorkspaceRootPath = @"C:\temp",
            TimeoutSeconds = 600,
            UserIsolationEnabled = true,
        };

        var taskId = Guid.Parse("ee7c9b36-1234-1234-1234-123456789012");
        var result = _resolver.Resolve(config, "azure-test-user", taskId, "AzureDevOps", "jblprd", "JGP Applications");

        // 验证路径结构
        Assert.True(result.RepositoryPath.Length <= 200, $"Path too long: {result.RepositoryPath.Length}");
        Assert.Contains("azure-test-user", result.RepositoryPath, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("AzureDevOps", result.RepositoryPath, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("jblprd", result.RepositoryPath, StringComparison.OrdinalIgnoreCase);

        // 验证最终路径格式
        var expectedPath = @"C:\temp\azure-test-user\AzureDevOps_jblprd_JGP_Applic";
        Assert.StartsWith(@"C:\temp\azure-test-user", result.RepositoryPath, StringComparison.OrdinalIgnoreCase);
    }
}