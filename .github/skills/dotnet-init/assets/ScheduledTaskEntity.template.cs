using SqlSugar;

namespace __ProjectName__.Entities;

[SugarTable("ScheduledTasks")]
public class ScheduledTaskEntity
{
    [SugarColumn(IsPrimaryKey = true)]
    public Guid Id { get; set; } = Guid.NewGuid();

    public string NtId { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public Guid? AgentId { get; set; }

    public Guid? RepositoryId { get; set; }

    public string? Branch { get; set; }

    public string? TargetPath { get; set; }

    public TriggerType TriggerType { get; set; } = TriggerType.Cron;

    public string? CronExpression { get; set; }

    public long? IntervalMs { get; set; }

    public DateTime? OnceTime { get; set; }

    public string? ParamsJson { get; set; }

    public ScheduleStatus Status { get; set; } = ScheduleStatus.Idle;

    public DateTime? NextRunTime { get; set; }

    public DateTime? LastRunTime { get; set; }

    public int TotalRuns { get; set; } = 0;

    public int SuccessRuns { get; set; } = 0;

    public int FailedRuns { get; set; } = 0;

    public bool Enabled { get; set; } = true;

    public bool IsDeleted { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public enum TriggerType { Cron, Interval, Once }
public enum ScheduleStatus { Idle, Running, Failed, Disabled }

[SugarTable("ScheduledTaskExecutions")]
public class ScheduledTaskExecutionEntity
{
    [SugarColumn(IsPrimaryKey = true)]
    public Guid Id { get; set; } = Guid.NewGuid();

    public string NtId { get; set; } = string.Empty;

    public Guid ScheduledTaskId { get; set; }

    public DateTime StartTime { get; set; } = DateTime.UtcNow;

    public DateTime? EndTime { get; set; }

    public ExecutionStatus Status { get; set; } = ExecutionStatus.Running;

    public string? ResultJson { get; set; }

    public string? ErrorMessage { get; set; }
}

public enum ExecutionStatus { Success, Failed, Running }
