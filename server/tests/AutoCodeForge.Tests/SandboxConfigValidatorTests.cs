using AutoCodeForge.Application.Validators;
using AutoCodeForge.Core.DTOs.Config;
using AutoCodeForge.Core.Exceptions;

namespace AutoCodeForge.Tests;

public sealed class SandboxConfigValidatorTests
{
    private readonly SandboxConfigValidator _validator = new();

    [Fact]
    public void ValidateAndThrow_WithValidConfig_ShouldPass()
    {
        var config = new SandboxConfigDto
        {
            WorkspaceRootPath = @"C:\\sandbox-root",
            AllowedWritePaths = [@"C:\\sandbox-root\\users", @"C:\\sandbox-root\\cache"],
            TimeoutSeconds = 600,
            UserIsolationEnabled = true,
        };

        _validator.ValidateAndThrow(config);
    }

    [Fact]
    public void ValidateAndThrow_WithRelativeRoot_ShouldThrow()
    {
        var config = new SandboxConfigDto
        {
            WorkspaceRootPath = "relative\\path",
            TimeoutSeconds = 600,
        };

        Assert.Throws<ValidationException>(() => _validator.ValidateAndThrow(config));
    }

    [Fact]
    public void ValidateAndThrow_WithAllowedPathOutsideRoot_ShouldThrow()
    {
        var config = new SandboxConfigDto
        {
            WorkspaceRootPath = @"C:\\sandbox-root",
            AllowedWritePaths = [@"C:\\other-root"],
            TimeoutSeconds = 600,
        };

        Assert.Throws<ValidationException>(() => _validator.ValidateAndThrow(config));
    }
}
