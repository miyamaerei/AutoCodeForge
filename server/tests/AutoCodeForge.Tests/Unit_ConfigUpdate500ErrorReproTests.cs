using AutoCodeForge.Application.Services;
using AutoCodeForge.Core.DTOs.Config;
using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Enums;
using AutoCodeForge.Core.Exceptions;
using AutoCodeForge.Core.Interfaces;
using AutoCodeForge.Infrastructure.Repositories;
using SqlSugar;

namespace AutoCodeForge.Tests;

/// <summary>
/// Tests for reproducing the 500 error when updating config via API
/// </summary>
public sealed class ConfigUpdate500ErrorReproTests : IDisposable
{
    private readonly string _dbPath;
    private readonly ISqlSugarClient _db;
    private readonly string _testNtId = "Administrator";
    private readonly string _testEncryptionKey;
    private readonly EncryptionService _encryptionService;

    public ConfigUpdate500ErrorReproTests()
    {
        _dbPath = Path.Combine(Path.GetTempPath(), $"autocodeforge.500error.{Guid.NewGuid():N}.db");
        _db = new SqlSugarScope(new ConnectionConfig
        {
            ConnectionString = $"Data Source={_dbPath}",
            DbType = DbType.Sqlite,
            IsAutoCloseConnection = true,
            InitKeyType = InitKeyType.Attribute,
        });

        _db.CodeFirst.InitTables<ConfigurationEntry>();
        _db.CodeFirst.InitTables<ConfigHistoryEntity>();

        _testEncryptionKey = EncryptionService.GenerateKey();
        _encryptionService = new EncryptionService(_testEncryptionKey);
    }

    [Fact]
    public async Task UpsertAsync_WithAdministratorUser_ShouldSucceed()
    {
        // Arrange
        var currentUser = new TestCurrentUser(_testNtId);
        var configRepo = new ConfigRepository(_db, currentUser);
        var historyRepo = new ConfigHistoryRepository(_db, currentUser);
        var configService = new ConfigService(configRepo, historyRepo, _encryptionService, currentUser);

        // Act - This is the exact same operation as the API endpoint
        var result = await configService.UpsertAsync(
            ConfigType.Preferences,
            "locale",
            "zh-CN",
            isEncrypted: false,
            description: null,
            group: null);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("locale", result.ConfigKey);
        Assert.Equal("zh-CN", result.ConfigValue);
        Assert.Equal(ConfigType.Preferences, result.ConfigType);
    }

    [Fact]
    public async Task UpsertAsync_WithNonExistentNtId_ShouldSucceed()
    {
        // Arrange - User doesn't exist in the database, but we're still creating a config for them
        var currentUser = new TestCurrentUser("NonExistent.User");
        var configRepo = new ConfigRepository(_db, currentUser);
        var historyRepo = new ConfigHistoryRepository(_db, currentUser);
        var configService = new ConfigService(configRepo, historyRepo, _encryptionService, currentUser);

        // Act
        var result = await configService.UpsertAsync(
            ConfigType.Preferences,
            "test.key",
            "{\"test\": true}",
            isEncrypted: false);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("test.key", result.ConfigKey);
    }

    [Fact]
    public async Task UpsertAsync_WithEncryptedValue_ShouldEncryptAndDecrypt()
    {
        // Arrange
        var currentUser = new TestCurrentUser(_testNtId);
        var configRepo = new ConfigRepository(_db, currentUser);
        var historyRepo = new ConfigHistoryRepository(_db, currentUser);
        var configService = new ConfigService(configRepo, historyRepo, _encryptionService, currentUser);

        var sensitiveValue = "my-secret-password-123";

        // Act
        var result = await configService.UpsertAsync(
            ConfigType.Preferences,
            "password",
            sensitiveValue,
            isEncrypted: true);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(sensitiveValue, result.ConfigValue);
        Assert.True(result.IsEncrypted);

        // Verify it's stored encrypted
        var storedConfig = await configRepo.GetByTypeAndKeyAsync(
            ConfigType.Preferences,
            "password",
            _testNtId,
            false);

        Assert.NotNull(storedConfig);
        Assert.NotEqual(sensitiveValue, storedConfig.ConfigValue);
        Assert.NotEqual(result.ConfigValue, storedConfig.ConfigValue);
    }

