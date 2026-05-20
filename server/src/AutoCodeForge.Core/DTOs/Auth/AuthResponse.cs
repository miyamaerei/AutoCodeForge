namespace AutoCodeForge.Core.DTOs.Auth;

/// <summary>
/// Represents authentication response payload.
/// </summary>
public class AuthResponse
{
    /// <summary>
    /// Gets or sets JWT access token.
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets token expiration timestamp in UTC.
    /// </summary>
    public DateTime ExpiresAtUtc { get; set; }

    /// <summary>
    /// Gets or sets user ntid.
    /// </summary>
    public string NtId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets user display name.
    /// </summary>
    public string UserName { get; set; } = string.Empty;
}
