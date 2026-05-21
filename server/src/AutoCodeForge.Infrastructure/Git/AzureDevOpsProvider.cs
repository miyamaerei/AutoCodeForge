using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using AutoCodeForge.Core.DTOs.Repository;
using AutoCodeForge.Core.Interfaces;

namespace AutoCodeForge.Infrastructure.Git;

/// <summary>
/// Azure DevOps provider implementation.
/// </summary>
public class AzureDevOpsProvider : IGitProvider
{
    private readonly HttpClient _httpClient;
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(10);

    /// <summary>
    /// Initializes a new instance of the <see cref="AzureDevOpsProvider"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client.</param>
    public AzureDevOpsProvider(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<bool> VerifyCredentialsAsync(string repositoryUrl, string token, CancellationToken cancellationToken = default)
    {
        try
        {
            var (org, project, repo) = ExtractOrgProjectRepo(repositoryUrl);
            if (string.IsNullOrWhiteSpace(org) || string.IsNullOrWhiteSpace(project) || string.IsNullOrWhiteSpace(repo))
            {
                return false;
            }

            using var cts = new CancellationTokenSource(DefaultTimeout);
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, cancellationToken);
            var request = new HttpRequestMessage(HttpMethod.Get, $"https://dev.azure.com/{org}/{project}/_apis/git/repositories/{repo}?api-version=7.0");
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
        var (org, project, repo) = ExtractOrgProjectRepo(repositoryUrl);
        if (string.IsNullOrWhiteSpace(org) || string.IsNullOrWhiteSpace(project) || string.IsNullOrWhiteSpace(repo))
        {
            return Enumerable.Empty<GitBranchDto>();
        }

        try
        {
            using var cts = new CancellationTokenSource(DefaultTimeout);
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, cancellationToken);
            var url = $"https://dev.azure.com/{org}/{project}/_apis/git/repositories/{repo}/refs?api-version=7.0";
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            AddAuthHeader(request, token);

            var response = await _httpClient.SendAsync(request, linkedCts.Token);
            if (!response.IsSuccessStatusCode)
            {
                return Enumerable.Empty<GitBranchDto>();
            }

            var content = await response.Content.ReadAsStringAsync(linkedCts.Token);
            using var doc = JsonDocument.Parse(content);
            var branches = new List<GitBranchDto>();

            if (doc.RootElement.TryGetProperty("value", out var refs))
            {
                foreach (var @ref in refs.EnumerateArray())
                {
                    var name = @ref.GetProperty("name").GetString() ?? string.Empty;
                    if (name.StartsWith("refs/heads/"))
                    {
                        var dto = new GitBranchDto
                        {
                            Name = name.Replace("refs/heads/", string.Empty),
                            CommitSha = @ref.GetProperty("objectId").GetString() ?? string.Empty,
                            IsDefault = false,
                        };
                        branches.Add(dto);
                    }
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
        var (org, project, repo) = ExtractOrgProjectRepo(repositoryUrl);
        if (string.IsNullOrWhiteSpace(org) || string.IsNullOrWhiteSpace(project) || string.IsNullOrWhiteSpace(repo))
        {
            return Enumerable.Empty<GitCommitDto>();
        }

        try
        {
            using var cts = new CancellationTokenSource(DefaultTimeout);
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, cancellationToken);
            var url = $"https://dev.azure.com/{org}/{project}/_apis/git/repositories/{repo}/commits?searchCriteria.itemVersion.version={branch}&$top={Math.Min(limit, 100)}&api-version=7.0";
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

            if (doc.RootElement.TryGetProperty("value", out var commitArray))
            {
                foreach (var commit in commitArray.EnumerateArray())
                {
                    var dto = new GitCommitDto
                    {
                        Sha = commit.GetProperty("commitId").GetString() ?? string.Empty,
                        Message = commit.GetProperty("comment").GetString() ?? string.Empty,
                        Author = commit.GetProperty("author").GetProperty("name").GetString() ?? string.Empty,
                        CreatedAtUtc = DateTime.Parse(commit.GetProperty("author").GetProperty("date").GetString() ?? DateTime.UtcNow.ToString()).ToUniversalTime(),
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
        var (org, project, repo) = ExtractOrgProjectRepo(repositoryUrl);
        if (string.IsNullOrWhiteSpace(org) || string.IsNullOrWhiteSpace(project) || string.IsNullOrWhiteSpace(repo))
        {
            return new GitPullRequestDto();
        }

        try
        {
            using var cts = new CancellationTokenSource(DefaultTimeout);
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, cancellationToken);
            var payload = new
            {
                sourceRefName = $"refs/heads/{request.SourceBranch}",
                targetRefName = $"refs/heads/{request.TargetBranch}",
                title = request.Title,
                description = request.Description,
            };

            var url = $"https://dev.azure.com/{org}/{project}/_apis/git/repositories/{repo}/pullrequests?api-version=7.0";
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, url)
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
            var createdAtUtc = DateTime.Parse(pr.GetProperty("creationDate").GetString() ?? DateTime.UtcNow.ToString()).ToUniversalTime();
            var updatedAtUtc = createdAtUtc;
            if (pr.TryGetProperty("closedDate", out var closedDateElement)
                && closedDateElement.ValueKind == JsonValueKind.String
                && DateTime.TryParse(closedDateElement.GetString(), out var parsedClosedDate))
            {
                updatedAtUtc = parsedClosedDate.ToUniversalTime();
            }

            return new GitPullRequestDto
            {
                Id = pr.GetProperty("pullRequestId").ToString(),
                Number = pr.GetProperty("pullRequestId").GetInt32(),
                Title = pr.GetProperty("title").GetString() ?? string.Empty,
                Description = pr.GetProperty("description").GetString(),
                SourceBranch = pr.GetProperty("sourceRefName").GetString() ?? string.Empty,
                TargetBranch = pr.GetProperty("targetRefName").GetString() ?? string.Empty,
                State = pr.GetProperty("status").GetString() ?? string.Empty,
                Author = pr.GetProperty("createdBy").GetProperty("displayName").GetString() ?? string.Empty,
                Url = pr.GetProperty("url").GetString() ?? string.Empty,
                CreatedAtUtc = createdAtUtc,
                UpdatedAtUtc = updatedAtUtc,
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
        string state = "active",
        int limit = 20,
        CancellationToken cancellationToken = default)
    {
        var (org, project, repo) = ExtractOrgProjectRepo(repositoryUrl);
        if (string.IsNullOrWhiteSpace(org) || string.IsNullOrWhiteSpace(project) || string.IsNullOrWhiteSpace(repo))
        {
            return Enumerable.Empty<GitPullRequestDto>();
        }

        try
        {
            using var cts = new CancellationTokenSource(DefaultTimeout);
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, cancellationToken);
            var statusFilter = state switch
            {
                "closed" => "abandoned,completed",
                "merged" => "completed",
                _ => "active",
            };
            var url = $"https://dev.azure.com/{org}/{project}/_apis/git/repositories/{repo}/pullrequests?searchCriteria.status={statusFilter}&$top={Math.Min(limit, 100)}&api-version=7.0";
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

            if (doc.RootElement.TryGetProperty("value", out var prArray))
            {
                foreach (var pr in prArray.EnumerateArray())
                {
                    var createdAtUtc = DateTime.Parse(pr.GetProperty("creationDate").GetString() ?? DateTime.UtcNow.ToString()).ToUniversalTime();
                    var updatedAtUtc = createdAtUtc;
                    if (pr.TryGetProperty("closedDate", out var closedDateElement)
                        && closedDateElement.ValueKind == JsonValueKind.String
                        && DateTime.TryParse(closedDateElement.GetString(), out var parsedClosedDate))
                    {
                        updatedAtUtc = parsedClosedDate.ToUniversalTime();
                    }

                    var dto = new GitPullRequestDto
                    {
                        Id = pr.GetProperty("pullRequestId").ToString(),
                        Number = pr.GetProperty("pullRequestId").GetInt32(),
                        Title = pr.GetProperty("title").GetString() ?? string.Empty,
                        Description = pr.GetProperty("description").GetString(),
                        SourceBranch = pr.GetProperty("sourceRefName").GetString() ?? string.Empty,
                        TargetBranch = pr.GetProperty("targetRefName").GetString() ?? string.Empty,
                        State = pr.GetProperty("status").GetString() ?? string.Empty,
                        Author = pr.GetProperty("createdBy").GetProperty("displayName").GetString() ?? string.Empty,
                        Url = pr.GetProperty("url").GetString() ?? string.Empty,
                        CreatedAtUtc = createdAtUtc,
                        UpdatedAtUtc = updatedAtUtc,
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

    private static (string org, string project, string repo) ExtractOrgProjectRepo(string url)
    {
        try
        {
            // Parse URLs like: https://dev.azure.com/org/project/_git/repo
            var uri = new Uri(url);
            var segments = uri.AbsolutePath.Trim('/').Split('/');
            if (segments.Length >= 4 && segments[2] == "_git")
            {
                return (segments[0], segments[1], segments[3]);
            }

            return (string.Empty, string.Empty, string.Empty);
        }
        catch
        {
            return (string.Empty, string.Empty, string.Empty);
        }
    }

    private static void AddAuthHeader(HttpRequestMessage request, string token)
    {
        var auth = $":{token}";
        var authBytes = Encoding.ASCII.GetBytes(auth);
        var authEncoded = Convert.ToBase64String(authBytes);
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authEncoded);
        request.Headers.UserAgent.ParseAdd("AutoCodeForge");
    }
}
