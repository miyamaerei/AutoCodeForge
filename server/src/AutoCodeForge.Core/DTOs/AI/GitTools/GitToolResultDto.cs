namespace AutoCodeForge.Core.DTOs.AI.GitTools;

/// <summary>
/// Defines normalized output for one Git tool call.
/// </summary>
public class GitToolResultDto
{
    /// <summary>
    /// Gets or sets a value indicating whether the call succeeded.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets operation name.
    /// </summary>
    public string Operation { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets summary message.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets serialized payload.
    /// </summary>
    public object? Data { get; set; }

    /// <summary>
    /// Gets or sets mapped error code.
    /// </summary>
    public string? ErrorCode { get; set; }

    /// <summary>
    /// Gets or sets actionable suggestion for users.
    /// </summary>
    public string? Suggestion { get; set; }
}
