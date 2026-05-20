using AutoCodeForge.Core.Entities.Base;
using SqlSugar;

namespace AutoCodeForge.Core.Entities;

/// <summary>
/// Represents runtime logs for a task.
/// </summary>
[SugarTable("TaskLogs")]
public class TaskLogEntity : UserOwnedEntity
{
    /// <summary>
    /// Gets or sets the task log identifier.
    /// </summary>
    [SugarColumn(IsPrimaryKey = true)]
    public new Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the task identifier.
    /// </summary>
    public Guid TaskId { get; set; }

    /// <summary>
    /// Gets or sets the log level.
    /// </summary>
    [SugarColumn(Length = 32, IsNullable = false)]
    public string Level { get; set; } = "Info";

    /// <summary>
    /// Gets or sets the log message.
    /// </summary>
    [SugarColumn(ColumnDataType = "TEXT", IsNullable = false)]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the log source.
    /// </summary>
    [SugarColumn(Length = 128, IsNullable = true)]
    public string? Source { get; set; }
}
