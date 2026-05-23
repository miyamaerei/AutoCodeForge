using AutoCodeForge.Application.Services;
using AutoCodeForge.Core.DTOs.AI.GitTools;
using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Exceptions;
using AutoCodeForge.Core.Interfaces;
using AutoCodeForge.Infrastructure.Repositories;
using SqlSugar;

namespace AutoCodeForge.Tests;

public sealed class GitSkillPolicyServiceTests : IDisposable
{
    private readonly string _dbPath;
    private readonly ISqlSugarClient _db;
    private readonly GitSkillPolicyService _service;

    public GitSkillPolicyServiceTests()
    {
        _dbPath = Path.Combine(Path.GetTempPath(), $"autocodeforge.gitskillpolicy.{Guid.NewGuid():N}.db");
        _db = new SqlSugarScope(new ConnectionConfig
        {
            ConnectionString = $"Data Source={_dbPath}",
            DbType = DbType.Sqlite,
            IsAutoCloseConnection = true,
            InitKeyType = InitKeyType.Attribute,
        });

        _db.CodeFirst.InitTables(typeof(GitSkillGrantEntity));

        var repository = new GitSkillGrantRepository(_db, new TestCurrentUser("dev.user"));
        _service = new GitSkillPolicyService(repository);
    }

    [Fact]
    public async Task GetByRepositoryAsync_WhenMissing_ShouldReturnReadOnlyDefault()
    {
        var result = await _service.GetByRepositoryAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.Equal("ReadOnly", result.Level);
    }

    [Fact]
    public async Task UpsertAsync_WithValidLevel_ShouldPersist()
    {
        var repositoryId = Guid.NewGuid();
        var result = await _service.UpsertAsync(repositoryId, new UpdateGitSkillGrantRequest
        {
            Level = "Write",
        }, CancellationToken.None);

        Assert.Equal(repositoryId, result.RepositoryId);
        Assert.Equal("Write", result.Level);
    }

    [Fact]
    public async Task UpsertAsync_WithInvalidLevel_ShouldThrowValidationException()
    {
        await Assert.ThrowsAsync<ValidationException>(() =>
            _service.UpsertAsync(Guid.NewGuid(), new UpdateGitSkillGrantRequest
            {
                Level = "UnknownLevel",
            }, CancellationToken.None));
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
