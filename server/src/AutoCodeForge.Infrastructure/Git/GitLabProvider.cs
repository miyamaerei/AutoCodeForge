using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using AutoCodeForge.Core.DTOs.Repository;
using AutoCodeForge.Core.Interfaces;

namespace AutoCodeForge.Infrastructure.Git;

/// <summary>
/// GitLab provider implementation.
/// </summary>
public class GitLabProvider : IGitProvider
{
    private readonly HttpClient _httpClient;
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(10);

    /// <summary>
    /// Initializes a new instance of the <see cref="GitLabProvider"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client.</param>
    public GitLabProvider(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<bool> VerifyCredentialsAsync(string repositoryUrl, string token, CancellationToken cancellationToken = default)
    {
        try
        {
            var baseUrl = ExtractBaseUrl(repositoryUrl);
            var projectId = ExtractProjectId(repositoryUrl);
            if (string.IsNullOrWhiteSpace(baseUrl) || string.IsNullOrWhiteSpace(projectId))
            {
                return false;
            }

            using var cts = new CancellationTokenSource(DefaultTimeout);
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, cancellationToken);
            var encodedId = Uri.EscapeDataString(projectId);
            var request = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}/api/v4/projects/{encodedId}");
            request.Headers.Add("PRIVATE-TOKEN", token);

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
        var baseUrl = ExtractBaseUrl(repositoryUrl);
        var projectId = ExtractProjectId(repositoryUrl);
        if (string.IsNullOrWhiteSpace(baseUrl) || string.IsNullOrWhiteSpace(projectId))
        {
            return Enumerable.Empty<GitBranchDto>();
        }

        try
        {
            using var cts = new CancellationTokenSource(DefaultTimeout);
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, cancellationToken);
            var encodedId = Uri.EscapeDataString(projectId);
            var request = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}/api/v4/projects/{encodedId}/repository/branches");
            request.Headers.Add("PRIVATE-TOKEN", token);

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
                        CommitSha = branch.GetProperty("commit").GetProperty("id").GetString() ?? string.Empty,
                        IsDefault = branch.GetProperty("default").GetBoolean(),
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
        var baseUrl = ExtractBaseUrl(repositoryUrl);
        var projectId = ExtractProjectId(repositoryUrl);
        if (string.IsNullOrWhiteSpace(baseUrl) || string.IsNullOrWhiteSpace(projectId))
        {
            return Enumerable.Empty<GitCommitDto>();
        }

        try
        {
            using var cts = new CancellationTokenSource(DefaultTimeout);
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, cancellationToken);
            var encodedId = Uri.EscapeDataString(projectId);
            var url = $"{baseUrl}/api/v4/projects/{encodedId}/repository/commits?ref={branch}&per_page={Math.Min(limit, 100)}";
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("PRIVATE-TOKEN", token);

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
                    var dto = new GitCommitDto
                    {
                        Sha = commit.GetProperty("id").GetString() ?? string.Empty,
                        Message = commit.GetProperty("message").GetString() ?? string.Empty,
                        Author = commit.GetProperty("author_name").GetString() ?? string.Empty,
                        CreatedAtUtc = DateTime.Parse(commit.GetProperty("created_at").GetString() ?? DateTime.UtcNow.ToString()).ToUniversalTime(),
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
        var baseUrl = ExtractBaseUrl(repositoryUrl);
        var projectId = ExtractProjectId(repositoryUrl);
        if (string.IsNullOrWhiteSpace(baseUrl) || string.IsNullOrWhiteSpace(projectId))
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
                description = request.Description,
                source_branch = request.SourceBranch,
                target_branch = request.TargetBranch,
            };

            var encodedId = Uri.EscapeDataString(projectId);
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/api/v4/projects/{encodedId}/merge_requests")
            {
                Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json"),
            };
            httpRequest.Headers.Add("PRIVATE-TOKEN", token);

            var response = await _httpClient.SendAsync(httpRequest, linkedCts.Token);
            if (!response.IsSuccessStatusCode)
            {
                return new GitPullRequestDto();
            }

            var content = await response.Content.ReadAsStringAsync(linkedCts.Token);
            using var doc = JsonDocument.Parse(content);
            var mr = doc.RootElement;

