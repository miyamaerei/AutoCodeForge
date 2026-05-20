using System.Reflection;
using AutoCodeForge.Api.Endpoints;
using AutoCodeForge.Api.Middleware;
using AutoCodeForge.Application.Services;
using AutoCodeForge.Core.Interfaces;
using AutoCodeForge.Core.Models;
using AutoCodeForge.Infrastructure.AI;
using AutoCodeForge.Infrastructure.Data;
using AutoCodeForge.Infrastructure.Helpers;
using AutoCodeForge.Infrastructure.Repositories;
using AutoCodeForge.Infrastructure.Repositories.Base;
using AutoCodeForge.Infrastructure.Services;

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
builder.Services.AddSingleton<PasswordHelper>();
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<AgentService>();
builder.Services.AddScoped<ChatService>();
builder.Services.AddScoped<ILlmGateway, LlmGateway>();
builder.Services.AddScoped<ChatSessionManager>();
builder.Services.AddScoped<AgentExecutor>();
builder.Services.AddScoped<AgentMatcher>();
builder.Services.AddScoped<IEnumerable<IAgentTool>>(_ => []);
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
app.MapGet("/health", () => ApiResponse<string>.Ok("healthy"));
app.MapAuthEndpoints();
app.MapAgentEndpoints();
app.MapChatEndpoints();
app.MapChatStreamEndpoints();

app.Run();

public partial class Program
{
}
