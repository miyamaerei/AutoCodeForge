using AutoCodeForge.Application.Services;
using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Enums;
using AutoCodeForge.Core.Exceptions;
using AutoCodeForge.Core.Interfaces;
using AutoCodeForge.Infrastructure.Repositories;
using SqlSugar;

namespace AutoCodeForge.Tests;

public sealed class ConfigServiceTests : IDisposable
{
    private readonly string _dbPath;
    private readonly ISqlSugarClient _db;
    private readonly string _testNtId = "test.user.001";
    private readonly string _testEncryptionKey;
    private readonly EncryptionService _encryptionService;
    private readonly ConfigService _configService;
    private readonly ConfigRepository _configRepository;
    private readonly ConfigHistoryRepository _historyRepository;

    public ConfigServiceTests()
    {
        _dbPath = Path.Combine(Path.GetTempPath(), $"autocodeforge.configservice.{Guid.NewGuid():N}.db");
        _db = new SqlSugarScope(new ConnectionConfig
        {
            ConnectionString = $"Data Source={_dbPath}",
            DbType = DbType.Sqlite,
            IsAutoCloseConnection = true,
            InitKeyType = InitKeyType.Attribute,
        });

        // Initialize tables
        _db.CodeFirst.InitTables<ConfigurationEntry>();
        _db.CodeFirst.InitTables<ConfigHistoryEntity>();

        _testEncryptionKey = EncryptionService.GenerateKey();
        _encryptionService = new EncryptionService(_testEncryptionKey);

        var currentUser = new TestCurrentUser(_testNtId);
        _configRepository = new ConfigRepository(_db, currentUser);
        _historyRepository = new ConfigHistoryRepository(_db, currentUser);
        _configService = new ConfigService(_configRepository, _historyRepository, _encryptionService, currentUser);
    }

    [Fact]
    public async Task UpsertAsync_WithNewConfig_ShouldCreateAndReturnConfig()
    {
        var configKey = "test.config.key001";
        var configValue = "{\"setting\": \"value\"}";
        
        var result = await _configService.UpsertAsync(
            ConfigType.Preferences,
            configKey,
            configValue,
            description: "Test configuration",
            group: "TestGroup");

        Assert.NotNull(result);
        Assert.Equal(configKey, result.ConfigKey);
        Assert.Equal(configValue, result.ConfigValue);
        Assert.Equal(ConfigType.Preferences, result.ConfigType);
        Assert.Equal("Test configuration", result.Description);
        Assert.Equal("TestGroup", result.Group);
        Assert.True(result.IsEnabled);
    }

    [Fact]
    public async Task UpsertAsync_WithExistingConfig_ShouldUpdateAndReturnConfig()
    {
        var configKey = "test.config.key002";
        var initialValue = "{\"setting\": \"initial\"}";
        var updatedValue = "{\"setting\": \"updated\"}";

        await _configService.UpsertAsync(ConfigType.Preferences, configKey, initialValue);
        var result = await _configService.UpsertAsync(ConfigType.Preferences, configKey, updatedValue);

        Assert.NotNull(result);
        Assert.Equal(configKey, result.ConfigKey);
        Assert.Equal(updatedValue, result.ConfigValue);
    }

    [Fact]
    public async Task UpsertAsync_WithEncryptedConfig_ShouldStoreEncryptedValue()
    {
        var configKey = "test.config.key003";
        var configValue = "sensitive_data_123";

        var result = await _configService.UpsertAsync(
            ConfigType.ApiKey,
            configKey,
            configValue,
            isEncrypted: true);

        // Verify decrypted value is correct
        var retrieved = await _configService.GetByTypeAndKeyAsync(ConfigType.ApiKey, configKey);
        Assert.NotNull(retrieved);
        Assert.Equal(configValue, retrieved.ConfigValue);

        // Check in database it's actually encrypted
        var inDb = await _configRepository.GetByTypeAndKeyAsync(ConfigType.ApiKey, configKey, _testNtId);
        Assert.NotNull(inDb);
        Assert.True(inDb.IsEncrypted);
        Assert.NotEqual(configValue, inDb.ConfigValue);
    }

    [Fact]
    public async Task UpsertAsync_WithEmptyKey_ShouldThrowValidationException()
    {
        var exception = await Assert.ThrowsAsync<ValidationException>(() => 
            _configService.UpsertAsync(ConfigType.Preferences, string.Empty, "value"));
        
        Assert.Contains("Config key", exception.Message);
    }

    [Fact]
    public async Task UpsertAsync_WithEmptyValue_ShouldThrowValidationException()
    {
        var exception = await Assert.ThrowsAsync<ValidationException>(() => 
            _configService.UpsertAsync(ConfigType.Preferences, "key", string.Empty));
        
        Assert.Contains("Config value", exception.Message);
    }

