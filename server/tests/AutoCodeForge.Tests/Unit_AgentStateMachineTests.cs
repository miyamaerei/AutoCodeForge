using AutoCodeForge.Application.StateMachines;
using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Exceptions;

namespace AutoCodeForge.Tests;

public class Unit_AgentStateMachineTests
{
    private readonly AgentStateMachine _stateMachine;

    public Unit_AgentStateMachineTests()
    {
        _stateMachine = new AgentStateMachine();
    }

    [Fact]
    public async Task HandleEventAsync_IdleToHandling_ValidTransition()
    {
        var agent = new AgentEntity { Id = Guid.NewGuid(), State = AgentState.Idle };

        var result = await _stateMachine.HandleEventAsync(agent, StateEvent.AssignTask);

        Assert.Equal(AgentState.Handling, result);
        Assert.Equal(AgentState.Handling, agent.State);
    }

    [Fact]
    public async Task HandleEventAsync_IdleToLearning_ValidTransition()
    {
        var agent = new AgentEntity { Id = Guid.NewGuid(), State = AgentState.Idle };

        var result = await _stateMachine.HandleEventAsync(agent, StateEvent.StartLearning);

        Assert.Equal(AgentState.Learning, result);
        Assert.Equal(AgentState.Learning, agent.State);
    }

    [Fact]
    public async Task HandleEventAsync_IdleToDormant_ValidTransition()
    {
        var agent = new AgentEntity { Id = Guid.NewGuid(), State = AgentState.Idle };

        var result = await _stateMachine.HandleEventAsync(agent, StateEvent.EnterDormant);

        Assert.Equal(AgentState.Dormant, result);
        Assert.Equal(AgentState.Dormant, agent.State);
    }

    [Fact]
    public async Task HandleEventAsync_HandlingToIdle_ValidTransition()
    {
        var agent = new AgentEntity { Id = Guid.NewGuid(), State = AgentState.Handling };

        var result = await _stateMachine.HandleEventAsync(agent, StateEvent.CompleteTask);

        Assert.Equal(AgentState.Idle, result);
        Assert.Equal(AgentState.Idle, agent.State);
    }

    [Fact]
    public async Task HandleEventAsync_HandlingToIdleOnFail_ValidTransition()
    {
        var agent = new AgentEntity { Id = Guid.NewGuid(), State = AgentState.Handling };

        var result = await _stateMachine.HandleEventAsync(agent, StateEvent.FailTask);

        Assert.Equal(AgentState.Idle, result);
        Assert.Equal(AgentState.Idle, agent.State);
    }

    [Fact]
    public async Task HandleEventAsync_HandlingToDormant_ValidTransition()
    {
        var agent = new AgentEntity { Id = Guid.NewGuid(), State = AgentState.Handling };

        var result = await _stateMachine.HandleEventAsync(agent, StateEvent.EnterDormant);

        Assert.Equal(AgentState.Dormant, result);
        Assert.Equal(AgentState.Dormant, agent.State);
    }

    [Fact]
    public async Task HandleEventAsync_LearningToIdle_ValidTransition()
    {
        var agent = new AgentEntity { Id = Guid.NewGuid(), State = AgentState.Learning };

        var result = await _stateMachine.HandleEventAsync(agent, StateEvent.CompleteLearning);

        Assert.Equal(AgentState.Idle, result);
        Assert.Equal(AgentState.Idle, agent.State);
    }

    [Fact]
    public async Task HandleEventAsync_LearningToIdleOnInterrupt_ValidTransition()
    {
        var agent = new AgentEntity { Id = Guid.NewGuid(), State = AgentState.Learning };

        var result = await _stateMachine.HandleEventAsync(agent, StateEvent.InterruptLearning);

        Assert.Equal(AgentState.Idle, result);
        Assert.Equal(AgentState.Idle, agent.State);
    }

    [Fact]
    public async Task HandleEventAsync_DormantToIdle_ValidTransition()
    {
        var agent = new AgentEntity { Id = Guid.NewGuid(), State = AgentState.Dormant };

        var result = await _stateMachine.HandleEventAsync(agent, StateEvent.WakeUp);

        Assert.Equal(AgentState.Idle, result);
        Assert.Equal(AgentState.Idle, agent.State);
    }

    [Fact]
    public async Task HandleEventAsync_InvalidTransition_ThrowsException()
    {
        var agent = new AgentEntity { Id = Guid.NewGuid(), State = AgentState.Idle };

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _stateMachine.HandleEventAsync(agent, StateEvent.CompleteTask));
    }

    [Fact]
    public async Task HandleEventAsync_AssignTaskToHandling_ThrowsException()
    {
        var agent = new AgentEntity { Id = Guid.NewGuid(), State = AgentState.Handling };

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _stateMachine.HandleEventAsync(agent, StateEvent.AssignTask));
    }
}