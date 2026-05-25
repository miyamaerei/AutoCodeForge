using AutoCodeForge.Application.Services;
using AutoCodeForge.Core.DTOs.Task;
using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Exceptions;
using AutoCodeForge.Core.Interfaces;
using AutoCodeForge.Infrastructure.Repositories;
using SqlSugar;

namespace AutoCodeForge.Tests;

public sealed class TaskServiceTests : IDisposable
{
    private readonly string _dbPath;
    private readonly ISqlSugarClient _db;
    private readonly TaskService _taskService;

    public TaskServiceTests()
    {
        _dbPath = Path.Combine(Path.GetTempPath(), $"autocodeforge.taskservice.{Guid.NewGuid():N}.db");
        _db = new SqlSugarScope(new ConnectionConfig
        {
            ConnectionString = $"Data Source={_dbPath}",
            DbType = DbType.Sqlite,
            IsAutoCloseConnection = true,
            InitKeyType = InitKeyType.Attribute,
        });

        _db.CodeFirst.InitTables(typeof(TaskEntity), typeof(TaskLogEntity));

        var currentUser = new TestCurrentUser("task.user");
        var taskRepository = new TaskRepository(_db, currentUser);
        var taskLogRepository = new TaskLogRepository(_db, currentUser);
        _taskService = new TaskService(taskRepository, taskLogRepository);
    }

    [Fact]
    public async Task CreateAndReadTask_ShouldReturnPendingTask()
    {
        var created = await _taskService.CreateAsync(new CreateTaskRequest
        {
            Title = "Task A",
            Description = "desc",
            Input = "hello",
        });

        var loaded = await _taskService.GetByIdAsync(created.Id);
        var logs = await _taskService.GetLogsAsync(created.Id);

        Assert.Equal("Task A", loaded.Title);
        Assert.Equal(AutoCodeForge.Core.Entities.TaskStatus.Pending.ToString(), loaded.Status);
        Assert.Equal("hello", loaded.Input);
        Assert.Single(logs);
        Assert.Equal("Task created", logs[0].Message);
    }

    [Fact]
    public async Task TryStartThenComplete_ShouldUpdateStatusAndResult()
    {
        var created = await _taskService.CreateAsync(new CreateTaskRequest
        {
            Title = "Task B",
            Input = "execute",
        });

        var started = await _taskService.TryStartAsync(created.Id);
        await _taskService.MarkCompletedAsync(created.Id, "{\"output\":\"ok\"}");
        var loaded = await _taskService.GetByIdAsync(created.Id);

        Assert.True(started);
        Assert.Equal(AutoCodeForge.Core.Entities.TaskStatus.Completed.ToString(), loaded.Status);
        Assert.Equal(100, loaded.Progress);
        Assert.NotNull(loaded.CompletedAtUtc);
        Assert.Equal("{\"output\":\"ok\"}", loaded.Result);
    }

    [Fact]
    public async Task ResumeWithoutPause_ShouldThrowValidation()
    {
        var created = await _taskService.CreateAsync(new CreateTaskRequest
        {
            Title = "Task C",
            Input = "run",
        });

        await Assert.ThrowsAsync<ValidationException>(() => _taskService.ResumeAsync(created.Id));
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

        public bool IsAdmin()
        {
            return false;
        }
    }
}
