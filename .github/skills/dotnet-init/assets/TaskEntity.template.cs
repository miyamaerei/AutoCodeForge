using SqlSugar;

namespace __ProjectName__.Entities;

[SugarTable("Tasks")]
public class TaskEntity
{
    [SugarColumn(IsPrimaryKey = true)]
    public Guid Id { get; set; } = Guid.NewGuid();

    public string NtId { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public Guid? RepositoryId { get; set; }

    public string? Branch { get; set; }

    public string? TargetPath { get; set; }

    public Guid? AgentId { get; set; }

    public TaskStatus Status { get; set; } = TaskStatus.Pending;

    public string? ResultJson { get; set; }

    public string? ErrorMessage { get; set; }

    public DateTime? CompletedAt { get; set; }

    public bool IsDeleted { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public enum TaskStatus
{
    Pending,
    Running,
    Completed,
    Failed,
    Paused
}

[SugarTable("TaskLogs")]
public class TaskLogEntity
{
    [SugarColumn(IsPrimaryKey = true)]
    public Guid Id { get; set; } = Guid.NewGuid();

    public string NtId { get; set; } = string.Empty;

    public Guid TaskId { get; set; }

    public string Level { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
