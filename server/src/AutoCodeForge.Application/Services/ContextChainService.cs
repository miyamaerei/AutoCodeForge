using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Interfaces;
using AutoCodeForge.Core.Models;
using AutoCodeForge.Infrastructure.Repositories;

namespace AutoCodeForge.Application.Services;

public class ContextChainService
{
    private readonly TaskStepRepository _taskStepRepository;
    private readonly IArtifactStore _artifactStore;
    private readonly int _maxContextTokens;

    public ContextChainService(
        TaskStepRepository taskStepRepository,
        IArtifactStore artifactStore)
    {
        _taskStepRepository = taskStepRepository;
        _artifactStore = artifactStore;
        _maxContextTokens = 8192;
    }

    public async Task<StepContext> BuildStepContextAsync(Guid taskId, Guid currentStepId, CancellationToken cancellationToken = default)
    {
        var completedSteps = await _taskStepRepository.GetCompletedStepsAsync(taskId, currentStepId, cancellationToken);
        
        var artifacts = new List<ArtifactContract>();
        foreach (var step in completedSteps)
        {
            var stepArtifacts = await _artifactStore.ListArtifactsByStepIdAsync(step.Id, cancellationToken);
            artifacts.AddRange(stepArtifacts);
        }

        var context = new StepContext
        {
            TaskId = taskId,
            CurrentStepId = currentStepId,
            CompletedSteps = completedSteps,
            Artifacts = artifacts,
            GlobalContext = new Dictionary<string, object>()
        };

        await TruncateIfNeededAsync(context);
        
        return context;
    }

    public async Task<string> BuildStepInputAsync(Guid taskId, Guid currentStepId, CancellationToken cancellationToken = default)
    {
        var context = await BuildStepContextAsync(taskId, currentStepId, cancellationToken);
        
        var inputs = new List<string>();
        
        foreach (var artifact in context.Artifacts)
        {
            foreach (var item in artifact.Artifacts)
            {
                inputs.Add($"{item.Name} ({item.Type}):\n{item.Content}");
            }
        }

        return string.Join("\n\n---\n\n", inputs);
    }

    private async Task TruncateIfNeededAsync(StepContext context)
    {
        var currentSize = CalculateContextSize(context);
        
        if (currentSize <= _maxContextTokens)
            return;

        while (currentSize > _maxContextTokens && context.Artifacts.Count > 0)
        {
            var oldestArtifact = context.Artifacts.OrderBy(a => a.CreatedAtUtc).First();
            context.Artifacts.Remove(oldestArtifact);
            currentSize = CalculateContextSize(context);
        }
    }

    private int CalculateContextSize(StepContext context)
    {
        int size = 0;
        
        foreach (var artifact in context.Artifacts)
        {
            size += artifact.Summary.Length;
            foreach (var item in artifact.Artifacts)
            {
                size += item.Name.Length + item.Content.Length;
            }
        }
        
        return size;
    }
}

public class StepContext
{
    public Guid TaskId { get; set; }
    public Guid CurrentStepId { get; set; }
    public List<TaskStepEntity> CompletedSteps { get; set; } = new();
    public List<ArtifactContract> Artifacts { get; set; } = new();
    public Dictionary<string, object> GlobalContext { get; set; } = new();
}