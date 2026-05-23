using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Interfaces;

namespace AutoCodeForge.Application.Services;

public class InMemoryTaskEventPublisher : ITaskEventPublisher, IDisposable
{
    private readonly List<Func<TaskEvent, CancellationToken, Task>> _subscribers = new();
    private readonly object _lock = new();

    public Task PublishTaskCreatedAsync(TaskEntity task, CancellationToken cancellationToken = default)
    {
        var @event = new TaskEvent
        {
            EventType = "TaskCreated",
            TaskId = task.Id,
            Payload = new { task.Title, task.Status }
        };
        return PublishAsync(@event, cancellationToken);
    }

    public Task PublishTaskCompletedAsync(TaskEntity task, CancellationToken cancellationToken = default)
    {
        var @event = new TaskEvent
        {
            EventType = "TaskCompleted",
            TaskId = task.Id,
            Payload = new { task.Title, task.Status, task.Result }
        };
        return PublishAsync(@event, cancellationToken);
    }

    public Task PublishStepTransitionAsync(TaskStepEntity step, string previousStatus, CancellationToken cancellationToken = default)
    {
        var @event = new TaskEvent
        {
            EventType = "StepTransition",
            TaskId = step.TaskId,
            StepId = step.Id,
            Payload = new { step.Step, step.StepType, step.Status, PreviousStatus = previousStatus }
        };
        return PublishAsync(@event, cancellationToken);
    }

    public Task PublishArtifactCreatedAsync(Guid taskId, Guid stepId, Guid agentId, CancellationToken cancellationToken = default)
    {
        var @event = new TaskEvent
        {
            EventType = "ArtifactCreated",
            TaskId = taskId,
            StepId = stepId,
            AgentId = agentId
        };
        return PublishAsync(@event, cancellationToken);
    }

    public Task PublishFailureAsync(Guid taskId, Guid? stepId, string failureCategory, string message, CancellationToken cancellationToken = default)
    {
        var @event = new TaskEvent
        {
            EventType = "Failure",
            TaskId = taskId,
            StepId = stepId,
            Payload = new { FailureCategory = failureCategory, Message = message }
        };
        return PublishAsync(@event, cancellationToken);
    }

    public void Subscribe(Func<TaskEvent, CancellationToken, Task> handler)
    {
        lock (_lock)
        {
            _subscribers.Add(handler);
        }
    }

    public void Unsubscribe(Func<TaskEvent, CancellationToken, Task> handler)
    {
        lock (_lock)
        {
            _subscribers.Remove(handler);
        }
    }

    private async Task PublishAsync(TaskEvent @event, CancellationToken cancellationToken)
    {
        List<Func<TaskEvent, CancellationToken, Task>> subscribersCopy;
        lock (_lock)
        {
            subscribersCopy = new List<Func<TaskEvent, CancellationToken, Task>>(_subscribers);
        }

        foreach (var subscriber in subscribersCopy)
        {
            try
            {
                await subscriber(@event, cancellationToken);
            }
            catch
            {
            }
        }
    }

    public void Dispose()
    {
        lock (_lock)
        {
            _subscribers.Clear();
        }
    }
}

public class TaskEvent
{
    public string EventType { get; set; } = string.Empty;
    public Guid TaskId { get; set; }
    public Guid? StepId { get; set; }
    public Guid? AgentId { get; set; }
    public object? Payload { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}