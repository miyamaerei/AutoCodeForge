using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SqlSugar;
using __ProjectName__.Entities;

namespace __ProjectName__.Extensions;

public static class DatabaseInitializer
{
    public static async Task InitializeDatabaseAsync(this IHost host)
    {
        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ISqlSugarClient>();
        
        await CreateTablesAsync(db);
        await InitializeSeedDataAsync(db);
    }

    private static async Task CreateTablesAsync(ISqlSugarClient db)
    {
        // Create all entity tables using CodeFirst
        var entityTypes = new[]
        {
            typeof(UserEntity),
            typeof(RefreshTokenEntity),
            typeof(TaskEntity),
            typeof(TaskLogEntity),
            typeof(ChatSessionEntity),
            typeof(ChatMessageEntity),
            typeof(AgentEntity),
            typeof(RepositoryEntity),
            typeof(ScheduledTaskEntity),
            typeof(ScheduledTaskExecutionEntity),
            typeof(PipelineEntity),
            typeof(BuildEntity),
            typeof(GlobalConfigEntity),
            typeof(UserConfigEntity),
            typeof(LLMModelConfigEntity),
            typeof(AISessionConfigEntity),
            typeof(WikiPageEntity)
        };

        foreach (var type in entityTypes)
        {
            await db.CodeFirst.InitTablesAsync(type);
        }
    }

    private static async Task InitializeSeedDataAsync(ISqlSugarClient db)
    {
        await InitializeAdminUserAsync(db);
        await InitializeGlobalConfigsAsync(db);
        await InitializeDefaultAgentAsync(db);
    }

    private static async Task InitializeAdminUserAsync(ISqlSugarClient db)
    {
        var adminExists = await db.Queryable<UserEntity>()
            .Where(u => u.NtId == "admin")
            .AnyAsync();

        if (!adminExists)
        {
            var adminUser = new UserEntity
            {
                Id = Guid.NewGuid(),
                NtId = "admin",
                UserName = "System Administrator",
                Email = "admin@autocodeforge.local",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await db.Insertable(adminUser).ExecuteCommandAsync();
        }
    }

    private static async Task InitializeGlobalConfigsAsync(ISqlSugarClient db)
    {
        var existingConfigs = await db.Queryable<GlobalConfigEntity>()
            .Select(c => c.ConfigKey)
            .ToListAsync();

        var defaultConfigs = new List<GlobalConfigEntity>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Category = "system",
                ConfigKey = "app.name",
                ConfigValue = "\"AutoCodeForge\"",
                Description = "Application display name",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                Category = "system",
                ConfigKey = "app.version",
                ConfigValue = "\"1.0.0\"",
                Description = "Application version",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                Category = "security",
                ConfigKey = "password.minLength",
                ConfigValue = "6",
                Description = "Minimum password length",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                Category = "ai",
                ConfigKey = "rate.limit.perMinute",
                ConfigValue = "10",
                Description = "LLM call rate limit per minute per user",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                Category = "ai",
                ConfigKey = "session.maxMessages",
                ConfigValue = "100",
                Description = "Maximum messages per chat session",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        foreach (var config in defaultConfigs)
        {
            if (!existingConfigs.Contains(config.ConfigKey))
            {
                await db.Insertable(config).ExecuteCommandAsync();
            }
        }
    }

    private static async Task InitializeDefaultAgentAsync(ISqlSugarClient db)
    {
        var defaultAgentExists = await db.Queryable<AgentEntity>()
            .Where(a => a.Name == "Default Assistant")
            .AnyAsync();

        if (!defaultAgentExists)
        {
            var defaultAgent = new AgentEntity
            {
                Id = Guid.NewGuid(),
                NtId = string.Empty,
                Name = "Default Assistant",
                Description = "Your AI assistant for code automation tasks",
                Icon = "🤖",
                SystemPrompt = """
                    You are AutoCodeForge, an AI-powered code automation assistant.
                    You help users with software development tasks including code generation,
                    code review, documentation, and workflow automation.
                    Always respond in a helpful and professional manner.
                    """,
                KeywordsJson = "[{\"keyword\":\"code\",\"weight\":1.0},{\"keyword\":\"review\",\"weight\":0.8},{\"keyword\":\"automation\",\"weight\":0.9},{\"keyword\":\"development\",\"weight\":0.7}]",
                Enabled = true,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await db.Insertable(defaultAgent).ExecuteCommandAsync();
        }
    }
}
