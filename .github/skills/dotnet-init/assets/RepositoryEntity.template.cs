using SqlSugar;

namespace __ProjectName__.Entities;

[SugarTable("Repositories")]
public class RepositoryEntity
{
    [SugarColumn(IsPrimaryKey = true)]
    public Guid Id { get; set; } = Guid.NewGuid();

    public string NtId { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public GitProvider Provider { get; set; } = GitProvider.GitHub;

    public string Owner { get; set; } = string.Empty;

    public string Repository { get; set; } = string.Empty;

    public string DefaultBranch { get; set; } = "main";

    public AuthenticationType AuthType { get; set; } = AuthenticationType.Token;

    public string? CredentialRef { get; set; }

    public MergeStrategy MergeStrategy { get; set; } = MergeStrategy.Merge;

    public bool EnableCIPolicy { get; set; } = false;

    public bool IsDeleted { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public enum GitProvider { GitHub, AzureDevOps, GitLab }
public enum AuthenticationType { Token, App, OAuth }
public enum MergeStrategy { Merge, Squash, Rebase }
