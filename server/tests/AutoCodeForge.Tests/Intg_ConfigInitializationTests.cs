using AutoCodeForge.Application.Services;
using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Enums;
using AutoCodeForge.Core.Exceptions;
using AutoCodeForge.Core.Interfaces;
using AutoCodeForge.Infrastructure.Repositories;
using SqlSugar;

namespace AutoCodeForge.Tests;

public sealed class ConfigInitializationIntegrationTests : IDisposable
{
    private readonly string _dbPath;
    private readonly ISqlSugarClient _db;
    private readonly string _testNtId = "test.user.new001";
    private readonly string _adminNtId = "admin.user";
    private readonly string _testEncryptionKey;
    private readonly EncryptionService _encryptionService;

    public ConfigInitializationIntegrationTests()
    {
        _dbPath = Path.Combine(Path.GetTempPath(), $"autocodeforge.init.{Guid.NewGuid():N}.db");
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
    public async Task InitializeUserDefaultsForNtIdAsync_WithNewUser_ShouldCreateAllUserDefaults()
    {
        var currentUser = new TestCurrentUser(_testNtId);
        var configRepo = new ConfigRepository(_db, currentUser);
        var historyRepo = new ConfigHistoryRepository(_db, currentUser);
        var initService = new ConfigInitializationService(configRepo, currentUser);

        var count = await initService.InitializeUserDefaultsForNtIdAsync(_testNtId);

        Assert.True(count > 0);

        var configs = await configRepo.GetByNtIdAsync(_testNtId);
        Assert.NotEmpty(configs);
        Assert.All(configs, c => Assert.Equal(_testNtId, c.NtId));
    }

    [Fact]
    public async Task InitializeUserDefaultsForNtIdAsync_WithExistingUser_ShouldSkipExisting()
    {
        var currentUser = new TestCurrentUser(_testNtId);
        var configRepo = new ConfigRepository(_db, currentUser);
        var initService = new ConfigInitializationService(configRepo, currentUser);

        var count1 = await initService.InitializeUserDefaultsForNtIdAsync(_testNtId);
        var count2 = await initService.InitializeUserDefaultsForNtIdAsync(_testNtId);

        Assert.True(count1 > 0);
        Assert.Equal(0, count2);
    }

    [Fact]
    public async Task InitializeUserDefaultsForNtIdAsync_WithDifferentUsers_ShouldCreateSeparateConfigs()
    {
        var user1 = new TestCurrentUser("user1");
        var user2 = new TestCurrentUser("user2");
        var configRepo1 = new ConfigRepository(_db, user1);
        var configRepo2 = new ConfigRepository(_db, user2);
        var initService1 = new ConfigInitializationService(configRepo1, user1);
        var initService2 = new ConfigInitializationService(configRepo2, user2);

        await initService1.InitializeUserDefaultsForNtIdAsync("user1");
        await initService2.InitializeUserDefaultsForNtIdAsync("user2");

        var configs1 = await configRepo1.GetByNtIdAsync("user1");
        var configs2 = await configRepo2.GetByNtIdAsync("user2");

        Assert.NotEmpty(configs1);
        Assert.NotEmpty(configs2);
        Assert.Equal("user1", configs1.First().NtId);
        Assert.Equal("user2", configs2.First().NtId);
    }

    [Fact]
    public async Task InitializeUserDefaultsForNtIdAsync_ShouldSetCorrectCreatedBy()
    {
        var currentUser = new TestCurrentUser(_testNtId);
        var configRepo = new ConfigRepository(_db, currentUser);
        var initService = new ConfigInitializationService(configRepo, currentUser);

        await initService.InitializeUserDefaultsForNtIdAsync(_testNtId);

        var configs = await configRepo.GetByNtIdAsync(_testNtId);
        Assert.NotEmpty(configs);
        Assert.All(configs, c => Assert.Equal(_testNtId, c.CreatedBy));
    }

    [Fact]
    public async Task InitializeTenantDefaultsAsync_WithNoGlobalConfigs_ShouldCreateGlobalDefaults()
    {
        var adminUser = new TestCurrentUser(_adminNtId);
        var configRepo = new ConfigRepository(_db, adminUser);
        var initService = new ConfigInitializationService(configRepo, adminUser);

        var count = await initService.InitializeTenantDefaultsAsync();

        Assert.True(count > 0);
    }

    [Fact]
    public async Task InitializeTenantDefaultsAsync_WithExistingGlobalConfigs_ShouldSkipExisting()
    {
        var adminUser = new TestCurrentUser(_adminNtId);
        var configRepo = new ConfigRepository(_db, adminUser);
        var initService = new ConfigInitializationService(configRepo, adminUser);

        var count1 = await initService.InitializeTenantDefaultsAsync();
        var count2 = await initService.InitializeTenantDefaultsAsync();

        Assert.True(count1 > 0);
        Assert.Equal(0, count2);
    }

    [Fact]
    public async Task ResetToDefaultsAsync_WithExistingConfig_ShouldResetToDefault()
    {
        var currentUser = new TestCurrentUser(_testNtId);
        var configRepo = new ConfigRepository(_db, currentUser);
        var initService = new ConfigInitializationService(configRepo, currentUser);

        await initService.InitializeModuleDefaultsAsync(ConfigType.Preferences);

        var configs = await configRepo.GetByTypeAsync(ConfigType.Preferences, _testNtId);
        var config = configs.First();
        var originalValue = config.ConfigValue;

        config.ConfigValue = "{\"modified\": true}";
        await configRepo.UpdateAsync(config);

        var result = await initService.ResetToDefaultsAsync(ConfigType.Preferences);

        Assert.True(result);

        var resetConfig = await configRepo.GetByTypeAndKeyAsync(
            ConfigType.Preferences,
            config.ConfigKey,
            _testNtId);

        Assert.NotNull(resetConfig);
        Assert.Equal(originalValue, resetConfig.ConfigValue);
    }

    [Fact]
    public async Task ResetToDefaultsAsync_WithNonExistingConfig_ShouldReturnFalse()
    {
        var currentUser = new TestCurrentUser(_testNtId);
        var configRepo = new ConfigRepository(_db, currentUser);
        var initService = new ConfigInitializationService(configRepo, currentUser);

        var result = await initService.ResetToDefaultsAsync(ConfigType.Preferences);

        Assert.False(result);
    }

    [Fact]
    public async Task GetMissingUserConfigsAsync_WithFullInitialization_ShouldReturnEmpty()
    {
        var currentUser = new TestCurrentUser(_testNtId);
        var configRepo = new ConfigRepository(_db, currentUser);
        var initService = new ConfigInitializationService(configRepo, currentUser);

        await initService.InitializeUserDefaultsAsync();

        var missing = await initService.GetMissingUserConfigsAsync();

        Assert.Empty(missing);
    }

    [Fact]
    public async Task GetMissingUserConfigsAsync_WithPartialInitialization_ShouldReturnMissingTypes()
    {
        var currentUser = new TestCurrentUser(_testNtId);
        var configRepo = new ConfigRepository(_db, currentUser);
        var initService = new ConfigInitializationService(configRepo, currentUser);

        await initService.InitializeModuleDefaultsAsync(ConfigType.Preferences);

        var missing = await initService.GetMissingUserConfigsAsync();

        Assert.NotEmpty(missing);
        Assert.DoesNotContain(ConfigType.Preferences, missing);
    }

    [Fact]
    public async Task InitializeModuleDefaultsAsync_WithNoExistingConfig_ShouldReturnTrue()
    {
        var currentUser = new TestCurrentUser(_testNtId);
        var configRepo = new ConfigRepository(_db, currentUser);
        var initService = new ConfigInitializationService(configRepo, currentUser);

        var result = await initService.InitializeModuleDefaultsAsync(ConfigType.Preferences);

        Assert.True(result);
    }

    [Fact]
    public async Task InitializeModuleDefaultsAsync_WithExistingConfig_ShouldReturnFalse()
    {
        var currentUser = new TestCurrentUser(_testNtId);
        var configRepo = new ConfigRepository(_db, currentUser);
        var initService = new ConfigInitializationService(configRepo, currentUser);

        await initService.InitializeModuleDefaultsAsync(ConfigType.Preferences);
        var result = await initService.InitializeModuleDefaultsAsync(ConfigType.Preferences);

        Assert.False(result);
    }

    [Fact]
    public void GetAllTemplates_ShouldReturnAllConfigTypes()
    {
        var currentUser = new TestCurrentUser(_testNtId);
        var configRepo = new ConfigRepository(_db, currentUser);
        var initService = new ConfigInitializationService(configRepo, currentUser);

        var templates = initService.GetAllTemplates();

        Assert.NotEmpty(templates);
        Assert.True(templates.Count >= Enum.GetValues(typeof(ConfigType)).Length);
    }

    [Fact]
    public void GetConfigTemplate_ShouldReturnValidJson()
    {
        var currentUser = new TestCurrentUser(_testNtId);
        var configRepo = new ConfigRepository(_db, currentUser);
        var initService = new ConfigInitializationService(configRepo, currentUser);

        var template = initService.GetConfigTemplate(ConfigType.Preferences);

        Assert.False(string.IsNullOrWhiteSpace(template));
        Assert.Contains("{", template);
        Assert.Contains("}", template);
    }

    [Fact]
    public async Task FirstUserLoginScenario_WithNewUser_ShouldInitializeAllConfigs()
    {
        var newUserNtId = "brand.new.user";
        var newUser = new TestCurrentUser(newUserNtId);
        var configRepo = new ConfigRepository(_db, newUser);
        var historyRepo = new ConfigHistoryRepository(_db, newUser);
        var initService = new ConfigInitializationService(configRepo, newUser);
        var configService = new ConfigService(configRepo, historyRepo, _encryptionService, newUser);

        var missingBefore = await initService.GetMissingUserConfigsAsync();

        Assert.NotEmpty(missingBefore);

        var initializedCount = await initService.InitializeUserDefaultsForNtIdAsync(newUserNtId);

        Assert.True(initializedCount > 0);

        var missingAfter = await initService.GetMissingUserConfigsAsync();
        Assert.Empty(missingAfter);

        var userConfigs = await configService.GetConfigsByNtIdAsync();
        Assert.NotEmpty(userConfigs);
    }

    [Fact]
    public async Task AdminResetUserConfig_ShouldResetToDefault()
    {
        var userNtId = "reset.test.user";
        var user = new TestCurrentUser(userNtId);
        var userConfigRepo = new ConfigRepository(_db, user);
        var userInitService = new ConfigInitializationService(userConfigRepo, user);

        await userInitService.InitializeUserDefaultsForNtIdAsync(userNtId);

        var configs = await userConfigRepo.GetByNtIdAsync(userNtId);
        var targetConfig = configs.First();

        targetConfig.ConfigValue = "{\"hacked\": true}";
        await userConfigRepo.UpdateAsync(targetConfig);

        await userInitService.ResetToDefaultsAsync(targetConfig.ConfigType);

        var resetConfig = await userConfigRepo.GetByTypeAndKeyAsync(
            targetConfig.ConfigType,
            targetConfig.ConfigKey,
            userNtId);

        Assert.NotNull(resetConfig);
        Assert.DoesNotContain("hacked", resetConfig.ConfigValue);
    }

    [Fact]
    public async Task GetConfigsByNtIdAsync_WithNullNtId_ShouldThrowUnauthorizedException()
    {
        var nullUser = new TestCurrentUser(null);
        var configRepo = new ConfigRepository(_db, nullUser);
        var historyRepo = new ConfigHistoryRepository(_db, nullUser);
        var configService = new ConfigService(configRepo, historyRepo, _encryptionService, nullUser);

        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            configService.GetConfigsByNtIdAsync());
    }

