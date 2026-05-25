using System.Reflection;
using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Interfaces;
using AutoCodeForge.Infrastructure.BackgroundServices;
using AutoCodeForge.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using SqlSugar;

namespace AutoCodeForge.Tests;

public sealed class CronSchedulerServiceTests : IDisposable
{
    private readonly string _dbPath;
    private readonly ServiceProvider _serviceProvider;
    private readonly ISqlSugarClient _db;

    public CronSchedulerServiceTests()
    {
        _dbPath = Path.Combine(Path.GetTempPath(), $"autocodeforge.cronscheduler.{Guid.NewGuid():N}.db");
        _db = new SqlSugarScope(new ConnectionConfig
        {
            ConnectionString = $"Data Source={_dbPath}",
            DbType = DbType.Sqlite,
            IsAutoCloseConnection = true,
            InitKeyType = InitKeyType.Attribute,
        });

        _db.CodeFirst.InitTables(typeof(ScheduledTaskEntity), typeof(ScheduledTaskExecutionEntity), typeof(TaskEntity));

        var services = new ServiceCollection();
        services.AddSingleton(_db);
        services.AddSingleton<ICurrentUser>(new TestCurrentUser("scheduler.user"));
        services.AddScoped<ScheduledTaskRepository>();
        services.AddScoped<ScheduledTaskExecutionRepository>();
        services.AddScoped<TaskRepository>();

        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    public async Task SpawnTaskAsync_WithScheduledTask_CreatesTaskAndExecution()
    {
        ScheduledTaskEntity scheduled;
        TaskRepository taskRepository;
        ScheduledTaskExecutionRepository executionRepository;

        using (var scope = _serviceProvider.CreateScope())
        {
            var scheduledTaskRepository = scope.ServiceProvider.GetRequiredService<ScheduledTaskRepository>();
            taskRepository = scope.ServiceProvider.GetRequiredService<TaskRepository>();
            executionRepository = scope.ServiceProvider.GetRequiredService<ScheduledTaskExecutionRepository>();

            scheduled = await scheduledTaskRepository.CreateAsync(new ScheduledTaskEntity
            {
                Id = Guid.NewGuid(),
                Name = "Due Task",
                CronExpression = "0 */5 * * * ?",
                Status = ScheduleStatus.Active,
                TaskTitle = "Generated Task",
                Input = "{\"query\":\"health\"}",
                NextRunAtUtc = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            });
        }

        await InvokeSpawnTaskAsync(scheduled, DateTime.UtcNow, taskRepository, executionRepository, CancellationToken.None);

        var spawnedTasks = await _db.Queryable<TaskEntity>().ToListAsync();
        var executions = await _db.Queryable<ScheduledTaskExecutionEntity>().ToListAsync();

        Assert.Single(spawnedTasks);
        Assert.Single(executions);
        Assert.Equal("Generated Task", spawnedTasks[0].Title);
        Assert.Equal("{\"query\":\"health\"}", spawnedTasks[0].Input);
        Assert.Equal(scheduled.Id, executions[0].ScheduledTaskId);
        Assert.Equal("scheduler.user", spawnedTasks[0].NtId);
        Assert.Equal("scheduler.user", executions[0].NtId);
    }

    [Fact]
    public async Task TickAsync_WhenNoTaskDue_DoesNothing()
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var repo = scope.ServiceProvider.GetRequiredService<ScheduledTaskRepository>();
            await repo.CreateAsync(new ScheduledTaskEntity
            {
                Id = Guid.NewGuid(),
                Name = "Future Task",
                CronExpression = "0 */5 * * * ?",
                Status = ScheduleStatus.Active,
                TaskTitle = "Future Generated Task",
                Input = "payload",
                NextRunAtUtc = DateTime.UtcNow.AddHours(1),
            });
        }

        var scheduler = new CronSchedulerService(
            _serviceProvider.GetRequiredService<IServiceScopeFactory>(),
            NullLogger<CronSchedulerService>.Instance);

        await InvokeTickAsync(scheduler, CancellationToken.None);

        var spawnedCount = await _db.Queryable<TaskEntity>().CountAsync();
        var executionCount = await _db.Queryable<ScheduledTaskExecutionEntity>().CountAsync();

        Assert.Equal(0, spawnedCount);
        Assert.Equal(0, executionCount);
    }

    private static async Task InvokeTickAsync(CronSchedulerService scheduler, CancellationToken cancellationToken)
    {
        var tickMethod = typeof(CronSchedulerService).GetMethod("TickAsync", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(tickMethod);

        var tickTask = tickMethod!.Invoke(scheduler, new object[] { cancellationToken }) as Task;
        Assert.NotNull(tickTask);
        await tickTask!;
    }

    private static async Task InvokeSpawnTaskAsync(
        ScheduledTaskEntity scheduled,
        DateTime utcNow,
        TaskRepository taskRepository,
        ScheduledTaskExecutionRepository executionRepository,
        CancellationToken cancellationToken)
    {
        var spawnMethod = typeof(CronSchedulerService).GetMethod("SpawnTaskAsync", BindingFlags.Static | BindingFlags.NonPublic);
        Assert.NotNull(spawnMethod);

        var spawnTask = spawnMethod!.Invoke(
            null,
            new object[] { scheduled, utcNow, taskRepository, executionRepository, cancellationToken }) as Task;

        Assert.NotNull(spawnTask);
        await spawnTask!;
    }

    public void Dispose()
    {
        _serviceProvider.Dispose();

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
