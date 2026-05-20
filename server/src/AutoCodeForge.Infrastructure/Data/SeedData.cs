using AutoCodeForge.Core.Entities;
using AutoCodeForge.Infrastructure.Helpers;
using SqlSugar;

namespace AutoCodeForge.Infrastructure.Data;

/// <summary>
/// Inserts baseline development seed records.
/// </summary>
public class SeedData
{
    private readonly ISqlSugarClient _db;
    private readonly PasswordHelper _passwordHelper;

    /// <summary>
    /// Initializes a new instance of the <see cref="SeedData"/> class.
    /// </summary>
    /// <param name="db">The SqlSugar client.</param>
    /// <param name="passwordHelper">Password helper.</param>
    public SeedData(ISqlSugarClient db, PasswordHelper passwordHelper)
    {
        _db = db;
        _passwordHelper = passwordHelper;
    }

    /// <summary>
    /// Seeds baseline data when no records exist.
    /// </summary>
    /// <returns>A task representing asynchronous execution.</returns>
    public async Task InitializeSeedDataAsync()
    {
        var llmModelConfigId = await EnsureDefaultLlmModelConfigAsync();

        var exists = await _db.Queryable<UserEntity>().AnyAsync(user => user.NtId == "demo.user");
        if (!exists)
        {
            await _db.Insertable(new UserEntity
            {
                Id = Guid.NewGuid(),
                NtId = "demo.user",
                UserName = "Demo User",
                Email = "demo@autocodeforge.local",
                PasswordHash = _passwordHelper.HashPassword("Demo@123456"),
                IsDeleted = false,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow,
            }).ExecuteCommandAsync();
        }

        var hasGlobalConfig = await _db.Queryable<GlobalConfigEntity>()
            .AnyAsync(config => config.ConfigKey == "System.DefaultLanguage");
        if (!hasGlobalConfig)
        {
            await _db.Insertable(new GlobalConfigEntity
            {
                Id = Guid.NewGuid(),
                ConfigKey = "System.DefaultLanguage",
                ConfigValue = "en-US",
                Description = "Default language for new sessions",
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow,
            }).ExecuteCommandAsync();
        }

        var hasAgent = await _db.Queryable<AgentEntity>().AnyAsync(agent => agent.Name == "default-worker");
        if (!hasAgent)
        {
            await _db.Insertable(new AgentEntity
            {
                Id = Guid.NewGuid(),
                NtId = "demo.user",
                Name = "default-worker",
                Description = "Default worker agent profile",
                LlmModelConfigId = llmModelConfigId,
                IsEnabled = true,
                IsDeleted = false,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow,
            }).ExecuteCommandAsync();
        }
    }

    private async Task<Guid> EnsureDefaultLlmModelConfigAsync()
    {
        var existing = await _db.Queryable<LLMModelConfigEntity>()
            .FirstAsync(config => config.NtId == "demo.user" && config.ModelName == "gpt-4o-mini");
        if (existing is not null)
        {
            return existing.Id;
        }

        var entity = new LLMModelConfigEntity
        {
            Id = Guid.NewGuid(),
            NtId = "demo.user",
            Provider = LLMProvider.AzureOpenAI,
            ModelName = "gpt-4o-mini",
            Endpoint = "https://example.local/azure-openai",
            ApiKey = "encrypted-placeholder",
            IsDeleted = false,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
        };

        await _db.Insertable(entity).ExecuteCommandAsync();
        return entity.Id;
    }
}
