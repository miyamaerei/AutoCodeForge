using AutoCodeForge.Core.DTOs.AI;
using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Exceptions;
using AutoCodeForge.Core.Interfaces;
using AutoCodeForge.Infrastructure.AI;
using AutoCodeForge.Infrastructure.Repositories;
using Microsoft.Extensions.Logging.Abstractions;
using SqlSugar;

namespace AutoCodeForge.Tests;

public sealed class LlmGatewayTests : IDisposable
{
    private readonly string _dbPath;
    private readonly ISqlSugarClient _db;
    private readonly LLMModelConfigRepository _repository;
    private readonly LlmGateway _gateway;

    public LlmGatewayTests()
    {
        _dbPath = Path.Combine(Path.GetTempPath(), $"autocodeforge.llmgateway.{Guid.NewGuid():N}.db");
        _db = new SqlSugarScope(new ConnectionConfig
        {
            ConnectionString = $"Data Source={_dbPath}",
            DbType = DbType.Sqlite,
            IsAutoCloseConnection = true,
            InitKeyType = InitKeyType.Attribute,
        });

        _db.CodeFirst.InitTables(typeof(LLMModelConfigEntity));

        _repository = new LLMModelConfigRepository(_db, new TestCurrentUser("llm.user"));
        _gateway = new LlmGateway(_repository, NullLogger<LlmGateway>.Instance);
    }

    [Fact]
    public async Task ChatAsync_WhenNoMessages_ShouldThrowValidationException()
    {
        await Assert.ThrowsAsync<ValidationException>(() => _gateway.ChatAsync(new LlmRequest()));
    }

    [Fact]
    public async Task ChatAsync_WithPreferredModel_ShouldUseSelectedModelName()
    {
        var defaultModel = await CreateModelAsync("fallback-model");
        var preferredModel = await CreateModelAsync("preferred-model");

        var response = await _gateway.ChatAsync(new LlmRequest
        {
            PreferredModelId = preferredModel.Id,
            Messages =
            {
                new ChatMessage
                {
                    Role = "user",
                    Content = "Hello preferred",
                },
            },
        });

        Assert.Equal("preferred-model", response.ModelName);
        Assert.Equal("[AutoCodeForge AI] Hello preferred", response.Content);
        Assert.NotEqual(defaultModel.Id, preferredModel.Id);
    }

    [Fact]
    public async Task ChatWithToolsAsync_WhenToolMatches_ShouldAppendToolResult()
    {
        await CreateModelAsync("tool-model");
        var tool = new StubTool("SearchRepo", _ => Task.FromResult("tool-result"));

        var response = await _gateway.ChatWithToolsAsync(new LlmRequest
        {
            Messages =
            {
                new ChatMessage
                {
                    Role = "user",
                    Content = "SearchRepo find build errors",
                },
            },
        }, new[] { tool });

        Assert.Contains("[AutoCodeForge AI] SearchRepo find build errors", response.Content);
        Assert.Contains("[Tool:SearchRepo]", response.Content);
        Assert.Contains("tool-result", response.Content);
    }

    [Fact]
    public async Task ChatWithToolsAsync_WhenToolFails_ShouldAppendErrorMarker()
    {
        await CreateModelAsync("tool-model");
        var tool = new StubTool("InspectRepo", _ => throw new InvalidOperationException("tool blew up"));

        var response = await _gateway.ChatWithToolsAsync(new LlmRequest
        {
            Messages =
            {
                new ChatMessage
                {
                    Role = "user",
                    Content = "InspectRepo now",
                },
            },
        }, new[] { tool });

        Assert.Contains("[Tool:InspectRepo:ERROR]", response.Content);
        Assert.Contains("tool blew up", response.Content);
    }

    private async Task<LLMModelConfigEntity> CreateModelAsync(string modelName)
    {
        var model = new LLMModelConfigEntity
        {
            Id = Guid.NewGuid(),
            ModelName = modelName,
            Provider = LLMProvider.AzureOpenAI,
            Endpoint = "https://example.local",
            ApiKey = "encrypted-key",
        };

        return await _repository.CreateAsync(model);
    }

    public void Dispose()
    {
        if (_db is IDisposable disposable)
        {
            disposable.Dispose();
        }

        if (!File.Exists(_dbPath))
        {
            return;
        }

        try
        {
            File.Delete(_dbPath);
        }
        catch (IOException)
        {
        }
    }

    private sealed class StubTool : IAgentTool
    {
        private readonly Func<IReadOnlyDictionary<string, string>, Task<string>> _executeAsync;

        public StubTool(string name, Func<IReadOnlyDictionary<string, string>, Task<string>> executeAsync)
        {
            Name = name;
            _executeAsync = executeAsync;
        }

        public string Name { get; }

        public string Description => $"Stub tool {Name}";

        public Task<string> ExecuteAsync(IReadOnlyDictionary<string, string> arguments, CancellationToken cancellationToken = default)
        {
            return _executeAsync(arguments);
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