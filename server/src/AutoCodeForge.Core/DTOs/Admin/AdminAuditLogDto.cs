namespace AutoCodeForge.Core.DTOs.Admin;

/// <summary>
/// Response DTO for an admin audit log entry.
/// </summary>
public class AdminAuditLogDto
{
    /// <summary>Gets or sets the log entry identifier.</summary>
    public Guid Id { get; set; }

    /// <summary>Gets or sets the admin NtId.</summary>
    public string AdminNtId { get; set; } = string.Empty;

    /// <summary>Gets or sets the target tenant NtId, if applicable.</summary>
    public string? TargetNtId { get; set; }

    /// <summary>Gets or sets the HTTP method.</summary>
    public string HttpMethod { get; set; } = string.Empty;

    /// <summary>Gets or sets the request path.</summary>
    public string RequestPath { get; set; } = string.Empty;

    /// <summary>Gets or sets the resource type.</summary>
    public string? ResourceType { get; set; }

    /// <summary>Gets or sets the access decision (allow or deny).</summary>
    public string AccessDecision { get; set; } = string.Empty;

    /// <summary>Gets or sets a short reason code for the decision.</summary>
    public string DecisionReason { get; set; } = string.Empty;

    /// <summary>Gets or sets the resource identifier.</summary>
    public Guid? ResourceId { get; set; }

    /// <summary>Gets or sets the UTC time of the event.</summary>
    public DateTime OccurredAtUtc { get; set; }
}
