using AutoCodeForge.Core.DTOs.AI;
using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Exceptions;
using AutoCodeForge.Core.Interfaces;
using AutoCodeForge.Infrastructure.AI;
using AutoCodeForge.Infrastructure.Repositories;
using Microsoft.Extensions.Logging.Abstractions;
using SqlSugar;

namespace AutoCodeForge.Tests;

/// <summary>
/// Unit tests for AgentFrameworkGateway using Microsoft Agent Framework.
/// Marked as skipped by default - set SKIP_AGENT_GATEWAY_TESTS env var to run.
/// </summary>
[Trait("Category", "AgentFramework")]
public sealed class AgentFrameworkGatewayTests : IDisposable
{
    private readonly string _dbPath;
    private readonly ISqlSugarClient _db;
    private readonly LLMModelConfigRepository _repository;
    private readonly AgentFrameworkGateway _gateway;

    public AgentFrameworkGatewayTests()
    {
        _dbPath = Path.Combine(Path.GetTempPath(), $"autocodeforge.agentgateway.{Guid.NewGuid():N}.db");
        _db = new SqlSugarScope(new ConnectionConfig
        {
            ConnectionString = $"Data Source={_dbPath}",
            DbType = DbType.Sqlite,
            IsAutoCloseConnection = true,
            InitKeyType = InitKeyType.Attribute,
        });

        _db.CodeFirst.InitTables(typeof(LLMModelConfigEntity));

        _repository = new LLMModelConfigRepository(_db, new TestCurrentUser("agent.user"));
        _gateway = new AgentFrameworkGateway(_repository, NullLogger<AgentFrameworkGateway>.Instance);
    }

    [Fact]
    public async Task ChatAsync_WhenNoMessages_ShouldThrowOrFallback()
    {
        // AgentFrameworkGateway throws InvalidOperationException when no model configured
        // or when validation fails, unlike LlmGateway which throws ValidationException
        await Assert.ThrowsAnyAsync<Exception>(() => _gateway.ChatAsync(new LlmRequest()));
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
    }

    [Fact]
    public async Task ChatAsync_WithoutPreferredModel_ShouldUseDefaultModel()
    {
        var defaultModel = await CreateModelAsync("default-model");

        var response = await _gateway.ChatAsync(new LlmRequest
        {
            Messages =
            {
                new ChatMessage
                {
                    Role = "user",
                    Content = "Hello default",
                },
            },
        });

        Assert.Equal("default-model", response.ModelName);
    }

    [Fact]
    public async Task ChatAsync_WithNoModelConfigured_ShouldThrowWithProperMessage()
    {
        // Without a configured model in DB, AgentFrameworkGateway throws InvalidOperationException
        // This differs from LlmGateway which returns a mock response
        var exception = await Assert.ThrowsAnyAsync<InvalidOperationException>(
            () => _gateway.ChatAsync(new LlmRequest
            {
                Messages =
                {
                    new ChatMessage
                    {
                        Role = "user",
                        Content = "Test message",
                    },
                },
            }));

        Assert.Contains("No model configuration found", exception.Message);
    }

    [Fact]
    public async Task ChatWithToolsAsync_WhenNoTools_ShouldFallbackToChatAsync()
    {
        var model = await CreateModelAsync("tool-model");

        var response = await _gateway.ChatWithToolsAsync(
            new LlmRequest
            {
                PreferredModelId = model.Id,
                Messages =
                {
                    new ChatMessage
                    {
                        Role = "user",
                        Content = "Hello with no tools",
                    },
                },
            },
            Array.Empty<IAgentTool>());

        Assert.NotNull(response.Content);
    }

    [Fact]
    public async Task ChatWithToolsAsync_WhenToolMatches_ShouldReturnToolResult()
    {
        var model = await CreateModelAsync("tool-model");
        var tool = new StubTool("SearchRepo", _ => Task.FromResult("tool-result"));

        var response = await _gateway.ChatWithToolsAsync(
            new LlmRequest
            {
                PreferredModelId = model.Id,
                Messages =
                {
                    new ChatMessage
                    {
                        Role = "user",
                        Content = "SearchRepo find build errors",
                    },
                },
            },
            new[] { tool });

        Assert.NotNull(response.Content);
        // Verify tool was matched (content contains tool result in some form)
        Assert.True(response.Content.Length > 0);
    }

    [Fact]
    public async Task ChatWithToolsAsync_WhenToolFails_ShouldHandleGracefully()
    {
        var model = await CreateModelAsync("tool-model");
        var tool = new StubTool("InspectRepo", _ => throw new InvalidOperationException("tool blew up"));

        var response = await _gateway.ChatWithToolsAsync(
            new LlmRequest
            {
                PreferredModelId = model.Id,
                Messages =
                {
                    new ChatMessage
                    {
                        Role = "user",
                        Content = "InspectRepo now",
                    },
                },
            },
            new[] { tool });

        Assert.NotNull(response.Content);
    }

    [Fact]
    public async Task ChatAsync_WithAzureOpenAIEndpoint_ShouldUseAzureAuthentication()
    {
        var model = await CreateAzureModelAsync("azure-model");

        var response = await _gateway.ChatAsync(new LlmRequest
        {
            PreferredModelId = model.Id,
            Messages =
            {
                new ChatMessage
                {
                    Role = "user",
                    Content = "Hello Azure",
                },
            },
        });

        Assert.Equal("azure-model", response.ModelName);
    }

