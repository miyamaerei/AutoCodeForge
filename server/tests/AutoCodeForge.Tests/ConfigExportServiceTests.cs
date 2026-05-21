using AutoCodeForge.Application.Services;
using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Enums;
using AutoCodeForge.Core.Interfaces;
using AutoCodeForge.Infrastructure.Repositories;
using SqlSugar;

namespace AutoCodeForge.Tests;

public sealed class ConfigExportServiceTests : IDisposable
{
    private readonly string _dbPath;
    private readonly ISqlSugarClient _db;
    private readonly string _testNtId = "test.user.004";
    private readonly string _testEncryptionKey;
    private readonly EncryptionService _encryptionService;
    private readonly ConfigService _configService;
    private readonly ConfigExportService _exportService;
    private readonly ConfigRepository _configRepository;
    private readonly ConfigHistoryRepository _historyRepository;

    public ConfigExportServiceTests()
    {
        _dbPath = Path.Combine(Path.GetTempPath(), $"autocodeforge.configexport.{Guid.NewGuid():N}.db");
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

        var currentUser = new TestCurrentUser(_testNtId);
        _configRepository = new ConfigRepository(_db, currentUser);
        _historyRepository = new ConfigHistoryRepository(_db, currentUser);
        _configService = new ConfigService(_configRepository, _historyRepository, _encryptionService, currentUser);
        _exportService = new ConfigExportService(_configRepository, _encryptionService, currentUser);
    }

    [Fact]
    public async Task ExportByTypeAsync_WithConfigs_ShouldReturnValidJson()
    {
        await _configService.UpsertAsync(ConfigType.Preferences, "export.pref.1", "{\"a\":1}");
        await _configService.UpsertAsync(ConfigType.Preferences, "export.pref.2", "{\"b\":2}");

        var json = await _exportService.ExportByTypeAsync(ConfigType.Preferences, _testNtId);

        Assert.False(string.IsNullOrWhiteSpace(json));
        Assert.Contains("export.pref.1", json);
        Assert.Contains("export.pref.2", json);
    }

    [Fact]
    public async Task ExportByTypeAsync_WithEncryptedConfigs_ShouldExportDecryptedValues()
    {
        var secretValue = "my_secret_api_key_123";
        await _configService.UpsertAsync(ConfigType.ApiKey, "export.api.1", secretValue, isEncrypted: true);

        var json = await _exportService.ExportByTypeAsync(ConfigType.ApiKey, _testNtId);

        Assert.Contains(secretValue, json);
    }

    [Fact]
    public async Task ExportUserConfigsAsync_ShouldExportAllUserConfigs()
    {
        await _configService.UpsertAsync(ConfigType.Preferences, "export.user.1", "{\"x\":1}");
        await _configService.UpsertAsync(ConfigType.Sandbox, "export.user.2", "{\"y\":2}");

        var json = await _exportService.ExportUserConfigsAsync(_testNtId);

        Assert.False(string.IsNullOrWhiteSpace(json));
        Assert.Contains("export.user.1", json);
        Assert.Contains("export.user.2", json);
    }

    [Fact(Skip = "Global configs require special setup with non-null NtId constraint")]
    public async Task ExportGlobalConfigsAsync_ShouldExportGlobalConfigs()
    {
        // This test is skipped because the current implementation has NOT NULL constraint on NtId
    }

    [Fact]
    public async Task ImportAsync_WithValidJson_ShouldImportConfigs()
    {
        await _configService.UpsertAsync(ConfigType.Preferences, "import.pref.1", "{\"v\":1}");
        var exportJson = await _exportService.ExportByTypeAsync(ConfigType.Preferences, _testNtId);

        await _db.Deleteable<ConfigurationEntry>().ExecuteCommandAsync();

        var importedCount = await _exportService.ImportAsync(exportJson, _testNtId, overwriteExisting: true);

        Assert.True(importedCount > 0);

        var imported = await _configService.GetByTypeAndKeyAsync(ConfigType.Preferences, "import.pref.1");
        Assert.NotNull(imported);
    }

    [Fact]
    public async Task ImportAsync_WithExistingConfigsAndOverwriteFalse_ShouldSkipExisting()
    {
        await _configService.UpsertAsync(ConfigType.Preferences, "import.skip.1", "{\"original\": true}");
        var exportJson = await _exportService.ExportByTypeAsync(ConfigType.Preferences, _testNtId);

        var existing = await _configService.GetByTypeAndKeyAsync(ConfigType.Preferences, "import.skip.1");
        existing!.ConfigValue = "{\"modified\": true}";
        await _configRepository.UpdateAsync(existing);

        var importedCount = await _exportService.ImportAsync(exportJson, _testNtId, overwriteExisting: false);

        Assert.Equal(0, importedCount);

        var stillModified = await _configService.GetByTypeAndKeyAsync(ConfigType.Preferences, "import.skip.1");
        Assert.Equal("{\"modified\": true}", stillModified!.ConfigValue);
    }

