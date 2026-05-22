using AutoCodeForge.Core.DTOs.Config;
using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Enums;
using AutoCodeForge.Core.Exceptions;
using AutoCodeForge.Core.Helpers;

namespace AutoCodeForge.Application.Services;

/// <summary>
/// Provides LLM-specific configuration management services.
/// Supports multiple models, credentials, and global settings.
/// </summary>
public class LlmConfigService
{
    private readonly ConfigService _configService;

    /// <summary>
    /// Initializes a new instance of the <see cref="LlmConfigService"/> class.
    /// </summary>
    /// <param name="configService">The configuration service.</param>
    public LlmConfigService(ConfigService configService)
    {
        _configService = configService;
    }

    /// <summary>
    /// Gets all LLM configurations.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The list of LLM configuration entries.</returns>
    public async Task<List<ConfigurationEntry>> GetAllConfigsAsync(CancellationToken cancellationToken = default)
    {
        return await _configService.GetByTypeAsync(ConfigType.Llm, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Gets all LLM model configurations.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The list of LLM model configurations.</returns>
    public async Task<List<LlmModelConfigDto>> GetAllModelsAsync(CancellationToken cancellationToken = default)
    {
        var configs = await _configService.GetByTypeAsync(ConfigType.Llm, cancellationToken: cancellationToken);
        var models = new List<LlmModelConfigDto>();

        foreach (var config in configs)
        {
            if (config.ConfigKey.StartsWith("model."))
            {
                try
                {
                    var model = JsonHelper.Deserialize<LlmModelConfigDto>(config.ConfigValue);
                    if (model != null)
                    {
                        models.Add(model);
                    }
                }
                catch
                {
                    // Ignore invalid JSON entries
                }
            }
        }

        return models;
    }

    /// <summary>
    /// Gets an LLM model configuration by key.
    /// </summary>
    /// <param name="configKey">The configuration key.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The LLM model configuration, or null if not found.</returns>
    public async Task<LlmModelConfigDto?> GetModelAsync(string configKey, CancellationToken cancellationToken = default)
    {
        var config = await _configService.GetByTypeAndKeyAsync(ConfigType.Llm, configKey, cancellationToken: cancellationToken);
        if (config == null)
        {
            return null;
        }

        return JsonHelper.Deserialize<LlmModelConfigDto>(config.ConfigValue);
    }

    /// <summary>
    /// Creates or updates an LLM model configuration.
    /// </summary>
    /// <param name="modelConfig">The model configuration.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created/updated configuration entry.</returns>
    public async Task<ConfigurationEntry> UpsertModelAsync(LlmModelConfigDto modelConfig, CancellationToken cancellationToken = default)
    {
        if (modelConfig == null)
        {
            throw new ValidationException("Model configuration cannot be null");
        }

        modelConfig.Type = LlmConfigSubType.Model;
        var configKey = LlmConfigKeyBuilder.BuildModelKey(modelConfig.Provider, modelConfig.ModelName);
        var configValue = JsonHelper.Serialize(modelConfig);

        return await _configService.UpsertAsync(
            ConfigType.Llm,
            configKey,
            configValue,
            isEncrypted: false,
            description: $"LLM Model: {modelConfig.Provider} - {modelConfig.ModelName}",
            group: "models",
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Creates or updates an LLM credential configuration.
    /// </summary>
    /// <param name="credentialConfig">The credential configuration.</param>
    /// <param name="purpose">The credential purpose/name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created/updated configuration entry.</returns>
    public async Task<ConfigurationEntry> UpsertCredentialAsync(LlmCredentialConfigDto credentialConfig, string purpose, CancellationToken cancellationToken = default)
    {
        if (credentialConfig == null)
        {
            throw new ValidationException("Credential configuration cannot be null");
        }

        credentialConfig.Type = LlmConfigSubType.Credential;
        var configKey = LlmConfigKeyBuilder.BuildCredentialKey(credentialConfig.ProviderType, purpose);
        
        // Determine if encryption is needed
        bool isEncrypted = credentialConfig.ProviderType == CredentialProviderType.ApiKey && 
                           !string.IsNullOrWhiteSpace(credentialConfig.ApiKey);
        
        var configValue = JsonHelper.Serialize(credentialConfig);

        return await _configService.UpsertAsync(
            ConfigType.Llm,
            configKey,
            configValue,
            isEncrypted: isEncrypted,
            description: credentialConfig.Description ?? $"LLM Credential: {purpose}",
            group: "credentials",
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Gets an LLM credential configuration by key.
    /// </summary>
    /// <param name="configKey">The configuration key.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The LLM credential configuration, or null if not found.</returns>
    public async Task<LlmCredentialConfigDto?> GetCredentialAsync(string configKey, CancellationToken cancellationToken = default)
    {
        var config = await _configService.GetByTypeAndKeyAsync(ConfigType.Llm, configKey, cancellationToken: cancellationToken);
        if (config == null)
        {
            return null;
        }

        return JsonHelper.Deserialize<LlmCredentialConfigDto>(config.ConfigValue);
    }

    /// <summary>
    /// Gets all LLM credential configurations.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The list of LLM credential configurations.</returns>
    public async Task<List<LlmCredentialConfigDto>> GetAllCredentialsAsync(CancellationToken cancellationToken = default)
    {
        var configs = await _configService.GetByTypeAsync(ConfigType.Llm, cancellationToken: cancellationToken);
        var credentials = new List<LlmCredentialConfigDto>();

        foreach (var config in configs)
        {
            if (config.ConfigKey.StartsWith("credential."))
            {
                try
                {
                    var credential = JsonHelper.Deserialize<LlmCredentialConfigDto>(config.ConfigValue);
                    if (credential != null)
                    {
                        credentials.Add(credential);
                    }
                }
                catch
                {
                    // Ignore invalid JSON entries
                }
            }
        }

        return credentials;
    }

    /// <summary>
    /// Gets the LLM global settings configuration.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The LLM settings configuration, or default settings if not found.</returns>
    public async Task<LlmSettingsConfigDto> GetSettingsAsync(CancellationToken cancellationToken = default)
    {
        var configKey = LlmConfigKeyBuilder.GetSettingsKey();
        var config = await _configService.GetByTypeAndKeyAsync(ConfigType.Llm, configKey, cancellationToken: cancellationToken);
        
        if (config == null)
        {
            return new LlmSettingsConfigDto();
        }

        return JsonHelper.Deserialize<LlmSettingsConfigDto>(config.ConfigValue) ?? new LlmSettingsConfigDto();
    }

    /// <summary>
    /// Creates or updates the LLM global settings configuration.
    /// </summary>
    /// <param name="settings">The settings configuration.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created/updated configuration entry.</returns>
    public async Task<ConfigurationEntry> UpsertSettingsAsync(LlmSettingsConfigDto settings, CancellationToken cancellationToken = default)
    {
        if (settings == null)
        {
            throw new ValidationException("Settings cannot be null");
        }

        settings.Type = LlmConfigSubType.Settings;
        var configKey = LlmConfigKeyBuilder.GetSettingsKey();
        var configValue = JsonHelper.Serialize(settings);

        return await _configService.UpsertAsync(
            ConfigType.Llm,
            configKey,
            configValue,
            isEncrypted: false,
            description: "LLM Global Settings",
            group: "settings",
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Gets the default model configuration.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The default model configuration, or null if not found.</returns>
    public async Task<LlmModelConfigDto?> GetDefaultModelAsync(CancellationToken cancellationToken = default)
    {
        var settings = await GetSettingsAsync(cancellationToken);
        if (!string.IsNullOrWhiteSpace(settings.DefaultModelKey))
        {
            return await GetModelAsync(settings.DefaultModelKey, cancellationToken);
        }

        // Fallback: find first active model marked as default
        var models = await GetAllModelsAsync(cancellationToken);
        return models.FirstOrDefault(m => m.IsDefault && m.IsActive);
    }

    /// <summary>
    /// Deletes an LLM configuration by key.
    /// </summary>
    /// <param name="configKey">The configuration key.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task DeleteConfigAsync(string configKey, CancellationToken cancellationToken = default)
    {
        await _configService.DeleteAsync(ConfigType.Llm, configKey, cancellationToken);
    }
}
