using AutoCodeForge.Core.Entities;
using SqlSugar;

namespace AutoCodeForge.Infrastructure.Data;

/// <summary>
/// Initializes database schema and optional seed data.
/// </summary>
public class DatabaseInitializer
{
    private readonly ISqlSugarClient _db;
    private readonly SeedData _seedData;

    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseInitializer"/> class.
    /// </summary>
    /// <param name="db">The SqlSugar client.</param>
    /// <param name="seedData">The seed data initializer.</param>
    public DatabaseInitializer(ISqlSugarClient db, SeedData seedData)
    {
        _db = db;
        _seedData = seedData;
    }

    /// <summary>
    /// Initializes all tables and optionally inserts seed data.
    /// </summary>
    /// <param name="isDevelopment">Whether current environment is development.</param>
    /// <returns>A task representing asynchronous execution.</returns>
    public async Task InitializeAsync(bool isDevelopment)
    {
        _db.CodeFirst.InitTables(
            typeof(UserEntity),
            typeof(TaskEntity),
            typeof(TaskLogEntity),
            typeof(TaskStepEntity),
            typeof(ChatSessionEntity),
            typeof(ChatMessageEntity),
            typeof(AgentEntity),
            typeof(RepositoryEntity),
            typeof(ReviewRuleSetEntity),
            typeof(ReviewTaskEntity),
            typeof(ReviewFindingEntity),
            typeof(RepoSandboxWorkspaceEntity),
            typeof(ScheduledTaskEntity),
            typeof(ScheduledTaskExecutionEntity),
            typeof(PipelineEntity),
            typeof(BuildEntity),
            typeof(GlobalConfigEntity),
            typeof(UserConfigEntity),
            typeof(LLMModelConfigEntity),
            typeof(AISessionConfigEntity),
            typeof(WikiPageEntity),
            typeof(AdminAuditLogEntity),
            typeof(GitSkillGrantEntity),
            typeof(AgentToolInvocationEntity),
            typeof(ConfigurationEntry),
            typeof(ConfigHistoryEntity));

        if (isDevelopment)
        {
            await _seedData.InitializeSeedDataAsync();
        }
    }
}
