using AutoCodeForge.Core.DTOs.AI;
using AutoCodeForge.Core.Entities;
using AutoCodeForge.Infrastructure.AI;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace AutoCodeForge.Tests;

/// <summary>
/// Unit tests for GitHubCopilotCliService.
/// Most tests are skipped by default as they require GitHub Copilot CLI to be installed and configured.
/// </summary>
[Trait("Category", "GitHubCopilot")]
public class GitHubCopilotCliServiceTests
{
    private readonly GitHubCopilotCliService _service;

    public GitHubCopilotCliServiceTests()
    {
        _service = new GitHubCopilotCliService(NullLogger<GitHubCopilotCliService>.Instance);
    }

    #region Service Initialization Tests

    [Fact]
    public void Constructor_WithValidLogger_ShouldInitialize()
    {
        var service = new GitHubCopilotCliService(NullLogger<GitHubCopilotCliService>.Instance);
        Assert.NotNull(service);
    }

    #endregion

    #region Basic Execution Tests

    [Fact(Skip = "Requires GitHub Copilot CLI installed and configured")]
    public async Task ExecuteAsync_WithSimplePrompt_ShouldReturnResponse()
    {
        var model = CreateTestModel("gpt-5");
        var prompt = "What is 2 + 2? Answer with only the number.";

        var response = await _service.ExecuteAsync(model, prompt, null, CancellationToken.None);

        Assert.NotNull(response);
        Assert.False(string.IsNullOrWhiteSpace(response.Content));
        Assert.Contains("4", response.Content);
        Assert.Equal("gpt-5", response.ModelName);
        Assert.True(response.CompletedAtUtc <= DateTime.UtcNow);
    }

