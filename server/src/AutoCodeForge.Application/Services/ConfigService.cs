using AutoCodeForge.Core.DTOs.Config;
using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Exceptions;
using AutoCodeForge.Core.Helpers;
using AutoCodeForge.Core.Models;
using AutoCodeForge.Application.Validators;
using AutoCodeForge.Infrastructure.Repositories;

namespace AutoCodeForge.Application.Services;

/// <summary>
/// Provides configuration management for global and user-specific configs.
/// </summary>
public class ConfigService
{
    private const string SandboxConfigKey = "SandboxConfig";

    private readonly GlobalConfigRepository _globalConfigRepository;
    private readonly UserConfigRepository _userConfigRepository;
    private readonly SandboxConfigValidator _sandboxConfigValidator;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigService"/> class.
    /// </summary>
    /// <param name="globalConfigRepository">The global config repository.</param>
    /// <param name="userConfigRepository">The user config repository.</param>
    public ConfigService(
        GlobalConfigRepository globalConfigRepository,
        UserConfigRepository userConfigRepository,
        SandboxConfigValidator sandboxConfigValidator)
    {
        _globalConfigRepository = globalConfigRepository;
        _userConfigRepository = userConfigRepository;
        _sandboxConfigValidator = sandboxConfigValidator;
    }

    /// <summary>
    /// Gets sandbox config for current user.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The sandbox config; null when not configured.</returns>
    public async Task<SandboxConfigDto?> GetSandboxConfigAsync(CancellationToken cancellationToken = default)
    {
        var config = await _userConfigRepository.GetByKeyAsync(SandboxConfigKey, cancellationToken);
        if (config is null || string.IsNullOrWhiteSpace(config.ConfigValue))
        {
            return null;
        }

        return JsonHelper.Deserialize<SandboxConfigDto>(config.ConfigValue);
    }

    /// <summary>
    /// Creates or updates sandbox config for current user.
    /// </summary>
    /// <param name="sandboxConfig">The sandbox config payload.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated sandbox config.</returns>
    public async Task<SandboxConfigDto> UpsertSandboxConfigAsync(
        SandboxConfigDto sandboxConfig,
        CancellationToken cancellationToken = default)
    {
        if (sandboxConfig is null)
        {
            throw new ValidationException("Sandbox config cannot be null");
        }

        _sandboxConfigValidator.ValidateAndThrow(sandboxConfig);
        var payload = JsonHelper.Serialize(sandboxConfig);
        var existing = await _userConfigRepository.GetByKeyAsync(SandboxConfigKey, cancellationToken);

        if (existing is null)
        {
            var entity = new UserConfigEntity
            {
                Id = Guid.NewGuid(),
                ConfigKey = SandboxConfigKey,
                ConfigValue = payload,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow,
            };

            await _userConfigRepository.CreateAsync(entity, cancellationToken);
            return sandboxConfig;
        }

        existing.ConfigValue = payload;
        existing.UpdatedAtUtc = DateTime.UtcNow;
        await _userConfigRepository.UpdateAsync(existing, cancellationToken);
        return sandboxConfig;
    }

    /// <summary>
    /// Gets a global config by key.
    /// </summary>
    /// <param name="configKey">The config key.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The config response or null.</returns>
    public async Task<ConfigResponse?> GetGlobalConfigAsync(string configKey, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(configKey))
        {
            throw new ValidationException("Config key cannot be empty");
        }

