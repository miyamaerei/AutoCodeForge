using AutoCodeForge.Infrastructure.Helpers;

namespace AutoCodeForge.Tests;

public sealed class PasswordHelperTests
{
    private readonly PasswordHelper _passwordHelper = new();

    [Fact]
    public void HashAndVerify_WithCorrectPassword_ShouldPass()
    {
        var hash = _passwordHelper.HashPassword("P@ssw0rd123");

        var ok = _passwordHelper.VerifyPassword("P@ssw0rd123", hash);

        Assert.True(ok);
    }

    [Fact]
    public void Verify_WithInvalidHashFormat_ShouldReturnFalse()
    {
        var ok = _passwordHelper.VerifyPassword("P@ssw0rd123", "invalid.hash.format");

        Assert.False(ok);
    }
}