    [Fact]
    public async Task GetByTypeAndKeyAsync_WithExistingConfig_ShouldReturnConfig()
    {
        var configKey = "test.config.key004";
        var configValue = "{\"test\": \"data\"}";
        
        await _configService.UpsertAsync(ConfigType.Preferences, configKey, configValue);
        var result = await _configService.GetByTypeAndKeyAsync(ConfigType.Preferences, configKey);

        Assert.NotNull(result);
        Assert.Equal(configKey, result.ConfigKey);
        Assert.Equal(configValue, result.ConfigValue);
    }

    [Fact]
    public async Task GetByTypeAndKeyAsync_WithNonExistingConfig_ShouldReturnNull()
    {
        var result = await _configService.GetByTypeAndKeyAsync(ConfigType.Preferences, "non.existing.key");
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByTypeAsync_WithMultipleConfigs_ShouldReturnAllOfType()
    {
        await _configService.UpsertAsync(ConfigType.Preferences, "pref.key1", "{\"a\":1}");
        await _configService.UpsertAsync(ConfigType.Preferences, "pref.key2", "{\"b\":2}");
        await _configService.UpsertAsync(ConfigType.Sandbox, "sand.key1", "{\"c\":3}");

        var results = await _configService.GetByTypeAsync(ConfigType.Preferences);

        Assert.Equal(2, results.Count);
        Assert.All(results, r => Assert.Equal(ConfigType.Preferences, r.ConfigType));
    }

    [Fact]
    public async Task DeleteAsync_WithExistingConfig_ShouldDeleteConfig()
    {
        var configKey = "test.config.key005";
        await _configService.UpsertAsync(ConfigType.Preferences, configKey, "{\"d\":4}");

        await _configService.DeleteAsync(ConfigType.Preferences, configKey);
        var result = await _configService.GetByTypeAndKeyAsync(ConfigType.Preferences, configKey);

        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistingConfig_ShouldThrowNotFoundException()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => 
            _configService.DeleteAsync(ConfigType.Preferences, "non.existing.key"));
    }

    [Fact]
    public async Task UpsertAsync_ShouldCreateHistoryRecord()
    {
        var configKey = "test.config.key006";
        
        var config = await _configService.UpsertAsync(ConfigType.Preferences, configKey, "{\"e\":5}");
        var historyCount = await _historyRepository.GetCountAsync(config.Id);

        Assert.Equal(1, historyCount);
    }

    [Fact]
    public async Task GetConfigsByNtIdAsync_ShouldReturnUserConfigs()
    {
        await _configService.UpsertAsync(ConfigType.Preferences, "user.pref1", "{\"f\":6}");
        await _configService.UpsertAsync(ConfigType.Sandbox, "user.sand1", "{\"g\":7}");

        var results = await _configService.GetConfigsByNtIdAsync();

        Assert.Equal(2, results.Count);
    }

    [Fact]
    public async Task GetTypeCountsAsync_ShouldReturnCorrectCounts()
    {
        await _configService.UpsertAsync(ConfigType.Preferences, "count.pref1", "{\"h\":8}");
        await _configService.UpsertAsync(ConfigType.Preferences, "count.pref2", "{\"i\":9}");
        await _configService.UpsertAsync(ConfigType.Sandbox, "count.sand1", "{\"j\":10}");

        var counts = await _configService.GetTypeCountsAsync();

        Assert.True(counts.ContainsKey(ConfigType.Preferences));
        Assert.True(counts.ContainsKey(ConfigType.Sandbox));
        Assert.Equal(2, counts[ConfigType.Preferences]);
        Assert.Equal(1, counts[ConfigType.Sandbox]);
    }

    [Fact]
    public async Task UpsertSandboxConfigAsync_ShouldCreateSandboxConfig()
    {
        var sandboxValue = new Core.DTOs.Config.SandboxConfigDto
        {
            WorkspaceRootPath = "/test/path",
            TimeoutSeconds = 1200,
            UserIsolationEnabled = true
        };

        var result = await _configService.UpsertSandboxConfigAsync(sandboxValue);
        var retrieved = await _configService.GetSandboxConfigAsync();

        Assert.NotNull(retrieved);
        Assert.Equal(sandboxValue.WorkspaceRootPath, retrieved.WorkspaceRootPath);
        Assert.Equal(sandboxValue.TimeoutSeconds, retrieved.TimeoutSeconds);
        Assert.Equal(sandboxValue.UserIsolationEnabled, retrieved.UserIsolationEnabled);
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
                // Ignore
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
            return false;
        }
    }
}
