using System.Diagnostics;
using System.Text.Json;
using AutoCodeForge.Application.AI;
using AutoCodeForge.Application.Security;
using AutoCodeForge.Application.Services;
using AutoCodeForge.Core.DTOs.AI.GitTools;
using AutoCodeForge.Core.Exceptions;
using AutoCodeForge.Core.Interfaces;
using AutoCodeForge.Infrastructure.AI;
using AutoCodeForge.Infrastructure.Logging;

namespace AutoCodeForge.Application.Tools;

/// <summary>
/// Read-only Git operations exposed as agent tool.
/// </summary>
public class GitReadToolset : IAgentTool
{
    private readonly RepositoryService _repositoryService;
    private readonly GitSkillPermissionGuard _permissionGuard;
    private readonly GitContextHydrator _contextHydrator;
    private readonly AgentToolAuditLogger _auditLogger;
    private readonly GitSkillErrorMapper _errorMapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="GitReadToolset"/> class.
    /// </summary>
    public GitReadToolset(
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
    public string Name => "GitReadToolset";

    /// <inheritdoc />
    public string Description => "Read-only Git operations: list branches, get commits, list pull requests.";

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
                throw new ValidationException("Missing repositoryId for Git read operation.");
            }

            await _permissionGuard.EnsureAllowedAsync(repositoryId.Value, operation, cancellationToken);

            object data = operation switch
            {
                "list-branches" => await _repositoryService.GetBranchesAsync(repositoryId.Value, cancellationToken),
                "get-commits" => await _repositoryService.GetCommitsAsync(
                    repositoryId.Value,
                    hydrated.TryGetValue("branch", out var branch) ? branch : "main",
                    hydrated.TryGetValue("limit", out var limitRaw) && int.TryParse(limitRaw, out var limit) ? limit : 10,
                    cancellationToken),
                "list-pull-requests" => await _repositoryService.ListPullRequestsAsync(
                    repositoryId.Value,
                    hydrated.TryGetValue("state", out var state) ? state : "open",
                    hydrated.TryGetValue("limit", out var prLimitRaw) && int.TryParse(prLimitRaw, out var prLimit) ? prLimit : 20,
                    cancellationToken),
                _ => throw new ValidationException($"Unsupported read operation: {operation}"),
            };

            var result = new GitToolResultDto
            {
                Success = true,
                Operation = operation,
                Message = "Git read operation succeeded.",
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

    private static string DetermineOperation(IReadOnlyDictionary<string, string> input)
    {
        if (input.TryGetValue("operation", out var operation) && !string.IsNullOrWhiteSpace(operation))
        {
            return operation.Trim().ToLowerInvariant();
        }

        if (input.TryGetValue("query", out var query))
        {
            if (query.Contains("commit", StringComparison.OrdinalIgnoreCase))
            {
                return "get-commits";
            }

            if (query.Contains("pull", StringComparison.OrdinalIgnoreCase) || query.Contains("pr", StringComparison.OrdinalIgnoreCase))
            {
                return "list-pull-requests";
            }
        }

        return "list-branches";
    }

    private static Guid? ParseGuid(IReadOnlyDictionary<string, string> input, string key)
    {
        return input.TryGetValue(key, out var raw) && Guid.TryParse(raw, out var value)
            ? value
            : null;
    }
}
