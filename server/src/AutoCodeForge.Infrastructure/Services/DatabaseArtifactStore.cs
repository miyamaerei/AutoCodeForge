using AutoCodeForge.Core.Interfaces;
using AutoCodeForge.Core.Models;
using Newtonsoft.Json;
using SqlSugar;

namespace AutoCodeForge.Infrastructure.Services;

public class DatabaseArtifactStore : IArtifactStore
{
    private readonly ISqlSugarClient _db;

    public DatabaseArtifactStore(ISqlSugarClient db)
    {
        _db = db;
    }

    public async Task<ArtifactContract> StoreArtifactAsync(ArtifactContract artifact, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        
        var entity = new ArtifactEntity
        {
            Id = artifact.Id,
            TaskId = artifact.TaskId,
            StepId = artifact.StepId,
            AgentId = artifact.AgentId,
            StepName = artifact.StepName,
            ArtifactsJson = JsonConvert.SerializeObject(artifact.Artifacts),
            Summary = artifact.Summary,
            IssuesJson = JsonConvert.SerializeObject(artifact.Issues),
            MetricsJson = JsonConvert.SerializeObject(artifact.Metrics),
            CreatedAtUtc = artifact.CreatedAtUtc
        };

        await _db.Insertable(entity).ExecuteCommandAsync();
        return artifact;
    }

    public async Task<ArtifactContract?> GetArtifactAsync(Guid artifactId, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        
        var entity = await _db.Queryable<ArtifactEntity>()
            .FirstAsync(a => a.Id == artifactId);
        
        return entity != null ? ToContract(entity) : null;
    }

    public async Task<List<ArtifactContract>> ListArtifactsByTaskIdAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        
        var entities = await _db.Queryable<ArtifactEntity>()
            .Where(a => a.TaskId == taskId)
            .OrderBy(a => a.CreatedAtUtc)
            .ToListAsync();
        
        return entities.Select(ToContract).ToList();
    }

    public async Task<List<ArtifactContract>> ListArtifactsByStepIdAsync(Guid stepId, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        
        var entities = await _db.Queryable<ArtifactEntity>()
            .Where(a => a.StepId == stepId)
            .OrderBy(a => a.CreatedAtUtc)
            .ToListAsync();
        
        return entities.Select(ToContract).ToList();
    }

    public async Task DeleteArtifactAsync(Guid artifactId, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        await _db.Deleteable<ArtifactEntity>().In(artifactId).ExecuteCommandAsync();
    }

    private static ArtifactContract ToContract(ArtifactEntity entity)
    {
        return new ArtifactContract
        {
            Id = entity.Id,
            TaskId = entity.TaskId,
            StepId = entity.StepId,
            AgentId = entity.AgentId,
            StepName = entity.StepName,
            Artifacts = JsonConvert.DeserializeObject<List<ArtifactItem>>(entity.ArtifactsJson) ?? new(),
            Summary = entity.Summary,
            Issues = JsonConvert.DeserializeObject<List<string>>(entity.IssuesJson) ?? new(),
            Metrics = JsonConvert.DeserializeObject<ArtifactMetrics>(entity.MetricsJson) ?? new(),
            CreatedAtUtc = entity.CreatedAtUtc
        };
    }
}

[SugarTable("Artifacts")]
public class ArtifactEntity
{
    [SugarColumn(IsPrimaryKey = true)]
    public Guid Id { get; set; }
    
    public Guid TaskId { get; set; }
    
    public Guid StepId { get; set; }
    
    public Guid AgentId { get; set; }
    
    [SugarColumn(Length = 100)]
    public string StepName { get; set; } = string.Empty;
    
    [SugarColumn(ColumnDataType = "TEXT")]
    public string ArtifactsJson { get; set; } = string.Empty;
    
    [SugarColumn(ColumnDataType = "TEXT")]
    public string Summary { get; set; } = string.Empty;
    
    [SugarColumn(ColumnDataType = "TEXT")]
    public string IssuesJson { get; set; } = string.Empty;
    
    [SugarColumn(ColumnDataType = "TEXT")]
    public string MetricsJson { get; set; } = string.Empty;
    
    public DateTime CreatedAtUtc { get; set; }
}