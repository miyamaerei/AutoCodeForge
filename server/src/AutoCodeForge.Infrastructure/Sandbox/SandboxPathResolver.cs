using AutoCodeForge.Core.DTOs.Config;
using AutoCodeForge.Core.DTOs.RepoSync;
using AutoCodeForge.Core.Exceptions;

namespace AutoCodeForge.Infrastructure.Sandbox;

/// <summary>
/// Resolves sandbox paths with user and task isolation.
/// </summary>
public class SandboxPathResolver
{
    /// <summary>
    /// Resolves safe sandbox paths for one repo sync task.
    /// </summary>
    /// <param name="sandboxConfig">Sandbox configuration snapshot.</param>
    /// <param name="ntId">Current user ntId.</param>
    /// <param name="taskId">Task identifier.</param>
    /// <param name="provider">Repository provider.</param>
    /// <param name="owner">Repository owner.</param>
    /// <param name="repo">Repository name.</param>
    /// <returns>Resolved path result.</returns>
    public SandboxPathResolutionResult Resolve(
        SandboxConfigDto sandboxConfig,
        string ntId,
        Guid taskId,
        string provider,
        string owner,
        string repo)
    {
        if (sandboxConfig is null)
        {
            throw new ValidationException("Sandbox config is required");
        }

        if (string.IsNullOrWhiteSpace(ntId))
        {
            throw new ValidationException("NtId is required for sandbox path resolution");
        }

        var workspaceRoot = Path.GetFullPath(sandboxConfig.WorkspaceRootPath.Trim());
        if (!Path.IsPathFullyQualified(workspaceRoot))
        {
            throw new ValidationException("workspaceRootPath must be absolute");
        }

        var safeNtId = SanitizeSegment(ntId);
        var safeProvider = SanitizeSegment(provider);
        var safeOwner = SanitizeSegment(owner);
        var safeRepo = SanitizeSegment(repo);

        var effectiveSandboxPath = sandboxConfig.UserIsolationEnabled
            ? Path.GetFullPath(Path.Combine(workspaceRoot, "users", safeNtId, "tasks", taskId.ToString("N")))
            : Path.GetFullPath(Path.Combine(workspaceRoot, "tasks", taskId.ToString("N")));

        EnsureWithinRoot(effectiveSandboxPath, workspaceRoot);

        var repoFolder = $"{safeProvider}_{safeOwner}_{safeRepo}";
        var repoPath = Path.GetFullPath(Path.Combine(effectiveSandboxPath, "repo", repoFolder));
        EnsureWithinRoot(repoPath, workspaceRoot);

        return new SandboxPathResolutionResult
        {
            WorkspaceRootPath = workspaceRoot,
            EffectiveSandboxPath = effectiveSandboxPath,
            RepositoryPath = repoPath,
            RelativeRepoPath = Path.GetRelativePath(workspaceRoot, repoPath),
        };
    }

    private static void EnsureWithinRoot(string path, string workspaceRoot)
    {
        var normalizedRoot = EnsureTrailingSeparator(workspaceRoot);
        var normalizedPath = EnsureTrailingSeparator(path);
        if (!normalizedPath.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase))
        {
            throw new ValidationException("Resolved sandbox path escaped workspace root");
        }
    }

    private static string SanitizeSegment(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "unknown";
        }

        var trimmed = value.Trim();
        var cleaned = new char[trimmed.Length];
        for (var i = 0; i < trimmed.Length; i++)
        {
            var ch = trimmed[i];
            cleaned[i] = char.IsLetterOrDigit(ch) || ch is '-' or '_' ? ch : '_';
        }

        var normalized = new string(cleaned).Trim('_');
        if (normalized.Length == 0)
        {
            normalized = "unknown";
        }

        if (normalized.Length > 64)
        {
            normalized = normalized[..64];
        }

        return normalized;
    }

    private static string EnsureTrailingSeparator(string value)
    {
        return value.EndsWith(Path.DirectorySeparatorChar)
            ? value
            : value + Path.DirectorySeparatorChar;
    }
}