    [Fact]
    public async Task GetByTypeAsync_AfterUpsert_ShouldReturnDecryptedValues()
    {
        // Arrange
        var currentUser = new TestCurrentUser(_testNtId);
        var configRepo = new ConfigRepository(_db, currentUser);
        var historyRepo = new ConfigHistoryRepository(_db, currentUser);
        var configService = new ConfigService(configRepo, historyRepo, _encryptionService, currentUser);

        await configService.UpsertAsync(ConfigType.Preferences, "test1", "value1");
        await configService.UpsertAsync(ConfigType.Preferences, "test2", "value2");

        // Act
        var configs = await configService.GetByTypeAsync(ConfigType.Preferences);

        // Assert
        Assert.NotEmpty(configs);
        Assert.Equal(2, configs.Count);
        Assert.Contains(configs, c => c.ConfigKey == "test1" && c.ConfigValue == "value1");
        Assert.Contains(configs, c => c.ConfigKey == "test2" && c.ConfigValue == "value2");
    }

    [Fact]
    public async Task UpsertAsync_WithEmptyKey_ShouldThrowValidationException()
    {
        // Arrange
        var currentUser = new TestCurrentUser(_testNtId);
        var configRepo = new ConfigRepository(_db, currentUser);
        var historyRepo = new ConfigHistoryRepository(_db, currentUser);
        var configService = new ConfigService(configRepo, historyRepo, _encryptionService, currentUser);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() =>
            configService.UpsertAsync(ConfigType.Preferences, "", "value"));
    }

    [Fact]
    public async Task UpsertAsync_WithEmptyValue_ShouldThrowValidationException()
    {
        // Arrange
        var currentUser = new TestCurrentUser(_testNtId);
        var configRepo = new ConfigRepository(_db, currentUser);
        var historyRepo = new ConfigHistoryRepository(_db, currentUser);
        var configService = new ConfigService(configRepo, historyRepo, _encryptionService, currentUser);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() =>
            configService.UpsertAsync(ConfigType.Preferences, "key", ""));
    }

    [Fact]
    public async Task UpdateExistingConfig_ShouldUpdateAndLogHistory()
    {
        // Arrange
        var currentUser = new TestCurrentUser(_testNtId);
        var configRepo = new ConfigRepository(_db, currentUser);
        var historyRepo = new ConfigHistoryRepository(_db, currentUser);
        var configService = new ConfigService(configRepo, historyRepo, _encryptionService, currentUser);

        await configService.UpsertAsync(ConfigType.Preferences, "updatable", "original");

        // Act
        var updated = await configService.UpsertAsync(ConfigType.Preferences, "updatable", "modified");

        // Assert
        Assert.NotNull(updated);
        Assert.Equal("modified", updated.ConfigValue);

        var history = await historyRepo.GetByConfigIdAsync(updated.Id);
        Assert.NotEmpty(history);
        Assert.Contains(history, h => h.Operation == "Updated");
    }

    public void Dispose()
    {
        if (_db is IDisposable disposable)
        {
            disposable.Dispose();
        }

        if (File.Exists(_dbPath))
        {
            try
            {
                File.Delete(_dbPath);
            }
            catch (IOException)
            {
            }
        }
    }

    private sealed class TestCurrentUser : ICurrentUser
    {
        private readonly string? _ntId;

        public TestCurrentUser(string? ntId)
        {
            _ntId = ntId;
        }

        public string? GetCurrentNtId()
        {
            return _ntId;
        }

        public bool IsAdmin()
        {
            return _ntId == "admin";
        }
    }
}
