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
        var resolvedPath = ResolveExecutablePath(executable);
        process.StartInfo.FileName = resolvedPath;

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

        _logger.LogDebug("Executing: {Executable} {Arguments}", resolvedPath, arguments);

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

    /// <summary>
    /// 解析可执行文件路径，支持相对命令名和完整路径
    /// </summary>
    private static string ResolveExecutablePath(string executable)
    {
        // 如果已经是完整路径，直接返回
        if (Path.IsPathRooted(executable) && File.Exists(executable))
        {
            return executable;
        }

        // 如果包含路径分隔符，尝试在当前目录查找
        if (executable.Contains(Path.DirectorySeparatorChar) || executable.Contains(Path.AltDirectorySeparatorChar))
        {
            var fullPath = Path.Combine(Environment.CurrentDirectory, executable);
            if (File.Exists(fullPath))
                return fullPath;
        }

        // 在 PATH 环境变量中搜索
        var pathEnv = Environment.GetEnvironmentVariable("PATH") ?? "";
        var paths = pathEnv.Split(Path.PathSeparator);

        foreach (var path in paths)
        {
            if (string.IsNullOrWhiteSpace(path))
                continue;

            // Windows 上优先尝试 .cmd 后缀（npm 全局脚本）
            if (Path.DirectorySeparatorChar == '\\' && !executable.EndsWith(".cmd", StringComparison.OrdinalIgnoreCase))
            {
                var cmdPath = Path.Combine(path, executable + ".cmd");
                if (File.Exists(cmdPath))
                    return cmdPath;
            }

            // 尝试直接路径
            var fullPath = Path.Combine(path, executable);
            if (File.Exists(fullPath))
            {
                // 检查是否是有效的可执行文件（不是脚本文件）
                var ext = Path.GetExtension(fullPath).ToLowerInvariant();
                if (string.IsNullOrEmpty(ext) || ext == ".exe" || ext == ".cmd" || ext == ".bat")
                {
                    return fullPath;
                }
                // 如果是脚本文件(.ps1, .js等)，需要通过 shell 执行
                if (ext == ".ps1")
                {
                    return $"powershell.exe \"{fullPath}\"";
                }
            }
        }

        // 最后返回原始命令，让系统自己报错
        return executable;
    }
}
