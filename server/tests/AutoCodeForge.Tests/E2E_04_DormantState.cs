/**
 * E2E测试 - Dormant休眠状态
 * 
 * 测试覆盖：
 * 1. Agent进入Dormant状态
 * 2. Dormant状态不参与任务分配
 * 3. 唤醒Dormant Agent
 * 4. Dormant状态保留上下文
 */

using AutoCodeForge.Core.Entities;

namespace AutoCodeForge.Tests;

/// <summary>
/// 多Agent协作端到端测试 - Dormant休眠状态模块
/// </summary>
public sealed partial class E2E_MultiAgentCollaborationTests
{
    #region Dormant休眠状态测试

    /// <summary>
    /// 测试Agent进入Dormant状态
    /// </summary>
    [Fact]
    public async Task DormantState_Should_TransitionFromIdle()
    {
        Console.WriteLine("😴 测试：Agent进入休眠状态");

        var agent = TestDataFactory.CreateAgentInState(AgentRole.Worker, AgentState.Idle);
        agent.Name = "Worker-Dormant-Test";
        await _context.AgentRepository.CreateAsync(agent);

        // 手动设置为Dormant状态
        agent.State = AgentState.Dormant;
        agent.StateChangedAtUtc = DateTime.UtcNow;
        await _context.AgentRepository.UpdateAsync(agent);

        // 验证状态变更
        var updatedAgent = await _context.AgentRepository.GetByIdAsync(agent.Id);
        Assert.Equal(AgentState.Dormant, updatedAgent?.State);
        Console.WriteLine("Agent已进入Dormant状态");

        // 创建休眠记录
        await _context.CreateTestDormantRecordAsync(agent.Id, "测试休眠");
        Console.WriteLine("休眠记录已创建");

        Console.WriteLine("✅ Dormant状态转换测试完成！");
    }

    /// <summary>
    /// 测试Dormant状态不参与任务分配
    /// </summary>
    [Fact]
    public async Task DormantAgent_Should_NotReceiveTasks()
    {
        Console.WriteLine("🚫 测试：Dormant Agent不接收任务");

        var dormantAgent = TestDataFactory.CreateAgentInState(AgentRole.Worker, AgentState.Dormant);
        dormantAgent.Name = "Worker-Dormant";
        await _context.AgentRepository.CreateAsync(dormantAgent);

        var idleAgent = TestDataFactory.CreateAgentInState(AgentRole.Worker, AgentState.Idle);
        idleAgent.Name = "Worker-Idle";
        await _context.AgentRepository.CreateAsync(idleAgent);

        var task = TestDataFactory.CreateTask("Dormant Allocation Test");
        await _context.TaskRepository.CreateAsync(task);

        // 尝试分配任务
        var (assignedAgent, _) = await _context.TaskOrchestrator.AssignTaskAsync(task.Id, AgentRole.Worker);
        
        // 验证分配给Idle Agent，而非Dormant Agent
        Assert.NotNull(assignedAgent);
        Assert.Equal("Worker-Idle", assignedAgent.Name);
        Assert.NotEqual(dormantAgent.Id, assignedAgent.Id);
        Console.WriteLine("任务分配给了Idle Agent，Dormant Agent未被选中");

        Console.WriteLine("✅ Dormant Agent不接收任务测试完成！");
    }

    /// <summary>
    /// 测试唤醒Dormant Agent
    /// </summary>
    [Fact]
    public async Task DormantAgent_Should_WakeUp()
    {
        Console.WriteLine("☀️ 测试：唤醒Dormant Agent");

        var agent = TestDataFactory.CreateAgentInState(AgentRole.Worker, AgentState.Dormant);
        agent.Name = "Worker-Wake-Test";
        await _context.AgentRepository.CreateAsync(agent);

        // 唤醒Agent
        agent.State = AgentState.Idle;
        agent.StateChangedAtUtc = DateTime.UtcNow;
        await _context.AgentRepository.UpdateAsync(agent);

        // 更新休眠记录
        var dormantRecord = await _context.CreateTestDormantRecordAsync(agent.Id, "测试休眠", isWoken: true);
        dormantRecord.IsWoken = true;
        await _context.AgentDormantRecordRepository.UpdateAsync(dormantRecord);

        // 验证状态
        var updatedAgent = await _context.AgentRepository.GetByIdAsync(agent.Id);
        Assert.Equal(AgentState.Idle, updatedAgent?.State);
        Console.WriteLine("Agent已被唤醒，状态变为Idle");

        // 现在可以接收任务
        var task = TestDataFactory.CreateTask("Wake Test Task");
        await _context.TaskRepository.CreateAsync(task);

        var (assigned, _) = await _context.TaskOrchestrator.AssignTaskAsync(task.Id, AgentRole.Worker);
        Assert.Equal("Worker-Wake-Test", assigned?.Name);
        Console.WriteLine("唤醒后的Agent成功接收任务");

        Console.WriteLine("✅ 唤醒Dormant Agent测试完成！");
    }

    /// <summary>
    /// 测试Dormant状态保留学习进度上下文
    /// </summary>
    [Fact]
    public async Task DormantAgent_Should_PreserveLearningProgress()
    {
        Console.WriteLine("📚 测试：Dormant状态保留学习进度");

        var agent = TestDataFactory.CreateAgentInState(AgentRole.Worker, AgentState.Learning);
        agent.Name = "Worker-Learning-Test";
        agent.LearningProgress = 75;
        agent.SkillTags = "C#, .NET, WebAPI";
        await _context.AgentRepository.CreateAsync(agent);

        // 创建学习记录
        await _context.CreateTestLearningRecordAsync(agent.Id, LearningTriggerType.IdleTimeout);

        // 进入Dormant状态
        agent.State = AgentState.Dormant;
        await _context.AgentRepository.UpdateAsync(agent);

        // 唤醒后验证上下文保留
        agent.State = AgentState.Idle;
        await _context.AgentRepository.UpdateAsync(agent);

        var updatedAgent = await _context.AgentRepository.GetByIdAsync(agent.Id);
        Assert.Equal(75, updatedAgent?.LearningProgress);
        Assert.Equal("C#, .NET, WebAPI", updatedAgent?.SkillTags);
        Console.WriteLine("学习进度和技能标签完整保留");

        Console.WriteLine("✅ Dormant状态上下文保留测试完成！");
    }

    #endregion
}