using AutoCodeForge.Application.Services;
using AutoCodeForge.Core.DTOs.Auth;
using AutoCodeForge.Core.Exceptions;
using AutoCodeForge.Core.Interfaces;
using AutoCodeForge.Core.Models;
using AutoCodeForge.Infrastructure.Helpers;
using AutoCodeForge.Infrastructure.Repositories;
using Microsoft.Extensions.Options;
using SqlSugar;

namespace AutoCodeForge.Tests;

public sealed class AuthServiceTests : IDisposable
{
    private readonly string _dbPath;
    private readonly ISqlSugarClient _db;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _dbPath = Path.Combine(Path.GetTempPath(), $"autocodeforge.authservice.{Guid.NewGuid():N}.db");
        _db = new SqlSugarScope(new ConnectionConfig
        {
            ConnectionString = $"Data Source={_dbPath}",
            DbType = DbType.Sqlite,
            IsAutoCloseConnection = true,
            InitKeyType = InitKeyType.Attribute,
        });

        _db.CodeFirst.InitTables(typeof(AutoCodeForge.Core.Entities.UserEntity));

        var jwtOptions = Options.Create(new JwtOptions
        {
            Issuer = "AutoCodeForge.Tests",
            Audience = "AutoCodeForge.Tests.Client",
            Key = "TEST_ONLY_CHANGE_ME_12345678901234567890",
            ExpireMinutes = 60,
        });

        var userRepository = new UserRepository(_db, new TestCurrentUser(null));
        var passwordHelper = new PasswordHelper();
        var jwtService = new JwtService(jwtOptions);
        _authService = new AuthService(userRepository, passwordHelper, jwtService);
    }

    [Fact]
    public async Task RegisterThenLogin_ShouldReturnTokenAndProfile()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var request = new RegisterRequest
        {
            NtId = $"user.{suffix}",
            UserName = "Test User",
            Email = "test.user@example.com",
            Password = "P@ssw0rd123",
        };

        var register = await _authService.RegisterAsync(request);
        var login = await _authService.LoginAsync(new LoginRequest
        {
            NtId = request.NtId,
            Password = request.Password,
        });

        Assert.False(string.IsNullOrWhiteSpace(register.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(login.AccessToken));
        Assert.Equal(request.NtId, login.NtId);
        Assert.Equal(request.UserName, login.UserName);
        Assert.True(login.ExpiresAtUtc > DateTime.UtcNow);
    }

    [Fact]
    public async Task Login_WithWrongPassword_ShouldThrowUnauthorized()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        await _authService.RegisterAsync(new RegisterRequest
        {
            NtId = $"user.{suffix}",
            UserName = "Test User",
            Password = "P@ssw0rd123",
        });

        await Assert.ThrowsAsync<UnauthorizedException>(() => _authService.LoginAsync(new LoginRequest
        {
            NtId = $"user.{suffix}",
            Password = "wrong-password",
        }));
    }

    [Fact]
    public async Task GetCurrentUser_WhenMissing_ShouldThrowNotFound()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _authService.GetCurrentUserAsync("missing.user"));
    }

    public void Dispose()
    {
        if (_db is IDisposable disposable)
        {
            disposable.Dispose();
        }

        if (!File.Exists(_dbPath))
        {
            return;
        }

        try
        {
            File.Delete(_dbPath);
        }
        catch (IOException)
        {
            // The OS can still hold a short-lived sqlite lock at teardown.
        }
    }

    private sealed class TestCurrentUser : ICurrentUser
    {
        private readonly string? _ntId;

        public TestCurrentUser(string? ntId)
        {
            _ntId = ntId;
        }

        public string? GetCurrentNtId()
        {
            return _ntId;
        }
    }
}
