using AutoCodeForge.Application.Services;
using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Enums;
using AutoCodeForge.Core.Exceptions;
using AutoCodeForge.Core.Interfaces;
using AutoCodeForge.Infrastructure.Repositories;
using SqlSugar;

namespace AutoCodeForge.Tests;

public sealed class ConfigHistoryServiceTests : IDisposable
{
    private readonly string _dbPath;
    private readonly ISqlSugarClient _db;
    private readonly string _testNtId = "test.user.003";
    private readonly string _testEncryptionKey;
    private readonly ConfigService _configService;
    private readonly ConfigHistoryService _historyService;
    private readonly ConfigRepository _configRepository;
    private readonly ConfigHistoryRepository _historyRepository;

    public ConfigHistoryServiceTests()
    {
        _dbPath = Path.Combine(Path.GetTempPath(), $"autocodeforge.confighistory.{Guid.NewGuid():N}.db");
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
        var encryptionService = new EncryptionService(_testEncryptionKey);

        var currentUser = new TestCurrentUser(_testNtId);
        _configRepository = new ConfigRepository(_db, currentUser);
        _historyRepository = new ConfigHistoryRepository(_db, currentUser);
        _configService = new ConfigService(_configRepository, _historyRepository, encryptionService, currentUser);
        _historyService = new ConfigHistoryService(_historyRepository, _configRepository);
    }

    [Fact]
    public async Task GetByConfigIdAsync_WithHistoryRecords_ShouldReturnRecords()
    {
        var configKey = "history.test.key1";
        var config = await _configService.UpsertAsync(ConfigType.Preferences, configKey, "{\"v\":1}");
        await _configService.UpsertAsync(ConfigType.Preferences, configKey, "{\"v\":2}");
        await _configService.UpsertAsync(ConfigType.Preferences, configKey, "{\"v\":3}");

        var history = await _historyService.GetByConfigIdAsync(config.Id);

        Assert.Equal(3, history.Count);
        Assert.All(history, h => Assert.Equal(config.Id, h.ConfigId));
    }

    [Fact]
    public async Task GetByConfigIdAsync_WithPagination_ShouldReturnPaginatedResults()
    {
        var configKey = "history.test.key2";
        var config = await _configService.UpsertAsync(ConfigType.Preferences, configKey, "{\"v\":1}");

        for (int i = 2; i <= 10; i++)
        {
            await _configService.UpsertAsync(ConfigType.Preferences, configKey, $"{{\"v\":{i}}}");
        }

        var page1 = await _historyService.GetByConfigIdAsync(config.Id, page: 1, pageSize: 5);
        var page2 = await _historyService.GetByConfigIdAsync(config.Id, page: 2, pageSize: 5);

        Assert.Equal(5, page1.Count);
        Assert.Equal(5, page2.Count);
    }

    [Fact]
    public async Task GetByConfigTypeAsync_ShouldReturnHistoryForType()
    {
        var config1 = await _configService.UpsertAsync(ConfigType.Preferences, "hist.type.key1", "{\"t\":1}");
        var config2 = await _configService.UpsertAsync(ConfigType.Sandbox, "hist.type.key2", "{\"t\":2}");

        await _configService.UpsertAsync(ConfigType.Preferences, "hist.type.key1", "{\"t\":11}");
        await _configService.UpsertAsync(ConfigType.Sandbox, "hist.type.key2", "{\"t\":22}");

        var prefHistory = await _historyService.GetByConfigTypeAsync(ConfigType.Preferences);
        var sandboxHistory = await _historyService.GetByConfigTypeAsync(ConfigType.Sandbox);

        Assert.True(prefHistory.Count >= 1);
        Assert.True(sandboxHistory.Count >= 1);
        Assert.Contains(prefHistory, h => h.ConfigType == ConfigType.Preferences);
        Assert.Contains(sandboxHistory, h => h.ConfigType == ConfigType.Sandbox);
    }

