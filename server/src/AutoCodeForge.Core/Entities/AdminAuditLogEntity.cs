using AutoCodeForge.Core.Entities.Base;
using SqlSugar;

namespace AutoCodeForge.Core.Entities;

/// <summary>
/// Records an admin cross-tenant access event for audit trail purposes.
/// </summary>
[SugarTable("AdminAuditLogs")]
public class AdminAuditLogEntity : AuditableEntity
{
    /// <summary>
    /// Gets or sets the log entry identifier.
    /// </summary>
    [SugarColumn(IsPrimaryKey = true)]
    public new Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the NtId of the admin performing the action.
    /// </summary>
    [SugarColumn(Length = 128, IsNullable = false)]
    public string AdminNtId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the target tenant NtId being accessed, when applicable.
    /// </summary>
    [SugarColumn(Length = 128, IsNullable = true)]
    public string? TargetNtId { get; set; }

    /// <summary>
    /// Gets or sets the HTTP method of the action.
    /// </summary>
    [SugarColumn(Length = 16, IsNullable = false)]
    public string HttpMethod { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the request path that triggered the audit entry.
    /// </summary>
    [SugarColumn(Length = 500, IsNullable = false)]
    public string RequestPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the resource type being accessed.
    /// </summary>
    [SugarColumn(Length = 100, IsNullable = true)]
    public string? ResourceType { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the specific resource accessed, when applicable.
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public Guid? ResourceId { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp of the audit event.
    /// </summary>
    public DateTime OccurredAtUtc { get; set; } = DateTime.UtcNow;
}
