using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Enums;
using AutoCodeForge.Core.Exceptions;
using AutoCodeForge.Infrastructure.Repositories;

namespace AutoCodeForge.Application.Services;

/// <summary>
/// Provides services for managing configuration history records and rollback operations.
/// </summary>
public class ConfigHistoryService
{
    private readonly ConfigHistoryRepository _historyRepository;
    private readonly ConfigRepository _configRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigHistoryService"/> class.
    /// </summary>
    /// <param name="historyRepository">The configuration history repository.</param>
    /// <param name="configRepository">The configuration repository.</param>
    public ConfigHistoryService(
        ConfigHistoryRepository historyRepository,
        ConfigRepository configRepository)
    {
        _historyRepository = historyRepository;
        _configRepository = configRepository;
    }

    /// <summary>
    /// Gets history records for a specific configuration entry.
    /// </summary>
    /// <param name="configId">The configuration entry ID.</param>
    /// <param name="page">The page number.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The paginated history records.</returns>
    public async Task<List<ConfigHistoryEntity>> GetByConfigIdAsync(
        Guid configId,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        return await _historyRepository.GetByConfigIdAsync(configId, page, pageSize, cancellationToken);
    }

    /// <summary>
    /// Gets history records for configurations of a specific type.
    /// </summary>
    /// <param name="configType">The configuration type.</param>
    /// <param name="page">The page number.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The paginated history records.</returns>
    public async Task<List<ConfigHistoryEntity>> GetByConfigTypeAsync(
        ConfigType configType,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        return await _historyRepository.GetByConfigTypeAsync(configType, page, pageSize, cancellationToken);
    }

    /// <summary>
    /// Gets all history records for a specific user.
    /// </summary>
    /// <param name="changedBy">The user identifier.</param>
    /// <param name="page">The page number.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The paginated history records.</returns>
    public async Task<List<ConfigHistoryEntity>> GetByChangedByAsync(
        string changedBy,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        return await _historyRepository.GetByChangedByAsync(changedBy, page, pageSize, cancellationToken);
    }

    /// <summary>
    /// Rolls back a configuration to a previous version.
    /// </summary>
    /// <param name="historyId">The history record ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The rolled-back configuration entry.</returns>
    public async Task<ConfigurationEntry> RollbackAsync(Guid historyId, CancellationToken cancellationToken = default)
    {
        var historyEntry = await _historyRepository.GetByIdAsync(historyId, true, cancellationToken);
        if (historyEntry is null)
        {
            throw new NotFoundException("History record not found");
        }

        var configEntry = await _configRepository.GetByIdAsync(historyEntry.ConfigId, true, cancellationToken);
        if (configEntry is null)
        {
            throw new NotFoundException("Configuration entry not found");
        }

        var previousValue = configEntry.ConfigValue;
        configEntry.ConfigValue = historyEntry.PreviousValue ?? string.Empty;
        configEntry.UpdatedAtUtc = DateTime.UtcNow;

        await _configRepository.UpdateAsync(configEntry, cancellationToken);

        var newHistoryEntry = new ConfigHistoryEntity
        {
            Id = Guid.NewGuid(),
            ConfigId = configEntry.Id,
            ConfigType = configEntry.ConfigType,
            ConfigKey = configEntry.ConfigKey,
            PreviousValue = previousValue,
            NewValue = configEntry.ConfigValue,
            Operation = "Rollback",
            ChangedBy = historyEntry.ChangedBy
        };

        await _historyRepository.CreateAsync(newHistoryEntry, cancellationToken);
        return configEntry;
    }

    /// <summary>
    /// Gets the latest history record for a configuration entry.
    /// </summary>
    /// <param name="configId">The configuration entry ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The latest history record, or null.</returns>
    public async Task<ConfigHistoryEntity?> GetLatestAsync(Guid configId, CancellationToken cancellationToken = default)
    {
        return await _historyRepository.GetLatestAsync(configId, cancellationToken);
    }

    /// <summary>
    /// Gets the count of history records for a configuration entry.
    /// </summary>
    /// <param name="configId">The configuration entry ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The count of history records.</returns>
    public async Task<int> GetCountAsync(Guid configId, CancellationToken cancellationToken = default)
    {
        return await _historyRepository.GetCountAsync(configId, cancellationToken);
    }
}