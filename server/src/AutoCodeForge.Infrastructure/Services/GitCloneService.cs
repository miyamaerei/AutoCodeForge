using AutoCodeForge.Infrastructure.Git;

namespace AutoCodeForge.Infrastructure.Services;

/// <summary>
/// Provides repository clone orchestration service.
/// </summary>
public class GitCloneService
{
    private readonly LibGit2SharpProvider _gitProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="GitCloneService"/> class.
    /// </summary>
    /// <param name="gitProvider">LibGit2Sharp provider.</param>
    public GitCloneService(LibGit2SharpProvider gitProvider)
    {
        _gitProvider = gitProvider;
    }

    /// <summary>
    /// Clones or pulls one repository and returns synced commit sha.
    /// </summary>
    /// <param name="repositoryUrl">Repository URL.</param>
    /// <param name="token">Access token.</param>
    /// <param name="branch">Branch to sync.</param>
    /// <param name="targetPath">Local path.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Synced commit sha.</returns>
    public Task<string?> CloneOrPullAsync(
        string repositoryUrl,
        string token,
        string branch,
        string targetPath,
        CancellationToken cancellationToken = default)
    {
        return _gitProvider.CloneOrPullAsync(repositoryUrl, token, branch, targetPath, cancellationToken);
    }
}
