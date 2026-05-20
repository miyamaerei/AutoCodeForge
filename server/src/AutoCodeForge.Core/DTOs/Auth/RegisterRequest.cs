using System.ComponentModel.DataAnnotations;

namespace AutoCodeForge.Core.DTOs.Auth;

/// <summary>
/// Represents user registration payload.
/// </summary>
public class RegisterRequest
{
    /// <summary>
    /// Gets or sets the ntid.
    /// </summary>
    [Required]
    [StringLength(128, MinimumLength = 3)]
    public string NtId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display name.
    /// </summary>
    [Required]
    [StringLength(128, MinimumLength = 2)]
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets email.
    /// </summary>
    [EmailAddress]
    [StringLength(256)]
    public string? Email { get; set; }

}
