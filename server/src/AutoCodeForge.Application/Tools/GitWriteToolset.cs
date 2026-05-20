using System.Diagnostics;
using System.Text.Json;
using AutoCodeForge.Application.AI;
using AutoCodeForge.Application.Security;
using AutoCodeForge.Application.Services;
using AutoCodeForge.Core.DTOs.AI.GitTools;
using AutoCodeForge.Core.DTOs.Repository;
using AutoCodeForge.Core.Exceptions;
using AutoCodeForge.Core.Interfaces;
using AutoCodeForge.Infrastructure.AI;
using AutoCodeForge.Infrastructure.Logging;

namespace AutoCodeForge.Application.Tools;

/// <summary>
/// Write-level Git operations exposed as agent tool.
/// </summary>
public class GitWriteToolset : IAgentTool
{
    private readonly RepositoryService _repositoryService;
    private readonly GitSkillPermissionGuard _permissionGuard;
    private readonly GitContextHydrator _contextHydrator;
    private readonly AgentToolAuditLogger _auditLogger;
    private readonly GitSkillErrorMapper _errorMapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="GitWriteToolset"/> class.
    /// </summary>
    public GitWriteToolset(
        RepositoryService repositoryService,
        GitSkillPermissionGuard permissionGuard,
        GitContextHydrator contextHydrator,
        AgentToolAuditLogger auditLogger,
        GitSkillErrorMapper errorMapper)
    {
        _repositoryService = repositoryService;
        _permissionGuard = permissionGuard;
        _contextHydrator = contextHydrator;
        _auditLogger = auditLogger;
        _errorMapper = errorMapper;
    }

    /// <inheritdoc />
    public string Name => "GitWriteToolset";

    /// <inheritdoc />
    public string Description => "Write Git operations: create pull request and push branch with policy guard.";

    /// <inheritdoc />
    public async Task<string> ExecuteAsync(
        IReadOnlyDictionary<string, string> input,
        CancellationToken cancellationToken = default)
    {
        var timer = Stopwatch.StartNew();
        var hydrated = await _contextHydrator.HydrateAsync(input, cancellationToken);
        var operation = DetermineOperation(hydrated);

        Guid? repositoryId = null;
        Guid? sessionId = null;
        Guid? taskId = null;

        try
        {
            repositoryId = ParseGuid(hydrated, "repositoryId");
            sessionId = ParseGuid(hydrated, "sessionId");
            taskId = ParseGuid(hydrated, "taskId");

            if (!repositoryId.HasValue)
            {
                throw new ValidationException("Missing repositoryId for Git write operation.");
            }

            await _permissionGuard.EnsureAllowedAsync(repositoryId.Value, operation, cancellationToken);

            object data = operation switch
            {
                "create-pull-request" => await CreatePullRequestAsync(repositoryId.Value, hydrated, cancellationToken),
                "push" => await PushAsync(repositoryId.Value, hydrated, cancellationToken),
                "create-branch" => throw new ValidationException("create-branch is not supported in current provider abstraction."),
                "commit-changes" => throw new ValidationException("commit-changes is not supported in current provider abstraction."),
                _ => throw new ValidationException($"Unsupported write operation: {operation}"),
            };

            var result = new GitToolResultDto
            {
                Success = true,
                Operation = operation,
                Message = "Git write operation succeeded.",
                Data = data,
            };

            var payload = JsonSerializer.Serialize(result);
            await _auditLogger.LogAsync(
                Name,
                JsonSerializer.Serialize(hydrated),
                payload,
                "Succeeded",
                timer.ElapsedMilliseconds,
                sessionId,
                taskId,
                repositoryId,
                null,
                cancellationToken);

            return payload;
        }
        catch (Exception ex)
        {
            var (errorCode, suggestion) = _errorMapper.Map(ex);
            var result = new GitToolResultDto
            {
                Success = false,
                Operation = operation,
                Message = ex.Message,
                ErrorCode = errorCode,
                Suggestion = suggestion,
            };

            var payload = JsonSerializer.Serialize(result);
            await _auditLogger.LogAsync(
                Name,
                JsonSerializer.Serialize(hydrated),
                payload,
                "Failed",
                timer.ElapsedMilliseconds,
                sessionId,
                taskId,
                repositoryId,
                errorCode,
                cancellationToken);

            return payload;
        }
    }

    private async Task<object> CreatePullRequestAsync(
        Guid repositoryId,
        IReadOnlyDictionary<string, string> input,
        CancellationToken cancellationToken)
    {
        if (!input.TryGetValue("title", out var title) || string.IsNullOrWhiteSpace(title))
        {
            throw new ValidationException("Missing required argument: title");
        }

        if (!input.TryGetValue("sourceBranch", out var sourceBranch) || string.IsNullOrWhiteSpace(sourceBranch))
        {
            throw new ValidationException("Missing required argument: sourceBranch");
        }

        if (!input.TryGetValue("targetBranch", out var targetBranch) || string.IsNullOrWhiteSpace(targetBranch))
        {
            throw new ValidationException("Missing required argument: targetBranch");
        }

        var request = new CreateGitPullRequestRequest
        {
            Title = title,
            Description = input.TryGetValue("description", out var description) ? description : null,
            SourceBranch = sourceBranch,
            TargetBranch = targetBranch,
        };

        return await _repositoryService.CreatePullRequestAsync(repositoryId, request, cancellationToken);
    }

    private async Task<object> PushAsync(
        Guid repositoryId,
        IReadOnlyDictionary<string, string> input,
        CancellationToken cancellationToken)
    {
        if (!input.TryGetValue("localPath", out var localPath) || string.IsNullOrWhiteSpace(localPath))
        {
            throw new ValidationException("Missing required argument: localPath");
        }

        var branch = input.TryGetValue("branch", out var branchRaw) && !string.IsNullOrWhiteSpace(branchRaw)
            ? branchRaw
            : "main";

        var pushed = await _repositoryService.PushAsync(repositoryId, localPath, branch, cancellationToken);
        return new
        {
            pushed,
            branch,
        };
    }

    private static string DetermineOperation(IReadOnlyDictionary<string, string> input)
    {
        if (input.TryGetValue("operation", out var operation) && !string.IsNullOrWhiteSpace(operation))
        {
            return operation.Trim().ToLowerInvariant();
        }

        return "create-pull-request";
    }

    private static Guid? ParseGuid(IReadOnlyDictionary<string, string> input, string key)
    {
        return input.TryGetValue(key, out var raw) && Guid.TryParse(raw, out var value)
            ? value
            : null;
    }
}
