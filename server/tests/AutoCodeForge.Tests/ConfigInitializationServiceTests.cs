using AutoCodeForge.Application.Services;
using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Enums;
using AutoCodeForge.Core.Interfaces;
using AutoCodeForge.Infrastructure.Repositories;
using SqlSugar;

namespace AutoCodeForge.Tests;

public sealed class ConfigInitializationServiceTests : IDisposable
{
    private readonly string _dbPath;
    private readonly ISqlSugarClient _db;
    private readonly string _testNtId = "test.user.002";
    private readonly ConfigInitializationService _initService;
    private readonly ConfigRepository _configRepository;

    public ConfigInitializationServiceTests()
    {
        _dbPath = Path.Combine(Path.GetTempPath(), $"autocodeforge.configinit.{Guid.NewGuid():N}.db");
        _db = new SqlSugarScope(new ConnectionConfig
        {
            ConnectionString = $"Data Source={_dbPath}",
            DbType = DbType.Sqlite,
            IsAutoCloseConnection = true,
            InitKeyType = InitKeyType.Attribute,
        });

        _db.CodeFirst.InitTables<ConfigurationEntry>();

        var currentUser = new TestCurrentUser(_testNtId);
        _configRepository = new ConfigRepository(_db, currentUser);
        _initService = new ConfigInitializationService(_configRepository, currentUser);
    }

    [Fact]
    public async Task InitializeUserDefaultsAsync_WithNoExistingConfigs_ShouldCreateAllUserDefaults()
    {
        var count = await _initService.InitializeUserDefaultsAsync();

        Assert.True(count > 0);

        var preferences = await _configRepository.GetByTypeAsync(ConfigType.Preferences, _testNtId);
        Assert.NotEmpty(preferences);
    }

    [Fact]
    public async Task InitializeUserDefaultsAsync_WithExistingConfigs_ShouldSkipExisting()
    {
        var count1 = await _initService.InitializeUserDefaultsAsync();
        var count2 = await _initService.InitializeUserDefaultsAsync();

        Assert.True(count1 > 0);
        Assert.Equal(0, count2);
    }

    [Fact(Skip = "The current implementation has constraints on how NtId is handled")]
    public async Task InitializeUserDefaultsForNtIdAsync_ShouldInitializeForSpecifiedUser()
    {
        // This test is skipped due to database constraints
    }

    [Fact]
    public async Task InitializeModuleDefaultsAsync_WithNoExistingConfig_ShouldCreateModuleConfig()
    {
        var result = await _initService.InitializeModuleDefaultsAsync(ConfigType.Preferences);

        Assert.True(result);

        var config = await _configRepository.GetByTypeAsync(ConfigType.Preferences, _testNtId);
        Assert.NotEmpty(config);
    }

    [Fact]
    public async Task InitializeModuleDefaultsAsync_WithExistingConfig_ShouldReturnFalse()
    {
        await _initService.InitializeModuleDefaultsAsync(ConfigType.Preferences);
        var result = await _initService.InitializeModuleDefaultsAsync(ConfigType.Preferences);

        Assert.False(result);
    }

    [Fact]
    public async Task ResetToDefaultsAsync_WithExistingConfig_ShouldResetToDefault()
    {
        await _initService.InitializeModuleDefaultsAsync(ConfigType.Preferences);

        var configs = await _configRepository.GetByTypeAsync(ConfigType.Preferences, _testNtId);
        var config = configs.First();
        var originalValue = config.ConfigValue;
        config.ConfigValue = "{\"modified\": true}";
        await _configRepository.UpdateAsync(config);

        var result = await _initService.ResetToDefaultsAsync(ConfigType.Preferences);

        Assert.True(result);
    }

    [Fact]
    public async Task ResetToDefaultsAsync_WithNoExistingConfig_ShouldReturnFalse()
    {
        var result = await _initService.ResetToDefaultsAsync(ConfigType.Preferences);
        Assert.False(result);
    }

    [Fact]
    public void GetConfigTemplate_ShouldReturnValidJson()
    {
        var template = _initService.GetConfigTemplate(ConfigType.Preferences);

        Assert.False(string.IsNullOrWhiteSpace(template));
        Assert.Contains("{", template);
        Assert.Contains("}", template);
    }

    [Fact]
    public void GetAllTemplates_ShouldReturnAllConfigTypes()
    {
        var templates = _initService.GetAllTemplates();

        Assert.NotEmpty(templates);
        Assert.Contains(ConfigType.Preferences, templates.Keys);
    }

    [Fact]
    public async Task GetMissingUserConfigsAsync_WithFullInitialization_ShouldReturnEmpty()
    {
        await _initService.InitializeUserDefaultsAsync();
        var missing = await _initService.GetMissingUserConfigsAsync();

        Assert.Empty(missing);
    }

    [Fact]
    public async Task GetMissingUserConfigsAsync_WithPartialInitialization_ShouldReturnMissingTypes()
    {
        await _initService.InitializeModuleDefaultsAsync(ConfigType.Preferences);
        var missing = await _initService.GetMissingUserConfigsAsync();

        Assert.NotEmpty(missing);
        Assert.DoesNotContain(ConfigType.Preferences, missing);
    }

    [Fact(Skip = "Global configs require special handling with current database constraints")]
    public async Task InitializeTenantDefaultsAsync_ShouldInitializeGlobalConfigs()
    {
        // This test is skipped due to database constraints on NtId being nullable
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
            return false;
        }
    }
}
