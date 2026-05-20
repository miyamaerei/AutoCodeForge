using AutoCodeForge.Core.DTOs.Repository;
using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Exceptions;
using AutoCodeForge.Infrastructure.Repositories;

namespace AutoCodeForge.Application.Services;

/// <summary>
/// Manages review-related repository settings.
/// </summary>
public class RepositoryReviewSettingsService
{
    private readonly RepositoryRepository _repositoryRepository;
    private readonly ReviewRuleSetRepository _reviewRuleSetRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="RepositoryReviewSettingsService"/> class.
    /// </summary>
    /// <param name="repositoryRepository">The repository repository.</param>
    /// <param name="reviewRuleSetRepository">The review rule set repository.</param>
    public RepositoryReviewSettingsService(
        RepositoryRepository repositoryRepository,
        ReviewRuleSetRepository reviewRuleSetRepository)
    {
        _repositoryRepository = repositoryRepository;
        _reviewRuleSetRepository = reviewRuleSetRepository;
    }

    /// <summary>
    /// Updates repository default review rule set binding.
    /// </summary>
    /// <param name="repositoryId">The repository identifier.</param>
    /// <param name="request">The update request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated review settings.</returns>
    public async Task<RepositoryReviewSettingsDto> UpdateAsync(
        Guid repositoryId,
        UpdateRepositoryReviewSettingsRequest request,
        CancellationToken cancellationToken = default)
    {
        var repository = await _repositoryRepository.GetByIdAsync(repositoryId, false, cancellationToken)
            ?? throw new NotFoundException("Repository not found");

        if (request.DefaultReviewRuleSetId.HasValue)
        {
            var ruleSet = await _reviewRuleSetRepository.GetByIdAsync(request.DefaultReviewRuleSetId.Value, false, cancellationToken)
                ?? throw new NotFoundException("Review rule set not found");

            if (!ruleSet.IsEnabled)
            {
                throw new ValidationException("Review rule set is disabled");
            }

            if (ruleSet.Level == ReviewRuleSetLevel.Repository && ruleSet.RepositoryId != repositoryId)
            {
                throw new ValidationException("Repository-scoped review rule set must belong to the same repository");
            }
        }

        repository.DefaultReviewRuleSetId = request.DefaultReviewRuleSetId;
        await _repositoryRepository.UpdateAsync(repository, cancellationToken);

        return new RepositoryReviewSettingsDto
        {
            RepositoryId = repository.Id,
            DefaultReviewRuleSetId = repository.DefaultReviewRuleSetId,
        };
    }
}