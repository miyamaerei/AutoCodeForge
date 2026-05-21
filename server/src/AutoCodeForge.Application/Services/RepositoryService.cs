using AutoCodeForge.Core.DTOs.Repository;
using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Exceptions;
using AutoCodeForge.Core.Interfaces;
using AutoCodeForge.Infrastructure.Git;
using AutoCodeForge.Infrastructure.Repositories;
using AutoCodeForge.Infrastructure.Services;

namespace AutoCodeForge.Application.Services;

/// <summary>
/// Provides repository CRUD operations and Git provider management.
/// </summary>
public class RepositoryService
{
    private readonly RepositoryRepository _repository;
    private readonly GitProviderFactory _providerFactory;
    private readonly DataProtectionService _dataProtectionService;

    /// <summary>
    /// Initializes a new instance of the <see cref="RepositoryService"/> class.
    /// </summary>
    /// <param name="repository">The repository repository.</param>
    /// <param name="providerFactory">The Git provider factory.</param>
    /// <param name="dataProtectionService">The data protection service.</param>
    public RepositoryService(
        RepositoryRepository repository,
        GitProviderFactory providerFactory,
        DataProtectionService dataProtectionService)
    {
        _repository = repository;
        _providerFactory = providerFactory;
        _dataProtectionService = dataProtectionService;
    }

    /// <summary>
    /// Creates a new repository and verifies credentials with Git provider.
    /// </summary>
    /// <param name="request">The create request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created repository DTO.</returns>
    public async Task<RepositoryDto> CreateAsync(CreateRepositoryRequest request, CancellationToken cancellationToken = default)
    {
        // Validate URL format
        if (!Uri.TryCreate(request.Url, UriKind.Absolute, out _))
        {
            throw new ValidationException("Invalid repository URL format");
        }

        // Check if repository already exists
        var existing = await _repository.GetByUrlAsync(request.Url, cancellationToken);
        if (existing != null)
        {
            throw new ValidationException("Repository URL already registered");
        }

        // Verify credentials with Git provider
        var provider = _providerFactory.CreateProvider(request.Provider);
        if (provider == null)
        {
            throw new ValidationException($"Unsupported Git provider: {request.Provider}");
        }

        var canConnect = await provider.VerifyCredentialsAsync(request.Url, request.Token, cancellationToken);
        if (!canConnect)
        {
            throw new ValidationException("Failed to verify credentials with Git provider");
        }

        // Create entity
        var entity = new RepositoryEntity
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Url = request.Url.Trim(),
            Provider = request.Provider,
            AuthType = request.AuthType,
            MergeStrategy = request.MergeStrategy,
            EncryptedToken = _dataProtectionService.Encrypt(request.Token),
            Branch = request.Branch?.Trim(),
        };

