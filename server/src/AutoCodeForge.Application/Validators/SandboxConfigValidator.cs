using AutoCodeForge.Core.DTOs.Config;
using AutoCodeForge.Core.Exceptions;

namespace AutoCodeForge.Application.Validators;

/// <summary>
/// Validates sandbox configuration values.
/// </summary>
public class SandboxConfigValidator
{
    private const int MinTimeoutSeconds = 30;
    private const int MaxTimeoutSeconds = 7200;

    /// <summary>
    /// Validates sandbox config and throws validation exception if invalid.
    /// </summary>
    /// <param name="config">Sandbox config to validate.</param>
    public void ValidateAndThrow(SandboxConfigDto config)
    {
        if (config is null)
        {
            throw new ValidationException("Sandbox config cannot be null");
        }

        if (string.IsNullOrWhiteSpace(config.WorkspaceRootPath))
        {
            throw new ValidationException("workspaceRootPath is required");
        }

        if (!Path.IsPathFullyQualified(config.WorkspaceRootPath))
        {
            throw new ValidationException("workspaceRootPath must be an absolute local path");
        }

        var normalizedRoot = Path.GetFullPath(config.WorkspaceRootPath.Trim());
        if (normalizedRoot.StartsWith("\\\\", StringComparison.Ordinal))
        {
            throw new ValidationException("workspaceRootPath must be a local path, UNC path is not allowed");
        }

        if (config.TimeoutSeconds < MinTimeoutSeconds || config.TimeoutSeconds > MaxTimeoutSeconds)
        {
            throw new ValidationException($"timeoutSeconds must be between {MinTimeoutSeconds} and {MaxTimeoutSeconds}");
        }

        foreach (var rawPath in config.AllowedWritePaths)
        {
            if (string.IsNullOrWhiteSpace(rawPath))
            {
                throw new ValidationException("allowedWritePaths cannot contain empty values");
            }

            if (!Path.IsPathFullyQualified(rawPath))
            {
                throw new ValidationException("allowedWritePaths must be absolute paths");
            }

            var normalizedAllowed = Path.GetFullPath(rawPath.Trim());
            if (!IsWithinRoot(normalizedAllowed, normalizedRoot))
            {
                throw new ValidationException("allowedWritePaths must stay inside workspaceRootPath");
            }
        }
    }

    private static bool IsWithinRoot(string path, string root)
    {
        var normalizedRoot = EnsureTrailingSeparator(root);
        var normalizedPath = EnsureTrailingSeparator(path);
        return normalizedPath.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase);
    }

    private static string EnsureTrailingSeparator(string value)
    {
        return value.EndsWith(Path.DirectorySeparatorChar)
            ? value
            : value + Path.DirectorySeparatorChar;
    }
}
