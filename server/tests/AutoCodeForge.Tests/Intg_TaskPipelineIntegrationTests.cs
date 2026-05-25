/**
 * 任务流水线集成测试
 * 
 * 测试覆盖：
 * 1. 步骤创建与状态管理
 * 2. 上下文构建
 * 3. 步骤状态流转
 */

using AutoCodeForge.Application.Services;
using AutoCodeForge.Core.Entities;

namespace AutoCodeForge.Tests;

public sealed class Intg_TaskPipelineIntegrationTests : IDisposable
{
    private readonly IntegrationTestContext _context;
    private readonly ContextChainService _contextChainService;

    public Intg_TaskPipelineIntegrationTests()
    {
        _context = new IntegrationTestContext("test-user");
        _contextChainService = _context.ContextChainService;
    }

    [Fact]
    public async Task CreateStepsForTask_Should_WorkCorrectly()
    {
        var task = TestDataFactory.CreateTask();
        await _context.TaskRepository.CreateAsync(task);

        var steps = TestDataFactory.CreateStepsForTask(task.Id,
            TaskStepType.DemandAnalyse,
            TaskStepType.QueryCurrent,
            TaskStepType.MakePlan,
            TaskStepType.Development,
            TaskStepType.TestVerify,
            TaskStepType.CommitPr,
            TaskStepType.FinalAudit);

        foreach (var step in steps)
        {
            await _context.TaskStepRepository.CreateAsync(step);
        }

        var allSteps = await _context.TaskStepRepository.GetByTaskIdAsync(task.Id);

        Assert.Equal(7, allSteps.Count);
        Assert.All(allSteps, s => Assert.Equal(TaskStepStatus.Pending, s.Status));

        Console.WriteLine("[流水线测试1] 7步流程步骤创建成功");
    }

    [Fact]
    public async Task BuildStepContext_Should_WorkCorrectly()
    {
        var task = TestDataFactory.CreateTask();
        var step1 = TestDataFactory.CreateStep(task.Id, 1, TaskStepType.DemandAnalyse);
        var step2 = TestDataFactory.CreateStep(task.Id, 2, TaskStepType.QueryCurrent);
        
        await _context.TaskRepository.CreateAsync(task);
        await _context.TaskStepRepository.CreateAsync(step1);
        await _context.TaskStepRepository.CreateAsync(step2);

        var context = await _contextChainService.BuildStepContextAsync(task.Id, step2.Id);

        Assert.NotNull(context);
        Assert.Equal(task.Id, context.TaskId);

        Console.WriteLine("[流水线测试2] 上下文构建成功");
    }

    [Fact]
    public async Task BuildStepInput_Should_WorkCorrectly()
    {
        var task = TestDataFactory.CreateTask();
        var step1 = TestDataFactory.CreateStep(task.Id, 1, TaskStepType.DemandAnalyse);
        var step2 = TestDataFactory.CreateStep(task.Id, 2, TaskStepType.QueryCurrent);
        
        await _context.TaskRepository.CreateAsync(task);
        await _context.TaskStepRepository.CreateAsync(step1);
        await _context.TaskStepRepository.CreateAsync(step2);

        var input = await _contextChainService.BuildStepInputAsync(task.Id, step2.Id);

        Assert.NotNull(input);

        Console.WriteLine("[流水线测试3] 步骤输入构建成功");
    }

    [Fact]
    public async Task StepStatusTransition_Should_WorkCorrectly()
    {
        var task = TestDataFactory.CreateTask();
        var step = TestDataFactory.CreateStep(task.Id, 1, TaskStepType.DemandAnalyse);
        
        await _context.TaskRepository.CreateAsync(task);
        await _context.TaskStepRepository.CreateAsync(step);

        step.Status = TaskStepStatus.Handling;
        await _context.TaskStepRepository.UpdateAsync(step);

        var handlingStep = await _context.TaskStepRepository.GetByIdAsync(step.Id);
        Assert.Equal(TaskStepStatus.Handling, handlingStep?.Status);

        step.Status = TaskStepStatus.Completed;
        await _context.TaskStepRepository.UpdateAsync(step);

        var completedStep = await _context.TaskStepRepository.GetByIdAsync(step.Id);
        Assert.Equal(TaskStepStatus.Completed, completedStep?.Status);

        Console.WriteLine("[流水线测试4] 步骤状态流转成功");
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}