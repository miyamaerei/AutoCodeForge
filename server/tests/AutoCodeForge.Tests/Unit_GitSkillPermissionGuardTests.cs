using AutoCodeForge.Application.Security;
using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Exceptions;
using AutoCodeForge.Core.Interfaces;
using AutoCodeForge.Infrastructure.Repositories;
using SqlSugar;

namespace AutoCodeForge.Tests;

public sealed class GitSkillPermissionGuardTests : IDisposable
{
    private readonly string _dbPath;
    private readonly ISqlSugarClient _db;
    private readonly GitSkillGrantRepository _grantRepository;
    private readonly GitSkillPermissionGuard _guard;

    public GitSkillPermissionGuardTests()
    {
        _dbPath = Path.Combine(Path.GetTempPath(), $"autocodeforge.gitskillguard.{Guid.NewGuid():N}.db");
        _db = new SqlSugarScope(new ConnectionConfig
        {
            ConnectionString = $"Data Source={_dbPath}",
            DbType = DbType.Sqlite,
            IsAutoCloseConnection = true,
            InitKeyType = InitKeyType.Attribute,
        });

        _db.CodeFirst.InitTables(typeof(GitSkillGrantEntity));

        _grantRepository = new GitSkillGrantRepository(_db, new TestCurrentUser("dev.user"));
        _guard = new GitSkillPermissionGuard(_grantRepository);
    }

    [Fact]
    public async Task EnsureAllowedAsync_WhenNoGrantAndReadOperation_ShouldAllow()
    {
        await _guard.EnsureAllowedAsync(Guid.NewGuid(), "list-branches", CancellationToken.None);
    }

    [Fact]
    public async Task EnsureAllowedAsync_WhenNoGrantAndWriteOperation_ShouldThrowForbidden()
    {
        await Assert.ThrowsAsync<ForbiddenException>(() =>
            _guard.EnsureAllowedAsync(Guid.NewGuid(), "create-pull-request", CancellationToken.None));
    }

    [Fact]
    public async Task EnsureAllowedAsync_WhenWriteGrantAndWriteOperation_ShouldAllow()
    {
        var repositoryId = Guid.NewGuid();
        await _grantRepository.CreateAsync(new GitSkillGrantEntity
        {
            Id = Guid.NewGuid(),
            RepositoryId = repositoryId,
            Level = "Write",
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
        });

        await _guard.EnsureAllowedAsync(repositoryId, "create-pull-request", CancellationToken.None);
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

        public bool IsAdmin()
        {
            return false;
        }
    }
}
