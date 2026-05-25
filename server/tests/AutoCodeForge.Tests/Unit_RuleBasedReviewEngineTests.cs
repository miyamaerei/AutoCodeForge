using AutoCodeForge.Core.DTOs.Review;
using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Exceptions;
using AutoCodeForge.Infrastructure.Review;

namespace AutoCodeForge.Tests;

public sealed class RuleBasedReviewEngineTests : IDisposable
{
    private readonly string _workspacePath;
    private readonly RuleBasedReviewEngine _engine = new();

    public RuleBasedReviewEngineTests()
    {
        _workspacePath = Path.Combine(Path.GetTempPath(), $"autocodeforge.review.{Guid.NewGuid():N}");
        Directory.CreateDirectory(_workspacePath);
    }

    [Fact]
    public async Task ExecuteAsync_WhenRuleMatches_ShouldReturnRelativeFinding()
    {
        var sourceDirectory = Directory.CreateDirectory(Path.Combine(_workspacePath, "src"));
        var filePath = Path.Combine(sourceDirectory.FullName, "Example.cs");
        await File.WriteAllTextAsync(filePath, "namespace Demo;\n// TODO: remove hard-coded token\npublic class Example { }\n");

        var result = await _engine.ExecuteAsync(new ReviewExecutionRequestDto
        {
            WorkspacePath = _workspacePath,
            Rules =
            [
                new ReviewRuleDto
                {
                    Code = "TODO001",
                    Name = "Todo marker",
                    Severity = ReviewFindingSeverity.Low,
                    FilePattern = "*.cs",
                    ContainsText = "TODO",
                    Message = "Source contains TODO marker",
                    Suggestion = "Track work in an issue or remove the marker",
                },
            ],
        });

        var finding = Assert.Single(result.Findings);
        Assert.Equal("TODO001", finding.RuleCode);
        Assert.Equal("src/Example.cs", finding.FilePath);
        Assert.Equal(2, finding.LineNumber);
        Assert.Equal("// TODO: remove hard-coded token", finding.Evidence);
        Assert.Equal(1, result.FilesScanned);
        Assert.Equal(1, result.RulesEvaluated);
    }

    [Fact]
    public async Task ExecuteAsync_WhenRuleDoesNotMatch_ShouldReturnNoFindings()
    {
        await File.WriteAllTextAsync(Path.Combine(_workspacePath, "README.md"), "clean content\n");

        var result = await _engine.ExecuteAsync(new ReviewExecutionRequestDto
        {
            WorkspacePath = _workspacePath,
            Rules =
            [
                new ReviewRuleDto
                {
                    Code = "SEC001",
                    Name = "Forbidden literal",
                    Severity = ReviewFindingSeverity.High,
                    FilePattern = "*.md",
                    ContainsText = "password=",
                    Message = "Forbidden literal found",
                },
            ],
        });

        Assert.Empty(result.Findings);
        Assert.Equal(1, result.FilesScanned);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task ExecuteAsync_WhenWorkspaceMissing_ShouldThrowValidationException()
    {
        var missingPath = Path.Combine(_workspacePath, "missing");

        await Assert.ThrowsAsync<ValidationException>(() => _engine.ExecuteAsync(new ReviewExecutionRequestDto
        {
            WorkspacePath = missingPath,
        }));
    }

    public void Dispose()
    {
        if (!Directory.Exists(_workspacePath))
        {
            return;
        }

        try
        {
            Directory.Delete(_workspacePath, true);
        }
        catch (IOException)
        {
            // Ignore transient file locks on teardown.
        }
    }
}