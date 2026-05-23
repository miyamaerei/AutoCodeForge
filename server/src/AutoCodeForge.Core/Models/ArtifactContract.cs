namespace AutoCodeForge.Core.Models;

public class ArtifactContract
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public Guid TaskId { get; set; }
    
    public Guid StepId { get; set; }
    
    public Guid AgentId { get; set; }
    
    public string StepName { get; set; } = string.Empty;
    
    public List<ArtifactItem> Artifacts { get; set; } = new();
    
    public string Summary { get; set; } = string.Empty;
    
    public List<string> Issues { get; set; } = new();
    
    public ArtifactMetrics Metrics { get; set; } = new();
    
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}

public class ArtifactItem
{
    public string Type { get; set; } = string.Empty;
    
    public string Name { get; set; } = string.Empty;
    
    public string Content { get; set; } = string.Empty;
    
    public string Format { get; set; } = "text";
}

public class ArtifactMetrics
{
    public long ExecutionTimeMs { get; set; }
    
    public long TokenUsage { get; set; }
}