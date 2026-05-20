namespace AutoCodeForge.Core.Models;

/// <summary>
/// Defines JWT configuration options.
/// </summary>
public class JwtOptions
{
    /// <summary>
    /// Gets or sets issuer.
    /// </summary>
    public string Issuer { get; set; } = "AutoCodeForge";

    /// <summary>
    /// Gets or sets audience.
    /// </summary>
    public string Audience { get; set; } = "AutoCodeForge.Client";

    /// <summary>
    /// Gets or sets signing key.
    /// </summary>
    public string Key { get; set; } = "CHANGE_ME_IN_PRODUCTION_WITH_AT_LEAST_32_CHARS";

    /// <summary>
    /// Gets or sets token validity in minutes.
    /// </summary>
    public int ExpireMinutes { get; set; } = 120;
}
