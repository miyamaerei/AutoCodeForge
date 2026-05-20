namespace AutoCodeForge.Core.DTOs.AI;

/// <summary>
/// Represents one LLM chat message.
/// </summary>
public class ChatMessage
{
    /// <summary>
    /// Gets or sets role name (system/user/assistant/tool).
    /// </summary>
    public string Role { get; set; } = "user";

    /// <summary>
    /// Gets or sets message content.
    /// </summary>
    public string Content { get; set; } = string.Empty;
}
