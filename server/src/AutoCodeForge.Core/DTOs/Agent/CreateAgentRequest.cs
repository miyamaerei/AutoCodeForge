using System.ComponentModel.DataAnnotations;

namespace AutoCodeForge.Core.DTOs.Agent;

/// <summary>
/// Represents an agent creation request.
/// </summary>
public class CreateAgentRequest
{
    /// <summary>
    /// Gets or sets agent name.
    /// </summary>
    [Required]
    [MaxLength(120)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets optional agent description.
    /// </summary>
    [MaxLength(2000)]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets optional matching keywords separated by comma.
    /// </summary>
    [MaxLength(500)]
    public string? Keywords { get; set; }

    /// <summary>
    /// Gets or sets optional system prompt.
    /// </summary>
    [MaxLength(8000)]
    public string? SystemPrompt { get; set; }

    /// <summary>
    /// Gets or sets preferred model config identifier.
    /// </summary>
    public Guid? LlmModelConfigId { get; set; }

    /// <summary>
    /// Gets or sets whether the agent is enabled.
    /// </summary>
    public bool IsEnabled { get; set; } = true;
}
