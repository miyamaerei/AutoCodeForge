using System.ComponentModel.DataAnnotations;

namespace AutoCodeForge.Core.DTOs.Chat;

/// <summary>
/// Represents a create session request.
/// </summary>
public class CreateSessionRequest
{
    /// <summary>
    /// Gets or sets session title.
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets optional initial agent identifier.
    /// </summary>
    public Guid? AgentId { get; set; }
}
