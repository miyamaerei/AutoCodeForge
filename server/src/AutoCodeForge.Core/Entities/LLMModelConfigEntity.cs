using AutoCodeForge.Core.Entities.Base;
using SqlSugar;

namespace AutoCodeForge.Core.Entities;

/// <summary>
/// Represents user model provider configuration.
/// </summary>
[SugarTable("LlmModelConfigs")]
public class LLMModelConfigEntity : UserOwnedEntity
{
    /// <summary>
    /// Gets or sets the model config identifier.
    /// </summary>
    [SugarColumn(IsPrimaryKey = true)]
    public new Guid Id { get; set; }

    /// <summary>
    /// Gets or sets provider.
    /// </summary>
    public LLMProvider Provider { get; set; } = LLMProvider.AzureOpenAI;

    /// <summary>
    /// Gets or sets model display name.
    /// </summary>
    [SugarColumn(Length = 100, IsNullable = false)]
    public string ModelName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets endpoint URL.
    /// </summary>
    [SugarColumn(Length = 500, IsNullable = true)]
    public string? Endpoint { get; set; }

    /// <summary>
/// Gets or sets encrypted api key.
/// </summary>
[SugarColumn(Length = 1024, IsNullable = true)]
public string? ApiKey { get; set; }

/// <summary>
/// Gets or sets the CLI executable path (used for GitHub Copilot CLI).
/// </summary>
[SugarColumn(Length = 500, IsNullable = true)]
public string? CliExecutable { get; set; }

/// <summary>
/// Gets or sets the organization name (used for GitHub Copilot).
/// </summary>
[SugarColumn(Length = 100, IsNullable = true)]
public string? Organization { get; set; }

/// <summary>
/// Gets or sets the authentication mode (interactive/pat).
/// </summary>
[SugarColumn(Length = 20, IsNullable = true)]
public string? AuthMode { get; set; }

/// <summary>
/// Gets or sets the PAT environment variable name.
/// </summary>
[SugarColumn(Length = 50, IsNullable = true)]
public string? PatEnvVar { get; set; }
}

/// <summary>
/// Defines large language model providers.
/// </summary>
public enum LLMProvider
{
    AzureOpenAI = 0,
    OpenAI = 1,
    Anthropic = 2,
    Ollama = 3,
    GitHubCopilot = 4,
}
