namespace AutoCodeForge.Core.DTOs.Agent;

/// <summary>
/// Represents an agent response.
/// </summary>
public class AgentResponse
{
    /// <summary>
    /// Gets or sets agent identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets agent name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets optional description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets matching keywords.
    /// </summary>
    public string? Keywords { get; set; }

    /// <summary>
    /// Gets or sets system prompt.
    /// </summary>
    public string? SystemPrompt { get; set; }

    /// <summary>
    /// Gets or sets model config identifier.
    /// </summary>
    public Guid? LlmModelConfigId { get; set; }

    /// <summary>
    /// Gets or sets whether agent is enabled.
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// Gets or sets UTC created timestamp.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets UTC updated timestamp.
    /// </summary>
    public DateTime UpdatedAtUtc { get; set; }
}
