using AutoCodeForge.Core.Entities.Base;
using SqlSugar;

namespace AutoCodeForge.Core.Entities;

/// <summary>
/// Records one agent tool invocation for audit and troubleshooting.
/// </summary>
[SugarTable("AgentToolInvocations")]
public class AgentToolInvocationEntity : AuditableEntity
{
    /// <summary>
    /// Gets or sets invocation identifier.
    /// </summary>
    [SugarColumn(IsPrimaryKey = true)]
    public new Guid Id { get; set; }

    /// <summary>
    /// Gets or sets operator NtId.
    /// </summary>
    [SugarColumn(Length = 128, IsNullable = true)]
    public string? NtId { get; set; }

    /// <summary>
    /// Gets or sets chat session identifier.
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public Guid? SessionId { get; set; }

    /// <summary>
    /// Gets or sets task identifier.
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public Guid? TaskId { get; set; }

    /// <summary>
    /// Gets or sets repository identifier.
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public Guid? RepositoryId { get; set; }

    /// <summary>
    /// Gets or sets tool name.
    /// </summary>
    [SugarColumn(Length = 100, IsNullable = false)]
    public string ToolName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets input digest.
    /// </summary>
    [SugarColumn(Length = 256, IsNullable = true)]
    public string? InputDigest { get; set; }

    /// <summary>
    /// Gets or sets output digest.
    /// </summary>
    [SugarColumn(Length = 256, IsNullable = true)]
    public string? OutputDigest { get; set; }

    /// <summary>
    /// Gets or sets status text.
    /// </summary>
    [SugarColumn(Length = 32, IsNullable = false)]
    public string Status { get; set; } = "Succeeded";

    /// <summary>
    /// Gets or sets error code when invocation fails.
    /// </summary>
    [SugarColumn(Length = 64, IsNullable = true)]
    public string? ErrorCode { get; set; }

    /// <summary>
    /// Gets or sets latency in milliseconds.
    /// </summary>
    public long LatencyMs { get; set; }
}
