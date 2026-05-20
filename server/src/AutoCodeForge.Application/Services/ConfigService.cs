using AutoCodeForge.Core.DTOs.Config;
using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Enums;
using AutoCodeForge.Core.Exceptions;
using AutoCodeForge.Core.Helpers;
using AutoCodeForge.Core.Interfaces;
using AutoCodeForge.Infrastructure.Repositories;

namespace AutoCodeForge.Application.Services;

/// <summary>
/// Provides unified configuration management services for all configuration types.
/// Supports global and user-specific configurations with encryption and history tracking.
/// </summary>
public class ConfigService
{
    private const string SandboxConfigKey = "user.sandbox.default";

    private readonly ConfigRepository _configRepository;
    private readonly ConfigHistoryRepository _historyRepository;
    private readonly EncryptionService _encryptionService;
    private readonly ICurrentUser _currentUser;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigService"/> class.
    /// </summary>
    /// <param name="configRepository">The configuration repository.</param>
    /// <param name="historyRepository">The configuration history repository.</param>
    /// <param name="encryptionService">The encryption service.</param>
    /// <param name="currentUser">The current user provider.</param>
    public ConfigService(
        ConfigRepository configRepository,
        ConfigHistoryRepository historyRepository,
        EncryptionService encryptionService,
        ICurrentUser currentUser)
    {
        _configRepository = configRepository;
        _historyRepository = historyRepository;
        _encryptionService = encryptionService;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Gets configurations by type.
    /// </summary>
    /// <param name="configType">The configuration type.</param>
    /// <param name="ntId">The NTID for user-specific configs (null for global).</param>
    /// <param name="includeAllUsers">Whether to include configs from all users.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The configuration entries.</returns>
    public async Task<List<ConfigurationEntry>> GetByTypeAsync(
        ConfigType configType,
        bool includeAllUsers = false,
        CancellationToken cancellationToken = default)
    {
        var configs = await _configRepository.GetByTypeAsync(configType, _currentUser.GetCurrentNtId(), includeAllUsers, cancellationToken);

        foreach (var config in configs)
        {
            if (config.IsEncrypted)
            {
                config.ConfigValue = _encryptionService.Decrypt(config.ConfigValue);
            }
        }

        return configs;
    }

    /// <summary>
    /// Gets a single configuration entry by type and key.
    /// </summary>
    /// <param name="configType">The configuration type.</param>
    /// <param name="configKey">The configuration key.</param>
    /// <param name="ntId">The NTID for user-specific configs (null for global).</param>
    /// <param name="includeAllUsers">Whether to include configs from all users.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The configuration entry, or null.</returns>
    public async Task<ConfigurationEntry?> GetByTypeAndKeyAsync(
        ConfigType configType,
        string configKey,
        bool includeAllUsers = false,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(configKey))
        {
            throw new ValidationException("Config key cannot be empty");
        }

        var config = await _configRepository.GetByTypeAndKeyAsync(configType, configKey, _currentUser.GetCurrentNtId(), includeAllUsers, cancellationToken);

        if (config != null && config.IsEncrypted)
        {
            config.ConfigValue = _encryptionService.Decrypt(config.ConfigValue);
        }

        return config;
    }

    /// <summary>
    /// Creates or updates a configuration entry.
    /// </summary>
    /// <param name="configType">The configuration type.</param>
    /// <param name="configKey">The configuration key.</param>
    /// <param name="configValue">The configuration value.</param>
    /// <param name="isEncrypted">Whether the value should be encrypted.</param>
    /// <param name="description">Optional description.</param>
    /// <param name="group">Optional logical group.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created/updated configuration entry.</returns>
    public async Task<ConfigurationEntry> UpsertAsync(
        ConfigType configType,
        string configKey,
        string configValue,
        bool isEncrypted = false,
        string? description = null,
        string? group = null,
        CancellationToken cancellationToken = default)
    {
        ValidateConfigKey(configKey);
        ValidateConfigValue(configValue);

        var ntId = _currentUser.GetCurrentNtId();
        var existing = await _configRepository.GetByTypeAndKeyAsync(configType, configKey, ntId, false, cancellationToken);
        var currentUserNtId = _currentUser.GetCurrentNtId() ?? "unknown";

        string storedValue = isEncrypted ? _encryptionService.Encrypt(configValue) : configValue;

        if (existing is null)
        {
            var newEntry = new ConfigurationEntry
            {
                Id = Guid.NewGuid(),
                ConfigType = configType,
                ConfigKey = configKey,
                ConfigValue = storedValue,
                NtId = ntId,
                IsEncrypted = isEncrypted,
                IsEnabled = true,
                Description = description,
                Group = group,
                CreatedBy = currentUserNtId,
                UpdatedBy = currentUserNtId
            };

            await _configRepository.CreateAsync(newEntry, cancellationToken);
            await LogHistoryAsync(newEntry.Id, configType, configKey, null, storedValue, "Created", currentUserNtId, cancellationToken);

            newEntry.ConfigValue = configValue;
            return newEntry;
        }

        var previousValue = existing.ConfigValue;

        existing.ConfigValue = storedValue;
        existing.IsEncrypted = isEncrypted;
        existing.Description = description;
        existing.Group = group;
        existing.UpdatedBy = currentUserNtId;

        await _configRepository.UpdateAsync(existing, cancellationToken);
        await LogHistoryAsync(existing.Id, configType, configKey, previousValue, storedValue, "Updated", currentUserNtId, cancellationToken);

        existing.ConfigValue = configValue;
        return existing;
    }

