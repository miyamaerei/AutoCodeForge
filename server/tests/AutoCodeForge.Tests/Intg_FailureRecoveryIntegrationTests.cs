/**
 * 失败恢复集成测试
 * 
 * 测试覆盖：
 * 1. 失败处理
 */

using AutoCodeForge.Application.Services;
using AutoCodeForge.Core.Entities;

namespace AutoCodeForge.Tests;

public sealed class Intg_FailureRecoveryIntegrationTests : IDisposable
{
    private readonly IntegrationTestContext _context;
    private readonly FailureRecoveryService _failureRecoveryService;

    public Intg_FailureRecoveryIntegrationTests()
    {
        _context = new IntegrationTestContext("test-user");
        _failureRecoveryService = _context.FailureRecoveryService;
    }

    [Fact]
    public async Task HandleFailure_Should_TriggerRetry_WhenRetryCountNotExhausted()
    {
        var task = TestDataFactory.CreateTask();
        var step = TestDataFactory.CreateStep(task.Id, 1, TaskStepType.DemandAnalyse);
        
        await _context.TaskRepository.CreateAsync(task);
        await _context.TaskStepRepository.CreateAsync(step);

        await _failureRecoveryService.HandleFailureAsync(step.Id, FailureCategory.Unknown, "Test failure");

        var updatedStep = await _context.TaskStepRepository.GetByIdAsync(step.Id);
        Assert.Equal(TaskStepStatus.Pending, updatedStep?.Status);
        Assert.Equal(1, updatedStep?.RetryCount);

        Console.WriteLine("[失败恢复测试1] 失败触发重试成功");
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}