            return new GitPullRequestDto
            {
                Id = mr.GetProperty("id").GetString() ?? string.Empty,
                Number = mr.GetProperty("iid").GetInt32(),
                Title = mr.GetProperty("title").GetString() ?? string.Empty,
                Description = mr.GetProperty("description").GetString(),
                SourceBranch = mr.GetProperty("source_branch").GetString() ?? string.Empty,
                TargetBranch = mr.GetProperty("target_branch").GetString() ?? string.Empty,
                State = mr.GetProperty("state").GetString() ?? string.Empty,
                Author = mr.GetProperty("author").GetProperty("username").GetString() ?? string.Empty,
                Url = mr.GetProperty("web_url").GetString() ?? string.Empty,
                CreatedAtUtc = DateTime.Parse(mr.GetProperty("created_at").GetString() ?? DateTime.UtcNow.ToString()).ToUniversalTime(),
                UpdatedAtUtc = DateTime.Parse(mr.GetProperty("updated_at").GetString() ?? DateTime.UtcNow.ToString()).ToUniversalTime(),
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
        string state = "opened",
        int limit = 20,
        CancellationToken cancellationToken = default)
    {
        var baseUrl = ExtractBaseUrl(repositoryUrl);
        var projectId = ExtractProjectId(repositoryUrl);
        if (string.IsNullOrWhiteSpace(baseUrl) || string.IsNullOrWhiteSpace(projectId))
        {
            return Enumerable.Empty<GitPullRequestDto>();
        }

        try
        {
            using var cts = new CancellationTokenSource(DefaultTimeout);
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, cancellationToken);
            var encodedId = Uri.EscapeDataString(projectId);
            var url = $"{baseUrl}/api/v4/projects/{encodedId}/merge_requests?state={state}&per_page={Math.Min(limit, 100)}";
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("PRIVATE-TOKEN", token);

            var response = await _httpClient.SendAsync(request, linkedCts.Token);
            if (!response.IsSuccessStatusCode)
            {
                return Enumerable.Empty<GitPullRequestDto>();
            }

            var content = await response.Content.ReadAsStringAsync(linkedCts.Token);
            using var doc = JsonDocument.Parse(content);
            var mrs = new List<GitPullRequestDto>();

            if (doc.RootElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var mr in doc.RootElement.EnumerateArray())
                {
                    var dto = new GitPullRequestDto
                    {
                        Id = mr.GetProperty("id").GetString() ?? string.Empty,
                        Number = mr.GetProperty("iid").GetInt32(),
                        Title = mr.GetProperty("title").GetString() ?? string.Empty,
                        Description = mr.GetProperty("description").GetString(),
                        SourceBranch = mr.GetProperty("source_branch").GetString() ?? string.Empty,
                        TargetBranch = mr.GetProperty("target_branch").GetString() ?? string.Empty,
                        State = mr.GetProperty("state").GetString() ?? string.Empty,
                        Author = mr.GetProperty("author").GetProperty("username").GetString() ?? string.Empty,
                        Url = mr.GetProperty("web_url").GetString() ?? string.Empty,
                        CreatedAtUtc = DateTime.Parse(mr.GetProperty("created_at").GetString() ?? DateTime.UtcNow.ToString()).ToUniversalTime(),
                        UpdatedAtUtc = DateTime.Parse(mr.GetProperty("updated_at").GetString() ?? DateTime.UtcNow.ToString()).ToUniversalTime(),
                    };
                    mrs.Add(dto);
                }
            }

            return mrs;
        }
        catch
        {
            return Enumerable.Empty<GitPullRequestDto>();
        }
    }

    public Task<bool> CloneAsync(string repositoryUrl, string token, string targetPath, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(true);
    }

    public Task<bool> FetchAsync(string repositoryUrl, string token, string localPath, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(true);
    }

    public Task<bool> PushAsync(string repositoryUrl, string token, string localPath, string branch = "main", CancellationToken cancellationToken = default)
    {
        return Task.FromResult(true);
    }

    private static string ExtractBaseUrl(string repositoryUrl)
    {
        try
        {
            var uri = new Uri(repositoryUrl);
            return $"{uri.Scheme}://{uri.Host}";
        }
        catch
        {
            return string.Empty;
        }
    }

    private static string ExtractProjectId(string repositoryUrl)
    {
        try
        {
            var uri = new Uri(repositoryUrl);
            var path = uri.AbsolutePath.Trim('/').Replace(".git", string.Empty);
            return path; // GitLab uses project path as ID
        }
        catch
        {
            return string.Empty;
        }
    }
}
