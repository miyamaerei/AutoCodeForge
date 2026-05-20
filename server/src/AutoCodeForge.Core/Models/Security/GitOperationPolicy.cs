namespace AutoCodeForge.Core.Models.Security;

/// <summary>
/// Defines Git operation permission levels.
/// </summary>
public enum GitSkillPermissionLevel
{
    ReadOnly = 0,
    Write = 1,
    Dangerous = 2,
}

/// <summary>
/// Provides policy mapping from operation names to required permission levels.
/// </summary>
public static class GitOperationPolicy
{
    private static readonly Dictionary<string, GitSkillPermissionLevel> Policy = new(StringComparer.OrdinalIgnoreCase)
    {
        ["list-branches"] = GitSkillPermissionLevel.ReadOnly,
        ["get-commits"] = GitSkillPermissionLevel.ReadOnly,
        ["list-pull-requests"] = GitSkillPermissionLevel.ReadOnly,
        ["get-diff"] = GitSkillPermissionLevel.ReadOnly,
        ["create-pull-request"] = GitSkillPermissionLevel.Write,
        ["push"] = GitSkillPermissionLevel.Write,
        ["create-branch"] = GitSkillPermissionLevel.Write,
        ["commit-changes"] = GitSkillPermissionLevel.Write,
        ["force-push"] = GitSkillPermissionLevel.Dangerous,
        ["delete-branch"] = GitSkillPermissionLevel.Dangerous,
    };

    /// <summary>
    /// Gets required permission level for one operation.
    /// Unknown operations default to <see cref="GitSkillPermissionLevel.Dangerous"/>.
    /// </summary>
    /// <param name="operation">The operation name.</param>
    /// <returns>The required permission level.</returns>
    public static GitSkillPermissionLevel GetRequiredLevel(string operation)
    {
        if (string.IsNullOrWhiteSpace(operation))
        {
            return GitSkillPermissionLevel.Dangerous;
        }

        return Policy.TryGetValue(NormalizeOperation(operation), out var level)
            ? level
            : GitSkillPermissionLevel.Dangerous;
    }

    /// <summary>
    /// Normalizes operation to a policy key.
    /// </summary>
    /// <param name="operation">The operation name.</param>
    /// <returns>The normalized operation.</returns>
    public static string NormalizeOperation(string operation)
    {
        return operation.Trim().ToLowerInvariant();
    }
}
