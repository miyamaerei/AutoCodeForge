using System.ComponentModel.DataAnnotations;
using AutoCodeForge.Application.Services;
using AutoCodeForge.Core.DTOs.Config;
using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Enums;
using AutoCodeForge.Core.Models;
using AutoCodeForge.Core.Providers;

namespace AutoCodeForge.Api.Endpoints;

/// <summary>
/// Registers config-related API endpoints for the unified configuration system.
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
        var configGroup = app.MapGroup("/api/v1/configs");

        // CRUD endpoints for specific config types
        configGroup.MapGet("/{configType}", GetConfigsByType);
        configGroup.MapGet("/{configType}/{configKey}", GetConfigByTypeAndKey);
        configGroup.MapPost("/{configType}", CreateConfig);
        configGroup.MapPut("/{configType}/{configKey}", UpdateConfig);
        configGroup.MapDelete("/{configType}/{configKey}", DeleteConfig);

        // Initialization and reset endpoints
        configGroup.MapPost("/{configType}/init", InitConfig);
        configGroup.MapPost("/{configType}/reset", ResetConfig);
        configGroup.MapGet("/{configType}/defaults", GetConfigDefaults);

        // History endpoints
        configGroup.MapGet("/history", GetConfigHistory);
        configGroup.MapGet("/history/{configId}", GetConfigHistoryById);
        configGroup.MapPost("/history/{historyId}/rollback", RollbackConfig);

        // Import/Export endpoints
        configGroup.MapGet("/{configType}/export", ExportConfig);
        configGroup.MapPost("/{configType}/import", ImportConfig);
        configGroup.MapPost("/batch", BatchUpdateConfig);

        // User configs
        configGroup.MapGet("/user/all", GetAllUserConfigs);

        // Sandbox config (backward compatibility)
        configGroup.MapGet("/user/sandbox", GetSandboxConfig);
        configGroup.MapPut("/user/sandbox", UpdateSandboxConfig);

        return app;
    }

    private static async Task<IResult> GetConfigsByType(
        ConfigType configType,
        ConfigService service,
        HttpContext context,
        CancellationToken ct)
    {
        if (!HasAccess(context, configType))
        {
            return Results.Forbid();
        }

        var configs = await service.GetByTypeAsync(configType, IsAdmin(context), ct);
        var response = configs.Select(MapToResponse).ToList();
        return Results.Ok(ApiResponse<List<ConfigResponse>>.Ok(response));
    }

    private static async Task<IResult> GetConfigByTypeAndKey(
        ConfigType configType,
        string configKey,
        ConfigService service,
        HttpContext context,
        CancellationToken ct)
    {
        if (!HasAccess(context, configType))
        {
            return Results.Forbid();
        }

        var config = await service.GetByTypeAndKeyAsync(configType, configKey, IsAdmin(context), ct);
        if (config is null)
        {
            return Results.NotFound(ApiResponse<object?>.Fail("Config not found"));
        }

        return Results.Ok(ApiResponse<ConfigResponse>.Ok(MapToResponse(config)));
    }

    private static async Task<IResult> CreateConfig(
        ConfigType configType,
        ConfigRequest request,
        ConfigService service,
        HttpContext context,
        CancellationToken ct)
    {
        if (!HasWriteAccess(context, configType))
        {
            return Results.Forbid();
        }

        ValidateModel(request);
        var config = await service.UpsertAsync(
            configType,
            request.ConfigKey,
            request.ConfigValue,
            request.IsEncrypted,
            request.Description,
            request.Group,
            ct);

        return Results.Created($"/api/v1/configs/{configType}/{request.ConfigKey}",
            ApiResponse<ConfigResponse>.Ok(MapToResponse(config), "Config created"));
    }

    private static async Task<IResult> UpdateConfig(
        ConfigType configType,
        string configKey,
        ConfigRequest request,
        ConfigService service,
        HttpContext context,
        CancellationToken ct)
    {
        if (!HasWriteAccess(context, configType))
        {
            return Results.Forbid();
        }

        ValidateModel(request);
        var config = await service.UpsertAsync(
            configType,
            configKey,
            request.ConfigValue,
            request.IsEncrypted,
            request.Description,
            request.Group,
            ct);

        return Results.Ok(ApiResponse<ConfigResponse>.Ok(MapToResponse(config), "Config updated"));
    }

    private static async Task<IResult> DeleteConfig(
        ConfigType configType,
        string configKey,
        ConfigService service,
        HttpContext context,
        CancellationToken ct)
    {
        if (!HasWriteAccess(context, configType))
        {
            return Results.Forbid();
        }

        await service.DeleteAsync(configType, configKey, ct);
        return Results.Ok(ApiResponse<object?>.Ok(null, "Config deleted"));
    }

    private static async Task<IResult> InitConfig(
        ConfigType configType,
        InitConfigRequest? request,
        ConfigInitializationService service,
        HttpContext context,
        CancellationToken ct)
    {
        if (!HasAccess(context, configType))
        {
            return Results.Forbid();
        }

        var ntId = request?.NtId ?? GetCurrentNtId(context);
        if (string.IsNullOrEmpty(ntId))
        {
            return Results.BadRequest(ApiResponse<object?>.Fail("NTID is required"));
        }

        bool initialized = await service.InitializeModuleDefaultsAsync(configType, ct);

        var result = new ConfigInitResult
        {
            Success = initialized,
            InitializedCount = initialized ? 1 : 0,
            Message = initialized ? "Config initialized successfully" : "Config already exists"
        };

        return Results.Ok(ApiResponse<ConfigInitResult>.Ok(result));
    }

    private static async Task<IResult> ResetConfig(
        ConfigType configType,
        ResetConfigRequest? request,
        ConfigInitializationService service,
        HttpContext context,
        CancellationToken ct)
    {
        if (!HasWriteAccess(context, configType))
        {
            return Results.Forbid();
        }

        var ntId = request?.NtId ?? GetCurrentNtId(context);
        bool reset = await service.ResetToDefaultsAsync(configType, ct);

        return reset
            ? Results.Ok(ApiResponse<object?>.Ok(null, "Config reset to defaults"))
            : Results.NotFound(ApiResponse<object?>.Fail("Config not found"));
    }

    private static IResult GetConfigDefaults(ConfigType configType)
    {
        var template = new ConfigTemplateResponse
        {
            ConfigType = configType,
            DefaultKey = ConfigDefaultsProvider.GetDefaultKey(configType),
            Description = ConfigDefaultsProvider.GetDescription(configType),
            DefaultValue = ConfigDefaultsProvider.GetDefaultValue(configType)
        };

        return Results.Ok(ApiResponse<ConfigTemplateResponse>.Ok(template));
    }

    private static async Task<IResult> GetConfigHistory(
        ConfigType? configType,
        string? changedBy,
        int page,
        int pageSize,
        ConfigHistoryService service,
        HttpContext context,
        CancellationToken ct)
    {
        if (!IsAdmin(context))
        {
            return Results.Forbid();
        }

        List<ConfigHistoryEntity> history;

        if (configType.HasValue)
        {
            history = await service.GetByConfigTypeAsync(configType.Value, page, pageSize, ct);
        }
        else if (!string.IsNullOrWhiteSpace(changedBy))
        {
            history = await service.GetByChangedByAsync(changedBy, page, pageSize, ct);
        }
        else
        {
            // Return all history records when no filter is provided
            history = await service.GetAllAsync(page, pageSize, ct);
        }

        var response = history.Select(MapToHistoryResponse).ToList();
        return Results.Ok(ApiResponse<List<ConfigHistoryResponse>>.Ok(response));
    }

    private static async Task<IResult> GetConfigHistoryById(
        Guid configId,
        int page,
        int pageSize,
        ConfigHistoryService service,
        HttpContext context,
        CancellationToken ct)
    {
        if (!IsAdmin(context))
        {
            return Results.Forbid();
        }

        var history = await service.GetByConfigIdAsync(configId, page, pageSize, ct);
        var response = history.Select(MapToHistoryResponse).ToList();
        return Results.Ok(ApiResponse<List<ConfigHistoryResponse>>.Ok(response));
    }

    private static async Task<IResult> RollbackConfig(
        Guid historyId,
        ConfigHistoryService service,
        HttpContext context,
        CancellationToken ct)
    {
        if (!IsAdmin(context))
        {
            return Results.Forbid();
        }

        var config = await service.RollbackAsync(historyId, ct);
        return Results.Ok(ApiResponse<ConfigResponse>.Ok(MapToResponse(config), "Config rolled back"));
    }

    private static async Task<IResult> ExportConfig(
        ConfigType configType,
        ConfigExportService service,
        HttpContext context,
        CancellationToken ct)
    {
        if (!IsAdmin(context))
        {
            return Results.Forbid();
        }

        var ntId = GetCurrentNtId(context);
        var json = await service.ExportByTypeAsync(configType, ntId, true, ct);
        return Results.Ok(ApiResponse<string>.Ok(json));
    }

    private static async Task<IResult> ImportConfig(
        ConfigType configType,
        ImportConfigRequest request,
        ConfigExportService service,
        HttpContext context,
        CancellationToken ct)
    {
        if (!IsAdmin(context))
        {
            return Results.Forbid();
        }

        if (string.IsNullOrWhiteSpace(request.JsonData))
        {
            return Results.BadRequest(ApiResponse<object?>.Fail("JSON data cannot be empty"));
        }

        var ntId = GetCurrentNtId(context);
        int count = await service.ImportAsync(request.JsonData, ntId, request.OverwriteExisting, ct);

        var result = new ConfigImportResult
        {
            Success = true,
            ImportedCount = count,
            Message = $"Imported {count} configurations"
        };

        return Results.Ok(ApiResponse<ConfigImportResult>.Ok(result));
    }

    private static async Task<IResult> BatchUpdateConfig(
        BatchConfigRequest request,
        ConfigExportService service,
        HttpContext context,
        CancellationToken ct)
    {
        if (!IsAdmin(context))
        {
            return Results.Forbid();
        }

        if (request.Configs == null || request.Configs.Count == 0)
        {
            return Results.BadRequest(ApiResponse<object?>.Fail("No configurations provided"));
        }

        var ntId = GetCurrentNtId(context);
        var configs = request.Configs.Select(c => new ConfigurationEntry
        {
            ConfigType = c.ConfigType,
            ConfigKey = c.ConfigKey,
            ConfigValue = c.ConfigValue,
            IsEncrypted = c.IsEncrypted,
            IsEnabled = c.IsEnabled,
            Description = c.Description,
            Group = c.Group
        }).ToList();

        int count = await service.BatchUpdateAsync(configs, ntId, ct);
        return Results.Ok(ApiResponse<object?>.Ok(null, $"Updated {count} configurations"));
    }

    private static async Task<IResult> GetAllUserConfigs(
        ConfigService service,
        HttpContext context,
        CancellationToken ct)
    {
        var configs = await service.GetConfigsByNtIdAsync(ct);
        var response = configs.Select(MapToResponse).ToList();
        return Results.Ok(ApiResponse<List<ConfigResponse>>.Ok(response));
    }

    private static async Task<IResult> GetSandboxConfig(ConfigService service, HttpContext context, CancellationToken ct)
    {
        var result = await service.GetSandboxConfigAsync(ct);
        if (result is null)
        {
            return Results.NotFound(ApiResponse<object?>.Fail("Sandbox config not found"));
        }

        return Results.Ok(ApiResponse<SandboxConfigDto>.Ok(result));
    }

    private static async Task<IResult> UpdateSandboxConfig(
        SandboxConfigDto request,
        ConfigService service,
        HttpContext context,
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

    private static string? GetCurrentNtId(HttpContext context)
    {
        return context.User.FindFirst("NtId")?.Value;
    }

    private static bool HasAccess(HttpContext context, ConfigType configType)
    {
        if (IsAdmin(context))
        {
            return true;
        }

        return configType switch
        {
            ConfigType.Preferences => true,
            ConfigType.Sandbox => true,
            ConfigType.Workflow => true,
            ConfigType.Notification => true,
            ConfigType.Knowledge => true,
            ConfigType.Schedule => true,
            ConfigType.Review => true,
            ConfigType.DeepWiki => true,
            ConfigType.Integration => true,
            ConfigType.Skill => true,
            ConfigType.Repository => true,
            ConfigType.Llm => true,
            ConfigType.Git => true,
            _ => false
        };
    }

    private static bool HasWriteAccess(HttpContext context, ConfigType configType)
    {
        if (IsAdmin(context))
        {
            return true;
        }

        return configType switch
        {
            ConfigType.Preferences => true,
            ConfigType.Sandbox => true,
            ConfigType.Workflow => true,
            ConfigType.Notification => true,
            ConfigType.Knowledge => true,
            ConfigType.Schedule => true,
            ConfigType.Review => true,
            ConfigType.DeepWiki => true,
            ConfigType.Integration => true,
            ConfigType.Skill => true,
            ConfigType.Repository => true,
            ConfigType.Llm => true,
            ConfigType.Git => true,
            _ => false
        };
    }

    private static ConfigResponse MapToResponse(ConfigurationEntry entity)
    {
        return new ConfigResponse
        {
            Id = entity.Id,
            ConfigType = entity.ConfigType,
            ConfigKey = entity.ConfigKey,
            ConfigValue = entity.ConfigValue,
            IsEncrypted = entity.IsEncrypted,
            IsEnabled = entity.IsEnabled,
            Description = entity.Description,
            Group = entity.Group,
            CreatedBy = entity.CreatedBy,
            UpdatedBy = entity.UpdatedBy,
            CreatedAt = entity.CreatedAtUtc,
            UpdatedAt = entity.UpdatedAtUtc
        };
    }

    private static ConfigHistoryResponse MapToHistoryResponse(ConfigHistoryEntity entity)
    {
        return new ConfigHistoryResponse
        {
            Id = entity.Id,
            ConfigId = entity.ConfigId,
            ConfigType = entity.ConfigType,
            ConfigKey = entity.ConfigKey,
            PreviousValue = entity.PreviousValue,
            NewValue = entity.NewValue,
            Operation = entity.Operation,
            ChangedBy = entity.ChangedBy,
            ChangedAt = entity.CreatedAtUtc
        };
    }
}
