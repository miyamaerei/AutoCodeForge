namespace AutoCodeForge.Core.DTOs.Chat;

/// <summary>
/// Represents one chat message.
/// </summary>
public class ChatMessageResponse
{
    /// <summary>
    /// Gets or sets message identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets session identifier.
    /// </summary>
    public Guid ChatSessionId { get; set; }

    /// <summary>
    /// Gets or sets message role.
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets message body.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets model name when present.
    /// </summary>
    public string? ModelName { get; set; }

    /// <summary>
    /// Gets or sets UTC created timestamp.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }
}
