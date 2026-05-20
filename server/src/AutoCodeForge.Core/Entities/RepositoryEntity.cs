using AutoCodeForge.Core.Entities.Base;
using SqlSugar;

namespace AutoCodeForge.Core.Entities;

/// <summary>
/// Represents a connected source code repository.
/// </summary>
[SugarTable("Repositories")]
public class RepositoryEntity : UserOwnedEntity
{
    /// <summary>
    /// Gets or sets the repository identifier.
    /// </summary>
    [SugarColumn(IsPrimaryKey = true)]
    public new Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the repository name.
    /// </summary>
    [SugarColumn(Length = 200, IsNullable = false)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the repository URL.
    /// </summary>
    [SugarColumn(Length = 500, IsNullable = false)]
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the source control provider.
    /// </summary>
    public GitProvider Provider { get; set; } = GitProvider.GitHub;

    /// <summary>
    /// Gets or sets authentication type.
    /// </summary>
    public AuthenticationType AuthType { get; set; } = AuthenticationType.Token;

    /// <summary>
    /// Gets or sets merge strategy.
    /// </summary>
    public MergeStrategy MergeStrategy { get; set; } = MergeStrategy.Squash;

    /// <summary>
    /// Gets or sets the encrypted authentication token.
    /// </summary>
    [SugarColumn(Length = 1000, IsNullable = false)]
    public string EncryptedToken { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the platform-assigned webhook identifier, when registered.
    /// </summary>
    [SugarColumn(Length = 200, IsNullable = true)]
    public string? WebhookId { get; set; }

    /// <summary>
    /// Gets or sets the default review rule set identifier for new review tasks.
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public Guid? DefaultReviewRuleSetId { get; set; }

    /// <summary>
    /// Gets or sets the HMAC signing secret used to verify incoming webhook payloads.
    /// </summary>
    [SugarColumn(Length = 512, IsNullable = true)]
    public string? WebhookSecret { get; set; }
}

/// <summary>
/// Defines git providers.
/// </summary>
public enum GitProvider
{
    GitHub = 0,
    GitLab = 1,
    AzureDevOps = 2,
    Bitbucket = 3,
}

/// <summary>
/// Defines repository authentication types.
/// </summary>
public enum AuthenticationType
{
    Token = 0,
    SshKey = 1,
    UsernamePassword = 2,
}

/// <summary>
/// Defines merge policies.
/// </summary>
public enum MergeStrategy
{
    MergeCommit = 0,
    Squash = 1,
    Rebase = 2,
}
