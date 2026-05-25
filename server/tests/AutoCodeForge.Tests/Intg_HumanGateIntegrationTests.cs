/**
 * HumanGate门控集成测试
 * 
 * 测试覆盖：
 * 1. 门控创建与状态管理
 */

using AutoCodeForge.Application.Services;
using AutoCodeForge.Core.Entities;

namespace AutoCodeForge.Tests;

public sealed class Intg_HumanGateIntegrationTests : IDisposable
{
    private readonly IntegrationTestContext _context;

    public Intg_HumanGateIntegrationTests()
    {
        _context = new IntegrationTestContext("test-user");
    }

    [Fact]
    public async Task CreateGate_Should_CreatePendingGate()
    {
        var task = TestDataFactory.CreateTask();
        var step = TestDataFactory.CreateStep(task.Id, 1, TaskStepType.DemandAnalyse);
        
        await _context.TaskRepository.CreateAsync(task);
        await _context.TaskStepRepository.CreateAsync(step);

        var gate = TestDataFactory.CreatePendingGate(task.Id, step.Id, HumanGateType.PlanApproval);
        await _context.HumanGateRepository.CreateAsync(gate);

        var retrievedGate = await _context.HumanGateRepository.GetByIdAsync(gate.Id);
        Assert.NotNull(retrievedGate);
        Assert.Equal(HumanGateStatus.Pending, retrievedGate.Status);

        Console.WriteLine("[门控测试1] 门控创建成功");
    }

    [Fact]
    public async Task GateStatusTransition_Should_WorkCorrectly()
    {
        var task = TestDataFactory.CreateTask();
        var step = TestDataFactory.CreateStep(task.Id, 1, TaskStepType.DemandAnalyse);
        var gate = TestDataFactory.CreatePendingGate(task.Id, step.Id, HumanGateType.PlanApproval);
        
        await _context.TaskRepository.CreateAsync(task);
        await _context.TaskStepRepository.CreateAsync(step);
        await _context.HumanGateRepository.CreateAsync(gate);

        gate.Status = HumanGateStatus.Approved;
        gate.RespondedAtUtc = DateTime.UtcNow;
        await _context.HumanGateRepository.UpdateAsync(gate);

        var updatedGate = await _context.HumanGateRepository.GetByIdAsync(gate.Id);
        Assert.Equal(HumanGateStatus.Approved, updatedGate?.Status);
        Assert.NotNull(updatedGate?.RespondedAtUtc);

        Console.WriteLine("[门控测试2] 门控状态流转成功");
    }

    [Fact]
    public async Task GetGateById_Should_ReturnGate()
    {
        var task = TestDataFactory.CreateTask();
        var step = TestDataFactory.CreateStep(task.Id, 1, TaskStepType.DemandAnalyse);
        var gate = TestDataFactory.CreatePendingGate(task.Id, step.Id, HumanGateType.CodeReview);
        
        await _context.TaskRepository.CreateAsync(task);
        await _context.TaskStepRepository.CreateAsync(step);
        await _context.HumanGateRepository.CreateAsync(gate);

        var retrievedGate = await _context.HumanGateRepository.GetByIdAsync(gate.Id);
        Assert.NotNull(retrievedGate);
        Assert.Equal(HumanGateType.CodeReview, retrievedGate.GateType);

        Console.WriteLine("[门控测试3] 获取门控详情成功");
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}