using System.Text.RegularExpressions;
using AutoCodeForge.Core.DTOs.Review;
using AutoCodeForge.Core.Exceptions;
using AutoCodeForge.Core.Interfaces;

namespace AutoCodeForge.Infrastructure.Review;

/// <summary>
/// Executes literal text rules over repository files and returns structured findings.
/// </summary>
public class RuleBasedReviewEngine : IReviewEngine
{
    /// <inheritdoc/>
    public async Task<ReviewExecutionResultDto> ExecuteAsync(
        ReviewExecutionRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.WorkspacePath))
        {
            throw new ValidationException("Review workspace path is required");
        }

        if (!Directory.Exists(request.WorkspacePath))
        {
            throw new ValidationException("Review workspace path does not exist");
        }

        var rules = request.Rules?
            .Where(rule => !string.IsNullOrWhiteSpace(rule.Code) && !string.IsNullOrWhiteSpace(rule.ContainsText))
            .ToList() ?? [];

        var findings = new List<ReviewFindingDto>();
        var errors = new List<string>();
        var files = Directory.EnumerateFiles(request.WorkspacePath, "*", SearchOption.AllDirectories)
            .Where(file => !file.Contains($"{Path.DirectorySeparatorChar}.git{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .ToList();

        foreach (var file in files)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var relativePath = NormalizeRelativePath(Path.GetRelativePath(request.WorkspacePath, file));
            string content;

            try
            {
                content = await File.ReadAllTextAsync(file, cancellationToken);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                errors.Add($"Failed to read {relativePath}: {ex.Message}");
                continue;
            }

            foreach (var rule in rules)
            {
                if (!MatchesPattern(relativePath, rule.FilePattern))
                {
                    continue;
                }

                var match = FindMatch(content, rule.ContainsText!);
                if (match is null)
                {
                    continue;
                }

                findings.Add(new ReviewFindingDto
                {
                    Severity = rule.Severity,
                    RuleCode = rule.Code,
                    FilePath = relativePath,
                    LineNumber = match.Value.LineNumber,
                    Message = string.IsNullOrWhiteSpace(rule.Message) ? rule.Name : rule.Message,
                    Suggestion = rule.Suggestion,
                    Evidence = match.Value.Evidence,
                });
            }
        }

        return new ReviewExecutionResultDto
        {
            Findings = findings,
            FilesScanned = files.Count,
            RulesEvaluated = rules.Count,
            Errors = errors,
        };
    }

    private static bool MatchesPattern(string relativePath, string? filePattern)
    {
        if (string.IsNullOrWhiteSpace(filePattern) || filePattern == "*")
        {
            return true;
        }

        var normalizedPattern = filePattern.Replace('\\', '/');
        var candidate = normalizedPattern.Contains('/')
            ? relativePath
            : Path.GetFileName(relativePath);
        var regex = "^" + Regex.Escape(filePattern)
            .Replace("\\*\\*", ".*")
            .Replace("\\*", "[^/]*")
            .Replace("\\?", ".") + "$";

        return Regex.IsMatch(candidate, regex, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    }

    private static (int LineNumber, string Evidence)? FindMatch(string content, string containsText)
    {
        using var reader = new StringReader(content);
        string? line;
        var lineNumber = 0;
        while ((line = reader.ReadLine()) is not null)
        {
            lineNumber++;
            if (line.Contains(containsText, StringComparison.OrdinalIgnoreCase))
            {
                return (lineNumber, line.Trim());
            }
        }

        return null;
    }

    private static string NormalizeRelativePath(string path)
    {
        return path.Replace('\\', '/');
    }
}