        var created = await _repository.CreateAsync(entity, cancellationToken);
        return ToDto(created);
    }

    /// <summary>
    /// Gets a repository by identifier.
    /// </summary>
    /// <param name="id">The repository identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The repository DTO.</returns>
    public async Task<RepositoryDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken: cancellationToken);
        if (entity == null)
        {
            throw new ValidationException("Repository not found");
        }

        return ToDto(entity);
    }

    /// <summary>
    /// Lists repositories for the current user with pagination.
    /// </summary>
    /// <param name="page">The page number.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The paged result of repositories.</returns>
    public async Task<AutoCodeForge.Core.Models.PagedResult<RepositoryDto>> GetPagedAsync(
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _repository.GetPagedAsync(page, pageSize, cancellationToken: cancellationToken);
        return new AutoCodeForge.Core.Models.PagedResult<RepositoryDto>
        {
            Items = result.Items.Select(ToDto).ToList(),
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize,
        };
    }

    /// <summary>
    /// Updates a repository.
    /// </summary>
    /// <param name="id">The repository identifier.</param>
    /// <param name="request">The update request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated repository DTO.</returns>
    public async Task<RepositoryDto> UpdateAsync(
        Guid id,
        UpdateRepositoryRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken: cancellationToken);
        if (entity == null)
        {
            throw new ValidationException("Repository not found");
        }

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            entity.Name = request.Name.Trim();
        }

        if (request.MergeStrategy.HasValue)
        {
            entity.MergeStrategy = request.MergeStrategy.Value;
        }

        if (request.Branch != null)
        {
            entity.Branch = request.Branch.Trim();
        }

        await _repository.UpdateAsync(entity, cancellationToken);
        return ToDto(entity);
    }

    /// <summary>
    /// Deletes a repository.
    /// </summary>
    /// <param name="id">The repository identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing asynchronous execution.</returns>
    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _repository.SoftDeleteAsync(id, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Lists branches for a repository.
    /// </summary>
    /// <param name="id">The repository identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of branch DTOs.</returns>
    public async Task<IEnumerable<GitBranchDto>> GetBranchesAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken: cancellationToken);
        if (entity == null)
        {
            throw new ValidationException("Repository not found");
        }

        var provider = _providerFactory.CreateProvider(entity.Provider);
        if (provider == null)
        {
            throw new ValidationException($"Unsupported Git provider: {entity.Provider}");
        }

        var token = _dataProtectionService.Decrypt(entity.EncryptedToken);
        return await provider.ListBranchesAsync(entity.Url, token, cancellationToken);
    }

    /// <summary>
    /// Gets commits from a repository.
    /// </summary>
    /// <param name="id">The repository identifier.</param>
    /// <param name="branch">The branch name.</param>
    /// <param name="limit">Maximum commits to retrieve.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of commit DTOs.</returns>
    public async Task<IEnumerable<GitCommitDto>> GetCommitsAsync(
        Guid id,
        string branch = "main",
        int limit = 10,
        CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken: cancellationToken);
        if (entity == null)
        {
            throw new ValidationException("Repository not found");
        }

        var provider = _providerFactory.CreateProvider(entity.Provider);
        if (provider == null)
        {
            throw new ValidationException($"Unsupported Git provider: {entity.Provider}");
        }

        var token = _dataProtectionService.Decrypt(entity.EncryptedToken);
        return await provider.GetCommitsAsync(entity.Url, token, branch, limit, cancellationToken);
    }

    /// <summary>
    /// Creates a pull/merge request.
    /// </summary>
    /// <param name="id">The repository identifier.</param>
    /// <param name="request">The pull request request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created pull request DTO.</returns>
    public async Task<GitPullRequestDto> CreatePullRequestAsync(
        Guid id,
        CreateGitPullRequestRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken: cancellationToken);
        if (entity == null)
        {
            throw new ValidationException("Repository not found");
        }

        var provider = _providerFactory.CreateProvider(entity.Provider);
        if (provider == null)
        {
            throw new ValidationException($"Unsupported Git provider: {entity.Provider}");
        }

        var token = _dataProtectionService.Decrypt(entity.EncryptedToken);
        return await provider.CreatePullRequestAsync(entity.Url, token, request, cancellationToken);
    }

    /// <summary>
    /// Lists pull/merge requests for a repository.
    /// </summary>
    /// <param name="id">The repository identifier.</param>
    /// <param name="state">Filter by state (open, closed).</param>
    /// <param name="limit">Maximum PRs to retrieve.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of pull request DTOs.</returns>
    public async Task<IEnumerable<GitPullRequestDto>> ListPullRequestsAsync(
        Guid id,
        string state = "open",
        int limit = 20,
        CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken: cancellationToken);
        if (entity == null)
        {
            throw new ValidationException("Repository not found");
        }

        var provider = _providerFactory.CreateProvider(entity.Provider);
        if (provider == null)
        {
            throw new ValidationException($"Unsupported Git provider: {entity.Provider}");
        }

        var token = _dataProtectionService.Decrypt(entity.EncryptedToken);
        return await provider.ListPullRequestsAsync(entity.Url, token, state, limit, cancellationToken);
    }

    /// <summary>
    /// Pushes one branch from local path to remote repository.
    /// </summary>
    /// <param name="id">The repository identifier.</param>
    /// <param name="localPath">The local repository path.</param>
    /// <param name="branch">The branch name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><see langword="true"/> when push succeeds; otherwise <see langword="false"/>.</returns>
    public async Task<bool> PushAsync(
        Guid id,
        string localPath,
        string branch = "main",
        CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken: cancellationToken);
        if (entity == null)
        {
            throw new ValidationException("Repository not found");
        }

        var provider = _providerFactory.CreateProvider(entity.Provider);
        if (provider == null)
        {
            throw new ValidationException($"Unsupported Git provider: {entity.Provider}");
        }

        var token = _dataProtectionService.Decrypt(entity.EncryptedToken);
        return await provider.PushAsync(entity.Url, token, localPath, branch, cancellationToken);
    }

    private static RepositoryDto ToDto(RepositoryEntity entity)
    {
        return new RepositoryDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Url = entity.Url,
            Provider = entity.Provider,
            AuthType = entity.AuthType,
            MergeStrategy = entity.MergeStrategy,
            DefaultReviewRuleSetId = entity.DefaultReviewRuleSetId,
            Branch = entity.Branch,
            CreatedAtUtc = entity.CreatedAtUtc,
            UpdatedAtUtc = entity.UpdatedAtUtc,
        };
    }
}
