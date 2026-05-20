using AutoCodeForge.Core.DTOs.AI.GitTools;
using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Exceptions;
using AutoCodeForge.Core.Models.Security;
using AutoCodeForge.Infrastructure.Repositories;

namespace AutoCodeForge.Application.Services;

/// <summary>
/// Provides CRUD-style operations for Git skill grants.
/// </summary>
public class GitSkillPolicyService
{
    private readonly GitSkillGrantRepository _grantRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="GitSkillPolicyService"/> class.
    /// </summary>
    /// <param name="grantRepository">The grant repository.</param>
    public GitSkillPolicyService(GitSkillGrantRepository grantRepository)
    {
        _grantRepository = grantRepository;
    }

    /// <summary>
    /// Gets Git skill grant by repository identifier.
    /// </summary>
    /// <param name="repositoryId">Repository identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The current repository-level grant.</returns>
    public async Task<GitSkillGrantResponse> GetByRepositoryAsync(Guid repositoryId, CancellationToken cancellationToken = default)
    {
        var entity = await _grantRepository.GetByRepositoryIdAsync(repositoryId, cancellationToken);
        if (entity is null)
        {
            return new GitSkillGrantResponse
            {
                RepositoryId = repositoryId,
                Level = GitSkillPermissionLevel.ReadOnly.ToString(),
                UpdatedAtUtc = DateTime.UtcNow,
            };
        }

        return ToResponse(entity);
    }

    /// <summary>
    /// Creates or updates repository-level Git skill grant.
    /// </summary>
    /// <param name="repositoryId">Repository identifier.</param>
    /// <param name="request">Update request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated grant response.</returns>
    public async Task<GitSkillGrantResponse> UpsertAsync(
        Guid repositoryId,
        UpdateGitSkillGrantRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<GitSkillPermissionLevel>(request.Level, true, out var level))
        {
            throw new ValidationException("Invalid Git skill level. Allowed values: ReadOnly, Write, Dangerous");
        }

        var existing = await _grantRepository.GetByRepositoryIdAsync(repositoryId, cancellationToken);
        if (existing is null)
        {
            var created = await _grantRepository.CreateAsync(new GitSkillGrantEntity
            {
                Id = Guid.NewGuid(),
                RepositoryId = repositoryId,
                Level = level.ToString(),
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow,
            }, cancellationToken);

            return ToResponse(created);
        }

        existing.Level = level.ToString();
        existing.UpdatedAtUtc = DateTime.UtcNow;
        await _grantRepository.UpdateAsync(existing, cancellationToken);
        return ToResponse(existing);
    }

    private static GitSkillGrantResponse ToResponse(GitSkillGrantEntity entity)
    {
        return new GitSkillGrantResponse
        {
            RepositoryId = entity.RepositoryId,
            Level = entity.Level,
            UpdatedAtUtc = entity.UpdatedAtUtc,
        };
    }
}
