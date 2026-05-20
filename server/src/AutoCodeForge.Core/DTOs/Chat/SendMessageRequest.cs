using System.ComponentModel.DataAnnotations;

namespace AutoCodeForge.Core.DTOs.Chat;

/// <summary>
/// Represents a send message request.
/// </summary>
public class SendMessageRequest
{
    /// <summary>
    /// Gets or sets user message text.
    /// </summary>
    [Required]
    [MaxLength(16000)]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets optional preferred agent identifier.
    /// </summary>
    public Guid? AgentId { get; set; }
}
