using LibGit2Sharp;
using LibGit2Sharp.Handlers;
using AutoCodeForge.Core.Models;

namespace AutoCodeForge.Infrastructure.Git;

/// <summary>
/// Provides clone and pull operations using LibGit2Sharp with configurable options.
/// </summary>
public class LibGit2SharpProvider
{
    private readonly GitOptions _gitOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="LibGit2SharpProvider"/> class.
    /// </summary>
    /// <param name="gitOptions">The Git configuration options.</param>
    public LibGit2SharpProvider(GitOptions gitOptions)
    {
        _gitOptions = gitOptions;
    }

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

        // 路径已经在 SandboxPathResolver 阶段优化过，直接使用
        var resolvedPath = targetPath;
        Directory.CreateDirectory(resolvedPath);

        var providerType = DetermineProviderType(repositoryUrl);
        var username = GetUsernameForProvider(providerType);
        var targetBranch = string.IsNullOrWhiteSpace(branch) ? "main" : branch.Trim();

        var hasRepo = Repository.IsValid(resolvedPath);
        if (!hasRepo)
        {
            var cloneOptions = new CloneOptions
            {
                BranchName = targetBranch,
                Checkout = true, // 确保检出文件
            };

            cloneOptions.FetchOptions.CredentialsProvider = CreateCredentialsProvider(username, token);
            cloneOptions.FetchOptions.OnTransferProgress = _ => !cancellationToken.IsCancellationRequested;

            Repository.Clone(repositoryUrl, resolvedPath, cloneOptions);
        }
        else
        {
            // 如果仓库已存在，先拉取最新代码
            using var repo = new Repository(resolvedPath);
            
            // 获取远程分支
            var fetchOptions = new FetchOptions
            {
                CredentialsProvider = CreateCredentialsProvider(username, token),
                OnTransferProgress = _ => !cancellationToken.IsCancellationRequested,
            };
            
            Commands.Fetch(repo, "origin", ["refs/heads/*:refs/remotes/origin/*"], fetchOptions, null);
            
            // 检出目标分支并确保追踪远程分支
            CheckoutBranchIfNeeded(repo, branch, token, username, cancellationToken);
            
            // 硬重置到远程分支，确保工作目录是最新的
            var remoteBranch = repo.Branches[$"origin/{targetBranch}"];
            if (remoteBranch != null)
            {
                repo.Reset(ResetMode.Hard, remoteBranch.Tip);
            }
            
            // 检出文件到工作目录
            Commands.Checkout(repo, repo.Head, new CheckoutOptions { CheckoutModifiers = CheckoutModifiers.Force });
            
            return Task.FromResult(repo.Head.Tip?.Sha);
        }

        using var repoAfter = new Repository(resolvedPath);
        return Task.FromResult(repoAfter.Head.Tip?.Sha);
    }

    /// <summary>
    /// Determines the Git provider type based on repository URL.
    /// </summary>
    /// <param name="repositoryUrl">The repository URL.</param>
    /// <returns>The provider type identifier.</returns>
    public string DetermineProviderType(string repositoryUrl)
    {
        if (_gitOptions.AzureDevOps.DomainPatterns.Any(pattern => 
            repositoryUrl.Contains(pattern, StringComparison.OrdinalIgnoreCase)))
        {
            return "AzureDevOps";
        }

        if (repositoryUrl.Contains("github.com", StringComparison.OrdinalIgnoreCase))
        {
            return "GitHub";
        }

        if (repositoryUrl.Contains("gitlab.com", StringComparison.OrdinalIgnoreCase))
        {
            return "GitLab";
        }

        return "Generic";
    }

    /// <summary>
    /// Gets the appropriate username for authentication based on provider type.
    /// </summary>
    /// <param name="providerType">The provider type.</param>
    /// <returns>The username to use for authentication.</returns>
    public string GetUsernameForProvider(string providerType)
    {
        return providerType switch
        {
            "AzureDevOps" => _gitOptions.AzureDevOps.Username,
            "GitHub" => _gitOptions.StringHandling.GitHubUsername,
            "GitLab" => _gitOptions.StringHandling.GitLabUsername,
            _ => _gitOptions.Providers.DefaultUsername,
        };
    }

    /// <summary>
    /// URL encodes special characters in repository path segments.
    /// </summary>
    /// <param name="pathSegment">The path segment to encode.</param>
    /// <returns>The encoded path segment.</returns>
    public string EncodePathSegment(string pathSegment)
    {
        if (string.IsNullOrWhiteSpace(pathSegment))
        {
            return string.Empty;
        }

        if (!_gitOptions.AzureDevOps.EnableUrlEncoding)
        {
            return pathSegment;
        }

        if (_gitOptions.StringHandling.NormalizeWhitespace)
        {
            foreach (var specialChar in _gitOptions.StringHandling.SpecialCharacters)
            {
                pathSegment = pathSegment.Replace(specialChar, _gitOptions.StringHandling.WhitespaceReplacement);
            }
        }

        return Uri.EscapeDataString(pathSegment);
    }

    private CredentialsHandler CreateCredentialsProvider(string username, string token)
    {
        return (_, _, _) => new UsernamePasswordCredentials
        {
            Username = username,
            Password = token,
        };
    }

    private void CheckoutBranchIfNeeded(
        Repository repo,
        string branch,
        string token,
        string username,
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
                    CredentialsProvider = CreateCredentialsProvider(username, token),
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