    [Fact]
    public async Task GetByChangedByAsync_ShouldReturnHistoryByUser()
    {
        await _configService.UpsertAsync(ConfigType.Preferences, "hist.user.key1", "{\"u\":1}");
        await _configService.UpsertAsync(ConfigType.Sandbox, "hist.user.key2", "{\"u\":2}");

        var history = await _historyService.GetByChangedByAsync(_testNtId);

        Assert.Equal(2, history.Count);
        Assert.All(history, h => Assert.Equal(_testNtId, h.ChangedBy));
    }

    [Fact]
    public async Task GetLatestAsync_ShouldReturnMostRecentHistory()
    {
        var configKey = "history.test.key3";
        var config = await _configService.UpsertAsync(ConfigType.Preferences, configKey, "{\"l\":1}");
        await _configService.UpsertAsync(ConfigType.Preferences, configKey, "{\"l\":2}");
        await _configService.UpsertAsync(ConfigType.Preferences, configKey, "{\"l\":3}");

        var latest = await _historyService.GetLatestAsync(config.Id);

        Assert.NotNull(latest);
        Assert.Equal("{\"l\":3}", latest.NewValue);
    }

    [Fact]
    public async Task GetCountAsync_ShouldReturnCorrectCount()
    {
        var configKey = "history.test.key4";
        var config = await _configService.UpsertAsync(ConfigType.Preferences, configKey, "{\"c\":1}");
        await _configService.UpsertAsync(ConfigType.Preferences, configKey, "{\"c\":2}");
        await _configService.UpsertAsync(ConfigType.Preferences, configKey, "{\"c\":3}");

        var count = await _historyService.GetCountAsync(config.Id);

        Assert.Equal(3, count);
    }

    [Fact]
    public async Task RollbackAsync_WithInvalidHistoryId_ShouldThrowNotFoundException()
    {
        var invalidId = Guid.NewGuid();

        await Assert.ThrowsAsync<NotFoundException>(() => _historyService.RollbackAsync(invalidId));
    }

    [Fact]
    public async Task RollbackAsync_ShouldCreateNewHistoryRecord()
    {
        var configKey = "history.test.key6";
        var config = await _configService.UpsertAsync(ConfigType.Preferences, configKey, "{\"rh\":1}");
        await _configService.UpsertAsync(ConfigType.Preferences, configKey, "{\"rh\":2}");

        var initialCount = await _historyService.GetCountAsync(config.Id);
        var history = await _historyService.GetByConfigIdAsync(config.Id);
        var firstHistory = history.Last();

        await _historyService.RollbackAsync(firstHistory.Id);
        var finalCount = await _historyService.GetCountAsync(config.Id);

        Assert.Equal(initialCount + 1, finalCount);
    }

    [Fact]
    public async Task HistoryRecords_ShouldContainCorrectOperationTypes()
    {
        var configKey = "history.test.key7";
        var config = await _configService.UpsertAsync(ConfigType.Preferences, configKey, "{\"op\":1}");
        await _configService.UpsertAsync(ConfigType.Preferences, configKey, "{\"op\":2}");

        var history = await _historyService.GetByConfigIdAsync(config.Id);
        var operations = history.Select(h => h.Operation).ToList();

        Assert.Contains("Created", operations);
        Assert.Contains("Updated", operations);
    }

    [Fact]
    public async Task HistoryRecords_ShouldTrackPreviousAndNewValues()
    {
        var configKey = "history.test.key8";
        var config = await _configService.UpsertAsync(ConfigType.Preferences, configKey, "{\"val\":1}");
        await _configService.UpsertAsync(ConfigType.Preferences, configKey, "{\"val\":2}");

        var history = await _historyService.GetByConfigIdAsync(config.Id);
        var updateRecord = history.First(h => h.Operation == "Updated");

        Assert.Equal("{\"val\":1}", updateRecord.PreviousValue);
        Assert.Equal("{\"val\":2}", updateRecord.NewValue);
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
