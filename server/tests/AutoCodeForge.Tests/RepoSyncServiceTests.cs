using AutoCodeForge.Application.Services;
using AutoCodeForge.Application.Validators;
using AutoCodeForge.Core.DTOs.Config;
using AutoCodeForge.Core.DTOs.RepoSync;
using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Exceptions;
using AutoCodeForge.Core.Interfaces;
using AutoCodeForge.Infrastructure.Repositories;
using SqlSugar;

namespace AutoCodeForge.Tests;

public sealed class RepoSyncServiceTests : IDisposable
{
    private readonly string _dbPath;
    private readonly ISqlSugarClient _db;
    private readonly RepoSyncService _service;
    private readonly TaskRepository _taskRepository;

    public RepoSyncServiceTests()
    {
        _dbPath = Path.Combine(Path.GetTempPath(), $"autocodeforge.reposync.{Guid.NewGuid():N}.db");
        _db = new SqlSugarScope(new ConnectionConfig
        {
            ConnectionString = $"Data Source={_dbPath}",
            DbType = DbType.Sqlite,
            IsAutoCloseConnection = true,
            InitKeyType = InitKeyType.Attribute,
        });

        _db.CodeFirst.InitTables(
            typeof(TaskEntity),
            typeof(TaskLogEntity),
            typeof(UserConfigEntity),
            typeof(RepositoryEntity),
            typeof(RepoSandboxWorkspaceEntity),
            typeof(GlobalConfigEntity),
            typeof(ConfigurationEntry),
            typeof(ConfigHistoryEntity));

        var currentUser = new TestCurrentUser("repo.sync.user");
        var taskRepository = new TaskRepository(_db, currentUser);
        var taskLogRepository = new TaskLogRepository(_db, currentUser);
        var repositoryRepository = new RepositoryRepository(_db, currentUser);
        var workspaceRepository = new RepoSandboxWorkspaceRepository(_db, currentUser);
        var configService = new ConfigService(
            new ConfigRepository(_db, currentUser),
            new ConfigHistoryRepository(_db, currentUser),
            new EncryptionService("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA="),
            currentUser);

        _taskRepository = taskRepository;
        _service = new RepoSyncService(taskRepository, taskLogRepository, repositoryRepository, workspaceRepository, configService);

        SeedRepository(repositoryRepository).GetAwaiter().GetResult();
        configService.UpsertSandboxConfigAsync(new SandboxConfigDto
        {
            WorkspaceRootPath = @"C:\\sandbox-root",
            AllowedWritePaths = [@"C:\\sandbox-root\\users"],
            TimeoutSeconds = 120,
            UserIsolationEnabled = true,
        }).GetAwaiter().GetResult();
    }

    [Fact]
    public async Task CreateTaskAsync_ShouldCreateRepoSyncTask()
    {
        var response = await _service.CreateTaskAsync(new CreateRepoSyncTaskRequest
        {
            RepositoryId = _repositoryId,
            Branch = "main",
        });

        Assert.Equal("RepoSyncToSandbox", response.TaskType);
        Assert.Equal("Pending", response.Status);
    }

    [Fact]
    public async Task CancelTaskAsync_ShouldSetCanceledStatus()
    {
        var created = await _service.CreateTaskAsync(new CreateRepoSyncTaskRequest
        {
            RepositoryId = _repositoryId,
            Branch = "main",
        });

        var canceled = await _service.CancelTaskAsync(created.Id);
        var reloaded = await _taskRepository.GetByIdAsync(created.Id);

        Assert.Equal("Canceled", canceled.Status);
        Assert.NotNull(reloaded);
        Assert.Equal(AutoCodeForge.Core.Entities.TaskStatus.Canceled, reloaded!.Status);
    }

    [Fact]
    public async Task CancelTaskAsync_WhenAlreadyCompleted_ShouldThrow()
    {
        var created = await _service.CreateTaskAsync(new CreateRepoSyncTaskRequest
        {
            RepositoryId = _repositoryId,
            Branch = "main",
        });

        var task = await _taskRepository.GetByIdAsync(created.Id) ?? throw new ValidationException("Task not found");
        task.Status = AutoCodeForge.Core.Entities.TaskStatus.Completed;
        await _taskRepository.UpdateAsync(task);

        await Assert.ThrowsAsync<ValidationException>(() => _service.CancelTaskAsync(created.Id));
    }

    private Guid _repositoryId;

    private async Task SeedRepository(RepositoryRepository repositoryRepository)
    {
        var entity = new RepositoryEntity
        {
            Id = Guid.NewGuid(),
            Name = "repo-a",
            Url = "https://github.com/owner/repo-a.git",
            Provider = GitProvider.GitHub,
            AuthType = AuthenticationType.Token,
            EncryptedToken = "encrypted",
            MergeStrategy = MergeStrategy.Squash,
        };

        _repositoryId = entity.Id;
        await repositoryRepository.CreateAsync(entity);
    }

    public void Dispose()
    {
        if (_db is IDisposable disposable)
        {
            disposable.Dispose();
        }

        if (File.Exists(_dbPath))
        {
            try
            {
                File.Delete(_dbPath);
            }
            catch (IOException)
            {
                // Ignore transient sqlite lock on teardown.
            }
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
