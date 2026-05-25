using AutoCodeForge.Application.Services;
using AutoCodeForge.Core.DTOs.Repository;
using AutoCodeForge.Core.DTOs.Review;
using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Interfaces;
using AutoCodeForge.Infrastructure.BackgroundServices.Handlers;
using AutoCodeForge.Infrastructure.Repositories;
using AutoCodeForge.Infrastructure.Review;
using Microsoft.Extensions.Logging.Abstractions;
using SqlSugar;

namespace AutoCodeForge.Tests;

public sealed class ReviewServiceTests : IDisposable
{
    private readonly string _dbPath;
    private readonly string _workspaceRoot;
    private readonly ISqlSugarClient _db;
    private readonly ReviewService _service;
    private readonly ReviewRuleSetService _reviewRuleSetService;
    private readonly RepositoryReviewSettingsService _repositoryReviewSettingsService;
    private readonly ReviewTaskHandler _handler;
    private readonly TaskRepository _taskRepository;
    private readonly ReviewRepository _reviewRepository;
    private Guid _repositoryId;

    public ReviewServiceTests()
    {
        _dbPath = Path.Combine(Path.GetTempPath(), $"autocodeforge.reviewservice.{Guid.NewGuid():N}.db");
        _workspaceRoot = Path.Combine(Path.GetTempPath(), $"autocodeforge.review.workspace.{Guid.NewGuid():N}");
        Directory.CreateDirectory(_workspaceRoot);

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
            typeof(RepositoryEntity),
            typeof(RepoSandboxWorkspaceEntity),
            typeof(ReviewRuleSetEntity),
            typeof(ReviewTaskEntity),
            typeof(ReviewFindingEntity));

        var currentUser = new TestCurrentUser("review.user");
        _taskRepository = new TaskRepository(_db, currentUser);
        var taskLogRepository = new TaskLogRepository(_db, currentUser);
        var repositoryRepository = new RepositoryRepository(_db, currentUser);
        var workspaceRepository = new RepoSandboxWorkspaceRepository(_db, currentUser);
        _reviewRepository = new ReviewRepository(_db, currentUser);
        var reviewRuleSetRepository = new ReviewRuleSetRepository(_db, currentUser);

        _service = new ReviewService(
            _taskRepository,
            taskLogRepository,
            repositoryRepository,
            workspaceRepository,
            _reviewRepository,
            reviewRuleSetRepository);

        _reviewRuleSetService = new ReviewRuleSetService(reviewRuleSetRepository, repositoryRepository);
        _repositoryReviewSettingsService = new RepositoryReviewSettingsService(repositoryRepository, reviewRuleSetRepository);

        _handler = new ReviewTaskHandler(
            _taskRepository,
            taskLogRepository,
            _reviewRepository,
            new RuleBasedReviewEngine(),
            NullLogger<ReviewTaskHandler>.Instance);

