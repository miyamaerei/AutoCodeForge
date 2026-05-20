using AutoCodeForge.Core.DTOs.Config;
using AutoCodeForge.Core.Exceptions;
using AutoCodeForge.Infrastructure.Sandbox;

namespace AutoCodeForge.Tests;

public sealed class SandboxPathResolverTests
{
    private readonly SandboxPathResolver _resolver = new();

    [Fact]
    public void Resolve_WithIsolationEnabled_ShouldBuildUserTaskPath()
    {
        var config = new SandboxConfigDto
        {
            WorkspaceRootPath = @"C:\\sandbox-root",
            TimeoutSeconds = 600,
            UserIsolationEnabled = true,
        };

        var taskId = Guid.NewGuid();
        var result = _resolver.Resolve(config, "alice", taskId, "github", "my-org", "repo-a");

        Assert.Contains(Path.Combine("users", "alice", "tasks", taskId.ToString("N")), result.EffectiveSandboxPath, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("github_my-org_repo-a", result.RepositoryPath, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Resolve_WithUnsafeSegments_ShouldSanitize()
    {
        var config = new SandboxConfigDto
        {
            WorkspaceRootPath = @"C:\\sandbox-root",
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
            WorkspaceRootPath = @"C:\\sandbox-root",
            TimeoutSeconds = 600,
            UserIsolationEnabled = true,
        };

        Assert.Throws<ValidationException>(() => _resolver.Resolve(config, string.Empty, Guid.NewGuid(), "github", "owner", "repo"));
    }
}
