namespace AutoCodeForge.Core.DTOs.AI;

/// <summary>
/// Represents one LLM response payload.
/// </summary>
public class LlmResponse
{
    /// <summary>
    /// Gets or sets output text.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets model name used by the response.
    /// </summary>
    public string ModelName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets UTC completion timestamp.
    /// </summary>
    public DateTime CompletedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets token usage when available.
    /// </summary>
    public int? TotalTokens { get; set; }
}