        SeedRepository(repositoryRepository, workspaceRepository).GetAwaiter().GetResult();
    }

    [Fact]
    public async Task CreateTaskAndExecuteAsync_ShouldPersistFindings()
    {
        var created = await _service.CreateTaskAsync(new CreateReviewTaskRequest
        {
            RepositoryId = _repositoryId,
            Branch = "main",
        });

        var task = await _taskRepository.GetByIdAsync(created.TaskId) ?? throw new InvalidOperationException("Task not found");
        await _handler.ExecuteAsync(task);

        var detail = await _service.GetTaskAsync(created.TaskId);
        var findings = await _service.GetFindingsAsync(created.TaskId);

        Assert.Equal("Completed", detail.Status);
        Assert.Single(findings);
        Assert.Equal("src/Sample.cs", findings[0].FilePath);
        Assert.Equal("REVIEW_TODO_001", findings[0].RuleCode);
        Assert.Equal(1, detail.TotalFindings);
    }

    [Fact]
    public async Task CancelTaskAsync_WhenPending_ShouldSetCanceledStatus()
    {
        var created = await _service.CreateTaskAsync(new CreateReviewTaskRequest
        {
            RepositoryId = _repositoryId,
        });

        var canceled = await _service.CancelTaskAsync(created.TaskId);

        Assert.Equal("Canceled", canceled.Status);
        Assert.Equal("Review task canceled by user", canceled.ErrorMessage);
    }

    [Fact]
    public async Task RerunTaskAsync_WhenCompleted_ShouldResetTaskState()
    {
        var created = await _service.CreateTaskAsync(new CreateReviewTaskRequest
        {
            RepositoryId = _repositoryId,
        });

        var task = await _taskRepository.GetByIdAsync(created.TaskId) ?? throw new InvalidOperationException("Task not found");
        await _handler.ExecuteAsync(task);

        var rerun = await _service.RerunTaskAsync(created.TaskId);
        var findings = await _service.GetFindingsAsync(created.TaskId);

        Assert.Equal("Pending", rerun.Status);
        Assert.Equal(0, rerun.TotalFindings);
        Assert.Empty(findings);
    }

    [Fact]
    public async Task RuleSetCrudAndRepositoryBinding_ShouldWork()
    {
        var ruleSet = await _reviewRuleSetService.CreateAsync(new CreateReviewRuleSetRequest
        {
            Name = "Repo policy",
            Description = "Repository-scoped rules",
            Level = ReviewRuleSetLevel.Repository,
            RepositoryId = _repositoryId,
            Version = "1.2.0",
            Rules =
            [
                new ReviewRuleDto
                {
                    Code = "SEC002",
                    Name = "Token literal",
                    Severity = ReviewFindingSeverity.High,
                    FilePattern = "*",
                    ContainsText = "token=",
                    Message = "Potential token literal detected",
                },
            ],
        });

        var updated = await _reviewRuleSetService.UpdateAsync(ruleSet.Id, new UpdateReviewRuleSetRequest
        {
            Description = "Updated description",
            IsEnabled = true,
        });

        var binding = await _repositoryReviewSettingsService.UpdateAsync(_repositoryId, new UpdateRepositoryReviewSettingsRequest
        {
            DefaultReviewRuleSetId = ruleSet.Id,
        });

        var paged = await _reviewRuleSetService.GetPagedAsync(1, 10, _repositoryId);

        Assert.Equal("Updated description", updated.Description);
        Assert.Equal(ruleSet.Id, binding.DefaultReviewRuleSetId);
        Assert.Contains(paged.Items, item => item.Id == ruleSet.Id);
    }

    [Fact]
    public async Task GetRepositoryTasksAsync_ShouldReturnCreatedTask()
    {
        var created = await _service.CreateTaskAsync(new CreateReviewTaskRequest
        {
            RepositoryId = _repositoryId,
        });

        var paged = await _service.GetRepositoryTasksAsync(_repositoryId, 1, 10);

        var item = Assert.Single(paged.Items);
        Assert.Equal(created.TaskId, item.TaskId);
        Assert.Equal(_repositoryId, item.RepositoryId);
    }

    private async Task SeedRepository(
        RepositoryRepository repositoryRepository,
        RepoSandboxWorkspaceRepository workspaceRepository)
    {
        var repoDirectory = Directory.CreateDirectory(Path.Combine(_workspaceRoot, "repo-a"));
        var sourceDirectory = Directory.CreateDirectory(Path.Combine(repoDirectory.FullName, "src"));
        await File.WriteAllTextAsync(Path.Combine(sourceDirectory.FullName, "Sample.cs"), "// TODO: follow up\npublic class Sample { }\n");

        var repository = new RepositoryEntity
        {
            Id = Guid.NewGuid(),
            Name = "repo-a",
            Url = "https://github.com/owner/repo-a.git",
            Provider = GitProvider.GitHub,
            AuthType = AuthenticationType.Token,
            EncryptedToken = "encrypted",
            MergeStrategy = MergeStrategy.Squash,
        };

        _repositoryId = repository.Id;
        await repositoryRepository.CreateAsync(repository);

        await workspaceRepository.CreateAsync(new RepoSandboxWorkspaceEntity
        {
            Id = Guid.NewGuid(),
            TaskId = Guid.NewGuid(),
            RepositoryId = _repositoryId,
            WorkspaceRootPath = _workspaceRoot,
            EffectiveSandboxPath = _workspaceRoot,
            RelativeRepoPath = "repo-a",
            Branch = "main",
            CommitSha = "abc123",
            Status = RepoSandboxWorkspaceStatus.Pulled,
            FinishedAtUtc = DateTime.UtcNow,
        });
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

        if (Directory.Exists(_workspaceRoot))
        {
            try
            {
                Directory.Delete(_workspaceRoot, true);
            }
            catch (IOException)
            {
                // Ignore transient file locks on teardown.
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