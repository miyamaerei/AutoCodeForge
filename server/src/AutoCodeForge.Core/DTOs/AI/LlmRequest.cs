using System.ComponentModel.DataAnnotations;

namespace AutoCodeForge.Core.DTOs.AI;

/// <summary>
/// Represents one LLM request payload.
/// </summary>
public class LlmRequest
{
    /// <summary>
    /// Gets or sets preferred model config identifier.
    /// </summary>
    public Guid? PreferredModelId { get; set; }

    /// <summary>
    /// Gets or sets optional system prompt.
    /// </summary>
    [MaxLength(8000)]
    public string? SystemPrompt { get; set; }

    /// <summary>
    /// Gets or sets request messages.
    /// </summary>
    [Required]
    public List<ChatMessage> Messages { get; set; } = [];

    /// <summary>
    /// Gets or sets max output token count.
    /// </summary>
    [Range(1, 32000)]
    public int MaxTokens { get; set; } = 2048;

    /// <summary>
    /// Gets or sets temperature.
    /// </summary>
    [Range(0, 2)]
    public decimal Temperature { get; set; } = 0.7m;
}
