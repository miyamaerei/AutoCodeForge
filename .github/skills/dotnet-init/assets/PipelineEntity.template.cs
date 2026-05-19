using SqlSugar;

namespace __ProjectName__.Entities;

[SugarTable("Pipelines")]
public class PipelineEntity
{
    [SugarColumn(IsPrimaryKey = true)]
    public Guid Id { get; set; } = Guid.NewGuid();

    public string NtId { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public Guid RepositoryId { get; set; }

    public string? ExternalPipelineId { get; set; }

    public PipelineStatus Status { get; set; } = PipelineStatus.Pending;

    public DateTime? LastBuildTime { get; set; }

    public string? WebUrl { get; set; }

    public bool IsDeleted { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public enum PipelineStatus { Success, Failed, Running, Pending, Cancelled }

[SugarTable("Builds")]
public class BuildEntity
{
    [SugarColumn(IsPrimaryKey = true)]
    public Guid Id { get; set; } = Guid.NewGuid();

    public string NtId { get; set; } = string.Empty;

    public Guid PipelineId { get; set; }

    public string? ExternalBuildId { get; set; }

    public string Branch { get; set; } = string.Empty;

    public string? CommitSha { get; set; }

    public BuildStatus Status { get; set; } = BuildStatus.Pending;

    public DateTime StartTime { get; set; } = DateTime.UtcNow;

    public DateTime? EndTime { get; set; }

    public string? WebUrl { get; set; }

    public bool IsDeleted { get; set; } = false;
}

public enum BuildStatus { Success, Failed, Running, Pending, Cancelled }