        var config = await _globalConfigRepository.GetByKeyAsync(configKey, cancellationToken);
        return config is null ? null : MapToResponse(config);
    }

    /// <summary>
    /// Gets all global configs with pagination.
    /// </summary>
    /// <param name="page">The page number (1-based).</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Paginated global configs.</returns>
    public async Task<PagedResult<ConfigResponse>> GetGlobalConfigsAsync(
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _globalConfigRepository.GetPagedAsync(page, pageSize, false, cancellationToken);
        var mappedItems = result.Items.Select(MapToResponse).ToList();
        return new PagedResult<ConfigResponse>(mappedItems, result.TotalCount, result.Page, result.PageSize);
    }

    /// <summary>
    /// Creates or updates a global config.
    /// </summary>
    /// <param name="request">The config update request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created/updated config response.</returns>
    public async Task<ConfigResponse> UpsertGlobalConfigAsync(
        UpdateConfigRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            throw new ValidationException("Config request cannot be null");
        }

        if (string.IsNullOrWhiteSpace(request.ConfigKey) || string.IsNullOrWhiteSpace(request.ConfigValue))
        {
            throw new ValidationException("ConfigKey and ConfigValue cannot be empty");
        }

        var existing = await _globalConfigRepository.GetByKeyAsync(request.ConfigKey, cancellationToken);

        if (existing is null)
        {
            var newConfig = new GlobalConfigEntity
            {
                Id = Guid.NewGuid(),
                ConfigKey = request.ConfigKey.Trim(),
                ConfigValue = request.ConfigValue.Trim(),
                Description = request.Description?.Trim(),
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow,
            };

            await _globalConfigRepository.CreateAsync(newConfig, cancellationToken);
            return MapToResponse(newConfig);
        }

        existing.ConfigValue = request.ConfigValue.Trim();
        existing.Description = request.Description?.Trim();
        existing.UpdatedAtUtc = DateTime.UtcNow;

        await _globalConfigRepository.UpdateAsync(existing, cancellationToken);
        return MapToResponse(existing);
    }

    /// <summary>
    /// Deletes a global config by key.
    /// </summary>
    /// <param name="configKey">The config key.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task DeleteGlobalConfigAsync(string configKey, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(configKey))
        {
            throw new ValidationException("Config key cannot be empty");
        }

        var config = await _globalConfigRepository.GetByKeyAsync(configKey, cancellationToken);
        if (config is null)
        {
            throw new NotFoundException($"Global config '{configKey}' not found");
        }

        await _globalConfigRepository.SoftDeleteAsync(config.Id, false, cancellationToken);
    }

    /// <summary>
    /// Gets a user config by key for the current user.
    /// </summary>
    /// <param name="configKey">The config key.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The config response or null.</returns>
    public async Task<ConfigResponse?> GetUserConfigAsync(string configKey, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(configKey))
        {
            throw new ValidationException("Config key cannot be empty");
        }

        var config = await _userConfigRepository.GetByKeyAsync(configKey, cancellationToken);
        return config is null ? null : MapToResponse(config);
    }

    /// <summary>
    /// Gets all user configs for the current user with pagination.
    /// </summary>
    /// <param name="page">The page number (1-based).</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Paginated user configs.</returns>
    public async Task<PagedResult<ConfigResponse>> GetUserConfigsAsync(
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _userConfigRepository.GetPagedAsync(page, pageSize, false, cancellationToken);
        var mappedItems = result.Items.Select(MapToResponse).ToList();
        return new PagedResult<ConfigResponse>(mappedItems, result.TotalCount, result.Page, result.PageSize);
    }

    /// <summary>
    /// Creates or updates a user config for the current user.
    /// </summary>
    /// <param name="request">The config update request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created/updated config response.</returns>
    public async Task<ConfigResponse> UpsertUserConfigAsync(
        UpdateConfigRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            throw new ValidationException("Config request cannot be null");
        }

        if (string.IsNullOrWhiteSpace(request.ConfigKey) || string.IsNullOrWhiteSpace(request.ConfigValue))
        {
            throw new ValidationException("ConfigKey and ConfigValue cannot be empty");
        }

        var existing = await _userConfigRepository.GetByKeyAsync(request.ConfigKey, cancellationToken);

        if (existing is null)
        {
            var newConfig = new UserConfigEntity
            {
                Id = Guid.NewGuid(),
                ConfigKey = request.ConfigKey.Trim(),
                ConfigValue = request.ConfigValue.Trim(),
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow,
            };

            await _userConfigRepository.CreateAsync(newConfig, cancellationToken);
            return MapToResponse(newConfig);
        }

        existing.ConfigValue = request.ConfigValue.Trim();
        existing.UpdatedAtUtc = DateTime.UtcNow;

        await _userConfigRepository.UpdateAsync(existing, cancellationToken);
        return MapToResponse(existing);
    }

    /// <summary>
    /// Deletes a user config by key.
    /// </summary>
    /// <param name="configKey">The config key.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task DeleteUserConfigAsync(string configKey, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(configKey))
        {
            throw new ValidationException("Config key cannot be empty");
        }

        var config = await _userConfigRepository.GetByKeyAsync(configKey, cancellationToken);
        if (config is null)
        {
            throw new NotFoundException($"User config '{configKey}' not found");
        }

        await _userConfigRepository.SoftDeleteAsync(config.Id, false, cancellationToken);
    }

    private static ConfigResponse MapToResponse(GlobalConfigEntity entity)
    {
        return new ConfigResponse
        {
            Id = entity.Id,
            ConfigKey = entity.ConfigKey,
            ConfigValue = entity.ConfigValue,
            Description = entity.Description,
            CreatedAt = entity.CreatedAtUtc,
            UpdatedAt = entity.UpdatedAtUtc,
        };
    }

    private static ConfigResponse MapToResponse(UserConfigEntity entity)
    {
        return new ConfigResponse
        {
            Id = entity.Id,
            ConfigKey = entity.ConfigKey,
            ConfigValue = entity.ConfigValue,
            CreatedAt = entity.CreatedAtUtc,
            UpdatedAt = entity.UpdatedAtUtc,
        };
    }
}
