/**
 * TaskStepFlowService 工序流转引擎测试
 *
 * 测试覆盖：
 * 1. InitializeStepsAsync - 初始化7步工序
 * 2. MoveToNextStepAsync - 推进工序
 * 3. RejectStepAsync - 驳回重试
 * 4. MoveToNextStepAsync - 步骤不存在异常
 */

using AutoCodeForge.Application.Services;
using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Exceptions;
using AutoCodeForge.Infrastructure.Repositories;

namespace AutoCodeForge.Tests;

/// <summary>
/// TaskStepFlowService 工序流转引擎测试
/// </summary>
public sealed class Unit_TaskStepFlowServiceTests : IDisposable
{
    private readonly IntegrationTestContext _context;
    private readonly TaskStepFlowService _service;

    public Unit_TaskStepFlowServiceTests()
    {
        _context = new IntegrationTestContext("test-user");
        
        var taskReviewService = new TaskReviewService(
            new TaskReviewRepository(_context.Db, _context.CurrentUser),
            _context.TaskStepRepository,
            _context.AgentRepository);
        
        _service = new TaskStepFlowService(
            _context.TaskStepRepository,
            _context.TaskRepository,
            taskReviewService,
            _context.HumanGateService);
    }

    [Fact]
    public async Task InitializeStepsAsync_Should_CreateSevenSteps()
    {
        var taskId = Guid.NewGuid();

        await _context.TaskRepository.CreateAsync(new TaskEntity
        {
            Id = taskId,
            Title = "Test Task",
            Status = Core.Entities.TaskStatus.Pending,
            IsDeleted = false,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
        });

        await _service.InitializeStepsAsync(taskId);

        var steps = await _context.TaskStepRepository.GetAllAsync();
        Assert.Equal(7, steps.Count);
        Assert.All(steps, step => Assert.Equal(taskId, step.TaskId));
    }

    [Fact]
    public async Task MoveToNextStepAsync_Should_CompleteCurrentStepAndActivateNext()
    {
        var taskId = Guid.NewGuid();

        await _context.TaskRepository.CreateAsync(new TaskEntity
        {
            Id = taskId,
            Title = "Test Task",
            Status = Core.Entities.TaskStatus.Pending,
            IsDeleted = false,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
        });

        await _service.InitializeStepsAsync(taskId);

        var steps = await _context.TaskStepRepository.GetAllAsync();
        var currentStep = steps.First(s => s.Step == 1);
        var nextStep = steps.First(s => s.Step == 2);

        Assert.Equal(TaskStepStatus.Handling, currentStep.Status);
        Assert.Equal(TaskStepStatus.Pending, nextStep.Status);

        var result = await _service.MoveToNextStepAsync(taskId, currentStep.Id);

        Assert.NotNull(result);
        Assert.Equal(nextStep.Id, result.Id);
        Assert.Equal(TaskStepStatus.Handling, result.Status);

        var updatedCurrentStep = await _context.TaskStepRepository.GetByIdAsync(currentStep.Id);
        Assert.Equal(TaskStepStatus.Completed, updatedCurrentStep?.Status);
    }

    [Fact]
    public async Task MoveToNextStepAsync_Should_ThrowException_WhenStepNotFound()
    {
        var taskId = Guid.NewGuid();
        var stepId = Guid.NewGuid();

        await Assert.ThrowsAsync<NotFoundException>(() => 
            _service.MoveToNextStepAsync(taskId, stepId));
    }

    [Fact]
    public async Task RejectStepAsync_Should_RestartStep()
    {
        var taskId = Guid.NewGuid();

        await _context.TaskRepository.CreateAsync(new TaskEntity
        {
            Id = taskId,
            Title = "Test Task",
            Status = Core.Entities.TaskStatus.Pending,
            IsDeleted = false,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
        });

        await _service.InitializeStepsAsync(taskId);

        var steps = await _context.TaskStepRepository.GetAllAsync();
        var step1 = steps.First(s => s.Step == 1);

        await _service.MoveToNextStepAsync(taskId, step1.Id);

        await _service.RejectStepAsync(taskId, step1.Id, "Rejected");

        var updatedStep1 = await _context.TaskStepRepository.GetByIdAsync(step1.Id);
        Assert.Equal(TaskStepStatus.Pending, updatedStep1?.Status);
        Assert.Equal(1, updatedStep1?.RetryCount);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}