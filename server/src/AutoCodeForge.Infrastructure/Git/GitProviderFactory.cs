using AutoCodeForge.Core.DTOs.Repository;
using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Interfaces;

namespace AutoCodeForge.Infrastructure.Git;

/// <summary>
/// Factory for creating Git provider instances based on platform.
/// </summary>
public class GitProviderFactory
{
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="GitProviderFactory"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client for API calls.</param>
    public GitProviderFactory(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Creates a Git provider instance based on the specified provider type.
    /// </summary>
    /// <param name="provider">The provider type.</param>
    /// <returns>An IGitProvider instance, or null if provider is unsupported.</returns>
    public IGitProvider? CreateProvider(GitProvider provider)
    {
        return provider switch
        {
            GitProvider.GitHub => new GitHubProvider(_httpClient),
            GitProvider.GitLab => new GitLabProvider(_httpClient),
            GitProvider.AzureDevOps => new AzureDevOpsProvider(_httpClient),
            _ => null,
        };
    }
}
