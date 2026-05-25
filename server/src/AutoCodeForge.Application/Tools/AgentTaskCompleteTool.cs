using AutoCodeForge.Core.Interfaces;
using AutoCodeForge.Core.Models;
using Microsoft.Extensions.Logging;

namespace AutoCodeForge.Application.Tools;

/// <summary>
/// Agent任务完成工具
/// 允许Agent报告任务完成并触发下一步调度
/// </summary>
public class AgentTaskCompleteTool : IAgentTool
{
    private readonly ITaskEventPublisher _eventPublisher;
    private readonly IArtifactStore _artifactStore;
    private readonly ILogger<AgentTaskCompleteTool> _logger;

    public AgentTaskCompleteTool(
        ITaskEventPublisher eventPublisher,
        IArtifactStore artifactStore,
        ILogger<AgentTaskCompleteTool> logger)
    {
        _eventPublisher = eventPublisher;
        _artifactStore = artifactStore;
        _logger = logger;
    }

    public string Name => "AgentTaskCompleteTool";

    public string Description => "Reports task completion, stores artifacts, and triggers next step scheduling.";

    public async Task<string> ExecuteAsync(
        IReadOnlyDictionary<string, string> input,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("AgentTaskCompleteTool executing");

        if (!input.TryGetValue("taskId", out var taskIdStr) || !Guid.TryParse(taskIdStr, out var taskId))
        {
            throw new ArgumentException("taskId is required");
        }

        if (!input.TryGetValue("stepId", out var stepIdStr) || !Guid.TryParse(stepIdStr, out var stepId))
        {
            throw new ArgumentException("stepId is required");
        }

        if (!input.TryGetValue("agentId", out var agentIdStr) || !Guid.TryParse(agentIdStr, out var agentId))
        {
            throw new ArgumentException("agentId is required");
        }

        var output = input.TryGetValue("output", out var outp) ? outp : string.Empty;
        var summary = input.TryGetValue("summary", out var sum) ? sum : string.Empty;

        var artifact = new ArtifactContract
        {
            TaskId = taskId,
            StepId = stepId,
            AgentId = agentId,
            StepName = "TaskComplete",
            Artifacts = new List<ArtifactItem>
            {
                new ArtifactItem
                {
                    Type = "output",
                    Name = "task_output.txt",
                    Content = output,
                    Format = "text"
                }
            },
            Summary = summary,
            Issues = new List<string>(),
            Metrics = new ArtifactMetrics()
        };

        await _artifactStore.StoreArtifactAsync(artifact, cancellationToken);

        await _eventPublisher.PublishStepTransitionAsync(null, "Completed", cancellationToken);

        _logger.LogInformation("Task {TaskId} step {StepId} completed by Agent {AgentId}", taskId, stepId, agentId);
        
        return $"Task completed successfully. TaskId: {taskId}, StepId: {stepId}. The orchestrator will schedule the next step.";
    }
}