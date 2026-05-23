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

    [Fact(Skip = "Requires GitHub Copilot CLI executable ('copilot') available in PATH or via CliExecutable")]
    public async Task ChatAsync_GitHubCopilotProvider_ShouldRouteToCopilotCli()
    {
        // 创建 GitHub Copilot 配置的模型，使用默认的 copilot CLI
        var copilotModel = await CreateGitHubCopilotModelAsync("copilot-test-model");

        var response = await _gateway.ChatAsync(new LlmRequest
        {
            PreferredModelId = copilotModel.Id,
            Messages =
            {
                new ChatMessage
                {
                    Role = "user",
                    Content = "Hello Copilot",
                },
            },
        });

        // 验证响应使用了模型名称
        Assert.Equal("copilot-test-model", response.ModelName);
        // 验证响应非空
        Assert.False(string.IsNullOrWhiteSpace(response.Content));
    }

    [Fact(Skip = "Requires GitHub Copilot CLI executable ('copilot') available in PATH or via CliExecutable")]
    public async Task ChatAsync_GitHubCopilotWithPatEnvVar_ShouldIncludePatEnvVar()
    {
        // 创建带有 PAT 环境变量配置的 GitHub Copilot 模型
        var copilotModel = await CreateGitHubCopilotModelAsync(
            "copilot-pat-model",
            patEnvVar: "GITHUB_TOKEN");

        var response = await _gateway.ChatAsync(new LlmRequest
        {
            PreferredModelId = copilotModel.Id,
            Messages =
            {
                new ChatMessage
                {
                    Role = "user",
                    Content = "Test PAT auth",
                },
            },
        });

        // 验证响应
        Assert.Equal("copilot-pat-model", response.ModelName);
        Assert.False(string.IsNullOrWhiteSpace(response.Content));
    }

    [Fact(Skip = "Requires GitHub Copilot CLI executable ('copilot') available in PATH or via CliExecutable")]
    public async Task ChatAsync_GitHubCopilotFallbackToDefault_WhenNoPreferredModel()
    {
        // 创建默认的 GitHub Copilot 模型（不设置 PreferredModelId）
        var defaultCopilot = await CreateGitHubCopilotModelAsync("default-copilot");

        var response = await _gateway.ChatAsync(new LlmRequest
        {
            Messages =
            {
                new ChatMessage
                {
                    Role = "user",
                    Content = "Hello default Copilot",
                },
            },
        });

        // 验证使用了默认的 GitHub Copilot 模型
        Assert.Equal("default-copilot", response.ModelName);
        Assert.False(string.IsNullOrWhiteSpace(response.Content));
    }

    [Fact(Skip = "Requires GitHub Copilot CLI executable ('copilot') available in PATH or via CliExecutable")]
    public async Task ChatAsync_GitHubCopilotWithOrganization_ShouldIncludeOrganization()
    {
        // 创建带有组织配置的 GitHub Copilot 模型
        var copilotModel = await CreateGitHubCopilotModelAsync(
            "copilot-org-model",
            organization: "test-org");

        var response = await _gateway.ChatAsync(new LlmRequest
        {
            PreferredModelId = copilotModel.Id,
            Messages =
            {
                new ChatMessage
                {
                    Role = "user",
                    Content = "Test organization",
                },
            },
        });

        // 验证响应
        Assert.Equal("copilot-org-model", response.ModelName);
        Assert.False(string.IsNullOrWhiteSpace(response.Content));
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

    /// <summary>
    /// 创建 GitHub Copilot 配置的模型
    /// </summary>
    private async Task<LLMModelConfigEntity> CreateGitHubCopilotModelAsync(
        string modelName,
        string? customCliPath = null,
        string? patEnvVar = null,
        string? organization = null)
    {
        // CLI 可执行文件路径：使用自定义路径或默认的 copilot
        var cliExecutable = string.IsNullOrWhiteSpace(customCliPath) ? "copilot" : customCliPath;

        var model = new LLMModelConfigEntity
        {
            Id = Guid.NewGuid(),
            ModelName = modelName,
            Provider = LLMProvider.GitHubCopilot,
            Endpoint = "https://github.com",
            ApiKey = "test-api-key",
            CliExecutable = cliExecutable,
            Organization = organization,
            AuthMode = string.IsNullOrEmpty(patEnvVar) ? "interactive" : "pat",
            PatEnvVar = patEnvVar,
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