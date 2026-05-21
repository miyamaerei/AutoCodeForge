namespace AutoCodeForge.Core.Models;

/// <summary>
/// Defines Git configuration options for handling different Git providers.
/// </summary>
public class GitOptions
{
    /// <summary>
    /// Gets or sets the configuration section name.
    /// </summary>
    public const string SectionName = "Git";

    /// <summary>
    /// Gets or sets Azure DevOps specific configuration.
    /// </summary>
    public AzureDevOpsOptions AzureDevOps { get; set; } = new AzureDevOpsOptions();

    /// <summary>
    /// Gets or sets path handling configuration.
    /// </summary>
    public PathOptions Path { get; set; } = new PathOptions();

    /// <summary>
    /// Gets or sets special string handling configuration.
    /// </summary>
    public StringHandlingOptions StringHandling { get; set; } = new StringHandlingOptions();

    /// <summary>
    /// Gets or sets provider-specific configurations.
    /// </summary>
    public ProviderOptions Providers { get; set; } = new ProviderOptions();
}

/// <summary>
/// Azure DevOps specific configuration options.
/// </summary>
public class AzureDevOpsOptions
{
    /// <summary>
    /// Gets or sets the list of domain patterns to identify Azure DevOps repositories.
    /// </summary>
    public List<string> DomainPatterns { get; set; } = new List<string> { "dev.azure.com", "visualstudio.com" };

    /// <summary>
    /// Gets or sets the username to use for Azure DevOps authentication.
    /// Azure DevOps requires an empty string as username when using PAT tokens.
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether URL encoding is enabled for Azure DevOps URLs.
    /// </summary>
    public bool EnableUrlEncoding { get; set; } = true;

    /// <summary>
    /// Gets or sets the default organization name.
    /// </summary>
    public string DefaultOrganization { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the default project name.
    /// </summary>
    public string DefaultProject { get; set; } = string.Empty;
}

/// <summary>
/// Path handling configuration options.
/// </summary>
public class PathOptions
{
    /// <summary>
    /// Gets or sets the maximum allowed path length for repositories.
    /// </summary>
    public int MaxPathLength { get; set; } = 260;

    /// <summary>
    /// Gets or sets the temporary path template for cloning repositories.
    /// Supports {guid} placeholder for unique directory names.
    /// </summary>
    public string TempPathTemplate { get; set; } = "C:\\temp\\repo-{guid}";

    /// <summary>
    /// Gets or sets a value indicating whether to shorten path names automatically.
    /// </summary>
    public bool AutoShortenPaths { get; set; } = true;

    /// <summary>
    /// Gets or sets the minimum length of unique path identifiers.
    /// </summary>
    public int ShortPathIdLength { get; set; } = 3;
}

/// <summary>
/// Special string handling configuration options.
/// </summary>
public class StringHandlingOptions
{
    /// <summary>
    /// Gets or sets the username to use for GitHub authentication.
    /// GitHub uses 'x-access-token' as username when using PAT tokens.
    /// </summary>
    public string GitHubUsername { get; set; } = "x-access-token";

    /// <summary>
    /// Gets or sets the username to use for GitLab authentication.
    /// </summary>
    public string GitLabUsername { get; set; } = "oauth2";

    /// <summary>
    /// Gets or sets characters that require URL encoding in repository paths.
    /// </summary>
    public List<string> SpecialCharacters { get; set; } = new List<string> { " ", "@", "#", "$", "%", "^", "&", "*", "(", ")", "+", ",", ";", "=", "?", "/" };

    /// <summary>
    /// Gets or sets a value indicating whether to normalize whitespace in repository names.
    /// </summary>
    public bool NormalizeWhitespace { get; set; } = true;

    /// <summary>
    /// Gets or sets the replacement character for whitespace normalization.
    /// </summary>
    public string WhitespaceReplacement { get; set; } = "_";
}

/// <summary>
/// Provider-specific configuration options.
/// </summary>
public class ProviderOptions
{
    /// <summary>
    /// Gets or sets the default username for generic Git providers.
    /// </summary>
    public string DefaultUsername { get; set; } = "git";

    /// <summary>
    /// Gets or sets a value indicating whether to use SSH authentication by default.
    /// </summary>
    public bool UseSshByDefault { get; set; } = false;

    /// <summary>
    /// Gets or sets the SSH port.
    /// </summary>
    public int SshPort { get; set; } = 22;

    /// <summary>
    /// Gets or sets the connection timeout in seconds.
    /// </summary>
    public int ConnectionTimeoutSeconds { get; set; } = 60;
}