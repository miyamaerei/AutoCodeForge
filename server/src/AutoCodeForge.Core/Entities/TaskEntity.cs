using AutoCodeForge.Core.Entities.Base;
using SqlSugar;

namespace AutoCodeForge.Core.Entities;

/// <summary>
/// Represents a user task.
/// </summary>
[SugarTable("Tasks")]
public class TaskEntity : UserOwnedEntity
{
    /// <summary>
    /// Gets or sets the task identifier.
    /// </summary>
    [SugarColumn(IsPrimaryKey = true)]
    public new Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the task title.
    /// </summary>
    [SugarColumn(Length = 200, IsNullable = false)]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the task description.
    /// </summary>
    [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets task input payload.
    /// </summary>
    [SugarColumn(ColumnDataType = "TEXT", IsNullable = false)]
    public string Input { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the logical domain served by the task.
    /// </summary>
    [SugarColumn(Length = 100, IsNullable = false)]
    public string DomainType { get; set; } = "General";

    /// <summary>
    /// Gets or sets the domain record identifier bound to this task.
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public Guid? DomainRecordId { get; set; }

    /// <summary>
    /// Gets or sets task result payload.
    /// </summary>
    [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
    public string? Result { get; set; }

    /// <summary>
    /// Gets or sets last error message.
    /// </summary>
    [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets task type.
    /// </summary>
    public TaskType TaskType { get; set; } = TaskType.General;

    /// <summary>
    /// Gets or sets serialized sandbox snapshot payload.
    /// </summary>
    [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
    public string? SandboxSnapshotJson { get; set; }

    /// <summary>
    /// Gets or sets serialized repository snapshot payload.
    /// </summary>
    [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
    public string? RepositorySnapshotJson { get; set; }

    /// <summary>
    /// Gets or sets related workspace record identifier.
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public Guid? WorkspaceRecordId { get; set; }

    /// <summary>
    /// Gets or sets bound agent identifier.
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public Guid? AgentId { get; set; }

    /// <summary>
    /// Gets or sets the current task status.
    /// </summary>
    public TaskStatus Status { get; set; } = TaskStatus.Pending;

    /// <summary>
    /// Gets or sets the current progress percentage.
    /// </summary>
    public int Progress { get; set; }

    /// <summary>
    /// Gets or sets the optional due timestamp in UTC.
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public DateTime? DueAtUtc { get; set; }

    /// <summary>
    /// Gets or sets execution start timestamp in UTC.
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public DateTime? StartedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets execution completion timestamp in UTC.
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public DateTime? CompletedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the current step number (1-7).
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public int? CurrentStep { get; set; }

    /// <summary>
    /// Gets or sets the current step ID.
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public Guid? CurrentStepId { get; set; }

    /// <summary>
    /// Gets or sets the associated workflow definition ID.
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public Guid? WorkflowId { get; set; }

    /// <summary>
    /// Gets or sets the associated workflow instance ID.
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public Guid? WorkflowInstanceId { get; set; }

    /// <summary>
    /// Gets or sets the current workflow node ID.
    /// </summary>
    [SugarColumn(Length = 100, IsNullable = true)]
    public string? WorkflowNodeId { get; set; }
}

/// <summary>
/// Defines supported task status values.
/// </summary>
public enum TaskStatus
{
    Pending = 0,
    Running = 1,
    Completed = 2,
    Failed = 3,
    Paused = 4,
    Canceled = 5,
}

/// <summary>
/// Defines supported task type values.
/// </summary>
public enum TaskType
{
    General = 0,
    RepoSyncToSandbox = 1,
    Review = 2,
}