    [Fact(Skip = "Global configs with null NtId are not supported due to database constraints")]
    public async Task ConfigService_GetByTypeAsync_WithNullUser_ShouldWorkForGlobalConfigs()
    {
        // This test is skipped because the current database schema has NOT NULL constraint on NtId
        // In a real scenario, global configs would need special handling
    }

    [Fact(Skip = "Global configs with null NtId are not supported due to database constraints")]
    public async Task UpsertAsync_WithNullUser_ShouldCreateGlobalConfig()
    {
        // This test is skipped because the current database schema has NOT NULL constraint on NtId
    }

    [Fact(Skip = "Global configs with null NtId are not supported due to database constraints")]
    public async Task DeleteAsync_WithNullUser_ShouldDeleteGlobalConfig()
    {
        // This test is skipped because the current database schema has NOT NULL constraint on NtId
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistingConfig_ShouldThrowNotFoundException()
    {
        var currentUser = new TestCurrentUser(_testNtId);
        var configRepo = new ConfigRepository(_db, currentUser);
        var historyRepo = new ConfigHistoryRepository(_db, currentUser);
        var configService = new ConfigService(configRepo, historyRepo, _encryptionService, currentUser);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            configService.DeleteAsync(ConfigType.Preferences, "non.existing.key"));
    }

