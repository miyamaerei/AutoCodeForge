using System.Reflection;
using AutoCodeForge.Api.Endpoints;
using AutoCodeForge.Api.Extensions;
using AutoCodeForge.Api.Middleware;
using AutoCodeForge.Core.Models;
using AutoCodeForge.Infrastructure.Data;
using AutoCodeForge.Infrastructure.Logging;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Configure CORS
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
var isDevelopment = builder.Environment.IsDevelopment();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        if (isDevelopment || allowedOrigins.Length == 0)
        {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        }
        else
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .SetIsOriginAllowedToAllowWildcardSubdomains();
        }
    });
});

// 配置 Jwt 和 Git 选项
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
builder.Services.PostConfigure<JwtOptions>(options =>
{
    var envKey = builder.Configuration["JWT_KEY"];
    if (!string.IsNullOrWhiteSpace(envKey))
    {
        options.Key = envKey;
    }
});
builder.Services.Configure<GitOptions>(builder.Configuration.GetSection(GitOptions.SectionName));

// 核心基础服务（需要最先注册）
builder.Services.AddCoreServices();
builder.Services.AddDataProtection();
builder.Services.AddSqlSugar(builder.Configuration);
builder.Services.AddHttpClient();

// 分组注册依赖
builder.Services.AddRepositories();
builder.Services.AddApplicationServices();
builder.Services.AddAIServices();
builder.Services.AddAgentTools();
builder.Services.AddGitServices();
builder.Services.AddBackgroundServices();
builder.Services.AddFactoryServices(builder.Configuration);

// Swagger 配置
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

// 日志配置
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseCors("AllowSpecificOrigins");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 开发环境禁用 HTTPS 重定向，避免前端代理问题
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
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
app.MapTaskStepEndpoints();
app.MapTaskOrchestrationEndpoints();
app.MapAgentCommunicationEndpoints();
app.MapFailureRecoveryEndpoints();
app.MapAgentRegistrationEndpoints();
app.MapNotificationEndpoints();
app.MapHumanGateEndpoints();
app.MapRepoSyncEndpoints();
app.MapReviewEndpoints();
app.MapScheduledTaskEndpoints();
app.MapRepositoryEndpoints();
app.MapPipelineEndpoints();
app.MapWikiEndpoints();
app.MapAdminEndpoints();
app.MapDashboardEndpoints();
app.MapWorkflowEndpoints();

app.Run();

public partial class Program { }
