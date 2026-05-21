using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Enums;
using AutoCodeForge.Core.Helpers;
using AutoCodeForge.Core.Interfaces;
using AutoCodeForge.Infrastructure.Repositories;

namespace AutoCodeForge.Application.Services;

/// <summary>
/// Provides configuration import and export services.
/// </summary>
public class ConfigExportService
{
    private readonly ConfigRepository _configRepository;
    private readonly EncryptionService _encryptionService;
    private readonly ICurrentUser _currentUser;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigExportService"/> class.
    /// </summary>
    /// <param name="configRepository">The configuration repository.</param>
    /// <param name="encryptionService">The encryption service.</param>
    /// <param name="currentUser">The current user provider.</param>
    public ConfigExportService(
        ConfigRepository configRepository,
        EncryptionService encryptionService,
        ICurrentUser currentUser)
    {
        _configRepository = configRepository;
        _encryptionService = encryptionService;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Exports configurations of a specific type to JSON.
    /// </summary>
    /// <param name="configType">The configuration type to export.</param>
    /// <param name="ntId">The NTID for user-specific configs (null for global).</param>
    /// <param name="includeAllUsers">Whether to include configs from all users.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The JSON string containing the exported configurations.</returns>
    public async Task<string> ExportByTypeAsync(
        ConfigType configType,
        string? ntId = null,
        bool includeAllUsers = false,
        CancellationToken cancellationToken = default)
    {
        var configs = await _configRepository.GetByTypeAsync(configType, ntId, includeAllUsers, cancellationToken);

        var exportData = configs.Select(c => new
        {
            c.Id,
            c.ConfigType,
            c.ConfigKey,
            ConfigValue = c.IsEncrypted ? _encryptionService.Decrypt(c.ConfigValue) : c.ConfigValue,
            c.NtId,
            c.IsEncrypted,
            c.IsEnabled,
            c.Description,
            c.Group,
            c.CreatedAtUtc,
            c.UpdatedAtUtc,
            c.CreatedBy,
            c.UpdatedBy
        }).ToList();

        return JsonHelper.Serialize(new { ConfigType = configType, Configs = exportData, ExportedAt = DateTime.UtcNow });
    }

    /// <summary>
    /// Exports all configurations for a specific user by NTID.
    /// </summary>
    /// <param name="ntId">The NTID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The JSON string containing all user configurations.</returns>
    public async Task<string> ExportUserConfigsAsync(string ntId, CancellationToken cancellationToken = default)
    {
        var configs = await _configRepository.GetByNtIdAsync(ntId, cancellationToken);

        var exportData = configs.Select(c => new
        {
            c.Id,
            c.ConfigType,
            c.ConfigKey,
            ConfigValue = c.IsEncrypted ? _encryptionService.Decrypt(c.ConfigValue) : c.ConfigValue,
            c.NtId,
            c.IsEncrypted,
            c.IsEnabled,
            c.Description,
            c.Group,
            c.CreatedAtUtc,
            c.UpdatedAtUtc,
            c.CreatedBy,
            c.UpdatedBy
        }).ToList();

        return JsonHelper.Serialize(new { NtId = ntId, Configs = exportData, ExportedAt = DateTime.UtcNow });
    }

    /// <summary>
    /// Exports all global configurations (admin-only).
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The JSON string containing all global configurations.</returns>
    public async Task<string> ExportGlobalConfigsAsync(CancellationToken cancellationToken = default)
    {
        var configs = await _configRepository.GetByTypeAsync(ConfigType.Global, null, true, cancellationToken);

        var exportData = configs.Select(c => new
        {
            c.Id,
            c.ConfigType,
            c.ConfigKey,
            ConfigValue = c.IsEncrypted ? _encryptionService.Decrypt(c.ConfigValue) : c.ConfigValue,
            c.NtId,
            c.IsEncrypted,
            c.IsEnabled,
            c.Description,
            c.Group,
            c.CreatedAtUtc,
            c.UpdatedAtUtc,
            c.CreatedBy,
            c.UpdatedBy
        }).ToList();

        return JsonHelper.Serialize(new { ConfigType = "Global", Configs = exportData, ExportedAt = DateTime.UtcNow });
    }

    /// <summary>
    /// Imports configurations from JSON data.
    /// </summary>
    /// <param name="jsonData">The JSON data containing configurations.</param>
    /// <param name="ntId">The NTID for user-specific configs (null for global).</param>
    /// <param name="overwriteExisting">Whether to overwrite existing configurations.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The number of imported configurations.</returns>
    public async Task<int> ImportAsync(
        string jsonData,
        string? ntId = null,
        bool overwriteExisting = false,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(jsonData))
        {
            throw new ArgumentException("JSON data cannot be null or empty", nameof(jsonData));
        }

        var importData = JsonHelper.Deserialize<ImportData>(jsonData);
        if (importData?.Configs == null)
        {
            throw new ArgumentException("Invalid import data format", nameof(jsonData));
        }

        var currentUserNtId = _currentUser.GetCurrentNtId() ?? "unknown";
        var importedCount = 0;

        foreach (var config in importData.Configs)
        {
            var existing = await _configRepository.GetByTypeAndKeyAsync(
                config.ConfigType,
                config.ConfigKey,
                ntId,
                false,
                cancellationToken);

            if (existing != null && !overwriteExisting)
            {
                continue;
            }

            string storedValue = config.IsEncrypted ? _encryptionService.Encrypt(config.ConfigValue) : config.ConfigValue;

            if (existing != null)
            {
                existing.ConfigValue = storedValue;
                existing.IsEncrypted = config.IsEncrypted;
                existing.IsEnabled = config.IsEnabled;
                existing.Description = config.Description;
                existing.Group = config.Group;
                existing.UpdatedBy = currentUserNtId;
                await _configRepository.UpdateAsync(existing, cancellationToken);
            }
            else
            {
                var newEntry = new ConfigurationEntry
                {
                    Id = Guid.NewGuid(),
                    ConfigType = config.ConfigType,
                    ConfigKey = config.ConfigKey,
                    ConfigValue = storedValue,
                    NtId = ntId,
                    IsEncrypted = config.IsEncrypted,
                    IsEnabled = config.IsEnabled,
                    Description = config.Description,
                    Group = config.Group,
                    CreatedBy = currentUserNtId,
                    UpdatedBy = currentUserNtId
                };
                await _configRepository.CreateAsync(newEntry, cancellationToken);
            }

            importedCount++;
        }

        return importedCount;
    }

