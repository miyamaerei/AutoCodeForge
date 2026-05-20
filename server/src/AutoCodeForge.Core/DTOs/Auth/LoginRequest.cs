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
    [StringLength(128, MinimumLength = 3)]
    public string? NtId { get; set; }

    /// <summary>
    /// Gets or sets the optional display name.
    /// </summary>
    [StringLength(128, MinimumLength = 2)]
    public string? UserName { get; set; }

    /// <summary>
    /// Gets or sets the optional email.
    /// </summary>
    [EmailAddress]
    [StringLength(256)]
    public string? Email { get; set; }
}
