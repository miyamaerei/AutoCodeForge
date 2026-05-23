using AutoCodeForge.Core.Entities.Base;
using SqlSugar;

namespace AutoCodeForge.Core.Entities;

/// <summary>
/// Represents a human intervention gate for task approval workflow.
/// </summary>
[SugarTable("HumanGates")]
public class HumanGateEntity : UserOwnedEntity
{
    [SugarColumn(IsPrimaryKey = true)]
    public new Guid Id { get; set; }

    [SugarColumn(IsNullable = false)]
    public Guid TaskId { get; set; }

    [SugarColumn(IsNullable = true)]
    public Guid? TaskStepId { get; set; }

    public HumanGateType GateType { get; set; }

    public HumanGateStatus Status { get; set; } = HumanGateStatus.Pending;

    [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
    public string? Reason { get; set; }

    [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
    public string? HumanResponse { get; set; }

    [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
    public string? ModificationsJson { get; set; }

    [SugarColumn(IsNullable = true)]
    public Guid? ReviewerUserId { get; set; }

    [SugarColumn(IsNullable = true)]
    public DateTime? RespondedAtUtc { get; set; }

    [SugarColumn(IsNullable = true)]
    public DateTime? TimeoutAtUtc { get; set; }
}

/// <summary>
/// Defines the types of human intervention gates.
/// </summary>
public enum HumanGateType
{
    RequirementConfirm = 1,
    PlanApproval = 2,
    CodeReview = 3,
    TestAcceptance = 4,
    MergeApproval = 5,
    FinalSignoff = 6,
    Emergency = 7,
}

/// <summary>
/// Defines the status of a human gate.
/// </summary>
public enum HumanGateStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2,
    Modified = 3,
    Timeout = 4,
    Cancelled = 5,
}