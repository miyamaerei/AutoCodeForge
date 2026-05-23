using AutoCodeForge.Application.Services;
using AutoCodeForge.Core.DTOs.Task;
using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Exceptions;
using AutoCodeForge.Core.Interfaces;
using AutoCodeForge.Infrastructure.Repositories;
using SqlSugar;

namespace AutoCodeForge.Tests;

public sealed class TaskStepServiceTests : IDisposable
{
    private readonly string _dbPath;
    private readonly ISqlSugarClient _db;
    private readonly TaskStepService _taskStepService;
    private readonly TaskRepository _taskRepository;
    private readonly TaskStepRepository _taskStepRepository;

    public TaskStepServiceTests()
    {
        _dbPath = Path.Combine(Path.GetTempPath(), $"autocodeforge.taskstepservice.{Guid.NewGuid():N}.db");
        _db = new SqlSugarScope(new ConnectionConfig
        {
            ConnectionString = $"Data Source={_dbPath}",
            DbType = DbType.Sqlite,
            IsAutoCloseConnection = true,
            InitKeyType = InitKeyType.Attribute,
        });

        _db.CodeFirst.InitTables(typeof(TaskEntity), typeof(TaskStepEntity));

        var currentUser = new TestCurrentUser("taskstep.user");
        _taskRepository = new TaskRepository(_db, currentUser);
        _taskStepRepository = new TaskStepRepository(_db, currentUser);
        _taskStepService = new TaskStepService(_taskStepRepository, _taskRepository, _db);
    }

    [Fact]
    public async Task InitializeSteps_ShouldCreateAll7Steps()
    {
        // Arrange
        var task = await CreateTestTaskAsync();

        // Act
        var steps = await _taskStepService.InitializeStepsAsync(task.Id);

        // Assert
        Assert.Equal(7, steps.Count);
        Assert.Equal(TaskStepType.DemandAnalyse.ToString(), steps[0].StepType);
        Assert.Equal(TaskStepType.QueryCurrent.ToString(), steps[1].StepType);
        Assert.Equal(TaskStepType.MakePlan.ToString(), steps[2].StepType);
        Assert.Equal(TaskStepType.Development.ToString(), steps[3].StepType);
        Assert.Equal(TaskStepType.TestVerify.ToString(), steps[4].StepType);
        Assert.Equal(TaskStepType.CommitPr.ToString(), steps[5].StepType);
        Assert.Equal(TaskStepType.FinalAudit.ToString(), steps[6].StepType);

        // Check task current step
        var updatedTask = await _taskRepository.GetByIdAsync(task.Id);
        Assert.NotNull(updatedTask);
        Assert.Equal(1, updatedTask.CurrentStep);
        Assert.Equal(steps[0].Id, updatedTask.CurrentStepId);
    }

    [Fact]
    public async Task InitializeSteps_ForNonExistentTask_ShouldThrowNotFoundException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => 
            _taskStepService.InitializeStepsAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task AdvanceStep_ShouldUpdateStatusAndMoveToNext()
    {
        // Arrange
        var task = await CreateTestTaskAsync();
        var steps = await _taskStepService.InitializeStepsAsync(task.Id);
        
        // 我们需要把第一个步骤状态更新为 Handling
        var firstStepEntity = await _taskStepRepository.GetByIdAsync(steps[0].Id);
        Assert.NotNull(firstStepEntity);
        firstStepEntity.Status = TaskStepStatus.Handling;
        firstStepEntity.StartedAtUtc = DateTime.UtcNow;
        await _taskStepRepository.UpdateAsync(firstStepEntity);

        // Act
        var nextStepResult = await _taskStepService.AdvanceStepAsync(task.Id, steps[0].Id, new AdvanceTaskStepRequest
        {
            Output = "Requirement analysis complete"
        });

        // Assert - 验证返回的是下一个步骤
        Assert.Equal(2, nextStepResult.Step);
        
        // 验证原步骤已完成
        var updatedFirstStep = await _taskStepRepository.GetByIdAsync(steps[0].Id);
        Assert.NotNull(updatedFirstStep);
        Assert.Equal(TaskStepStatus.Completed, updatedFirstStep.Status);
        Assert.Equal("Requirement analysis complete", updatedFirstStep.Output);

        // 检查任务当前步骤现在是 2
        var updatedTask = await _taskRepository.GetByIdAsync(task.Id);
        Assert.NotNull(updatedTask);
        Assert.Equal(2, updatedTask.CurrentStep);
    }

    [Fact]
    public async Task SkipStep_ShouldMarkAsSkippedAndMoveToNext()
    {
        // Arrange
        var task = await CreateTestTaskAsync();
        var steps = await _taskStepService.InitializeStepsAsync(task.Id);
        
        // 首先让第一个步骤状态为 Handling（活跃状态）
        var firstStepEntity = await _taskStepRepository.GetByIdAsync(steps[0].Id);
        Assert.NotNull(firstStepEntity);
        firstStepEntity.Status = TaskStepStatus.Handling;
        firstStepEntity.StartedAtUtc = DateTime.UtcNow;
        await _taskStepRepository.UpdateAsync(firstStepEntity);

        // Act - 跳过步骤 2（活跃步骤 1 的下一个步骤）
        var nextStepResult = await _taskStepService.SkipStepAsync(task.Id, steps[1].Id, new SkipTaskStepRequest
        {
            Reason = "Skipping for test"
        });

        // Assert - 验证返回的是步骤 3
        Assert.Equal(3, nextStepResult.Step);
        
        // 验证步骤 2 已被标记为跳过
        var skippedStep = await _taskStepRepository.GetByIdAsync(steps[1].Id);
        Assert.NotNull(skippedStep);
        Assert.Equal(TaskStepStatus.Skipped, skippedStep.Status);
        Assert.Equal("Skipping for test", skippedStep.SkipReason);

        // 验证任务当前步骤现在是 3
        var updatedTask = await _taskRepository.GetByIdAsync(task.Id);
        Assert.NotNull(updatedTask);
        Assert.Equal(3, updatedTask.CurrentStep);
    }

