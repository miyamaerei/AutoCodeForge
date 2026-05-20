using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using AutoCodeForge.Core.DTOs.Repository;
using AutoCodeForge.Core.Interfaces;

namespace AutoCodeForge.Infrastructure.Git;

/// <summary>
/// GitHub provider implementation.
/// </summary>
public class GitHubProvider : IGitProvider
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "https://api.github.com";
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(10);

    /// <summary>
    /// Initializes a new instance of the <see cref="GitHubProvider"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client.</param>
    public GitHubProvider(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<bool> VerifyCredentialsAsync(string repositoryUrl, string token, CancellationToken cancellationToken = default)
    {
        try
        {
            var (owner, repo) = ExtractOwnerRepo(repositoryUrl);
            if (string.IsNullOrWhiteSpace(owner) || string.IsNullOrWhiteSpace(repo))
            {
                return false;
            }

            using var cts = new CancellationTokenSource(DefaultTimeout);
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, cancellationToken);
            var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}/repos/{owner}/{repo}");
            AddAuthHeader(request, token);

            var response = await _httpClient.SendAsync(request, linkedCts.Token);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<IEnumerable<GitBranchDto>> ListBranchesAsync(string repositoryUrl, string token, CancellationToken cancellationToken = default)
    {
        var (owner, repo) = ExtractOwnerRepo(repositoryUrl);
        if (string.IsNullOrWhiteSpace(owner) || string.IsNullOrWhiteSpace(repo))
        {
            return Enumerable.Empty<GitBranchDto>();
        }

        try
        {
            using var cts = new CancellationTokenSource(DefaultTimeout);
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, cancellationToken);
            var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}/repos/{owner}/{repo}/branches");
            AddAuthHeader(request, token);

            var response = await _httpClient.SendAsync(request, linkedCts.Token);
            if (!response.IsSuccessStatusCode)
            {
                return Enumerable.Empty<GitBranchDto>();
            }

            var content = await response.Content.ReadAsStringAsync(linkedCts.Token);
            using var doc = JsonDocument.Parse(content);
            var branches = new List<GitBranchDto>();

            if (doc.RootElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var branch in doc.RootElement.EnumerateArray())
                {
                    var dto = new GitBranchDto
                    {
                        Name = branch.GetProperty("name").GetString() ?? string.Empty,
                        CommitSha = branch.GetProperty("commit").GetProperty("sha").GetString() ?? string.Empty,
                        IsDefault = branch.TryGetProperty("protected", out var isProtected) && isProtected.GetBoolean(),
                    };
                    branches.Add(dto);
                }
            }

            return branches;
        }
        catch
        {
            return Enumerable.Empty<GitBranchDto>();
        }
    }

    public async Task<IEnumerable<GitCommitDto>> GetCommitsAsync(
        string repositoryUrl,
        string token,
        string branch = "main",
        int limit = 10,
        CancellationToken cancellationToken = default)
    {
        var (owner, repo) = ExtractOwnerRepo(repositoryUrl);
        if (string.IsNullOrWhiteSpace(owner) || string.IsNullOrWhiteSpace(repo))
        {
            return Enumerable.Empty<GitCommitDto>();
        }

        try
        {
            using var cts = new CancellationTokenSource(DefaultTimeout);
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, cancellationToken);
            var url = $"{BaseUrl}/repos/{owner}/{repo}/commits?sha={branch}&per_page={Math.Min(limit, 100)}";
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            AddAuthHeader(request, token);

            var response = await _httpClient.SendAsync(request, linkedCts.Token);
            if (!response.IsSuccessStatusCode)
            {
                return Enumerable.Empty<GitCommitDto>();
            }

            var content = await response.Content.ReadAsStringAsync(linkedCts.Token);
            using var doc = JsonDocument.Parse(content);
            var commits = new List<GitCommitDto>();

            if (doc.RootElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var commit in doc.RootElement.EnumerateArray())
                {
                    var message = commit.GetProperty("commit").GetProperty("message").GetString() ?? string.Empty;
                    var author = commit.GetProperty("commit").GetProperty("author").GetProperty("name").GetString() ?? string.Empty;
                    var dateStr = commit.GetProperty("commit").GetProperty("author").GetProperty("date").GetString();

                    var dto = new GitCommitDto
                    {
                        Sha = commit.GetProperty("sha").GetString() ?? string.Empty,
                        Message = message,
                        Author = author,
                        CreatedAtUtc = DateTime.TryParse(dateStr, out var parsedDate) ? parsedDate.ToUniversalTime() : DateTime.UtcNow,
                    };
                    commits.Add(dto);
                }
            }

            return commits;
        }
        catch
        {
            return Enumerable.Empty<GitCommitDto>();
        }
    }

    public async Task<GitPullRequestDto> CreatePullRequestAsync(
        string repositoryUrl,
        string token,
        CreateGitPullRequestRequest request,
        CancellationToken cancellationToken = default)
    {
        var (owner, repo) = ExtractOwnerRepo(repositoryUrl);
        if (string.IsNullOrWhiteSpace(owner) || string.IsNullOrWhiteSpace(repo))
        {
            return new GitPullRequestDto();
        }

        try
        {
            using var cts = new CancellationTokenSource(DefaultTimeout);
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, cancellationToken);
            var payload = new
            {
                title = request.Title,
                body = request.Description,
                head = request.SourceBranch,
                @base = request.TargetBranch,
            };

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/repos/{owner}/{repo}/pulls")
            {
                Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json"),
            };
            AddAuthHeader(httpRequest, token);

            var response = await _httpClient.SendAsync(httpRequest, linkedCts.Token);
            if (!response.IsSuccessStatusCode)
            {
                return new GitPullRequestDto();
            }

            var content = await response.Content.ReadAsStringAsync(linkedCts.Token);
            using var doc = JsonDocument.Parse(content);
            var pr = doc.RootElement;

            return new GitPullRequestDto
            {
                Id = pr.GetProperty("id").GetString() ?? string.Empty,
                Number = pr.GetProperty("number").GetInt32(),
                Title = pr.GetProperty("title").GetString() ?? string.Empty,
                Description = pr.GetProperty("body").GetString(),
                SourceBranch = pr.GetProperty("head").GetProperty("ref").GetString() ?? string.Empty,
                TargetBranch = pr.GetProperty("base").GetProperty("ref").GetString() ?? string.Empty,
                State = pr.GetProperty("state").GetString() ?? string.Empty,
                Author = pr.GetProperty("user").GetProperty("login").GetString() ?? string.Empty,
                Url = pr.GetProperty("html_url").GetString() ?? string.Empty,
                CreatedAtUtc = DateTime.Parse(pr.GetProperty("created_at").GetString() ?? DateTime.UtcNow.ToString()).ToUniversalTime(),
                UpdatedAtUtc = DateTime.Parse(pr.GetProperty("updated_at").GetString() ?? DateTime.UtcNow.ToString()).ToUniversalTime(),
            };
        }
        catch
        {
            return new GitPullRequestDto();
        }
    }

    public async Task<IEnumerable<GitPullRequestDto>> ListPullRequestsAsync(
        string repositoryUrl,
        string token,
        string state = "open",
        int limit = 20,
        CancellationToken cancellationToken = default)
    {
        var (owner, repo) = ExtractOwnerRepo(repositoryUrl);
        if (string.IsNullOrWhiteSpace(owner) || string.IsNullOrWhiteSpace(repo))
        {
            return Enumerable.Empty<GitPullRequestDto>();
        }

        try
        {
            using var cts = new CancellationTokenSource(DefaultTimeout);
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, cancellationToken);
            var url = $"{BaseUrl}/repos/{owner}/{repo}/pulls?state={state}&per_page={Math.Min(limit, 100)}";
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            AddAuthHeader(request, token);

            var response = await _httpClient.SendAsync(request, linkedCts.Token);
            if (!response.IsSuccessStatusCode)
            {
                return Enumerable.Empty<GitPullRequestDto>();
            }

            var content = await response.Content.ReadAsStringAsync(linkedCts.Token);
            using var doc = JsonDocument.Parse(content);
            var prs = new List<GitPullRequestDto>();

            if (doc.RootElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var pr in doc.RootElement.EnumerateArray())
                {
                    var dto = new GitPullRequestDto
                    {
                        Id = pr.GetProperty("id").GetString() ?? string.Empty,
                        Number = pr.GetProperty("number").GetInt32(),
                        Title = pr.GetProperty("title").GetString() ?? string.Empty,
                        Description = pr.GetProperty("body").GetString(),
                        SourceBranch = pr.GetProperty("head").GetProperty("ref").GetString() ?? string.Empty,
                        TargetBranch = pr.GetProperty("base").GetProperty("ref").GetString() ?? string.Empty,
                        State = pr.GetProperty("state").GetString() ?? string.Empty,
                        Author = pr.GetProperty("user").GetProperty("login").GetString() ?? string.Empty,
                        Url = pr.GetProperty("html_url").GetString() ?? string.Empty,
                        CreatedAtUtc = DateTime.Parse(pr.GetProperty("created_at").GetString() ?? DateTime.UtcNow.ToString()).ToUniversalTime(),
                        UpdatedAtUtc = DateTime.Parse(pr.GetProperty("updated_at").GetString() ?? DateTime.UtcNow.ToString()).ToUniversalTime(),
                    };
                    prs.Add(dto);
                }
            }

            return prs;
        }
        catch
        {
            return Enumerable.Empty<GitPullRequestDto>();
        }
    }

    public Task<bool> CloneAsync(string repositoryUrl, string token, string targetPath, CancellationToken cancellationToken = default)
    {
        // Simulated implementation - real implementation would use git CLI or LibGit2Sharp
        return Task.FromResult(true);
    }

    public Task<bool> FetchAsync(string repositoryUrl, string token, string localPath, CancellationToken cancellationToken = default)
    {
        // Simulated implementation
        return Task.FromResult(true);
    }

    public Task<bool> PushAsync(string repositoryUrl, string token, string localPath, string branch = "main", CancellationToken cancellationToken = default)
    {
        // Simulated implementation
        return Task.FromResult(true);
    }

    private static (string owner, string repo) ExtractOwnerRepo(string url)
    {
        try
        {
            // Parse URLs like: https://github.com/owner/repo or https://github.com/owner/repo.git
            var uri = new Uri(url);
            var segments = uri.AbsolutePath.Trim('/').Split('/');
            if (segments.Length >= 2)
            {
                var repo = segments[1].Replace(".git", string.Empty);
                return (segments[0], repo);
            }

            return (string.Empty, string.Empty);
        }
        catch
        {
            return (string.Empty, string.Empty);
        }
    }

    private static void AddAuthHeader(HttpRequestMessage request, string token)
    {
        request.Headers.Authorization = new AuthenticationHeaderValue("token", token);
        request.Headers.UserAgent.ParseAdd("AutoCodeForge");
    }
}
