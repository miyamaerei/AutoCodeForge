using AutoCodeForge.Core.Entities.Base;
using SqlSugar;

namespace AutoCodeForge.Core.Entities;

[SugarTable("TaskReviews")]
public class TaskReviewEntity : UserOwnedEntity
{
    [SugarColumn(IsPrimaryKey = true)]
    public new Guid Id { get; set; }

    public Guid TaskId { get; set; }

    public Guid TaskStepId { get; set; }

    public Guid ReviewerAgentId { get; set; }

    public ReviewVerdict Verdict { get; set; }

    [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
    public string? Comment { get; set; }

    [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
    public string? Issues { get; set; }

    public DateTime ReviewedAtUtc { get; set; }
}

public enum ReviewVerdict
{
    Approved = 0,
    Rejected = 1
}