using AutoCodeForge.Core.DTOs.Wiki;
using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Exceptions;
using AutoCodeForge.Core.Models;
using AutoCodeForge.Infrastructure.Repositories;

namespace AutoCodeForge.Application.Services;

/// <summary>
/// Provides CRUD and search operations for wiki pages.
/// </summary>
public class WikiService
{
    private readonly WikiPageRepository _wikiPageRepository;
    private readonly RepositoryRepository _repositoryRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="WikiService"/> class.
    /// </summary>
    /// <param name="wikiPageRepository">The wiki repository.</param>
    /// <param name="repositoryRepository">The repository repository.</param>
    public WikiService(
        WikiPageRepository wikiPageRepository,
        RepositoryRepository repositoryRepository)
    {
        _wikiPageRepository = wikiPageRepository;
        _repositoryRepository = repositoryRepository;
    }

    /// <summary>
    /// Creates one wiki page.
    /// </summary>
    /// <param name="request">The creation request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created wiki page response.</returns>
    public async Task<WikiPageResponse> CreateAsync(CreateWikiPageRequest request, CancellationToken cancellationToken = default)
    {
        var slug = NormalizeSlug(request.Slug);
        await EnsureSlugUniqueAsync(slug, null, cancellationToken);
        await EnsureRepositoryExistsAsync(request.RepositoryId, cancellationToken);

        var entity = new WikiPageEntity
        {
            Id = Guid.NewGuid(),
            Title = request.Title.Trim(),
            Slug = slug,
            Content = request.Content.Trim(),
            RepositoryId = request.RepositoryId,
        };

        var created = await _wikiPageRepository.CreateAsync(entity, cancellationToken);
        return ToResponse(created);
    }

    /// <summary>
    /// Gets one wiki page by identifier.
    /// </summary>
    /// <param name="wikiPageId">The wiki page identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The wiki page response.</returns>
    public async Task<WikiPageResponse> GetByIdAsync(Guid wikiPageId, CancellationToken cancellationToken = default)
    {
        var entity = await GetOrThrowAsync(wikiPageId, cancellationToken);
        return ToResponse(entity);
    }

    /// <summary>
    /// Gets wiki pages with pagination and optional filters.
    /// </summary>
    /// <param name="keyword">Optional search keyword.</param>
    /// <param name="repositoryId">Optional repository filter.</param>
    /// <param name="page">Requested page number.</param>
    /// <param name="pageSize">Requested page size.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Paged wiki page response payload.</returns>
    public async Task<PagedResult<WikiPageResponse>> GetPagedAsync(
        string? keyword,
        Guid? repositoryId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        await EnsureRepositoryExistsAsync(repositoryId, cancellationToken);
        var paged = await _wikiPageRepository.SearchPagedAsync(keyword, repositoryId, page, pageSize, cancellationToken);

        return new PagedResult<WikiPageResponse>(
            paged.Items.Select(ToResponse).ToList(),
            paged.TotalCount,
            paged.Page,
            paged.PageSize);
    }

    /// <summary>
    /// Updates mutable fields of one wiki page.
    /// </summary>
    /// <param name="wikiPageId">The wiki page identifier.</param>
    /// <param name="request">The update request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated wiki page response.</returns>
    public async Task<WikiPageResponse> UpdateAsync(
        Guid wikiPageId,
        UpdateWikiPageRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await GetOrThrowAsync(wikiPageId, cancellationToken);

        if (!string.IsNullOrWhiteSpace(request.Title))
        {
            entity.Title = request.Title.Trim();
        }

        if (request.Slug != null)
        {
            var slug = NormalizeSlug(request.Slug);
            await EnsureSlugUniqueAsync(slug, wikiPageId, cancellationToken);
            entity.Slug = slug;
        }

        if (request.Content != null)
        {
            entity.Content = request.Content.Trim();
        }

        if (request.RepositoryId.HasValue)
        {
            var repositoryId = request.RepositoryId == Guid.Empty ? null : request.RepositoryId;
            await EnsureRepositoryExistsAsync(repositoryId, cancellationToken);
            entity.RepositoryId = repositoryId;
        }

        await _wikiPageRepository.UpdateAsync(entity, cancellationToken);
        return ToResponse(entity);
    }

    /// <summary>
    /// Soft-deletes one wiki page.
    /// </summary>
    /// <param name="wikiPageId">The wiki page identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing asynchronous execution.</returns>
    public async Task DeleteAsync(Guid wikiPageId, CancellationToken cancellationToken = default)
    {
        await _wikiPageRepository.SoftDeleteAsync(wikiPageId, false, cancellationToken);
    }

    private async Task<WikiPageEntity> GetOrThrowAsync(Guid wikiPageId, CancellationToken cancellationToken)
    {
        return await _wikiPageRepository.GetByIdAsync(wikiPageId, false, cancellationToken)
            ?? throw new NotFoundException($"Wiki page {wikiPageId} not found");
    }

    private async Task EnsureSlugUniqueAsync(string slug, Guid? excludeId, CancellationToken cancellationToken)
    {
        var existing = await _wikiPageRepository.GetBySlugAsync(slug, excludeId, cancellationToken);
        if (existing != null)
        {
            throw new ValidationException("Wiki slug already exists");
        }
    }

    private async Task EnsureRepositoryExistsAsync(Guid? repositoryId, CancellationToken cancellationToken)
    {
        if (!repositoryId.HasValue)
        {
            return;
        }

        _ = await _repositoryRepository.GetByIdAsync(repositoryId.Value, cancellationToken: cancellationToken)
            ?? throw new NotFoundException($"Repository {repositoryId.Value} not found");
    }

    private static string NormalizeSlug(string input)
    {
        var normalized = input.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new ValidationException("Slug is required");
        }

        return normalized;
    }

    private static WikiPageResponse ToResponse(WikiPageEntity entity) =>
        new()
        {
            Id = entity.Id,
            Title = entity.Title,
            Slug = entity.Slug,
            Content = entity.Content,
            RepositoryId = entity.RepositoryId,
            CreatedAtUtc = entity.CreatedAtUtc,
            UpdatedAtUtc = entity.UpdatedAtUtc,
        };
}