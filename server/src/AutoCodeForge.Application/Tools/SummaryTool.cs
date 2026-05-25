using AutoCodeForge.Core.Interfaces;
using AutoCodeForge.Core.Models;
using AutoCodeForge.Infrastructure.Repositories;
using Microsoft.Extensions.Logging;

namespace AutoCodeForge.Application.Tools;

/// <summary>
/// Summary tool for agent to generate task completion summaries and reports.
/// </summary>
public class SummaryTool : IAgentTool
{
    private readonly TaskRepository _taskRepository;
    private readonly TaskStepRepository _taskStepRepository;
    private readonly IArtifactStore _artifactStore;
    private readonly ILogger<SummaryTool> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SummaryTool"/> class.
    /// </summary>
    public SummaryTool(
        TaskRepository taskRepository,
        TaskStepRepository taskStepRepository,
        IArtifactStore artifactStore,
        ILogger<SummaryTool> logger)
    {
        _taskRepository = taskRepository;
        _taskStepRepository = taskStepRepository;
        _artifactStore = artifactStore;
        _logger = logger;
    }

    /// <inheritdoc />
    public string Name => "SummaryTool";

    /// <inheritdoc />
    public string Description => "Generates task completion summaries, reports, and documentation.";

    /// <inheritdoc />
    public async Task<string> ExecuteAsync(
        IReadOnlyDictionary<string, string> input,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Executing SummaryTool");

        if (!input.TryGetValue("taskId", out var taskIdStr) || !Guid.TryParse(taskIdStr, out var taskId))
        {
            throw new ArgumentException("Task ID is required");
        }

        var task = await _taskRepository.GetByIdAsync(taskId, false, cancellationToken);
        if (task == null)
        {
            throw new ArgumentException("Task not found");
        }

        var steps = await _taskStepRepository.GetByTaskIdAsync(taskId, cancellationToken);
        var completedSteps = steps.Where(s => s.Status == Core.Entities.TaskStepStatus.Completed).ToList();
        var failedSteps = steps.Where(s => s.Status == Core.Entities.TaskStepStatus.Failed).ToList();

        var summary = new System.Text.StringBuilder();
        summary.AppendLine($"# Task Completion Summary");
        summary.AppendLine();
        summary.AppendLine($"## Task Information");
        summary.AppendLine($"- **Title:** {task.Title}");
        summary.AppendLine($"- **Status:** {task.Status}");
        summary.AppendLine($"- **Type:** {task.TaskType}");
        summary.AppendLine($"- **Progress:** {task.Progress}%");
        summary.AppendLine();
        summary.AppendLine($"## Steps Summary");
        summary.AppendLine($"- **Total Steps:** {steps.Count}");
        summary.AppendLine($"- **Completed:** {completedSteps.Count}");
        summary.AppendLine($"- **Failed:** {failedSteps.Count}");
        summary.AppendLine();

        if (completedSteps.Any())
        {
            summary.AppendLine($"## Completed Steps");
            foreach (var step in completedSteps)
            {
                summary.AppendLine($"{step.Step}. {step.StepType}: Completed at {step.CompletedAtUtc:yyyy-MM-dd HH:mm:ss}");
                if (!string.IsNullOrEmpty(step.Output))
                {
                    var preview = step.Output.Length > 100 ? step.Output.Substring(0, 100) + "..." : step.Output;
                    summary.AppendLine($"   Output: {preview}");
                }
            }
        }

        if (failedSteps.Any())
        {
            summary.AppendLine($"## Failed Steps");
            foreach (var step in failedSteps)
            {
                summary.AppendLine($"{step.Step}. {step.StepType}: Failed (Retry Count: {step.RetryCount})");
            }
        }

        summary.AppendLine();
        summary.AppendLine($"**Generated At:** {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss UTC}");

        var summaryContent = summary.ToString();

        var currentStep = steps.FirstOrDefault(s => s.Status == Core.Entities.TaskStepStatus.Completed);
        var artifact = new ArtifactContract
        {
            TaskId = taskId,
            StepId = currentStep?.Id ?? Guid.Empty,
            AgentId = currentStep?.WorkerAgentId ?? Guid.Empty,
            StepName = "Summary",
            Artifacts = new List<ArtifactItem>
            {
                new ArtifactItem
                {
                    Type = "document",
                    Name = "task_summary.md",
                    Content = summaryContent,
                    Format = "markdown"
                }
            },
            Summary = summaryContent.Substring(0, Math.Min(summaryContent.Length, 500)),
            Issues = new List<string>(),
            Metrics = new ArtifactMetrics()
        };

        await _artifactStore.StoreArtifactAsync(artifact, cancellationToken);

        _logger.LogInformation("Summary generated for task {TaskId}", taskId);
        return summaryContent;
    }
}