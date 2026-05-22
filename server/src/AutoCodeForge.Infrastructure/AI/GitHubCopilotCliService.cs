using AutoCodeForge.Core.DTOs.AI;
using AutoCodeForge.Core.Entities;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace AutoCodeForge.Infrastructure.AI;

/// <summary>
/// GitHub Copilot integration service using CLI for current implementation.
/// This can be upgraded to use GitHub.Copilot.SDK when it becomes stable.
/// </summary>
/// <remarks>
/// Future enhancement: Integrate with Microsoft Agent Framework using CopilotClient.AsAIAgent()
/// when GitHub.Copilot.SDK reaches stable version. The implementation would follow this pattern:
/// 
/// var copilotClient = new CopilotClient(new CopilotClientOptions { AutoStart = true });
/// await copilotClient.StartAsync(cancellationToken);
/// var agent = copilotClient.AsAIAgent(sessionConfig, ownsClient: false);
/// var result = await agent.RunAsync(prompt, cancellationToken);
/// 
/// Reference: MAF + Copilot CLI Integration Guide
/// </remarks>
public class GitHubCopilotCliService : IGitHubCopilotService
{
    private readonly ILogger<GitHubCopilotCliService> _logger;
    private readonly SemaphoreSlim _executionLock = new(1, 1);

    public GitHubCopilotCliService(ILogger<GitHubCopilotCliService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Executes a prompt using GitHub Copilot CLI.
    /// </summary>
    public async Task<LlmResponse> ExecuteAsync(
        LLMModelConfigEntity model,
        string prompt,
        string? systemPrompt,
        CancellationToken cancellationToken = default)
    {
        var executable = string.IsNullOrWhiteSpace(model.CliExecutable) 
            ? "copilot" 
            : model.CliExecutable;

        _logger.LogInformation(
            "Calling GitHub Copilot CLI: {Executable}, Model={ModelName}",
            executable,
            model.ModelName);

        try
        {
            await _executionLock.WaitAsync(cancellationToken);
            try
            {
                return await ExecuteCliAsync(model, executable, prompt, systemPrompt, cancellationToken);
            }
            finally
            {
                _executionLock.Release();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling GitHub Copilot CLI");
            throw;
        }
    }

    private async Task<LlmResponse> ExecuteCliAsync(
        LLMModelConfigEntity model,
        string executable,
        string prompt,
        string? systemPrompt,
        CancellationToken cancellationToken)
    {
        using var process = new Process();
        process.StartInfo.FileName = executable;
        
        var arguments = $"-p \"{EscapeForShell(prompt)}\" --allow-all-tools --silent";
        if (!string.IsNullOrWhiteSpace(model.ModelName))
        {
            arguments += $" --model \"{model.ModelName}\"";
        }
        
        process.StartInfo.Arguments = arguments;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.CreateNoWindow = true;

        if (!string.IsNullOrWhiteSpace(model.PatEnvVar) && !string.IsNullOrWhiteSpace(model.ApiKey))
        {
            process.StartInfo.EnvironmentVariables[model.PatEnvVar] = model.ApiKey;
        }

        if (!string.IsNullOrWhiteSpace(model.Organization))
        {
            process.StartInfo.EnvironmentVariables["GITHUB_ORG"] = model.Organization;
        }

        _logger.LogDebug("Executing: {Executable} {Arguments}", executable, arguments);

        process.Start();

        var outputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var errorTask = process.StandardError.ReadToEndAsync(cancellationToken);

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(60));

        try
        {
            await process.WaitForExitAsync(timeoutCts.Token);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("GitHub Copilot CLI request timed out");
            try
            {
                process.Kill(entireProcessTree: true);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to kill process after timeout");
            }
            throw;
        }

        var output = await outputTask;
        var error = await errorTask;

        if (process.ExitCode != 0)
        {
            _logger.LogError(
                "GitHub Copilot CLI failed with exit code {ExitCode}: {Error}",
                process.ExitCode,
                error);
            throw new InvalidOperationException($"CLI execution failed with exit code {process.ExitCode}: {error}");
        }

        _logger.LogInformation("GitHub Copilot CLI completed successfully");

        return new LlmResponse
        {
            Content = output.Trim(),
            ModelName = model.ModelName ?? "GitHub Copilot CLI",
            CompletedAtUtc = DateTime.UtcNow,
            TotalTokens = 0,
        };
    }

    private static string EscapeForShell(string input)
    {
        return input
            .Replace("\"", "\\\"")
            .Replace("`", "\\`")
            .Replace("$", "\\$");
    }
}
