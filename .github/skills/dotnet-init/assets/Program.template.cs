using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.SwaggerUI;
using __ProjectName__.Configuration;
using __ProjectName__.Extensions;
using __ProjectName__.Middleware;
using __ProjectName__.Api;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Load configuration
var appSettings = builder.Configuration.Get<AppSettings>() ?? new AppSettings();
builder.Services.AddSingleton(appSettings);

// Add services to the container
builder.Services.AddSqlSugarSetup(builder.Configuration);

// Add controllers and minimal API
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "__ProjectName__", Version = "v1" });
});

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Configure JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = appSettings.JwtSettings.Issuer,
            ValidAudience = appSettings.JwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(appSettings.JwtSettings.Secret))
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddResponseCompression();

var app = builder.Build();

// Initialize database tables and seed data
await app.InitializeDatabaseAsync();

// Configure middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "__ProjectName__ API V1");
        c.RoutePrefix = "swagger";
        c.DocExpansion(DocExpansion.List);
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseResponseCompression();
app.UseMiddleware<ExceptionMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

// Register API endpoints
AuthApi.Register(app);

app.Run();
