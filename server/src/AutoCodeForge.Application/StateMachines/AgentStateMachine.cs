using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Exceptions;

namespace AutoCodeForge.Application.StateMachines;

public class AgentStateMachine
{
    public event EventHandler<StateTransitionEventArgs>? StateTransitioned;

    public async Task<AgentState> HandleEventAsync(AgentEntity agent, StateEvent @event, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        var currentState = agent.State;
        AgentState newState;

        switch (currentState)
        {
            case AgentState.Idle:
                newState = HandleIdleState(agent, @event);
                break;
            case AgentState.Handling:
                newState = HandleHandlingState(agent, @event);
                break;
            case AgentState.Learning:
                newState = HandleLearningState(agent, @event);
                break;
            case AgentState.Dormant:
                newState = HandleDormantState(agent, @event);
                break;
            default:
                throw new InvalidOperationException($"Unknown state: {currentState}");
        }

        if (newState != currentState)
        {
            agent.State = newState;
            agent.StateChangedAtUtc = DateTime.UtcNow;
            OnStateTransitioned(agent.Id, currentState, newState, @event);
        }

        return newState;
    }

    private AgentState HandleIdleState(AgentEntity agent, StateEvent @event)
    {
        return @event switch
        {
            StateEvent.AssignTask => AgentState.Handling,
            StateEvent.StartLearning => AgentState.Learning,
            StateEvent.EnterDormant => AgentState.Dormant,
            StateEvent.CompleteTask => throw new InvalidOperationException("Cannot complete task from Idle state"),
            StateEvent.FailTask => throw new InvalidOperationException("Cannot fail task from Idle state"),
            StateEvent.TimeoutTask => throw new InvalidOperationException("Cannot timeout from Idle state"),
            StateEvent.CompleteLearning => throw new InvalidOperationException("Cannot complete learning from Idle state"),
            StateEvent.InterruptLearning => throw new InvalidOperationException("Cannot interrupt learning from Idle state"),
            StateEvent.WakeUp => AgentState.Idle,
            _ => throw new InvalidOperationException($"Unknown event: {@event}")
        };
    }

    private AgentState HandleHandlingState(AgentEntity agent, StateEvent @event)
    {
        return @event switch
        {
            StateEvent.CompleteTask => AgentState.Idle,
            StateEvent.FailTask => AgentState.Idle,
            StateEvent.TimeoutTask => AgentState.Idle,
            StateEvent.AssignTask => throw new InvalidOperationException("Cannot assign task while handling"),
            StateEvent.StartLearning => throw new InvalidOperationException("Cannot start learning while handling"),
            StateEvent.EnterDormant => AgentState.Dormant,
            StateEvent.CompleteLearning => throw new InvalidOperationException("Cannot complete learning from Handling state"),
            StateEvent.InterruptLearning => throw new InvalidOperationException("Cannot interrupt learning from Handling state"),
            StateEvent.WakeUp => AgentState.Handling,
            _ => throw new InvalidOperationException($"Unknown event: {@event}")
        };
    }

    private AgentState HandleLearningState(AgentEntity agent, StateEvent @event)
    {
        return @event switch
        {
            StateEvent.CompleteLearning => AgentState.Idle,
            StateEvent.InterruptLearning => AgentState.Idle,
            StateEvent.EnterDormant => AgentState.Dormant,
            StateEvent.AssignTask => throw new InvalidOperationException("Cannot assign task while learning"),
            StateEvent.StartLearning => throw new InvalidOperationException("Already in Learning state"),
            StateEvent.CompleteTask => throw new InvalidOperationException("Cannot complete task from Learning state"),
            StateEvent.FailTask => throw new InvalidOperationException("Cannot fail task from Learning state"),
            StateEvent.TimeoutTask => throw new InvalidOperationException("Cannot timeout from Learning state"),
            StateEvent.WakeUp => AgentState.Learning,
            _ => throw new InvalidOperationException($"Unknown event: {@event}")
        };
    }

    private AgentState HandleDormantState(AgentEntity agent, StateEvent @event)
    {
        return @event switch
        {
            StateEvent.WakeUp => AgentState.Idle,
            StateEvent.AssignTask => throw new InvalidOperationException("Cannot assign task to dormant agent"),
            StateEvent.StartLearning => throw new InvalidOperationException("Cannot start learning for dormant agent"),
            StateEvent.CompleteTask => throw new InvalidOperationException("Cannot complete task from Dormant state"),
            StateEvent.FailTask => throw new InvalidOperationException("Cannot fail task from Dormant state"),
            StateEvent.TimeoutTask => throw new InvalidOperationException("Cannot timeout from Dormant state"),
            StateEvent.CompleteLearning => throw new InvalidOperationException("Cannot complete learning from Dormant state"),
            StateEvent.InterruptLearning => throw new InvalidOperationException("Cannot interrupt learning from Dormant state"),
            StateEvent.EnterDormant => AgentState.Dormant,
            _ => throw new InvalidOperationException($"Unknown event: {@event}")
        };
    }

    private void OnStateTransitioned(Guid agentId, AgentState fromState, AgentState toState, StateEvent triggerEvent)
    {
        StateTransitioned?.Invoke(this, new StateTransitionEventArgs(agentId, fromState, toState, triggerEvent));
    }
}

public enum StateEvent
{
    AssignTask,
    CompleteTask,
    FailTask,
    TimeoutTask,
    StartLearning,
    CompleteLearning,
    InterruptLearning,
    EnterDormant,
    WakeUp
}

public class StateTransitionEventArgs : EventArgs
{
    public Guid AgentId { get; }
    public AgentState FromState { get; }
    public AgentState ToState { get; }
    public StateEvent TriggerEvent { get; }

    public StateTransitionEventArgs(Guid agentId, AgentState fromState, AgentState toState, StateEvent triggerEvent)
    {
        AgentId = agentId;
        FromState = fromState;
        ToState = toState;
        TriggerEvent = triggerEvent;
    }
}