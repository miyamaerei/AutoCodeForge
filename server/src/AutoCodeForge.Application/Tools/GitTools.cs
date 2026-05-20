using System.Text.Json;
using AutoCodeForge.Core.DTOs.Repository;
using AutoCodeForge.Core.Interfaces;
using AutoCodeForge.Application.Services;

namespace AutoCodeForge.Application.Tools;

/// <summary>
/// Git operations tool for Agent integration.
/// </summary>
public class GitTools : IAgentTool
{
    private readonly RepositoryService _repositoryService;

    /// <summary>
    /// Gets the tool name.
    /// </summary>
    public string Name => "GitTool";

    /// <summary>
    /// Gets the tool description.
    /// </summary>
    public string Description => "Executes Git repository operations including listing branches, commits, and creating pull requests.";

    /// <summary>
    /// Initializes a new instance of the <see cref="GitTools"/> class.
    /// </summary>
    /// <param name="repositoryService">The repository service.</param>
    public GitTools(RepositoryService repositoryService)
    {
        _repositoryService = repositoryService;
    }

    /// <summary>
    /// Executes Git operations based on input parameters.
    /// </summary>
    /// <param name="input">The input arguments containing operation, repositoryId, and operation-specific parameters.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The operation result as JSON string.</returns>
    public async Task<string> ExecuteAsync(
        IReadOnlyDictionary<string, string> input,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!input.TryGetValue("operation", out var operation))
            {
                return JsonSerializer.Serialize(new { error = "Missing 'operation' parameter" });
            }

            if (!input.TryGetValue("repositoryId", out var repositoryIdStr) || !Guid.TryParse(repositoryIdStr, out var repositoryId))
            {
                return JsonSerializer.Serialize(new { error = "Invalid 'repositoryId' parameter" });
            }

            var result = operation.ToLowerInvariant() switch
            {
                "list-branches" => await ExecuteListBranchesAsync(repositoryId, input, cancellationToken),
                "get-commits" => await ExecuteGetCommitsAsync(repositoryId, input, cancellationToken),
                "list-pull-requests" => await ExecuteListPullRequestsAsync(repositoryId, input, cancellationToken),
                "create-pull-request" => await ExecuteCreatePullRequestAsync(repositoryId, input, cancellationToken),
                _ => JsonSerializer.Serialize(new { error = $"Unknown operation: {operation}" }),
            };

            return result;
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = $"Tool execution failed: {ex.Message}" });
        }
    }

    private async Task<string> ExecuteListBranchesAsync(Guid repositoryId, IReadOnlyDictionary<string, string> input, CancellationToken cancellationToken)
    {
        var branches = await _repositoryService.GetBranchesAsync(repositoryId, cancellationToken);
        return JsonSerializer.Serialize(new { success = true, branches });
    }

    private async Task<string> ExecuteGetCommitsAsync(Guid repositoryId, IReadOnlyDictionary<string, string> input, CancellationToken cancellationToken)
    {
        var branch = input.TryGetValue("branch", out var b) ? b : "main";
        var limit = input.TryGetValue("limit", out var l) && int.TryParse(l, out var parsed) ? parsed : 10;

        var commits = await _repositoryService.GetCommitsAsync(repositoryId, branch, limit, cancellationToken);
        return JsonSerializer.Serialize(new { success = true, commits });
    }

    private async Task<string> ExecuteListPullRequestsAsync(Guid repositoryId, IReadOnlyDictionary<string, string> input, CancellationToken cancellationToken)
    {
        var state = input.TryGetValue("state", out var s) ? s : "open";
        var limit = input.TryGetValue("limit", out var l) && int.TryParse(l, out var parsed) ? parsed : 20;

        var prs = await _repositoryService.ListPullRequestsAsync(repositoryId, state, limit, cancellationToken);
        return JsonSerializer.Serialize(new { success = true, pullRequests = prs });
    }

    private async Task<string> ExecuteCreatePullRequestAsync(Guid repositoryId, IReadOnlyDictionary<string, string> input, CancellationToken cancellationToken)
    {
        if (!input.TryGetValue("title", out var title) || string.IsNullOrWhiteSpace(title))
        {
            return JsonSerializer.Serialize(new { error = "Missing 'title' parameter" });
        }

        if (!input.TryGetValue("sourceBranch", out var sourceBranch) || string.IsNullOrWhiteSpace(sourceBranch))
        {
            return JsonSerializer.Serialize(new { error = "Missing 'sourceBranch' parameter" });
        }

        if (!input.TryGetValue("targetBranch", out var targetBranch) || string.IsNullOrWhiteSpace(targetBranch))
        {
            return JsonSerializer.Serialize(new { error = "Missing 'targetBranch' parameter" });
        }

        var request = new CreateGitPullRequestRequest
        {
            Title = title,
            Description = input.TryGetValue("description", out var desc) ? desc : null,
            SourceBranch = sourceBranch,
            TargetBranch = targetBranch,
        };

        var pr = await _repositoryService.CreatePullRequestAsync(repositoryId, request, cancellationToken);
        return JsonSerializer.Serialize(new { success = true, pullRequest = pr });
    }
}
