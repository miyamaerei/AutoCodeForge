using AutoCodeForge.Application.Services;
using AutoCodeForge.Core.DTOs.Agent;
using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Exceptions;
using AutoCodeForge.Core.Interfaces;
using AutoCodeForge.Infrastructure.Repositories;
using SqlSugar;

namespace AutoCodeForge.Tests;

public sealed class AgentServiceTests : IDisposable
{
    private readonly string _dbPath;
    private readonly ISqlSugarClient _db;
    private readonly AgentService _service;

    public AgentServiceTests()
    {
        _dbPath = Path.Combine(Path.GetTempPath(), $"autocodeforge.agentservice.{Guid.NewGuid():N}.db");
        _db = new SqlSugarScope(new ConnectionConfig
        {
            ConnectionString = $"Data Source={_dbPath}",
            DbType = DbType.Sqlite,
            IsAutoCloseConnection = true,
            InitKeyType = InitKeyType.Attribute,
        });

        _db.CodeFirst.InitTables(typeof(AgentEntity));

        var repository = new AgentRepository(_db, new TestCurrentUser("agent.user"));
        _service = new AgentService(repository);
    }

    [Fact]
    public async Task CreateAsync_ShouldTrimFieldsAndPersistEnabledAgent()
    {
        var response = await _service.CreateAsync(new CreateAgentRequest
        {
            Name = "  Writer Agent  ",
            Description = "  Handles docs  ",
            Keywords = " docs, write ",
            SystemPrompt = "prompt",
            IsEnabled = true,
        });

        Assert.Equal("Writer Agent", response.Name);
        Assert.Equal("Handles docs", response.Description);
        Assert.Equal("docs, write", response.Keywords);
        Assert.True(response.IsEnabled);
    }

    [Fact]
    public async Task UpdateAsync_WhenAgentMissing_ShouldThrowNotFoundException()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _service.UpdateAsync(Guid.NewGuid(), new UpdateAgentRequest
        {
            Name = "Updated",
            IsEnabled = true,
        }));
    }

    [Fact]
    public async Task MatchByInputAsync_ShouldReturnHighestKeywordScore()
    {
        await _service.CreateAsync(new CreateAgentRequest
        {
            Name = "Builder",
            Keywords = "build,compile,pipeline",
            IsEnabled = true,
        });

        await _service.CreateAsync(new CreateAgentRequest
        {
            Name = "Writer",
            Keywords = "docs,write,readme",
            IsEnabled = true,
        });

        await _service.CreateAsync(new CreateAgentRequest
        {
            Name = "Disabled",
            Keywords = "docs,write,review",
            IsEnabled = false,
        });

        var matched = await _service.MatchByInputAsync("Please update docs and README");

        Assert.NotNull(matched);
        Assert.Equal("Writer", matched!.Name);
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