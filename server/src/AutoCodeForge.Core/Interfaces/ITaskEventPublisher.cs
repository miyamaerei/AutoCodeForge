using AutoCodeForge.Core.Entities;

namespace AutoCodeForge.Core.Interfaces;

public interface ITaskEventPublisher
{
    Task PublishTaskCreatedAsync(TaskEntity task, CancellationToken cancellationToken = default);
    
    Task PublishTaskCompletedAsync(TaskEntity task, CancellationToken cancellationToken = default);
    
    Task PublishStepTransitionAsync(TaskStepEntity step, string previousStatus, CancellationToken cancellationToken = default);
    
    Task PublishArtifactCreatedAsync(Guid taskId, Guid stepId, Guid agentId, CancellationToken cancellationToken = default);
    
    Task PublishFailureAsync(Guid taskId, Guid? stepId, string failureCategory, string message, CancellationToken cancellationToken = default);
}