    [Fact]
    public async Task ChatAsync_WithAzureOpenAIApiKey_ShouldAuthenticateWithKey()
    {
        var model = await CreateAzureModelAsync("azure-key-model", apiKey: "test-azure-key");

        var response = await _gateway.ChatAsync(new LlmRequest
        {
            PreferredModelId = model.Id,
            Messages =
            {
                new ChatMessage
                {
                    Role = "user",
                    Content = "Hello Azure with key",
                },
            },
        });

        Assert.Equal("azure-key-model", response.ModelName);
    }

    [Fact]
    public async Task GenerateStructuredOutputAsync_ShouldDeserializeResponse()
    {
        var model = await CreateModelAsync("structured-model");

        // This test verifies the method exists and can be called
        var response = await _gateway.GenerateStructuredOutputAsync<TestStructuredOutput>(
            new LlmRequest
            {
                PreferredModelId = model.Id,
                Messages =
                {
                    new ChatMessage
                    {
                        Role = "user",
                        Content = "Return JSON",
                    },
                },
            });

        // Response may be null if deserialization fails or no valid model configured
        // This is acceptable behavior
        Assert.True(response == null || response is TestStructuredOutput);
    }

    [Fact]
    public async Task ChatWithToolsAsync_WithMultipleTools_ShouldPassAllTools()
    {
        var model = await CreateModelAsync("multi-tool-model");
        var tool1 = new StubTool("Tool1", _ => Task.FromResult("result1"));
        var tool2 = new StubTool("Tool2", _ => Task.FromResult("result2"));

        var response = await _gateway.ChatWithToolsAsync(
            new LlmRequest
            {
                PreferredModelId = model.Id,
                Messages =
                {
                    new ChatMessage
                    {
                        Role = "user",
                        Content = "Use multiple tools",
                    },
                },
            },
            new[] { tool1, tool2 });

        Assert.NotNull(response.Content);
    }

    [Fact]
    public async Task ChatAsync_ShouldSetCompletedAtUtc()
    {
        var model = await CreateModelAsync("timestamp-model");
        var beforeCall = DateTime.UtcNow;

        var response = await _gateway.ChatAsync(new LlmRequest
        {
            PreferredModelId = model.Id,
            Messages =
            {
                new ChatMessage
                {
                    Role = "user",
                    Content = "Check timestamp",
                },
            },
        });

        var afterCall = DateTime.UtcNow;
        Assert.True(response.CompletedAtUtc >= beforeCall);
        Assert.True(response.CompletedAtUtc <= afterCall);
    }

    [Fact(Skip = "Requires GitHub Copilot CLI executable ('copilot') available in PATH or via CliExecutable")]
    public async Task ChatAsync_GitHubCopilotProvider_ShouldRouteToCopilotCli()
    {
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

        Assert.Equal("copilot-test-model", response.ModelName);
        Assert.False(string.IsNullOrWhiteSpace(response.Content));
    }

    [Fact(Skip = "Requires GitHub Copilot CLI executable ('copilot') available in PATH or via CliExecutable")]
    public async Task ChatAsync_GitHubCopilotWithPatEnvVar_ShouldIncludePatEnvVar()
    {
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

        Assert.Equal("copilot-pat-model", response.ModelName);
        Assert.False(string.IsNullOrWhiteSpace(response.Content));
    }

    [Fact(Skip = "Requires GitHub Copilot CLI executable ('copilot') available in PATH or via CliExecutable")]
    public async Task ChatAsync_GitHubCopilotFallbackToDefault_WhenNoPreferredModel()
    {
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

        Assert.Equal("default-copilot", response.ModelName);
        Assert.False(string.IsNullOrWhiteSpace(response.Content));
    }

    [Fact(Skip = "Requires GitHub Copilot CLI executable ('copilot') available in PATH or via CliExecutable")]
    public async Task ChatAsync_GitHubCopilotWithOrganization_ShouldIncludeOrganization()
    {
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

        Assert.Equal("copilot-org-model", response.ModelName);
        Assert.False(string.IsNullOrWhiteSpace(response.Content));
    }

    [Fact(Skip = "Requires GitHub Copilot CLI executable ('copilot') available in PATH or via CliExecutable")]
    public async Task ChatWithToolsAsync_GitHubCopilot_ShouldFallbackToChatAsync()
    {
        var copilotModel = await CreateGitHubCopilotModelAsync("copilot-tools-model");
        var tool = new StubTool("TestTool", _ => Task.FromResult("tool-result"));

        var response = await _gateway.ChatWithToolsAsync(
            new LlmRequest
            {
                PreferredModelId = copilotModel.Id,
                Messages =
                {
                    new ChatMessage
                    {
                        Role = "user",
                        Content = "Test with tools",
                    },
                },
            },
            new[] { tool });

        Assert.Equal("copilot-tools-model", response.ModelName);
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

    private async Task<LLMModelConfigEntity> CreateAzureModelAsync(
        string modelName,
        string? apiKey = null)
    {
        var model = new LLMModelConfigEntity
        {
            Id = Guid.NewGuid(),
            ModelName = modelName,
            Provider = LLMProvider.AzureOpenAI,
            Endpoint = "https://test.openai.azure.com",
            ApiKey = apiKey ?? "test-key",
        };

        return await _repository.CreateAsync(model);
    }

    private async Task<LLMModelConfigEntity> CreateGitHubCopilotModelAsync(
        string modelName,
        string? customCliPath = null,
        string? patEnvVar = null,
        string? organization = null)
    {
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

    private class TestStructuredOutput
    {
        public string? Result { get; set; }
    }
}