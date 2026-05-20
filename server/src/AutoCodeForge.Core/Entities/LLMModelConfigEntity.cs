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
}
