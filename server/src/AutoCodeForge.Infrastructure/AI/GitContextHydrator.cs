using System.Text.Json;
using AutoCodeForge.Infrastructure.Repositories;

namespace AutoCodeForge.Infrastructure.AI;

/// <summary>
/// Enriches Git tool input with repository/task/session context.
/// </summary>
public class GitContextHydrator
{
    private readonly ChatSessionRepository _chatSessionRepository;
    private readonly TaskRepository _taskRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="GitContextHydrator"/> class.
    /// </summary>
    /// <param name="chatSessionRepository">The chat session repository.</param>
    /// <param name="taskRepository">The task repository.</param>
    public GitContextHydrator(ChatSessionRepository chatSessionRepository, TaskRepository taskRepository)
    {
        _chatSessionRepository = chatSessionRepository;
        _taskRepository = taskRepository;
    }

    /// <summary>
    /// Hydrates missing repositoryId/taskId from session-bound task snapshot.
    /// </summary>
    /// <param name="input">Original input dictionary.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Hydrated dictionary copy.</returns>
    public async Task<Dictionary<string, string>> HydrateAsync(
        IReadOnlyDictionary<string, string> input,
        CancellationToken cancellationToken = default)
    {
        var hydrated = new Dictionary<string, string>(input, StringComparer.OrdinalIgnoreCase);

        if (!hydrated.ContainsKey("taskId")
            && hydrated.TryGetValue("sessionId", out var sessionIdRaw)
            && Guid.TryParse(sessionIdRaw, out var sessionId))
        {
            var session = await _chatSessionRepository.GetByIdAsync(sessionId, false, cancellationToken);
            if (session?.TaskId is Guid taskId)
            {
                hydrated["taskId"] = taskId.ToString();
            }
        }

        if (!hydrated.ContainsKey("repositoryId")
            && hydrated.TryGetValue("taskId", out var taskIdRaw)
            && Guid.TryParse(taskIdRaw, out var taskIdFromInput))
        {
            var task = await _taskRepository.GetByIdAsync(taskIdFromInput, false, cancellationToken);
            var snapshotJson = task?.RepositorySnapshotJson;
            var repositoryId = TryExtractRepositoryId(snapshotJson);
            if (repositoryId.HasValue)
            {
                hydrated["repositoryId"] = repositoryId.Value.ToString();
            }
        }

        return hydrated;
    }

    private static Guid? TryExtractRepositoryId(string? snapshotJson)
    {
        if (string.IsNullOrWhiteSpace(snapshotJson))
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(snapshotJson);
            if (TryReadGuid(document.RootElement, "repositoryId", out var repositoryId)
                || TryReadGuid(document.RootElement, "id", out repositoryId)
                || TryReadGuid(document.RootElement, "RepositoryId", out repositoryId)
                || TryReadGuid(document.RootElement, "Id", out repositoryId))
            {
                return repositoryId;
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    private static bool TryReadGuid(JsonElement element, string propertyName, out Guid value)
    {
        value = Guid.Empty;
        if (!element.TryGetProperty(propertyName, out var property))
        {
            return false;
        }

        var raw = property.GetString();
        return Guid.TryParse(raw, out value);
    }
}