    [Fact]
    public async Task UnbindStep_ShouldMarkAsFailedForEarlyStep()
    {
        // Arrange
        var task = await CreateTestTaskAsync();
        var steps = await _taskStepService.InitializeStepsAsync(task.Id);
        
        // 更新步骤为 Handling 状态
        var firstStepEntity = await _taskStepRepository.GetByIdAsync(steps[0].Id);
        Assert.NotNull(firstStepEntity);
        firstStepEntity.Status = TaskStepStatus.Handling;
        firstStepEntity.StartedAtUtc = DateTime.UtcNow.AddMinutes(-40);
        await _taskStepRepository.UpdateAsync(firstStepEntity);

        // Act
        var result = await _taskStepService.UnbindStepAsync(task.Id, steps[0].Id, new UnbindTaskStepRequest
        {
            Reason = "Step timed out",
            FailureCategory = "Timeout"
        });

        // Assert
        Assert.Equal(TaskStepStatus.Failed.ToString(), result.Status);
        
        // 检查任务是否失败
        var updatedTask = await _taskRepository.GetByIdAsync(task.Id);
        Assert.NotNull(updatedTask);
        Assert.Equal(Core.Entities.TaskStatus.Failed, updatedTask.Status);
    }

    [Fact]
    public async Task BuildContext_ShouldIncludeCompletedStepsOutput()
    {
        // Arrange
        var task = await CreateTestTaskAsync();
        var steps = await _taskStepService.InitializeStepsAsync(task.Id);
        
        // 完成第一个步骤
        var firstStepEntity = await _taskStepRepository.GetByIdAsync(steps[0].Id);
        Assert.NotNull(firstStepEntity);
        firstStepEntity.Status = TaskStepStatus.Completed;
        firstStepEntity.Output = "First step output";
        await _taskStepRepository.UpdateAsync(firstStepEntity);

        // Act
        var context = await _taskStepService.BuildContextAsync(task.Id, steps[1].Id);

        // Assert
        Assert.Contains("Original Input", context);
        Assert.Contains("First step output", context);
    }

    [Fact]
    public async Task GetActiveStep_ShouldReturnCurrentActiveStep()
    {
        // Arrange
        var task = await CreateTestTaskAsync();
        var steps = await _taskStepService.InitializeStepsAsync(task.Id);

        // 首先让第一个步骤状态为 Handling
        var firstStepEntity = await _taskStepRepository.GetByIdAsync(steps[0].Id);
        Assert.NotNull(firstStepEntity);
        firstStepEntity.Status = TaskStepStatus.Handling;
        await _taskStepRepository.UpdateAsync(firstStepEntity);

        // Act
        var activeStep = await _taskStepService.GetActiveStepAsync(task.Id);

        // Assert
        Assert.NotNull(activeStep);
        Assert.Equal(1, activeStep.Step);
    }

    [Fact]
    public async Task GetByTaskId_ShouldReturnAllSteps()
    {
        // Arrange
        var task = await CreateTestTaskAsync();
        await _taskStepService.InitializeStepsAsync(task.Id);

        // Act
        var steps = await _taskStepService.GetByTaskIdAsync(task.Id);

        // Assert
        Assert.Equal(7, steps.Count);
    }

    [Fact]
    public async Task UpdateStep_ShouldUpdateStepDetails()
    {
        // Arrange
        var task = await CreateTestTaskAsync();
        var steps = await _taskStepService.InitializeStepsAsync(task.Id);

        // Act
        var result = await _taskStepService.UpdateAsync(steps[0].Id, new UpdateTaskStepRequest
        {
            Input = "Updated input",
            Output = "Updated output"
        });

        // Assert
        Assert.Equal("Updated input", result.Input);
        Assert.Equal("Updated output", result.Output);
    }

    [Fact]
    public async Task GetTimeoutSteps_ShouldReturnStepsOverTimeoutThreshold()
    {
        // Arrange
        var task = await CreateTestTaskAsync();
        var steps = await _taskStepService.InitializeStepsAsync(task.Id);
        
        var firstStepEntity = await _taskStepRepository.GetByIdAsync(steps[0].Id);
        Assert.NotNull(firstStepEntity);
        firstStepEntity.Status = TaskStepStatus.Handling;
        firstStepEntity.StartedAtUtc = DateTime.UtcNow.AddMinutes(-60);
        await _taskStepRepository.UpdateAsync(firstStepEntity);

        // Act
        var timeoutSteps = await _taskStepRepository.GetTimeoutStepsAsync(30);

        // Assert
        Assert.Single(timeoutSteps);
        Assert.Equal(firstStepEntity.Id, timeoutSteps[0].Id);
    }

    private async Task<TaskEntity> CreateTestTaskAsync()
    {
        var task = new TaskEntity
        {
            Id = Guid.NewGuid(),
            Title = "Test Task",
            Description = "Test Description",
            Status = Core.Entities.TaskStatus.Pending,
            TaskType = Core.Entities.TaskType.General,
            Progress = 0,
            Input = "test input",
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };
        return await _taskRepository.CreateAsync(task);
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
