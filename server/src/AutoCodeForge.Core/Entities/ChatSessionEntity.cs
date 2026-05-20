using AutoCodeForge.Core.Entities.Base;
using SqlSugar;

namespace AutoCodeForge.Core.Entities;

/// <summary>
/// Represents a chat conversation session.
/// </summary>
[SugarTable("ChatSessions")]
public class ChatSessionEntity : UserOwnedEntity
{
    /// <summary>
    /// Gets or sets the chat session identifier.
    /// </summary>
    [SugarColumn(IsPrimaryKey = true)]
    public new Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the session title.
    /// </summary>
    [SugarColumn(Length = 200, IsNullable = false)]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets bound agent identifier.
    /// </summary>
    public Guid? AgentId { get; set; }

    /// <summary>
    /// Gets or sets the related task identifier.
    /// </summary>
    public Guid? TaskId { get; set; }

    /// <summary>
    /// Gets or sets the latest message timestamp in UTC.
    /// </summary>
    public DateTime? LastMessageAtUtc { get; set; }
}
