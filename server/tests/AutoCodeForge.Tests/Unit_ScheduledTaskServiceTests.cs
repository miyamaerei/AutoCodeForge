using AutoCodeForge.Application.Services;
using AutoCodeForge.Core.DTOs.ScheduledTask;
using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Exceptions;
using AutoCodeForge.Core.Interfaces;
using AutoCodeForge.Infrastructure.Repositories;
using SqlSugar;

namespace AutoCodeForge.Tests;

public sealed class ScheduledTaskServiceTests : IDisposable
{
    private readonly string _dbPath;
    private readonly ISqlSugarClient _db;
    private readonly ScheduledTaskService _service;

    public ScheduledTaskServiceTests()
    {
        _dbPath = Path.Combine(Path.GetTempPath(), $"autocodeforge.scheduledtask.{Guid.NewGuid():N}.db");
        _db = new SqlSugarScope(new ConnectionConfig
        {
            ConnectionString = $"Data Source={_dbPath}",
            DbType = DbType.Sqlite,
            IsAutoCloseConnection = true,
            InitKeyType = InitKeyType.Attribute,
        });

        _db.CodeFirst.InitTables(typeof(ScheduledTaskEntity), typeof(ScheduledTaskExecutionEntity));

        var currentUser = new TestCurrentUser("scheduler.user");
        var scheduledTaskRepository = new ScheduledTaskRepository(_db, currentUser);
        var executionRepository = new ScheduledTaskExecutionRepository(_db, currentUser);
        _service = new ScheduledTaskService(scheduledTaskRepository, executionRepository);
    }

    [Fact]
    public async Task CreateAsync_WithValidCron_TrimsFieldsAndCalculatesNextRun()
    {
        var response = await _service.CreateAsync(new CreateScheduledTaskRequest
        {
            Name = "  Every Minute  ",
            CronExpression = "0 */1 * * * ?",
            TriggerType = TriggerType.Cron,
            Input = "  payload  ",
            TaskTitle = "  Nightly Run  ",
        });

        Assert.Equal("Every Minute", response.Name);
        Assert.Equal("payload", response.Input);
        Assert.Equal("Nightly Run", response.TaskTitle);
        Assert.Equal(ScheduleStatus.Active.ToString(), response.Status);
        Assert.NotNull(response.NextRunAtUtc);
        Assert.True(response.NextRunAtUtc > DateTime.UtcNow.AddMinutes(-1));
    }

    [Fact]
    public async Task CreateAsync_WithInvalidCron_ThrowsValidationException()
    {
        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            _service.CreateAsync(new CreateScheduledTaskRequest
            {
                Name = "Broken",
                CronExpression = "invalid cron",
                Input = "payload",
                TaskTitle = "title",
            }));

        Assert.Contains("Invalid Cron expression", exception.Message);
    }

    [Fact]
    public async Task PauseAndResume_ShouldEnforceStateMachine()
    {
        var created = await _service.CreateAsync(new CreateScheduledTaskRequest
        {
            Name = "State Test",
            CronExpression = "0 */1 * * * ?",
            Input = "payload",
            TaskTitle = "state task",
        });

        var paused = await _service.PauseAsync(created.Id);
        Assert.Equal(ScheduleStatus.Paused.ToString(), paused.Status);

        await Assert.ThrowsAsync<ValidationException>(() => _service.PauseAsync(created.Id));

        var resumed = await _service.ResumeAsync(created.Id);
        Assert.Equal(ScheduleStatus.Active.ToString(), resumed.Status);
        Assert.NotNull(resumed.NextRunAtUtc);

        await Assert.ThrowsAsync<ValidationException>(() => _service.ResumeAsync(created.Id));
    }

    [Fact]
    public void CalculateNextRun_WithInvalidCron_ReturnsNull()
    {
        var nextRun = ScheduledTaskService.CalculateNextRun("***", DateTime.UtcNow);
        Assert.Null(nextRun);
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
            // SQLite file locks can outlive test teardown briefly.
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
