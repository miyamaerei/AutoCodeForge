using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Enums;
using AutoCodeForge.Core.Providers;
using AutoCodeForge.Core.Interfaces;
using AutoCodeForge.Infrastructure.Repositories;

namespace AutoCodeForge.Application.Services;

/// <summary>
/// Provides services for initializing default configurations for users and tenants.
/// </summary>
public class ConfigInitializationService
{
    private readonly ConfigRepository _configRepository;
    private readonly ICurrentUser _currentUser;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigInitializationService"/> class.
    /// </summary>
    /// <param name="configRepository">The configuration repository.</param>
    /// <param name="currentUser">The current user provider.</param>
    public ConfigInitializationService(
        ConfigRepository configRepository,
        ICurrentUser currentUser)
    {
        _configRepository = configRepository;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Initializes default configurations for a new user.
    /// </summary>
    /// <param name="ntId">The NTID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The number of initialized configurations.</returns>
    public async Task<int> InitializeUserDefaultsAsync( CancellationToken cancellationToken = default)
    {
        var configTypes = ConfigDefaultsProvider.GetUserConfigTypes();
        return await InitializeConfigsAsync(configTypes, cancellationToken);
    }

    /// <summary>
    /// Initializes global default configurations for a new tenant.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The number of initialized configurations.</returns>
    public async Task<int> InitializeTenantDefaultsAsync(CancellationToken cancellationToken = default)
    {
        var configTypes = ConfigDefaultsProvider.GetGlobalConfigTypes();
        return await InitializeConfigsAsync(configTypes, cancellationToken);
    }

    /// <summary>
    /// Initializes default configurations for a specific module (lazy loading).
    /// </summary>
    /// <param name="ntId">The NTID (null for global).</param>
    /// <param name="configType">The configuration type/module.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if initialization was needed, false if already existed.</returns>
    public async Task<bool> InitializeModuleDefaultsAsync(
        ConfigType configType,
        CancellationToken cancellationToken = default)
    {
        var configKey = ConfigDefaultsProvider.GetDefaultKey(configType);
        var exists = await _configRepository.ExistsAsync(configType, configKey, _currentUser.GetCurrentNtId(), cancellationToken);

        if (exists)
        {
            return false;
        }

        var currentUserNtId = _currentUser.GetCurrentNtId() ?? "unknown";
        var config = new ConfigurationEntry
        {
            Id = Guid.NewGuid(),
            ConfigType = configType,
            ConfigKey = configKey,
            ConfigValue = ConfigDefaultsProvider.GetDefaultValue(configType),
            NtId = _currentUser.GetCurrentNtId(),
            IsEncrypted = false,
            IsEnabled = true,
            Description = ConfigDefaultsProvider.GetDescription(configType),
            Group = configType.ToString(),
            CreatedBy = currentUserNtId,
            UpdatedBy = currentUserNtId
        };

        await _configRepository.CreateAsync(config, cancellationToken);
        return true;
    }

    /// <summary>
    /// Resets a specific module's configuration to default values.
    /// </summary>
    /// <param name="ntId">The NTID (null for global).</param>
    /// <param name="configType">The configuration type/module.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if reset was successful.</returns>
    public async Task<bool> ResetToDefaultsAsync(
        ConfigType configType,
        CancellationToken cancellationToken = default)
    {
        var configKey = ConfigDefaultsProvider.GetDefaultKey(configType);
        var existing = await _configRepository.GetByTypeAndKeyAsync(configType, configKey, _currentUser.GetCurrentNtId(), false, cancellationToken);

        if (existing == null)
        {
            return false;
        }

        var currentUserNtId = _currentUser.GetCurrentNtId() ?? "unknown";
        existing.ConfigValue = ConfigDefaultsProvider.GetDefaultValue(configType);
        existing.IsEncrypted = false;
        existing.IsEnabled = true;
        existing.UpdatedBy = currentUserNtId;

        await _configRepository.UpdateAsync(existing, cancellationToken);
        return true;
    }

    /// <summary>
    /// Gets the default configuration template for a specific type.
    /// </summary>
    /// <param name="configType">The configuration type.</param>
    /// <returns>The default configuration value as JSON string.</returns>
    public string GetConfigTemplate(ConfigType configType)
    {
        return ConfigDefaultsProvider.GetDefaultValue(configType);
    }

    /// <summary>
    /// Gets all available configuration templates.
    /// </summary>
    /// <returns>A dictionary of configuration types and their default values.</returns>
    public Dictionary<ConfigType, string> GetAllTemplates()
    {
        var templates = new Dictionary<ConfigType, string>();

        foreach (ConfigType type in Enum.GetValues(typeof(ConfigType)))
        {
            templates[type] = ConfigDefaultsProvider.GetDefaultValue(type);
        }

        return templates;
    }

    /// <summary>
    /// Checks if a user has all required default configurations.
    /// </summary>
    /// <param name="ntId">The NTID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of missing configuration types.</returns>
    public async Task<List<ConfigType>> GetMissingUserConfigsAsync( CancellationToken cancellationToken = default)
    {
        var requiredTypes = ConfigDefaultsProvider.GetUserConfigTypes();
        var missingTypes = new List<ConfigType>();

        foreach (var type in requiredTypes)
        {
            var configKey = ConfigDefaultsProvider.GetDefaultKey(type);
            var exists = await _configRepository.ExistsAsync(type, configKey, _currentUser.GetCurrentNtId(), cancellationToken);

            if (!exists)
            {
                missingTypes.Add(type);
            }
        }

        return missingTypes;
    }

    private async Task<int> InitializeConfigsAsync(
        List<ConfigType> configTypes,
        CancellationToken cancellationToken)
    {
        var initializedCount = 0;
        var currentUserNtId = _currentUser.GetCurrentNtId() ?? "unknown";

        foreach (var configType in configTypes)
        {
            var configKey = ConfigDefaultsProvider.GetDefaultKey(configType);
            var exists = await _configRepository.ExistsAsync(configType, configKey, _currentUser.GetCurrentNtId(), cancellationToken);

            if (exists)
            {
                continue;
            }

            var config = new ConfigurationEntry
            {
                Id = Guid.NewGuid(),
                ConfigType = configType,
                ConfigKey = configKey,
                ConfigValue = ConfigDefaultsProvider.GetDefaultValue(configType),
                NtId = _currentUser.GetCurrentNtId(),
                IsEncrypted = false,
                IsEnabled = true,
                Description = ConfigDefaultsProvider.GetDescription(configType),
                Group = configType.ToString(),
                CreatedBy = currentUserNtId,
                UpdatedBy = currentUserNtId
            };

            await _configRepository.CreateAsync(config, cancellationToken);
            initializedCount++;
        }

        return initializedCount;
    }
}
