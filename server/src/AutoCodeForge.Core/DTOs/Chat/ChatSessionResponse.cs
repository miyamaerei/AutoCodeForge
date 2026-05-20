namespace AutoCodeForge.Core.DTOs.Chat;

/// <summary>
/// Represents one chat session.
/// </summary>
public class ChatSessionResponse
{
    /// <summary>
    /// Gets or sets session identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets session title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets agent identifier bound to this session.
    /// </summary>
    public Guid? AgentId { get; set; }

    /// <summary>
    /// Gets or sets latest message time.
    /// </summary>
    public DateTime? LastMessageAtUtc { get; set; }

    /// <summary>
    /// Gets or sets session created time.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }
}
