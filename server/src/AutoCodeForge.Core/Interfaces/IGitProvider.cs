using AutoCodeForge.Core.DTOs.Repository;

namespace AutoCodeForge.Core.Interfaces;

/// <summary>
/// Defines Git provider operations for multiple platforms.
/// </summary>
public interface IGitProvider
{
    /// <summary>
    /// Verifies that the provider can connect with given credentials.
    /// </summary>
    /// <param name="repositoryUrl">The repository URL.</param>
    /// <param name="token">The authentication token.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if connection succeeds; otherwise false.</returns>
    Task<bool> VerifyCredentialsAsync(string repositoryUrl, string token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists all branches in the repository.
    /// </summary>
    /// <param name="repositoryUrl">The repository URL.</param>
    /// <param name="token">The authentication token.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of branch DTOs.</returns>
    Task<IEnumerable<GitBranchDto>> ListBranchesAsync(string repositoryUrl, string token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets recent commits from a branch.
    /// </summary>
    /// <param name="repositoryUrl">The repository URL.</param>
    /// <param name="token">The authentication token.</param>
    /// <param name="branch">The branch name (default: main/master).</param>
    /// <param name="limit">Maximum number of commits to retrieve.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of commit DTOs.</returns>
    Task<IEnumerable<GitCommitDto>> GetCommitsAsync(
        string repositoryUrl,
        string token,
        string branch = "main",
        int limit = 10,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a pull/merge request.
    /// </summary>
    /// <param name="repositoryUrl">The repository URL.</param>
    /// <param name="token">The authentication token.</param>
    /// <param name="request">The pull request details.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created pull request DTO.</returns>
    Task<GitPullRequestDto> CreatePullRequestAsync(
        string repositoryUrl,
        string token,
        CreateGitPullRequestRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists pull/merge requests.
    /// </summary>
    /// <param name="repositoryUrl">The repository URL.</param>
    /// <param name="token">The authentication token.</param>
    /// <param name="state">Filter by state (open, closed, all).</param>
    /// <param name="limit">Maximum number of PRs to retrieve.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of pull request DTOs.</returns>
    Task<IEnumerable<GitPullRequestDto>> ListPullRequestsAsync(
        string repositoryUrl,
        string token,
        string state = "open",
        int limit = 20,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Clones a repository (local operation, may be simulated).
    /// </summary>
    /// <param name="repositoryUrl">The repository URL.</param>
    /// <param name="token">The authentication token.</param>
    /// <param name="targetPath">The local path to clone into.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if clone succeeds; otherwise false.</returns>
    Task<bool> CloneAsync(string repositoryUrl, string token, string targetPath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Fetches updates from remote repository (local operation).
    /// </summary>
    /// <param name="repositoryUrl">The repository URL.</param>
    /// <param name="token">The authentication token.</param>
    /// <param name="localPath">The local repository path.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if fetch succeeds; otherwise false.</returns>
    Task<bool> FetchAsync(string repositoryUrl, string token, string localPath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Pushes commits to remote repository (local operation).
    /// </summary>
    /// <param name="repositoryUrl">The repository URL.</param>
    /// <param name="token">The authentication token.</param>
    /// <param name="localPath">The local repository path.</param>
    /// <param name="branch">The branch to push.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if push succeeds; otherwise false.</returns>
    Task<bool> PushAsync(
        string repositoryUrl,
        string token,
        string localPath,
        string branch = "main",
        CancellationToken cancellationToken = default);
}