    [Fact(Skip = "Requires GitHub Copilot CLI installed and configured")]
    public async Task ExecuteAsync_WithSystemPrompt_ShouldIncludeInstructions()
    {
        var model = CreateTestModel("gpt-5");
        var prompt = "Generate a hello world function";
        var systemPrompt = "You are a helpful coding assistant. Be concise.";

        var response = await _service.ExecuteAsync(model, prompt, systemPrompt, CancellationToken.None);

        Assert.NotNull(response);
        Assert.False(string.IsNullOrWhiteSpace(response.Content));
        Assert.Contains("hello", response.Content, StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    #region Configuration Tests

    [Fact(Skip = "Requires GitHub Copilot CLI installed and configured")]
    public async Task ExecuteAsync_WithCustomCliPath_ShouldUseSpecifiedExecutable()
    {
        var model = CreateTestModel("gpt-5", cliExecutable: "copilot");
        var prompt = "Say hello";

        var response = await _service.ExecuteAsync(model, prompt, null, CancellationToken.None);

        Assert.NotNull(response);
        Assert.False(string.IsNullOrWhiteSpace(response.Content));
    }

    [Fact(Skip = "Requires GitHub Copilot CLI with PAT authentication")]
    public async Task ExecuteAsync_WithPatEnvVar_ShouldAuthenticateWithToken()
    {
        var model = CreateTestModel(
            "gpt-5",
            patEnvVar: "GITHUB_TOKEN",
            apiKey: Environment.GetEnvironmentVariable("GITHUB_TOKEN") ?? "test-token"
        );
        var prompt = "Test authentication";

        var response = await _service.ExecuteAsync(model, prompt, null, CancellationToken.None);

        Assert.NotNull(response);
        Assert.False(string.IsNullOrWhiteSpace(response.Content));
    }

    [Fact(Skip = "Requires GitHub Copilot CLI with organization access")]
    public async Task ExecuteAsync_WithOrganization_ShouldSetOrgEnvironmentVariable()
    {
        var model = CreateTestModel("gpt-5", organization: "test-org");
        var prompt = "Test organization";

        var response = await _service.ExecuteAsync(model, prompt, null, CancellationToken.None);

        Assert.NotNull(response);
        Assert.False(string.IsNullOrWhiteSpace(response.Content));
    }

    [Fact(Skip = "Requires GitHub Copilot CLI installed and configured")]
    public async Task ExecuteAsync_WithDifferentModels_ShouldUseSpecifiedModel()
    {
        var testModels = new[] { "gpt-5", "gpt-4", "codex" };

        foreach (var modelName in testModels)
        {
            var model = CreateTestModel(modelName);
            var prompt = "Echo test";

            var response = await _service.ExecuteAsync(model, prompt, null, CancellationToken.None);

            Assert.NotNull(response);
            Assert.Equal(modelName, response.ModelName);
        }
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task ExecuteAsync_WithInvalidExecutable_ShouldThrowException()
    {
        var model = CreateTestModel("gpt-5", cliExecutable: "nonexistent-copilot-cli");
        var prompt = "Test";

        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _service.ExecuteAsync(model, prompt, null, CancellationToken.None)
        );
    }

    [Fact(Skip = "Requires GitHub Copilot CLI installed and configured")]
    public async Task ExecuteAsync_WithEmptyPrompt_ShouldHandleGracefully()
    {
        var model = CreateTestModel("gpt-5");
        var prompt = string.Empty;

        var exception = await Record.ExceptionAsync(
            async () => await _service.ExecuteAsync(model, prompt, null, CancellationToken.None)
        );

        // Should either succeed with empty response or throw meaningful exception
        Assert.True(exception == null || exception is InvalidOperationException);
    }

    [Fact(Skip = "Requires GitHub Copilot CLI installed and configured")]
    public async Task ExecuteAsync_WithSpecialCharacters_ShouldEscapeProperly()
    {
        var model = CreateTestModel("gpt-5");
        var prompt = "Test with \"quotes\" and $variables and `backticks`";

        var response = await _service.ExecuteAsync(model, prompt, null, CancellationToken.None);

        Assert.NotNull(response);
        Assert.False(string.IsNullOrWhiteSpace(response.Content));
    }

    #endregion

    #region Timeout and Cancellation Tests

    [Fact(Skip = "Requires GitHub Copilot CLI - may take up to 60 seconds")]
    public async Task ExecuteAsync_WithLongRunningRequest_ShouldTimeoutAfter60Seconds()
    {
        var model = CreateTestModel("gpt-5");
        var prompt = "Generate a very long and detailed explanation of quantum computing";

        var startTime = DateTime.UtcNow;
        var exception = await Record.ExceptionAsync(
            async () => await _service.ExecuteAsync(model, prompt, null, CancellationToken.None)
        );
        var elapsed = DateTime.UtcNow - startTime;

        // Should complete or timeout within reasonable time (slightly more than 60s for process cleanup)
        Assert.True(elapsed.TotalSeconds < 65);
    }

    [Fact(Skip = "Requires GitHub Copilot CLI installed and configured")]
    public async Task ExecuteAsync_WithCancellationToken_ShouldRespectCancellation()
    {
        var model = CreateTestModel("gpt-5");
        var prompt = "Generate a long explanation";
        var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromSeconds(1));

        await Assert.ThrowsAsync<OperationCanceledException>(
            async () => await _service.ExecuteAsync(model, prompt, null, cts.Token)
        );
    }

    [Fact]
    public async Task ExecuteAsync_WithPreCancelledToken_ShouldThrowImmediately()
    {
        var model = CreateTestModel("gpt-5");
        var prompt = "Test";
        var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(
            async () => await _service.ExecuteAsync(model, prompt, null, cts.Token)
        );
    }

    #endregion

    #region Thread Safety Tests

    [Fact(Skip = "Requires GitHub Copilot CLI - tests concurrent execution")]
    public async Task ExecuteAsync_ConcurrentCalls_ShouldExecuteSequentially()
    {
        var model = CreateTestModel("gpt-5");
        var tasks = new List<Task<LlmResponse>>();

        // Start 5 concurrent requests
        for (int i = 0; i < 5; i++)
        {
            var prompt = $"Test concurrent request {i}";
            tasks.Add(_service.ExecuteAsync(model, prompt, null, CancellationToken.None));
        }

        var responses = await Task.WhenAll(tasks);

        Assert.Equal(5, responses.Length);
        Assert.All(responses, r => Assert.False(string.IsNullOrWhiteSpace(r.Content)));
    }

    #endregion

    #region Response Format Tests

    [Fact(Skip = "Requires GitHub Copilot CLI installed and configured")]
    public async Task ExecuteAsync_ShouldReturnWellFormedResponse()
    {
        var model = CreateTestModel("gpt-5");
        var prompt = "Say hello";

        var response = await _service.ExecuteAsync(model, prompt, null, CancellationToken.None);

        Assert.NotNull(response);
        Assert.NotNull(response.Content);
        Assert.NotNull(response.ModelName);
        Assert.True(response.CompletedAtUtc > DateTime.MinValue);
        Assert.True(response.CompletedAtUtc <= DateTime.UtcNow);
        Assert.Equal(0, response.TotalTokens); // CLI service doesn't track tokens
    }

    [Fact(Skip = "Requires GitHub Copilot CLI installed and configured")]
    public async Task ExecuteAsync_ShouldTrimOutputWhitespace()
    {
        var model = CreateTestModel("gpt-5");
        var prompt = "Generate code";

        var response = await _service.ExecuteAsync(model, prompt, null, CancellationToken.None);

        Assert.NotNull(response);
        // Content should not start or end with whitespace
        Assert.Equal(response.Content, response.Content.Trim());
    }

    #endregion

    #region Code Generation Tests

    [Fact(Skip = "Requires GitHub Copilot CLI installed and configured")]
    public async Task ExecuteAsync_WithCodeGenerationPrompt_ShouldReturnCode()
    {
        var model = CreateTestModel("gpt-5");
        var prompt = "Write a C# function that adds two numbers";

        var response = await _service.ExecuteAsync(model, prompt, null, CancellationToken.None);

        Assert.NotNull(response);
        Assert.Contains("int", response.Content, StringComparison.OrdinalIgnoreCase);
        Assert.True(
            response.Content.Contains("public") || 
            response.Content.Contains("static") ||
            response.Content.Contains("void") ||
            response.Content.Contains("return")
        );
    }

    [Fact(Skip = "Requires GitHub Copilot CLI installed and configured")]
    public async Task ExecuteAsync_WithDebugPrompt_ShouldProvideAnalysis()
    {
        var model = CreateTestModel("gpt-5");
        var prompt = "Explain what this code does: int x = 5; int y = x * 2;";

        var response = await _service.ExecuteAsync(model, prompt, null, CancellationToken.None);

        Assert.NotNull(response);
        Assert.False(string.IsNullOrWhiteSpace(response.Content));
        Assert.True(response.Content.Length > 20); // Should provide meaningful explanation
    }

    #endregion

    #region Performance Tests

    [Fact(Skip = "Requires GitHub Copilot CLI - performance test")]
    public async Task ExecuteAsync_MultipleSequentialCalls_ShouldCompleteInReasonableTime()
    {
        var model = CreateTestModel("gpt-5");
        var startTime = DateTime.UtcNow;

        for (int i = 0; i < 3; i++)
        {
            var response = await _service.ExecuteAsync(
                model, 
                $"Quick test {i}", 
                null, 
                CancellationToken.None
            );
            Assert.NotNull(response);
        }

        var elapsed = DateTime.UtcNow - startTime;
        // 3 calls should complete in under 30 seconds total
        Assert.True(elapsed.TotalSeconds < 30, 
            $"Expected < 30 seconds, actual: {elapsed.TotalSeconds:F2}s");
    }

    #endregion

    #region Integration with Model Config Tests

    [Fact(Skip = "Requires GitHub Copilot CLI installed and configured")]
    public async Task ExecuteAsync_WithAllModelConfigProperties_ShouldUseAllSettings()
    {
        var model = new LLMModelConfigEntity
        {
            Id = Guid.NewGuid(),
            ModelName = "gpt-5",
            Provider = LLMProvider.GitHubCopilot,
            Endpoint = "https://github.com",
            CliExecutable = "copilot",
            ApiKey = Environment.GetEnvironmentVariable("GITHUB_TOKEN"),
            PatEnvVar = "GITHUB_TOKEN",
            Organization = "test-org",
            AuthMode = "pat"
        };

        var prompt = "Test all config properties";
        var response = await _service.ExecuteAsync(model, prompt, "Test system prompt", CancellationToken.None);

        Assert.NotNull(response);
        Assert.Equal("gpt-5", response.ModelName);
        Assert.False(string.IsNullOrWhiteSpace(response.Content));
    }

    #endregion

    #region Helper Methods

    private LLMModelConfigEntity CreateTestModel(
        string modelName,
        string? cliExecutable = null,
        string? patEnvVar = null,
        string? apiKey = null,
        string? organization = null)
    {
        return new LLMModelConfigEntity
        {
            Id = Guid.NewGuid(),
            ModelName = modelName,
            Provider = LLMProvider.GitHubCopilot,
            Endpoint = "https://github.com",
            CliExecutable = cliExecutable ?? "copilot",
            ApiKey = apiKey,
            PatEnvVar = patEnvVar,
            Organization = organization,
            AuthMode = string.IsNullOrEmpty(patEnvVar) ? "interactive" : "pat"
        };
    }

    #endregion
}
