using AutoCodeForge.Core.Entities.Base;
using SqlSugar;

namespace AutoCodeForge.Core.Entities;

/// <summary>
/// Represents one message in a chat session.
/// </summary>
[SugarTable("ChatMessages")]
public class ChatMessageEntity : UserOwnedEntity
{
    /// <summary>
    /// Gets or sets the message identifier.
    /// </summary>
    [SugarColumn(IsPrimaryKey = true)]
    public new Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the owning chat session identifier.
    /// </summary>
    public Guid ChatSessionId { get; set; }

    /// <summary>
    /// Gets or sets the message role.
    /// </summary>
    public MessageType Type { get; set; }

    /// <summary>
    /// Gets or sets the message body.
    /// </summary>
    [SugarColumn(ColumnDataType = "TEXT", IsNullable = false)]
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets optional model name.
    /// </summary>
    [SugarColumn(Length = 100, IsNullable = true)]
    public string? ModelName { get; set; }
}

/// <summary>
/// Defines chat message roles.
/// </summary>
public enum MessageType
{
    System = 0,
    User = 1,
    Assistant = 2,
    Tool = 3,
}
