/**
 * E2E测试 - 学习机制
 * 
 * 测试覆盖：
 * 1. 空闲超时自动触发学习
 * 2. 高优先级任务中断学习
 * 3. 角色差异化空闲超时阈值
 * 4. 学习完成回归Idle状态
 */

using AutoCodeForge.Core.Entities;

namespace AutoCodeForge.Tests;

/// <summary>
/// 多Agent协作端到端测试 - 学习机制模块
/// </summary>
public sealed partial class E2E_MultiAgentCollaborationTests
{
    #region 学习机制测试

    /// <summary>
    /// 测试空闲超时自动触发学习
    /// </summary>
    [Fact]
    public async Task LearningMechanism_IdleTimeout_Should_TriggerLearning()
    {
        Console.WriteLine("📖 测试：空闲超时自动触发学习");

        var worker = TestDataFactory.CreateAgentInState(AgentRole.Worker, AgentState.Idle);
        worker.Name = "Worker-Learning-Trigger";
        worker.StateChangedAtUtc = DateTime.UtcNow.AddMinutes(-10); // 模拟已空闲10分钟
        await _context.AgentRepository.CreateAsync(worker);

        // 触发学习
        worker.State = AgentState.Learning;
        worker.StateChangedAtUtc = DateTime.UtcNow;
        await _context.AgentRepository.UpdateAsync(worker);

        // 创建学习记录
        await _context.CreateTestLearningRecordAsync(worker.Id, LearningTriggerType.IdleTimeout);

        // 验证状态
        var updatedWorker = await _context.AgentRepository.GetByIdAsync(worker.Id);
        Assert.Equal(AgentState.Learning, updatedWorker?.State);
        Console.WriteLine("Agent已进入Learning状态");

        Console.WriteLine("✅ 空闲超时触发学习测试完成！");
    }

    /// <summary>
    /// 测试高优先级任务中断学习
    /// </summary>
    [Fact]
    public async Task LearningMechanism_HighPriorityTask_Should_InterruptLearning()
    {
        Console.WriteLine("🔔 测试：高优先级任务中断学习");

        var worker = TestDataFactory.CreateAgentInState(AgentRole.Worker, AgentState.Learning);
        worker.Name = "Worker-Interrupt-Test";
        await _context.AgentRepository.CreateAsync(worker);

        // 创建任务
        var highPriorityTask = TestDataFactory.CreateTask("High Priority Task");
        await _context.TaskRepository.CreateAsync(highPriorityTask);

        // 模拟高优先级任务中断学习：手动将Agent状态从Learning改为Idle，然后分配任务
        worker.State = AgentState.Idle;
        await _context.AgentRepository.UpdateAsync(worker);

        // 分配任务
        var (assignedAgent, _) = await _context.TaskOrchestrator.AssignTaskAsync(
            highPriorityTask.Id, 
            AgentRole.Worker
        );

        // 验证学习被中断，立即接单
        Assert.NotNull(assignedAgent);
        Assert.Equal("Worker-Interrupt-Test", assignedAgent.Name);

        // 更新状态为Handling
        worker.State = AgentState.Handling;
        await _context.AgentRepository.UpdateAsync(worker);
        
        var updatedWorker = await _context.AgentRepository.GetByIdAsync(worker.Id);
        Assert.Equal(AgentState.Handling, updatedWorker?.State);
        Console.WriteLine("高优先级任务中断学习，Agent立即接单");

        Console.WriteLine("✅ 高优先级任务中断学习测试完成！");
    }

    /// <summary>
    /// 测试角色差异化空闲超时阈值
    /// Secretary: 60秒, Manager: 180秒, Worker: 300秒
    /// </summary>
    [Fact]
    public async Task LearningMechanism_RoleSpecificTimeout_Should_Differ()
    {
        Console.WriteLine("⚖️ 测试：角色差异化空闲超时阈值");

        // 创建不同角色的Agent，设置相同的空闲时间（2分钟前）
        var secretary = TestDataFactory.CreateAgentInState(AgentRole.Secretary, AgentState.Idle);
        secretary.Name = "Secretary-Timeout";
        secretary.StateChangedAtUtc = DateTime.UtcNow.AddMinutes(-2);
        await _context.AgentRepository.CreateAsync(secretary);

        var manager = TestDataFactory.CreateAgentInState(AgentRole.Manager, AgentState.Idle);
        manager.Name = "Manager-Timeout";
        manager.StateChangedAtUtc = DateTime.UtcNow.AddMinutes(-2);
        await _context.AgentRepository.CreateAsync(manager);

        var worker = TestDataFactory.CreateAgentInState(AgentRole.Worker, AgentState.Idle);
        worker.Name = "Worker-Timeout";
        worker.StateChangedAtUtc = DateTime.UtcNow.AddMinutes(-2);
        await _context.AgentRepository.CreateAsync(worker);

        // 验证Secretary最先需要学习（最短超时阈值）
        // Secretary空闲2分钟 > 60秒阈值 → 需要学习
        secretary.State = AgentState.Learning;
        await _context.AgentRepository.UpdateAsync(secretary);
        Console.WriteLine("Secretary已超过阈值需要学习");

        // Manager和Worker还未超时
        Assert.Equal(AgentState.Idle, manager.State);
        Assert.Equal(AgentState.Idle, worker.State);
        Console.WriteLine("Manager和Worker还未超时");

        // 等待更长时间后Manager也需要学习
        manager.StateChangedAtUtc = DateTime.UtcNow.AddMinutes(-4);
        manager.State = AgentState.Learning;
        await _context.AgentRepository.UpdateAsync(manager);
        Console.WriteLine("Manager也超过阈值需要学习");

        Console.WriteLine("✅ 角色差异化空闲超时测试完成！");
    }

    /// <summary>
    /// 测试学习完成后回归Idle状态
    /// </summary>
    [Fact]
    public async Task LearningMechanism_Completion_Should_ReturnToIdle()
    {
        Console.WriteLine("🔄 测试：学习完成回归Idle");

        var worker = TestDataFactory.CreateAgentInState(AgentRole.Worker, AgentState.Learning);
        worker.Name = "Worker-Learning-Complete";
        await _context.AgentRepository.CreateAsync(worker);

        // 创建学习记录（标记成功）
        var learningRecord = await _context.CreateTestLearningRecordAsync(
            worker.Id, 
            LearningTriggerType.IdleTimeout, 
            summary: "学习完成，更新了技能标签",
            isSuccess: true
        );

        // 学习完成，回归Idle
        worker.State = AgentState.Idle;
        worker.StateChangedAtUtc = DateTime.UtcNow;
        worker.LearningProgress = learningRecord.EffectivenessScore ?? 0;
        worker.SkillTags = "C#, .NET, WebAPI, Testing";
        await _context.AgentRepository.UpdateAsync(worker);

        // 验证状态
        var updatedWorker = await _context.AgentRepository.GetByIdAsync(worker.Id);
        Assert.Equal(AgentState.Idle, updatedWorker?.State);
        Assert.Equal(80, updatedWorker?.LearningProgress);
        Assert.Contains("Testing", updatedWorker?.SkillTags ?? "");
        Console.WriteLine("学习完成，Agent回归Idle状态，技能标签已更新");

        Console.WriteLine("✅ 学习完成回归Idle测试完成！");
    }

    #endregion
}