using System.ComponentModel.DataAnnotations;

namespace AutoCodeForge.Core.DTOs.Auth;

/// <summary>
/// Represents login request payload.
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// Gets or sets the ntid.
    /// </summary>
    [Required]
    [StringLength(128, MinimumLength = 3)]
    public string NtId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets plain password.
    /// </summary>
    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string Password { get; set; } = string.Empty;
}
