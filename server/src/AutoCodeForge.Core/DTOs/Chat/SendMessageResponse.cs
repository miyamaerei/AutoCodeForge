namespace AutoCodeForge.Core.DTOs.Chat;

/// <summary>
/// Represents a send message result.
/// </summary>
public class SendMessageResponse
{
    /// <summary>
    /// Gets or sets session identifier.
    /// </summary>
    public Guid SessionId { get; set; }

    /// <summary>
    /// Gets or sets selected agent identifier.
    /// </summary>
    public Guid AgentId { get; set; }

    /// <summary>
    /// Gets or sets user message.
    /// </summary>
    public ChatMessageResponse UserMessage { get; set; } = new();

    /// <summary>
    /// Gets or sets assistant message.
    /// </summary>
    public ChatMessageResponse AssistantMessage { get; set; } = new();
}
