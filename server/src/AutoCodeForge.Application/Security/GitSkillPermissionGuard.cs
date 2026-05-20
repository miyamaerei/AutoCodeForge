using AutoCodeForge.Core.Exceptions;
using AutoCodeForge.Core.Models.Security;
using AutoCodeForge.Infrastructure.Repositories;

namespace AutoCodeForge.Application.Security;

/// <summary>
/// Validates whether one Git skill operation is allowed for current user and repository.
/// </summary>
public class GitSkillPermissionGuard
{
    private readonly GitSkillGrantRepository _grantRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="GitSkillPermissionGuard"/> class.
    /// </summary>
    /// <param name="grantRepository">The Git skill grant repository.</param>
    public GitSkillPermissionGuard(GitSkillGrantRepository grantRepository)
    {
        _grantRepository = grantRepository;
    }

    /// <summary>
    /// Ensures caller has permission to execute the operation.
    /// </summary>
    /// <param name="repositoryId">Repository identifier.</param>
    /// <param name="operation">Git operation name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task EnsureAllowedAsync(
        Guid repositoryId,
        string operation,
        CancellationToken cancellationToken = default)
    {
        var normalized = GitOperationPolicy.NormalizeOperation(operation);
        var required = GitOperationPolicy.GetRequiredLevel(normalized);
        if (required == GitSkillPermissionLevel.Dangerous)
        {
            throw new ForbiddenException("Dangerous Git operations are disabled by policy.");
        }

        var grant = await _grantRepository.GetByRepositoryIdAsync(repositoryId, cancellationToken);
        var grantedLevel = ParseLevelOrDefault(grant?.Level, GitSkillPermissionLevel.ReadOnly);

        if (grantedLevel < required)
        {
            throw new ForbiddenException($"Operation '{normalized}' requires '{required}' permission.");
        }
    }

    private static GitSkillPermissionLevel ParseLevelOrDefault(string? level, GitSkillPermissionLevel fallback)
    {
        return Enum.TryParse<GitSkillPermissionLevel>(level, true, out var parsed) ? parsed : fallback;
    }
}
