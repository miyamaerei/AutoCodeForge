using System.Diagnostics;
using AutoCodeForge.Core.DTOs.AI;
using AutoCodeForge.Core.Interfaces;
using AutoCodeForge.Infrastructure.AI;
using AutoCodeForge.Infrastructure.Repositories;
using Microsoft.Extensions.Logging.Abstractions;
using SqlSugar;

namespace AutoCodeForge.Tests.Performance;

public sealed class LlmGatewayPerformanceTests : IDisposable
{
    private readonly string _dbPath;
    private readonly ISqlSugarClient _db;
    private readonly LlmGateway _gateway;

    public LlmGatewayPerformanceTests()
    {
        _dbPath = Path.Combine(Path.GetTempPath(), $"autocodeforge.llmperf.{Guid.NewGuid():N}.db");
        _db = new SqlSugarScope(new ConnectionConfig
        {
            ConnectionString = $"Data Source={_dbPath}",
            DbType = DbType.Sqlite,
            IsAutoCloseConnection = true,
            InitKeyType = InitKeyType.Attribute,
        });

        _db.CodeFirst.InitTables(typeof(AutoCodeForge.Core.Entities.LLMModelConfigEntity));

        var repository = new LLMModelConfigRepository(_db, new TestCurrentUser("perf.user"));
        _gateway = new LlmGateway(repository, NullLogger<LlmGateway>.Instance);
    }

    [Fact]
    public async Task ChatAsync_MockGateway_ShouldCompleteWithin500Milliseconds()
    {
        var request = new LlmRequest
        {
            Messages =
            {
                new ChatMessage
                {
                    Role = "user",
                    Content = "Measure mock llm latency",
                },
            },
        };

        var stopwatch = Stopwatch.StartNew();
        var response = await _gateway.ChatAsync(request);
        stopwatch.Stop();

        Assert.Equal("[AutoCodeForge AI] Measure mock llm latency", response.Content);
        Assert.True(stopwatch.ElapsedMilliseconds < 500, $"Expected <500ms but took {stopwatch.ElapsedMilliseconds}ms");
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