    [Fact]
    public async Task GetTypeCountsAsync_ShouldReturnCorrectCounts()
    {
        var currentUser = new TestCurrentUser(_testNtId);
        var configRepo = new ConfigRepository(_db, currentUser);
        var historyRepo = new ConfigHistoryRepository(_db, currentUser);
        var configService = new ConfigService(configRepo, historyRepo, _encryptionService, currentUser);

        await configService.UpsertAsync(ConfigType.Preferences, "count.test1", "{\"c\":1}");
        await configService.UpsertAsync(ConfigType.Preferences, "count.test2", "{\"c\":2}");
        await configService.UpsertAsync(ConfigType.Sandbox, "count.sandbox", "{\"s\":1}");

        var counts = await configService.GetTypeCountsAsync();

        Assert.True(counts.ContainsKey(ConfigType.Preferences));
        Assert.True(counts.ContainsKey(ConfigType.Sandbox));
        Assert.Equal(2, counts[ConfigType.Preferences]);
        Assert.Equal(1, counts[ConfigType.Sandbox]);
    }

    [Fact]
    public async Task GetSandboxConfigAsync_WithExistingConfig_ShouldReturnConfig()
    {
        var currentUser = new TestCurrentUser(_testNtId);
        var configRepo = new ConfigRepository(_db, currentUser);
        var historyRepo = new ConfigHistoryRepository(_db, currentUser);
        var configService = new ConfigService(configRepo, historyRepo, _encryptionService, currentUser);

        var sandboxConfig = new Core.DTOs.Config.SandboxConfigDto
        {
            WorkspaceRootPath = "/test/path",
            TimeoutSeconds = 1800
        };

        await configService.UpsertSandboxConfigAsync(sandboxConfig);

        var retrieved = await configService.GetSandboxConfigAsync();

        Assert.NotNull(retrieved);
        Assert.Equal("/test/path", retrieved.WorkspaceRootPath);
        Assert.Equal(1800, retrieved.TimeoutSeconds);
    }

