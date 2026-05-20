namespace AutoCodeForge.Core.ValueObjects;

/// <summary>
/// Represents encrypted git authentication credentials.
/// </summary>
public class GitCredential
{
    /// <summary>
    /// Gets or sets the authentication token (encrypted).
    /// </summary>
    public string? Token { get; set; }

    /// <summary>
    /// Gets or sets the username for basic auth.
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// Gets or sets the password for basic auth (encrypted).
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// Gets or sets the SSH private key (encrypted).
    /// </summary>
    public string? SshKey { get; set; }
}