    /// <summary>
    /// Deletes a configuration entry by type and key.
    /// </summary>
    /// <param name="configType">The configuration type.</param>
    /// <param name="configKey">The configuration key.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task DeleteAsync(
        ConfigType configType,
        string configKey,
        CancellationToken cancellationToken = default)
    {
        ValidateConfigKey(configKey);

        var ntId = _currentUser.GetCurrentNtId();
        var existing = await _configRepository.GetByTypeAndKeyAsync(configType, configKey, ntId, false, cancellationToken);

        if (existing is null)
        {
            throw new NotFoundException($"Config '{configKey}' not found");
        }

        var currentUserNtId = _currentUser.GetCurrentNtId() ?? "unknown";
        await LogHistoryAsync(existing.Id, configType, configKey, existing.ConfigValue, null, "Deleted", currentUserNtId, cancellationToken);
        await _configRepository.DeleteByTypeAsync(configType, ntId, cancellationToken);
    }

    /// <summary>
    /// Gets all configurations for current user.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The user's configuration entries.</returns>
    public async Task<List<ConfigurationEntry>> GetConfigsByNtIdAsync(CancellationToken cancellationToken = default)
    {
        var ntId = _currentUser.GetCurrentNtId() ?? throw new UnauthorizedException("User not authenticated");
        var configs = await _configRepository.GetByNtIdAsync(ntId, cancellationToken);

        foreach (var config in configs)
        {
            if (config.IsEncrypted)
            {
                config.ConfigValue = _encryptionService.Decrypt(config.ConfigValue);
            }
        }

        return configs;
    }

    /// <summary>
    /// Gets global sandbox config.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The sandbox config; null when not configured.</returns>
    public async Task<SandboxConfigDto?> GetSandboxConfigAsync(CancellationToken cancellationToken = default)
    {
        var config = await GetByTypeAndKeyAsync(ConfigType.Sandbox, SandboxConfigKey, false, cancellationToken);
        if (config is null || string.IsNullOrWhiteSpace(config.ConfigValue))
        {
            return null;
        }

        return JsonHelper.Deserialize<SandboxConfigDto>(config.ConfigValue);
    }

    /// <summary>
    /// Creates or updates sandbox config for current user.
    /// </summary>
    /// <param name="sandboxConfig">The sandbox config payload.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated sandbox config.</returns>
    public async Task<SandboxConfigDto> UpsertSandboxConfigAsync(
        SandboxConfigDto sandboxConfig,
        CancellationToken cancellationToken = default)
    {
        if (sandboxConfig is null)
        {
            throw new ValidationException("Sandbox config cannot be null");
        }

        var payload = JsonHelper.Serialize(sandboxConfig);
        await UpsertAsync(ConfigType.Sandbox, SandboxConfigKey, payload, cancellationToken: cancellationToken);
        return sandboxConfig;
    }

    /// <summary>
    /// Gets all configuration types with their entry counts.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A dictionary of config types and their counts.</returns>
    public async Task<Dictionary<ConfigType, int>> GetTypeCountsAsync(CancellationToken cancellationToken = default)
    {
        var ntId = _currentUser.GetCurrentNtId();
        return await _configRepository.GetTypeCountsAsync(ntId, cancellationToken);
    }

    private void ValidateConfigKey(string configKey)
    {
        if (string.IsNullOrWhiteSpace(configKey))
        {
            throw new ValidationException("Config key cannot be empty");
        }

        if (configKey.Length > 128)
        {
            throw new ValidationException("Config key cannot exceed 128 characters");
        }
    }

    private void ValidateConfigValue(string configValue)
    {
        if (string.IsNullOrWhiteSpace(configValue))
        {
            throw new ValidationException("Config value cannot be empty");
        }
    }

    private async Task LogHistoryAsync(
        Guid configId,
        ConfigType configType,
        string configKey,
        string? previousValue,
        string? newValue,
        string operation,
        string changedBy,
        CancellationToken cancellationToken)
    {
        var historyEntry = new ConfigHistoryEntity
        {
            Id = Guid.NewGuid(),
            ConfigId = configId,
            ConfigType = configType,
            ConfigKey = configKey,
            PreviousValue = previousValue,
            NewValue = newValue ?? string.Empty,
            Operation = operation,
            ChangedBy = changedBy
        };

        await _historyRepository.CreateAsync(historyEntry, cancellationToken);
        await _historyRepository.CleanupOldRecordsAsync(configId, cancellationToken);
    }
}