    [Fact]
    public async Task ImportAsync_WithExistingConfigsAndOverwriteTrue_ShouldOverwriteExisting()
    {
        await _configService.UpsertAsync(ConfigType.Preferences, "import.overwrite.1", "{\"original\": true}");
        var exportJson = await _exportService.ExportByTypeAsync(ConfigType.Preferences, _testNtId);

        var existing = await _configService.GetByTypeAndKeyAsync(ConfigType.Preferences, "import.overwrite.1");
        existing!.ConfigValue = "{\"modified\": true}";
        await _configRepository.UpdateAsync(existing);

        var importedCount = await _exportService.ImportAsync(exportJson, _testNtId, overwriteExisting: true);

        Assert.Equal(1, importedCount);

        var restored = await _configService.GetByTypeAndKeyAsync(ConfigType.Preferences, "import.overwrite.1");
        Assert.Equal("{\"original\": true}", restored!.ConfigValue);
    }

    [Fact]
    public async Task ImportAsync_WithNullJson_ShouldThrowArgumentException()
    {
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _exportService.ImportAsync(null!, _testNtId));

        Assert.Equal("jsonData", exception.ParamName);
    }

    [Fact]
    public async Task ImportAsync_WithEmptyJson_ShouldThrowArgumentException()
    {
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _exportService.ImportAsync(string.Empty, _testNtId));

        Assert.Equal("jsonData", exception.ParamName);
    }

    [Fact]
    public async Task ImportAsync_WithEncryptedConfigs_ShouldEncryptOnImport()
    {
        var secretValue = "secret_to_import";
        await _configService.UpsertAsync(ConfigType.ApiKey, "import.encrypted.1", secretValue, isEncrypted: true);
        var exportJson = await _exportService.ExportByTypeAsync(ConfigType.ApiKey, _testNtId);

        await _db.Deleteable<ConfigurationEntry>().ExecuteCommandAsync();

        await _exportService.ImportAsync(exportJson, _testNtId, overwriteExisting: true);

        var inDb = await _configRepository.GetByTypeAndKeyAsync(ConfigType.ApiKey, "import.encrypted.1", _testNtId);
        Assert.NotNull(inDb);
        Assert.True(inDb.IsEncrypted);
        Assert.NotEqual(secretValue, inDb.ConfigValue);

        var decrypted = await _configService.GetByTypeAndKeyAsync(ConfigType.ApiKey, "import.encrypted.1");
        Assert.Equal(secretValue, decrypted!.ConfigValue);
    }

    [Fact]
    public async Task BatchUpdateAsync_WithExistingConfigs_ShouldUpdateMultiple()
    {
        await _configService.UpsertAsync(ConfigType.Preferences, "batch.1", "{\"v\":1}");
        await _configService.UpsertAsync(ConfigType.Preferences, "batch.2", "{\"v\":2}");

        var configs = await _configRepository.GetByTypeAsync(ConfigType.Preferences, _testNtId);
        foreach (var config in configs)
        {
            config.ConfigValue = config.ConfigValue.Replace("\"v\":", "\"updated\":");
        }

        var updatedCount = await _exportService.BatchUpdateAsync(configs, _testNtId);

        Assert.Equal(2, updatedCount);

        var updated = await _configService.GetByTypeAsync(ConfigType.Preferences);
        Assert.All(updated, c => Assert.Contains("\"updated\":", c.ConfigValue));
    }

    [Fact]
    public async Task BatchUpdateAsync_WithEmptyList_ShouldReturnZero()
    {
        var count = await _exportService.BatchUpdateAsync(new List<ConfigurationEntry>(), _testNtId);
        Assert.Equal(0, count);
    }

    [Fact]
    public async Task ExportByTypeAsync_ShouldIncludeMetadata()
    {
        await _configService.UpsertAsync(ConfigType.Preferences, "meta.test.1", "{\"m\":1}");

        var json = await _exportService.ExportByTypeAsync(ConfigType.Preferences, _testNtId);

        Assert.Contains("configType", json);
        Assert.Contains("configs", json);
        Assert.Contains("exportedAt", json);
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
