namespace AutoCodeForge.Core.DTOs.AI.GitTools;

/// <summary>
/// Defines normalized input for one Git tool call.
/// </summary>
public class GitToolRequestDto
{
    /// <summary>
    /// Gets or sets operation name.
    /// </summary>
    public string Operation { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets repository identifier.
    /// </summary>
    public Guid? RepositoryId { get; set; }

    /// <summary>
    /// Gets or sets bound task identifier.
    /// </summary>
    public Guid? TaskId { get; set; }

    /// <summary>
    /// Gets or sets chat session identifier.
    /// </summary>
    public Guid? SessionId { get; set; }

    /// <summary>
    /// Gets or sets free-form arguments.
    /// </summary>
    public Dictionary<string, string> Arguments { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