    /// <summary>
    /// Batch updates configurations.
    /// </summary>
    /// <param name="configs">The configurations to update.</param>
    /// <param name="ntId">The NTID for user-specific configs (null for global).</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The number of updated configurations.</returns>
    public async Task<int> BatchUpdateAsync(
        List<ConfigurationEntry> configs,
        string? ntId = null,
        CancellationToken cancellationToken = default)
    {
        if (configs == null || configs.Count == 0)
        {
            return 0;
        }

        var updatedCount = 0;

        foreach (var config in configs)
        {
            var existing = await _configRepository.GetByTypeAndKeyAsync(
                config.ConfigType,
                config.ConfigKey,
                ntId,
                false,
                cancellationToken);

            if (existing != null)
            {
                existing.ConfigValue = config.IsEncrypted
                    ? _encryptionService.Encrypt(config.ConfigValue)
                    : config.ConfigValue;
                existing.IsEncrypted = config.IsEncrypted;
                existing.IsEnabled = config.IsEnabled;
                existing.Description = config.Description;
                existing.Group = config.Group;
                await _configRepository.UpdateAsync(existing, cancellationToken);
                updatedCount++;
            }
        }

        return updatedCount;
    }

    private class ImportData
    {
        public object? ConfigType { get; set; }
        public string? NtId { get; set; }
        public List<ImportConfigItem>? Configs { get; set; }
        public DateTime? ExportedAt { get; set; }
    }

    private class ImportConfigItem
    {
        public Guid Id { get; set; }
        public ConfigType ConfigType { get; set; }
        public string ConfigKey { get; set; } = string.Empty;
        public string ConfigValue { get; set; } = string.Empty;
        public string? NtId { get; set; }
        public bool IsEncrypted { get; set; }
        public bool IsEnabled { get; set; } = true;
        public string? Description { get; set; }
        public string? Group { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
