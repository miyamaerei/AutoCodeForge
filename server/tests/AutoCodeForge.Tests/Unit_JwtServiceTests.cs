using AutoCodeForge.Application.Services;
using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Models;
using Microsoft.Extensions.Options;

namespace AutoCodeForge.Tests;

public sealed class JwtServiceTests
{
    private readonly JwtService _jwtService;

    public JwtServiceTests()
    {
        _jwtService = new JwtService(Options.Create(new JwtOptions
        {
            Issuer = "AutoCodeForge.Tests",
            Audience = "AutoCodeForge.Tests.Client",
            Key = "TEST_ONLY_CHANGE_ME_12345678901234567890",
            ExpireMinutes = 30,
        }));
    }

    [Fact]
    public void GenerateAndValidateToken_ShouldSucceed()
    {
        var user = new UserEntity
        {
            Id = Guid.NewGuid(),
            NtId = "jwt.user",
            UserName = "JWT User",
            PasswordHash = "not-used",
            IsDeleted = false,
        };

        var token = _jwtService.GenerateToken(user);
        var principal = _jwtService.ValidateToken(token.Token);

        Assert.NotNull(principal);
        Assert.Equal(user.NtId, principal!.FindFirst("NtId")?.Value);
    }

    [Fact]
    public void ValidateToken_WithGarbageToken_ShouldReturnNull()
    {
        var principal = _jwtService.ValidateToken("invalid-token");

        Assert.Null(principal);
    }
}
