using System.ComponentModel.DataAnnotations;
using AutoCodeForge.Application.Services;
using AutoCodeForge.Core.DTOs.Config;
using AutoCodeForge.Core.Models;

namespace AutoCodeForge.Api.Endpoints;

/// <summary>
/// Registers config-related API endpoints for global and user configurations.
/// </summary>
public static class ConfigEndpoints
{
    /// <summary>
    /// Maps config endpoints.
    /// </summary>
    /// <param name="app">The route builder.</param>
    /// <returns>The route builder.</returns>
    public static IEndpointRouteBuilder MapConfigEndpoints(this IEndpointRouteBuilder app)
    {
        var globalGroup = app.MapGroup("/api/v1/configs/global");
        var userGroup = app.MapGroup("/api/v1/configs/user");

        // Global config endpoints (admin only)
        globalGroup.MapGet("/", GetGlobalConfigs);
        globalGroup.MapGet("/{configKey}", GetGlobalConfigByKey);
        globalGroup.MapPost("/", UpsertGlobalConfig);
        globalGroup.MapPut("/{configKey}", UpdateGlobalConfig);
        globalGroup.MapDelete("/{configKey}", DeleteGlobalConfig);

        // User config endpoints (user-scoped)
        userGroup.MapGet("/", GetUserConfigs);
        userGroup.MapGet("/{configKey}", GetUserConfigByKey);
        userGroup.MapPost("/", UpsertUserConfig);
        userGroup.MapPut("/{configKey}", UpdateUserConfig);
        userGroup.MapDelete("/{configKey}", DeleteUserConfig);
        userGroup.MapGet("/sandbox", GetSandboxConfig);
        userGroup.MapPut("/sandbox", UpsertSandboxConfig);

        return app;
    }

    private static async Task<IResult> GetGlobalConfigs(
        int page,
        int pageSize,
        ConfigService service,
        HttpContext context,
        CancellationToken ct)
    {
        if (!IsAdmin(context))
        {
            return Results.Forbid();
        }

        var result = await service.GetGlobalConfigsAsync(
            page <= 0 ? 1 : page,
            pageSize <= 0 ? 20 : pageSize,
            ct);
        return Results.Ok(ApiResponse<PagedResult<ConfigResponse>>.Ok(result));
    }

    private static async Task<IResult> GetGlobalConfigByKey(
        string configKey,
        ConfigService service,
        HttpContext context,
        CancellationToken ct)
    {
        if (!IsAdmin(context))
        {
            return Results.Forbid();
        }

        var result = await service.GetGlobalConfigAsync(configKey, ct);
        if (result is null)
        {
            return Results.NotFound(ApiResponse<object?>.Fail("Config not found"));
        }

        return Results.Ok(ApiResponse<ConfigResponse>.Ok(result));
    }

    private static async Task<IResult> UpsertGlobalConfig(
        UpdateConfigRequest request,
        ConfigService service,
        HttpContext context,
        CancellationToken ct)
    {
        if (!IsAdmin(context))
        {
            return Results.Forbid();
        }

        ValidateModel(request);
        var result = await service.UpsertGlobalConfigAsync(request, ct);
        return Results.Ok(ApiResponse<ConfigResponse>.Ok(result, "Global config saved"));
    }

    private static async Task<IResult> UpdateGlobalConfig(
        string configKey,
        UpdateConfigRequest request,
        ConfigService service,
        HttpContext context,
        CancellationToken ct)
    {
        if (!IsAdmin(context))
        {
            return Results.Forbid();
        }

        ValidateModel(request);
        request.ConfigKey = configKey;
        var result = await service.UpsertGlobalConfigAsync(request, ct);
        return Results.Ok(ApiResponse<ConfigResponse>.Ok(result, "Global config updated"));
    }

    private static async Task<IResult> DeleteGlobalConfig(
        string configKey,
        ConfigService service,
        HttpContext context,
        CancellationToken ct)
    {
        if (!IsAdmin(context))
        {
            return Results.Forbid();
        }

        await service.DeleteGlobalConfigAsync(configKey, ct);
        return Results.Ok(ApiResponse<object?>.Ok(null, "Global config deleted"));
    }

    private static async Task<IResult> GetUserConfigs(
        int page,
        int pageSize,
        ConfigService service,
        CancellationToken ct)
    {
        var result = await service.GetUserConfigsAsync(
            page <= 0 ? 1 : page,
            pageSize <= 0 ? 20 : pageSize,
            ct);
        return Results.Ok(ApiResponse<PagedResult<ConfigResponse>>.Ok(result));
    }

    private static async Task<IResult> GetUserConfigByKey(
        string configKey,
        ConfigService service,
        CancellationToken ct)
    {
        var result = await service.GetUserConfigAsync(configKey, ct);
        if (result is null)
        {
            return Results.NotFound(ApiResponse<object?>.Fail("Config not found"));
        }

        return Results.Ok(ApiResponse<ConfigResponse>.Ok(result));
    }

    private static async Task<IResult> UpsertUserConfig(
        UpdateConfigRequest request,
        ConfigService service,
        CancellationToken ct)
    {
        ValidateModel(request);
        var result = await service.UpsertUserConfigAsync(request, ct);
        return Results.Ok(ApiResponse<ConfigResponse>.Ok(result, "User config saved"));
    }

    private static async Task<IResult> UpdateUserConfig(
        string configKey,
        UpdateConfigRequest request,
        ConfigService service,
        CancellationToken ct)
    {
        ValidateModel(request);
        request.ConfigKey = configKey;
        var result = await service.UpsertUserConfigAsync(request, ct);
        return Results.Ok(ApiResponse<ConfigResponse>.Ok(result, "User config updated"));
    }

    private static async Task<IResult> DeleteUserConfig(
        string configKey,
        ConfigService service,
        CancellationToken ct)
    {
        await service.DeleteUserConfigAsync(configKey, ct);
        return Results.Ok(ApiResponse<object?>.Ok(null, "User config deleted"));
    }

    private static async Task<IResult> GetSandboxConfig(ConfigService service, CancellationToken ct)
    {
        var result = await service.GetSandboxConfigAsync(ct);
        if (result is null)
        {
            return Results.NotFound(ApiResponse<object?>.Fail("Sandbox config not found"));
        }

        return Results.Ok(ApiResponse<SandboxConfigDto>.Ok(result));
    }

    private static async Task<IResult> UpsertSandboxConfig(
        SandboxConfigDto request,
        ConfigService service,
        CancellationToken ct)
    {
        ValidateModel(request);
        var result = await service.UpsertSandboxConfigAsync(request, ct);
        return Results.Ok(ApiResponse<SandboxConfigDto>.Ok(result, "Sandbox config saved"));
    }

    private static void ValidateModel(object request)
    {
        var context = new ValidationContext(request);
        var results = new List<ValidationResult>();
        if (!Validator.TryValidateObject(request, context, results, true))
        {
            var message = string.Join("; ", results.Select(result => result.ErrorMessage));
            throw new AutoCodeForge.Core.Exceptions.ValidationException(message);
        }
    }

    private static bool IsAdmin(HttpContext context)
    {
        var role = context.User.FindFirst("Role")?.Value;
        return role == "Admin";
    }
}
