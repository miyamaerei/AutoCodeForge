using System.Text.RegularExpressions;

namespace AutoCodeForge.Infrastructure.Logging;

/// <summary>
/// Sanitizes sensitive values in logs.
/// </summary>
public static class LogSanitizer
{
    private static readonly Regex TokenRegex = new("(token|password|secret|pat)(\\s*[:=]\\s*)([^\\s;]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    /// <summary>
    /// Redacts sensitive token-like values.
    /// </summary>
    /// <param name="message">The source message.</param>
    /// <returns>Sanitized message.</returns>
    public static string Sanitize(string? message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return string.Empty;
        }

        return TokenRegex.Replace(message, "$1$2***");
    }
}
