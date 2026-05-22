using AutoCodeForge.Core.Enums;

namespace AutoCodeForge.Core.DTOs.Config;

/// <summary>
/// Represents LLM configuration types.
/// </summary>
public enum LlmConfigSubType
{
    /// <summary>
    /// LLM model configuration.
    /// </summary>
    Model,

    /// <summary>
    /// LLM credential configuration.
    /// </summary>
    Credential,

    /// <summary>
    /// LLM global settings.
    /// </summary>
    Settings
}

/// <summary>
/// Represents LLM provider types.
/// </summary>
public enum LlmProvider
{
    /// <summary>
    /// OpenAI API.
    /// </summary>
    OpenAI,

    /// <summary>
    /// Azure OpenAI Service.
    /// </summary>
    AzureOpenAI,

    /// <summary>
    /// GitHub Copilot.
    /// </summary>
    GitHubCopilot,

    /// <summary>
    /// Ollama local LLM.
    /// </summary>
    Ollama,

    /// <summary>
    /// Custom provider.
    /// </summary>
    Custom
}

/// <summary>
/// Represents credential provider types.
/// </summary>
public enum CredentialProviderType
{
    /// <summary>
    /// API key authentication.
    /// </summary>
    ApiKey,

    /// <summary>
    /// Environment variable authentication.
    /// </summary>
    EnvVar,

    /// <summary>
    /// Interactive authentication.
    /// </summary>
    Interactive,

    /// <summary>
    /// No authentication required.
    /// </summary>
    None
}

/// <summary>
/// LLM model configuration DTO.
/// </summary>
public class LlmModelConfigDto
{
    /// <summary>
    /// Gets or sets the configuration subtype.
    /// </summary>
    public LlmConfigSubType Type { get; set; } = LlmConfigSubType.Model;

    /// <summary>
    /// Gets or sets the LLM provider.
    /// </summary>
    public LlmProvider Provider { get; set; }

    /// <summary>
    /// Gets or sets the model name.
    /// </summary>
    public string ModelName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the API endpoint (optional for some providers).
    /// </summary>
    public string? Endpoint { get; set; }

    /// <summary>
    /// Gets or sets the associated credential key (null for providers without credentials).
    /// </summary>
    public string? CredentialKey { get; set; }

    /// <summary>
    /// Gets or sets the context window size.
    /// </summary>
    public int ContextWindow { get; set; } = 8192;

    /// <summary>
    /// Gets or sets the weight for model selection.
    /// </summary>
    public int Weight { get; set; } = 1;

    /// <summary>
    /// Gets or sets a value indicating whether this is the default model.
    /// </summary>
    public bool IsDefault { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether this model is active.
    /// </summary>
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// LLM credential configuration DTO.
/// </summary>
public class LlmCredentialConfigDto
{
    /// <summary>
    /// Gets or sets the configuration subtype.
    /// </summary>
    public LlmConfigSubType Type { get; set; } = LlmConfigSubType.Credential;

    /// <summary>
    /// Gets or sets the credential provider type.
    /// </summary>
    public CredentialProviderType ProviderType { get; set; }

    /// <summary>
    /// Gets or sets the API key (encrypted when stored).
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// Gets or sets the environment variable name for API key.
    /// </summary>
    public string? EnvVarName { get; set; }

    /// <summary>
    /// Gets or sets the credential description.
    /// </summary>
    public string? Description { get; set; }
}

/// <summary>
/// LLM global settings configuration DTO.
/// </summary>
public class LlmSettingsConfigDto
{
    /// <summary>
    /// Gets or sets the configuration subtype.
    /// </summary>
    public LlmConfigSubType Type { get; set; } = LlmConfigSubType.Settings;

    /// <summary>
    /// Gets or sets the default model key.
    /// </summary>
    public string? DefaultModelKey { get; set; }

    /// <summary>
    /// Gets or sets the fallback model key for failover.
    /// </summary>
    public string? FallbackModelKey { get; set; }

    /// <summary>
    /// Gets or sets the default temperature parameter.
    /// </summary>
    public decimal Temperature { get; set; } = 0.7m;

    /// <summary>
    /// Gets or sets the default maximum tokens.
    /// </summary>
    public int MaxTokens { get; set; } = 2048;

    /// <summary>
    /// Gets or sets the default timeout in seconds.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 60;

    /// <summary>
    /// Gets or sets the maximum number of retries.
    /// </summary>
    public int MaxRetries { get; set; } = 3;
}

/// <summary>
/// LLM configuration key builder utility.
/// </summary>
public static class LlmConfigKeyBuilder
{
    /// <summary>
    /// Builds a model configuration key.
    /// </summary>
    /// <param name="provider">The LLM provider.</param>
    /// <param name="modelName">The model name.</param>
    /// <returns>The configuration key.</returns>
    public static string BuildModelKey(LlmProvider provider, string modelName)
    {
        return $"model.{provider.ToString().ToLowerInvariant()}-{modelName.ToLowerInvariant().Replace(" ", "-")}";
    }

    /// <summary>
    /// Builds a credential configuration key.
    /// </summary>
    /// <param name="provider">The credential provider.</param>
    /// <param name="purpose">The credential purpose.</param>
    /// <returns>The configuration key.</returns>
    public static string BuildCredentialKey(CredentialProviderType provider, string purpose)
    {
        return $"credential.{provider.ToString().ToLowerInvariant()}-{purpose.ToLowerInvariant().Replace(" ", "-")}";
    }

    /// <summary>
    /// Gets the settings configuration key.
    /// </summary>
    /// <returns>The settings configuration key.</returns>
    public static string GetSettingsKey()
    {
        return "settings.default";
    }
}
