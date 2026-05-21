using AutoCodeForge.Core.DTOs.Config;
using AutoCodeForge.Core.DTOs.RepoSync;
using AutoCodeForge.Core.Exceptions;

namespace AutoCodeForge.Infrastructure.Sandbox;

/// <summary>
/// Resolves sandbox paths with user isolation.
/// Optimized for Windows path length limits (260 chars).
/// </summary>
public class SandboxPathResolver
{
    /// <summary>
    /// Maximum path length for Windows (260 - reserved for file names).
    /// </summary>
    private const int MaxPathLength = 200;

    /// <summary>
    /// Resolves safe sandbox paths for one repo sync task.
    /// Path structure: {workspaceRoot}/{ntId}/{provider}_{owner}_{repo}
    /// </summary>
    /// <param name="sandboxConfig">Sandbox configuration snapshot.</param>
    /// <param name="ntId">Current user ntId.</param>
    /// <param name="taskId">Task identifier (not used in path).</param>
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

        // 使用短名称，避免路径过长
        var safeNtId = ShortenSegment(ntId, 32);
        var safeProvider = ShortenSegment(provider, 12);
        var safeOwner = ShortenSegment(owner, 16);
        var safeRepo = ShortenSegment(repo, 20);

        // 新路径结构：{workspaceRoot}/{ntId}/{provider}_{owner}_{repo}
        var effectiveSandboxPath = Path.GetFullPath(Path.Combine(workspaceRoot, safeNtId));
        EnsureWithinRoot(effectiveSandboxPath, workspaceRoot);

        var repoFolder = $"{safeProvider}_{safeOwner}_{safeRepo}";
        var repoPath = Path.GetFullPath(Path.Combine(effectiveSandboxPath, repoFolder));
        EnsureWithinRoot(repoPath, workspaceRoot);

        // 如果路径仍然太长，进一步缩短仓库名称
        if (repoPath.Length > MaxPathLength)
        {
            repoPath = ShortenPathToFit(repoPath, workspaceRoot, MaxPathLength);
        }

        return new SandboxPathResolutionResult
        {
            WorkspaceRootPath = workspaceRoot,
            EffectiveSandboxPath = effectiveSandboxPath,
            RepositoryPath = repoPath,
            RelativeRepoPath = Path.GetRelativePath(workspaceRoot, repoPath),
        };
    }

    /// <summary>
    /// Shortens path to fit within Windows path length limit.
    /// </summary>
    private static string ShortenPathToFit(string repoPath, string workspaceRoot, int maxLength)
    {
        if (repoPath.Length <= maxLength)
        {
            return repoPath;
        }

        var pathParts = repoPath.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);
        if (pathParts.Length < 3) // 至少需要 workspaceRoot/ntId/repoFolder
        {
            return repoPath;
        }

        var ntId = pathParts[^2]; // 倒数第二部分是 ntId
        var repoName = pathParts[^1]; // 最后一部分是仓库文件夹名

        // 计算可用空间
        var prefixLength = workspaceRoot.Length + 1 + ntId.Length + 1; // {root}/{ntId}/
        var availableLength = maxLength - prefixLength - 1; // -1 for trailing separator

        if (availableLength < 10)
        {
            availableLength = 10; // 最小保留10字符
        }

        // 缩短仓库名称
        if (repoName.Length > availableLength)
        {
            var hash = Math.Abs(repoName.GetHashCode()).ToString("X8")[..6];
            repoName = repoName[..Math.Min(availableLength - 6, repoName.Length)] + hash;
        }

        return Path.Combine(workspaceRoot, ntId, repoName);
    }

    /// <summary>
    /// Shortens a segment string to specified maximum length.
    /// </summary>
    private static string ShortenSegment(string value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "unk";
        }

        var sanitized = SanitizeSegment(value);
        if (sanitized.Length <= maxLength)
        {
            return sanitized;
        }

        // 保留前缀和哈希值
        var prefixLength = maxLength - 6;
        var hash = Math.Abs(sanitized.GetHashCode()).ToString("X")[..4];
        return sanitized[..prefixLength] + hash;
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