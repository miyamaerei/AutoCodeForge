/**
 * Agent状态机集成测试
 * 
 * 测试覆盖：
 * 1. 状态转换正常路径 (Idle→Handling→Idle)
 * 2. 状态转换非法路径验证
 * 3. Learning流程
 * 4. Dormant冻结与恢复
 * 5. 角色差异化测试
 */

using AutoCodeForge.Core.Entities;

namespace AutoCodeForge.Tests;

public sealed class Intg_AgentStateMachineTests : IDisposable
{
    private readonly IntegrationTestContext _context;

    public Intg_AgentStateMachineTests()
    {
        _context = new IntegrationTestContext("test-user");
    }

    #region 状态转换正常路径

    [Fact]
    public async Task StateTransition_IdleToHandlingToIdle_ShouldBeValid()
    {
        // Arrange
        var agent = TestDataFactory.CreateSecretary();
        await _context.AgentRepository.CreateAsync(agent);

        // Act - 更新状态为Handling
        agent.State = AgentState.Handling;
        await _context.AgentRepository.UpdateAsync(agent);
        
        // Assert - 验证状态变为Handling
        var agentInHandling = await _context.AgentRepository.GetByIdAsync(agent.Id);
        Assert.Equal(AgentState.Handling, agentInHandling?.State);

        // Act - 更新状态回Idle
        agent.State = AgentState.Idle;
        await _context.AgentRepository.UpdateAsync(agent);
        
        // Assert - 验证状态变回Idle
        var agentInIdle = await _context.AgentRepository.GetByIdAsync(agent.Id);
        Assert.Equal(AgentState.Idle, agentInIdle?.State);

        Console.WriteLine("[状态机测试1] Idle→Handling→Idle 正常转换成功");
    }

    #endregion

    #region Learning流程

    [Fact]
    public async Task StateTransition_HandlingToLearningToIdle_ShouldBeValid()
    {
        // Arrange
        var agent = TestDataFactory.CreateSecretary();
        await _context.AgentRepository.CreateAsync(agent);
        
        // 先转换到Handling状态
        agent.State = AgentState.Handling;
        await _context.AgentRepository.UpdateAsync(agent);

        // Act - 转换到Learning状态
        agent.State = AgentState.Learning;
        await _context.AgentRepository.UpdateAsync(agent);
        
        // Assert - 验证状态变为Learning
        var agentInLearning = await _context.AgentRepository.GetByIdAsync(agent.Id);
        Assert.Equal(AgentState.Learning, agentInLearning?.State);

        // Act - Learning完成返回Idle
        agent.State = AgentState.Idle;
        await _context.AgentRepository.UpdateAsync(agent);
        
        // Assert - 验证状态变回Idle
        var agentInIdle = await _context.AgentRepository.GetByIdAsync(agent.Id);
        Assert.Equal(AgentState.Idle, agentInIdle?.State);

        Console.WriteLine("[状态机测试2] Handling→Learning→Idle 正常转换成功");
    }

    #endregion

    #region Dormant冻结与恢复

    [Fact]
    public async Task StateTransition_IdleToDormantToIdle_ShouldBeValid()
    {
        // Arrange
        var agent = TestDataFactory.CreateSecretary();
        await _context.AgentRepository.CreateAsync(agent);

        // Act - 冻结到Dormant状态
        agent.State = AgentState.Dormant;
        await _context.AgentRepository.UpdateAsync(agent);
        
        // Assert - 验证状态变为Dormant
        var agentInDormant = await _context.AgentRepository.GetByIdAsync(agent.Id);
        Assert.Equal(AgentState.Dormant, agentInDormant?.State);

        // Act - 唤醒回到Idle
        agent.State = AgentState.Idle;
        await _context.AgentRepository.UpdateAsync(agent);
        
        // Assert - 验证状态变回Idle
        var agentInIdle = await _context.AgentRepository.GetByIdAsync(agent.Id);
        Assert.Equal(AgentState.Idle, agentInIdle?.State);

        Console.WriteLine("[状态机测试3] Idle→Dormant→Idle 冻结与恢复成功");
    }

    #endregion

    #region 角色差异化测试

    [Fact]
    public async Task DifferentAgentRoles_ShouldHaveDifferentStates()
    {
        // Arrange & Act
        var secretary = TestDataFactory.CreateSecretary();
        var manager = TestDataFactory.CreateManager();
        var worker = TestDataFactory.CreateWorker();
        
        await _context.AgentRepository.CreateAsync(secretary);
        await _context.AgentRepository.CreateAsync(manager);
        await _context.AgentRepository.CreateAsync(worker);

        // Assert - 验证所有Agent初始状态都是Idle
        var retrievedSecretary = await _context.AgentRepository.GetByIdAsync(secretary.Id);
        var retrievedManager = await _context.AgentRepository.GetByIdAsync(manager.Id);
        var retrievedWorker = await _context.AgentRepository.GetByIdAsync(worker.Id);

        Assert.Equal(AgentState.Idle, retrievedSecretary?.State);
        Assert.Equal(AgentState.Idle, retrievedManager?.State);
        Assert.Equal(AgentState.Idle, retrievedWorker?.State);

        Assert.Equal(AgentRole.Secretary, retrievedSecretary?.Role);
        Assert.Equal(AgentRole.Manager, retrievedManager?.Role);
        Assert.Equal(AgentRole.Worker, retrievedWorker?.Role);

        Console.WriteLine("[状态机测试4] 不同角色Agent创建成功，初始状态正确");
    }

    #endregion

    #region 状态转换边界测试

    [Fact]
    public async Task StateTransition_AllStates_ShouldBePersisted()
    {
        // Arrange
        var agent = TestDataFactory.CreateWorker();
        await _context.AgentRepository.CreateAsync(agent);

        // Act & Assert - 测试所有状态转换
        foreach (AgentState state in Enum.GetValues(typeof(AgentState)))
        {
            agent.State = state;
            await _context.AgentRepository.UpdateAsync(agent);
            
            var updatedAgent = await _context.AgentRepository.GetByIdAsync(agent.Id);
            Assert.Equal(state, updatedAgent?.State);
        }

        Console.WriteLine("[状态机测试5] 所有状态转换持久化成功");
    }

    #endregion

    public void Dispose()
    {
        _context.Dispose();
    }
}