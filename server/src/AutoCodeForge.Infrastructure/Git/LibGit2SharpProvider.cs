using LibGit2Sharp;

namespace AutoCodeForge.Infrastructure.Git;

/// <summary>
/// Provides clone and pull operations using LibGit2Sharp.
/// </summary>
public class LibGit2SharpProvider
{
    /// <summary>
    /// Clones or pulls a repository to local path and returns current HEAD sha.
    /// </summary>
    /// <param name="repositoryUrl">Repository URL.</param>
    /// <param name="token">Access token.</param>
    /// <param name="branch">Target branch.</param>
    /// <param name="targetPath">Local repository path.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Latest commit sha after sync.</returns>
    public Task<string?> CloneOrPullAsync(
        string repositoryUrl,
        string token,
        string branch,
        string targetPath,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Directory.CreateDirectory(targetPath);

        var hasRepo = Repository.IsValid(targetPath);
        if (!hasRepo)
        {
            var cloneOptions = new CloneOptions
            {
                BranchName = branch,
                Checkout = true,
            };
            cloneOptions.FetchOptions.CredentialsProvider = (_, _, _) => new UsernamePasswordCredentials
            {
                Username = "x-access-token",
                Password = token,
            };
            cloneOptions.FetchOptions.OnTransferProgress = _ => !cancellationToken.IsCancellationRequested;

            Repository.Clone(repositoryUrl, targetPath, cloneOptions);
        }

        using var repo = new Repository(targetPath);
        CheckoutBranchIfNeeded(repo, branch, token, cancellationToken);

        var signature = new Signature("AutoCodeForge", "noreply@autocodeforge.local", DateTimeOffset.UtcNow);
        var pullOptions = new PullOptions
        {
            FetchOptions = new FetchOptions
            {
                CredentialsProvider = (_, _, _) => new UsernamePasswordCredentials
                {
                    Username = "x-access-token",
                    Password = token,
                },
                OnTransferProgress = _ => !cancellationToken.IsCancellationRequested,
            },
        };

        Commands.Pull(repo, signature, pullOptions);
        return Task.FromResult(repo.Head.Tip?.Sha);
    }

    private static void CheckoutBranchIfNeeded(
        Repository repo,
        string branch,
        string token,
        CancellationToken cancellationToken)
    {
        var desired = string.IsNullOrWhiteSpace(branch) ? "main" : branch.Trim();
        var local = repo.Branches[desired];
        if (local is null)
        {
            var remoteBranch = repo.Branches[$"origin/{desired}"];
            if (remoteBranch is not null)
            {
                local = repo.CreateBranch(desired, remoteBranch.Tip);
                repo.Branches.Update(local, options => options.TrackedBranch = remoteBranch.CanonicalName);
            }
            else
            {
                Commands.Fetch(repo, "origin", ["refs/heads/*:refs/remotes/origin/*"], new FetchOptions
                {
                    CredentialsProvider = (_, _, _) => new UsernamePasswordCredentials
                    {
                        Username = "x-access-token",
                        Password = token,
                    },
                    OnTransferProgress = _ => !cancellationToken.IsCancellationRequested,
                }, null);

                remoteBranch = repo.Branches[$"origin/{desired}"];
                if (remoteBranch is not null)
                {
                    local = repo.CreateBranch(desired, remoteBranch.Tip);
                    repo.Branches.Update(local, options => options.TrackedBranch = remoteBranch.CanonicalName);
                }
            }
        }

        if (local is not null)
        {
            Commands.Checkout(repo, local);
        }
    }
}