    [Fact]
    public async Task GetSandboxConfigAsync_WithNoConfig_ShouldReturnNull()
    {
        var currentUser = new TestCurrentUser(_testNtId);
        var configRepo = new ConfigRepository(_db, currentUser);
        var historyRepo = new ConfigHistoryRepository(_db, currentUser);
        var configService = new ConfigService(configRepo, historyRepo, _encryptionService, currentUser);

        var result = await configService.GetSandboxConfigAsync();

        Assert.Null(result);
    }

    [Fact]
    public async Task UpsertSandboxConfigAsync_WithNullConfig_ShouldThrowValidationException()
    {
        var currentUser = new TestCurrentUser(_testNtId);
        var configRepo = new ConfigRepository(_db, currentUser);
        var historyRepo = new ConfigHistoryRepository(_db, currentUser);
        var configService = new ConfigService(configRepo, historyRepo, _encryptionService, currentUser);

        await Assert.ThrowsAsync<ValidationException>(() =>
            configService.UpsertSandboxConfigAsync(null!));
    }

    [Fact]
    public async Task Validation_WithEmptyConfigKey_ShouldThrowValidationException()
    {
        var currentUser = new TestCurrentUser(_testNtId);
        var configRepo = new ConfigRepository(_db, currentUser);
        var historyRepo = new ConfigHistoryRepository(_db, currentUser);
        var configService = new ConfigService(configRepo, historyRepo, _encryptionService, currentUser);

        await Assert.ThrowsAsync<ValidationException>(() =>
            configService.UpsertAsync(ConfigType.Preferences, "", "{\"v\":1}"));

        await Assert.ThrowsAsync<ValidationException>(() =>
            configService.UpsertAsync(ConfigType.Preferences, "   ", "{\"v\":1}"));
    }

    [Fact]
    public async Task Validation_WithEmptyConfigValue_ShouldThrowValidationException()
    {
        var currentUser = new TestCurrentUser(_testNtId);
        var configRepo = new ConfigRepository(_db, currentUser);
        var historyRepo = new ConfigHistoryRepository(_db, currentUser);
        var configService = new ConfigService(configRepo, historyRepo, _encryptionService, currentUser);

        await Assert.ThrowsAsync<ValidationException>(() =>
            configService.UpsertAsync(ConfigType.Preferences, "valid.key", ""));

        await Assert.ThrowsAsync<ValidationException>(() =>
            configService.UpsertAsync(ConfigType.Preferences, "valid.key", "   "));
    }

    [Fact]
    public async Task Validation_WithTooLongConfigKey_ShouldThrowValidationException()
    {
        var currentUser = new TestCurrentUser(_testNtId);
        var configRepo = new ConfigRepository(_db, currentUser);
        var historyRepo = new ConfigHistoryRepository(_db, currentUser);
        var configService = new ConfigService(configRepo, historyRepo, _encryptionService, currentUser);

        var longKey = new string('a', 129);

        await Assert.ThrowsAsync<ValidationException>(() =>
            configService.UpsertAsync(ConfigType.Preferences, longKey, "{\"v\":1}"));
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
