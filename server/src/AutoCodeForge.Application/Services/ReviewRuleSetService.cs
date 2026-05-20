using AutoCodeForge.Core.DTOs.Review;
using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Exceptions;
using AutoCodeForge.Core.Helpers;
using AutoCodeForge.Core.Models;
using AutoCodeForge.Infrastructure.Repositories;

namespace AutoCodeForge.Application.Services;

/// <summary>
/// Provides CRUD operations for review rule sets.
/// </summary>
public class ReviewRuleSetService
{
    private readonly ReviewRuleSetRepository _reviewRuleSetRepository;
    private readonly RepositoryRepository _repositoryRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReviewRuleSetService"/> class.
    /// </summary>
    /// <param name="reviewRuleSetRepository">The review rule set repository.</param>
    /// <param name="repositoryRepository">The repository repository.</param>
    public ReviewRuleSetService(
        ReviewRuleSetRepository reviewRuleSetRepository,
        RepositoryRepository repositoryRepository)
    {
        _reviewRuleSetRepository = reviewRuleSetRepository;
        _repositoryRepository = repositoryRepository;
    }

    /// <summary>
    /// Creates one review rule set.
    /// </summary>
    /// <param name="request">The create request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created rule set.</returns>
    public async Task<ReviewRuleSetDto> CreateAsync(
        CreateReviewRuleSetRequest request,
        CancellationToken cancellationToken = default)
    {
        await ValidateScopeAsync(request.Level, request.RepositoryId, cancellationToken);
        ValidateRules(request.Rules);

        var entity = new ReviewRuleSetEntity
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            Level = request.Level,
            RepositoryId = request.Level == ReviewRuleSetLevel.Repository ? request.RepositoryId : null,
            IsEnabled = request.IsEnabled,
            Version = request.Version.Trim(),
            RulesJson = JsonHelper.Serialize(request.Rules),
        };

        var created = await _reviewRuleSetRepository.CreateAsync(entity, cancellationToken);
        return ToDto(created);
    }

    /// <summary>
    /// Gets one review rule set by identifier.
    /// </summary>
    /// <param name="id">The rule set identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The rule set DTO.</returns>
    public async Task<ReviewRuleSetDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _reviewRuleSetRepository.GetByIdAsync(id, false, cancellationToken)
            ?? throw new NotFoundException("Review rule set not found");
        return ToDto(entity);
    }

    /// <summary>
    /// Gets paged review rule sets.
    /// </summary>
    /// <param name="page">The page number.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="repositoryId">Optional repository identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Paged rule set DTOs.</returns>
    public async Task<PagedResult<ReviewRuleSetDto>> GetPagedAsync(
        int page,
        int pageSize,
        Guid? repositoryId = null,
        CancellationToken cancellationToken = default)
    {
        var paged = await _reviewRuleSetRepository.GetPagedAsync(page, pageSize, repositoryId, cancellationToken);
        return new PagedResult<ReviewRuleSetDto>(
            paged.Items.Select(ToDto).ToList(),
            paged.TotalCount,
            paged.Page,
            paged.PageSize);
    }

    /// <summary>
    /// Updates one review rule set.
    /// </summary>
    /// <param name="id">The rule set identifier.</param>
    /// <param name="request">The update request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated rule set DTO.</returns>
    public async Task<ReviewRuleSetDto> UpdateAsync(
        Guid id,
        UpdateReviewRuleSetRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await _reviewRuleSetRepository.GetByIdAsync(id, false, cancellationToken)
            ?? throw new NotFoundException("Review rule set not found");

        var level = request.Level ?? entity.Level;
        var repositoryId = request.Level.HasValue || request.RepositoryId.HasValue
            ? request.RepositoryId
            : entity.RepositoryId;
        await ValidateScopeAsync(level, repositoryId, cancellationToken);

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            entity.Name = request.Name.Trim();
        }

        if (request.Description is not null)
        {
            entity.Description = request.Description?.Trim();
        }

        if (request.Level.HasValue)
        {
            entity.Level = request.Level.Value;
        }

        if (request.Level.HasValue || request.RepositoryId.HasValue)
        {
            entity.RepositoryId = level == ReviewRuleSetLevel.Repository ? repositoryId : null;
        }

        if (request.IsEnabled.HasValue)
        {
            entity.IsEnabled = request.IsEnabled.Value;
        }

        if (!string.IsNullOrWhiteSpace(request.Version))
        {
            entity.Version = request.Version.Trim();
        }

        if (request.Rules is not null)
        {
            ValidateRules(request.Rules);
            entity.RulesJson = JsonHelper.Serialize(request.Rules);
        }

        await _reviewRuleSetRepository.UpdateAsync(entity, cancellationToken);
        return ToDto(entity);
    }

    /// <summary>
    /// Deletes one review rule set.
    /// </summary>
    /// <param name="id">The rule set identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing asynchronous execution.</returns>
    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _reviewRuleSetRepository.SoftDeleteAsync(id, false, cancellationToken);
    }

    private async Task ValidateScopeAsync(
        ReviewRuleSetLevel level,
        Guid? repositoryId,
        CancellationToken cancellationToken)
    {
        if (level == ReviewRuleSetLevel.Repository)
        {
            if (!repositoryId.HasValue)
            {
                throw new ValidationException("Repository-scoped rule set requires repositoryId");
            }

            var repository = await _repositoryRepository.GetByIdAsync(repositoryId.Value, false, cancellationToken);
            if (repository is null)
            {
                throw new ValidationException("Repository not found for repository-scoped rule set");
            }
        }
    }

    private static void ValidateRules(IReadOnlyCollection<ReviewRuleDto> rules)
    {
        if (rules.Count == 0)
        {
            throw new ValidationException("Review rule set must contain at least one rule");
        }

        if (rules.Any(rule => string.IsNullOrWhiteSpace(rule.Code)
            || string.IsNullOrWhiteSpace(rule.Name)
            || string.IsNullOrWhiteSpace(rule.ContainsText)
            || string.IsNullOrWhiteSpace(rule.Message)))
        {
            throw new ValidationException("Each review rule must define code, name, containsText, and message");
        }
    }

    private static ReviewRuleSetDto ToDto(ReviewRuleSetEntity entity)
    {
        return new ReviewRuleSetDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            Level = entity.Level,
            RepositoryId = entity.RepositoryId,
            IsEnabled = entity.IsEnabled,
            Version = entity.Version,
            Rules = JsonHelper.Deserialize<List<ReviewRuleDto>>(entity.RulesJson) ?? [],
            CreatedAtUtc = entity.CreatedAtUtc,
            UpdatedAtUtc = entity.UpdatedAtUtc,
        };
    }
}