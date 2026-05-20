using System.Reflection;
using AutoCodeForge.Api.Endpoints;
using AutoCodeForge.Api.Middleware;
using AutoCodeForge.Application.Services;
using AutoCodeForge.Core.Interfaces;
using AutoCodeForge.Core.Models;
using AutoCodeForge.Infrastructure.AI;
using AutoCodeForge.Infrastructure.BackgroundServices;
using AutoCodeForge.Infrastructure.Data;
using AutoCodeForge.Infrastructure.Helpers;
using AutoCodeForge.Infrastructure.Repositories;
using AutoCodeForge.Infrastructure.Repositories.Base;
using AutoCodeForge.Infrastructure.Services;
using AutoCodeForge.Infrastructure.Git;
using AutoCodeForge.Application.AI;
using AutoCodeForge.Application.Security;
using AutoCodeForge.Application.Tools;
using AutoCodeForge.Infrastructure.Logging;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
builder.Services.PostConfigure<JwtOptions>(options =>
{
    var envKey = builder.Configuration["JWT_KEY"];
    if (!string.IsNullOrWhiteSpace(envKey))
    {
        options.Key = envKey;
    }
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddDataProtection();
builder.Services.AddSqlSugar(builder.Configuration);
builder.Services.AddScoped<ICurrentUser, CurrentUser>();
builder.Services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<GlobalConfigRepository>();
builder.Services.AddScoped<UserConfigRepository>();
builder.Services.AddScoped<LLMModelConfigRepository>();
builder.Services.AddScoped<AgentRepository>();
builder.Services.AddScoped<ChatSessionRepository>();
builder.Services.AddScoped<ChatMessageRepository>();
builder.Services.AddScoped<TaskRepository>();
builder.Services.AddScoped<TaskLogRepository>();
builder.Services.AddScoped<RepoSandboxWorkspaceRepository>();
builder.Services.AddScoped<ReviewRepository>();
builder.Services.AddScoped<ReviewRuleSetRepository>();
builder.Services.AddScoped<ScheduledTaskRepository>();
builder.Services.AddScoped<ScheduledTaskExecutionRepository>();
builder.Services.AddScoped<PipelineRepository>();
builder.Services.AddScoped<BuildRepository>();
builder.Services.AddScoped<WikiPageRepository>();
builder.Services.AddScoped<GitSkillGrantRepository>();
builder.Services.AddScoped<AgentToolInvocationRepository>();
builder.Services.AddSingleton<PasswordHelper>();
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<RepositoryRepository>();
builder.Services.AddScoped<AdminAuditLogRepository>();
builder.Services.AddScoped<DataProtectionService>();
builder.Services.AddScoped<RepositoryService>();
builder.Services.AddScoped<RepositoryReviewSettingsService>();
builder.Services.AddScoped<RepoSyncService>();
builder.Services.AddScoped<ReviewService>();
builder.Services.AddScoped<ReviewRuleSetService>();
builder.Services.AddScoped<AdminAuditService>();
builder.Services.AddScoped<AutoCodeForge.Application.Validators.SandboxConfigValidator>();
builder.Services.AddScoped<GitSkillPolicyService>();
builder.Services.AddScoped<GitSkillPermissionGuard>();
builder.Services.AddScoped<GitSkillErrorMapper>();
builder.Services.AddScoped<GitContextHydrator>();
builder.Services.AddScoped<AgentToolAuditLogger>();
builder.Services.AddScoped(typeof(GitProviderFactory), sp =>
{
    var httpClient = sp.GetRequiredService<IHttpClientFactory>().CreateClient();
    return new GitProviderFactory(httpClient);
});
builder.Services.AddHttpClient();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<AgentService>();
builder.Services.AddScoped<ChatService>();
builder.Services.AddScoped<TaskService>();
builder.Services.AddScoped<ScheduledTaskService>();
builder.Services.AddScoped<PipelineService>();
builder.Services.AddScoped<WikiService>();
builder.Services.AddScoped<ConfigService>();
builder.Services.AddScoped<IReviewEngine, AutoCodeForge.Infrastructure.Review.RuleBasedReviewEngine>();
builder.Services.AddScoped<ILlmGateway, LlmGateway>();
builder.Services.AddScoped<ChatSessionManager>();
builder.Services.AddScoped<AgentExecutor>();
builder.Services.AddScoped<AgentMatcher>();
builder.Services.AddScoped<TaskExecutor>();
builder.Services.AddScoped<AutoCodeForge.Infrastructure.Sandbox.SandboxPathResolver>();
builder.Services.AddScoped<AutoCodeForge.Infrastructure.Git.LibGit2SharpProvider>();
builder.Services.AddScoped<GitCloneService>();
builder.Services.AddScoped<AutoCodeForge.Infrastructure.BackgroundServices.Handlers.RepoSyncTaskHandler>();
builder.Services.AddScoped<AutoCodeForge.Infrastructure.BackgroundServices.Handlers.ReviewTaskHandler>();
builder.Services.AddHostedService<TaskQueueService>();
builder.Services.AddHostedService<CronSchedulerService>();
builder.Services.AddHostedService<PipelineSyncService>();
builder.Services.AddScoped<IAgentTool, GitReadToolset>();
builder.Services.AddScoped<IAgentTool, GitWriteToolset>();
builder.Services.AddScoped<DatabaseInitializer>();
builder.Services.AddScoped<SeedData>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

// Configure structured logging with ILogger
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseMiddleware<JwtAuthMiddleware>();

using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
    await initializer.InitializeAsync(app.Environment.IsDevelopment());
}

app.MapGet("/", () => ApiResponse<string>.Ok("AutoCodeForge API is running"));
app.MapHealthEndpoints();
app.MapSystemEndpoints();
app.MapAuthEndpoints();
app.MapConfigEndpoints();
app.MapAgentEndpoints();
app.MapAgentSkillEndpoints();
app.MapChatEndpoints();
app.MapChatStreamEndpoints();
app.MapTaskEndpoints();
app.MapRepoSyncEndpoints();
app.MapReviewEndpoints();
app.MapScheduledTaskEndpoints();
app.MapRepositoryEndpoints();
app.MapPipelineEndpoints();
app.MapWikiEndpoints();
app.MapAdminEndpoints();

app.Run();

public partial class Program
{
}
