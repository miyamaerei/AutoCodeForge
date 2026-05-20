using AutoCodeForge.Core.DTOs.Admin;
using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Exceptions;
using AutoCodeForge.Core.Interfaces;
using AutoCodeForge.Core.Models;
using AutoCodeForge.Infrastructure.Repositories;

namespace AutoCodeForge.Application.Services;

/// <summary>
/// Enforces admin whitelist policy and writes cross-tenant audit log entries.
/// </summary>
public class AdminAuditService
{
    /// <summary>
    /// Config key used to store a comma-separated runtime admin whitelist in GlobalConfigs.
    /// </summary>
    public const string WhitelistConfigKey = "system:admin_whitelist";

    private const string DecisionAllow = "allow";
    private const string DecisionDeny = "deny";

    private readonly AdminAuditLogRepository _auditLogRepository;
    private readonly GlobalConfigRepository _globalConfigRepository;
    private readonly ICurrentUser _currentUser;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminAuditService"/> class.
    /// </summary>
    /// <param name="auditLogRepository">The admin audit log repository.</param>
    /// <param name="globalConfigRepository">The global config repository.</param>
    /// <param name="currentUser">The current user provider.</param>
    public AdminAuditService(
        AdminAuditLogRepository auditLogRepository,
        GlobalConfigRepository globalConfigRepository,
        ICurrentUser currentUser)
    {
        _auditLogRepository = auditLogRepository;
        _globalConfigRepository = globalConfigRepository;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Verifies that the current caller is authorised to perform admin operations.
    /// For cross-tenant admin operations, callers must be admin and match at least one whitelist rule.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <exception cref="ForbiddenException">Thrown when the caller is not an admin or not on the whitelist.</exception>
    public async Task AssertAdminAsync(CancellationToken cancellationToken = default)
    {
        var ntId = _currentUser.GetCurrentNtId();
        if (string.IsNullOrWhiteSpace(ntId))
        {
            throw new ForbiddenException("Admin access requires authentication");
        }

        if (!_currentUser.IsAdmin())
        {
            throw new ForbiddenException("Caller is not an admin");
        }

        if (await IsWhitelistedForScopeAsync(ntId, "*", null, cancellationToken))
        {
            return;
        }

        throw new ForbiddenException("Caller is not authorised for admin cross-tenant access");
    }

    /// <summary>
    /// Authorises one admin cross-tenant operation and writes a decision audit record.
    /// </summary>
    /// <param name="httpMethod">The HTTP method of the request.</param>
    /// <param name="requestPath">The request path.</param>
    /// <param name="resourceType">The logical resource scope (for example AdminAuditLogs).</param>
    /// <param name="targetNtId">The target tenant NtId being accessed, if applicable.</param>
    /// <param name="resourceId">The resource identifier, if applicable.</param>
    /// <param name="allowBootstrapWhenWhitelistEmpty">Whether to allow admin bootstrap when whitelist is empty.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <exception cref="ForbiddenException">Thrown when caller is unauthorised.</exception>
    public async Task AuthorizeCrossTenantAsync(
        string httpMethod,
        string requestPath,
        string resourceType,
        string? targetNtId = null,
        Guid? resourceId = null,
        bool allowBootstrapWhenWhitelistEmpty = false,
        CancellationToken cancellationToken = default)
    {
        var ntId = _currentUser.GetCurrentNtId();
        if (string.IsNullOrWhiteSpace(ntId))
        {
            await LogAccessAsync(
                httpMethod,
                requestPath,
                ComposeResourceType(resourceType, DecisionDeny, "unauthenticated"),
                resourceId,
                targetNtId,
                cancellationToken);
            throw new ForbiddenException("Admin access requires authentication");
        }

        if (!_currentUser.IsAdmin())
        {
            await LogAccessAsync(
                httpMethod,
                requestPath,
                ComposeResourceType(resourceType, DecisionDeny, "not-admin"),
                resourceId,
                targetNtId,
                cancellationToken);
            throw new ForbiddenException("Caller is not an admin");
        }

        if (await IsWhitelistedForScopeAsync(ntId, resourceType, targetNtId, cancellationToken))
        {
            await LogAccessAsync(
                httpMethod,
                requestPath,
                ComposeResourceType(resourceType, DecisionAllow, "whitelist-match"),
                resourceId,
                targetNtId,
                cancellationToken);
            return;
        }

        if (allowBootstrapWhenWhitelistEmpty && await IsWhitelistEmptyAsync(cancellationToken))
        {
            await LogAccessAsync(
                httpMethod,
                requestPath,
                ComposeResourceType(resourceType, DecisionAllow, "bootstrap-empty-whitelist"),
                resourceId,
                targetNtId,
                cancellationToken);
            return;
        }

        await LogAccessAsync(
            httpMethod,
            requestPath,
            ComposeResourceType(resourceType, DecisionDeny, "not-whitelisted"),
            resourceId,
            targetNtId,
            cancellationToken);
        throw new ForbiddenException("Caller is not authorised for admin cross-tenant access");
    }

    /// <summary>
    /// Records a cross-tenant admin access event.
    /// </summary>
    /// <param name="httpMethod">The HTTP method of the request.</param>
    /// <param name="requestPath">The request path.</param>
    /// <param name="resourceType">The resource type being accessed.</param>
    /// <param name="resourceId">The resource identifier, if applicable.</param>
    /// <param name="targetNtId">The tenant NtId being accessed, if applicable.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task LogAccessAsync(
        string httpMethod,
        string requestPath,
        string? resourceType = null,
        Guid? resourceId = null,
        string? targetNtId = null,
        CancellationToken cancellationToken = default)
    {
        var adminNtId = _currentUser.GetCurrentNtId() ?? "unknown";
        var entry = new AdminAuditLogEntity
        {
            Id = Guid.NewGuid(),
            AdminNtId = adminNtId,
            TargetNtId = targetNtId,
            HttpMethod = httpMethod,
            RequestPath = requestPath,
            ResourceType = resourceType,
            ResourceId = resourceId,
            OccurredAtUtc = DateTime.UtcNow,
        };

        await _auditLogRepository.CreateAsync(entry, cancellationToken);
    }

    /// <summary>
    /// Gets paged admin audit log entries.
    /// </summary>
    /// <param name="page">The page number.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="adminNtId">Optional admin NtId filter.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Paged audit log DTOs.</returns>
    public async Task<PagedResult<AdminAuditLogDto>> GetAuditLogsAsync(
        int page,
        int pageSize,
        string? adminNtId = null,
        CancellationToken cancellationToken = default)
    {
        var (items, total) = await _auditLogRepository.GetPagedLogsAsync(page, pageSize, adminNtId, cancellationToken);
        var normalizedPage = page <= 0 ? 1 : page;
        var normalizedSize = pageSize <= 0 ? 20 : Math.Min(pageSize, 100);
        return new PagedResult<AdminAuditLogDto>(
            items.Select(ToDto).ToList(),
            total,
            normalizedPage,
            normalizedSize);
    }

    /// <summary>
    /// Gets the current runtime admin whitelist from GlobalConfigs.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The comma-separated whitelist string, or empty when not configured.</returns>
    public async Task<string> GetWhitelistAsync(CancellationToken cancellationToken = default)
    {
        var config = await _globalConfigRepository.GetByKeyAsync(WhitelistConfigKey, cancellationToken);
        return config?.ConfigValue ?? string.Empty;
    }

    /// <summary>
    /// Updates the runtime admin whitelist in GlobalConfigs.
    /// </summary>
    /// <param name="commaSeparatedNtIds">Comma-separated NtIds to set as the whitelist.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task SetWhitelistAsync(string commaSeparatedNtIds, CancellationToken cancellationToken = default)
    {
        var normalized = NormalizeWhitelist(commaSeparatedNtIds);
        var existing = await _globalConfigRepository.GetByKeyAsync(WhitelistConfigKey, cancellationToken);
        if (existing is null)
        {
            var entry = new GlobalConfigEntity
            {
                Id = Guid.NewGuid(),
                ConfigKey = WhitelistConfigKey,
                ConfigValue = normalized,
                Description = "Runtime admin whitelist; supports NtId or NtId|ResourceScope|TargetTenant",
            };
            await _globalConfigRepository.CreateAsync(entry, cancellationToken);
        }
        else
        {
            existing.ConfigValue = normalized;
            await _globalConfigRepository.UpdateAsync(existing, cancellationToken);
        }
    }

    private async Task<bool> IsWhitelistedForScopeAsync(
        string ntId,
        string resourceType,
        string? targetNtId,
        CancellationToken cancellationToken)
    {
        var config = await _globalConfigRepository.GetByKeyAsync(WhitelistConfigKey, cancellationToken);
        if (config is null || string.IsNullOrWhiteSpace(config.ConfigValue))
        {
            return false;
        }

        var rules = ParseWhitelistRules(config.ConfigValue);
        if (rules.Count == 0)
        {
            return false;
        }

        var normalizedResource = NormalizeScopeValue(resourceType);
        var normalizedTarget = NormalizeScopeValue(targetNtId);

        foreach (var rule in rules)
        {
            if (!string.Equals(rule.AdminNtId, ntId, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (!ScopeMatches(rule.ResourceScope, normalizedResource))
            {
                continue;
            }

            if (!ScopeMatches(rule.TargetTenantScope, normalizedTarget))
            {
                continue;
            }

            return true;
        }

        return false;
    }

    private async Task<bool> IsWhitelistEmptyAsync(CancellationToken cancellationToken)
    {
        var config = await _globalConfigRepository.GetByKeyAsync(WhitelistConfigKey, cancellationToken);
        if (config is null || string.IsNullOrWhiteSpace(config.ConfigValue))
        {
            return true;
        }

        return ParseWhitelistRules(config.ConfigValue).Count == 0;
    }

    private static string NormalizeWhitelist(string raw)
    {
        var rules = ParseWhitelistRules(raw);
        if (rules.Count == 0)
        {
            return string.Empty;
        }

        var unique = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var values = new List<string>();

        foreach (var rule in rules)
        {
            var normalizedResource = NormalizeScopeValue(rule.ResourceScope);
            var normalizedTarget = NormalizeScopeValue(rule.TargetTenantScope);
            var key = $"{rule.AdminNtId}|{normalizedResource}|{normalizedTarget}";
            if (!unique.Add(key))
            {
                continue;
            }

            if (normalizedResource == "*" && normalizedTarget == "*")
            {
                values.Add(rule.AdminNtId);
                continue;
            }

            values.Add($"{rule.AdminNtId}|{normalizedResource}|{normalizedTarget}");
        }

        return string.Join(',', values);
    }

    private static List<WhitelistRule> ParseWhitelistRules(string raw)
    {
        var rules = new List<WhitelistRule>();
        if (string.IsNullOrWhiteSpace(raw))
        {
            return rules;
        }

        var tokens = raw
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var order = 0;
        foreach (var token in tokens)
        {
            var parts = token
                .Split('|', StringSplitOptions.TrimEntries)
                .Where(static part => !string.IsNullOrWhiteSpace(part))
                .ToArray();

            if (parts.Length == 0)
            {
                continue;
            }

            var adminNtId = parts[0].Trim();
            if (string.IsNullOrWhiteSpace(adminNtId))
            {
                continue;
            }

            var resourceScope = parts.Length > 1 ? parts[1] : "*";
            var targetScope = parts.Length > 2 ? parts[2] : "*";
            rules.Add(new WhitelistRule(
                adminNtId,
                NormalizeScopeValue(resourceScope),
                NormalizeScopeValue(targetScope),
                order++));
        }

        return rules;
    }

    private static string ComposeResourceType(string resourceType, string decision, string reasonCode)
    {
        var baseType = string.IsNullOrWhiteSpace(resourceType) ? "UnknownResource" : resourceType.Trim();
        return $"{baseType}#{decision}:{reasonCode}";
    }

    private static bool ScopeMatches(string ruleScope, string actual)
    {
        return ruleScope == "*"
            || string.Equals(ruleScope, actual, StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizeScopeValue(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "*";
        }

        return value.Trim();
    }

    private static (string Resource, string Decision, string Reason) ParseResourceType(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return ("UnknownResource", "unknown", string.Empty);
        }

        var index = value.IndexOf('#');
        if (index < 0)
        {
            return (value, "unknown", string.Empty);
        }

        var resource = value[..index];
        var decisionAndReason = value[(index + 1)..];
        var separator = decisionAndReason.IndexOf(':');
        if (separator < 0)
        {
            return (resource, decisionAndReason, string.Empty);
        }

        var decision = decisionAndReason[..separator];
        var reason = decisionAndReason[(separator + 1)..];
        return (resource, decision, reason);
    }

    private static AdminAuditLogDto ToDto(AdminAuditLogEntity entity)
    {
        var parsed = ParseResourceType(entity.ResourceType);
        return new AdminAuditLogDto
        {
            Id = entity.Id,
            AdminNtId = entity.AdminNtId,
            TargetNtId = entity.TargetNtId,
            HttpMethod = entity.HttpMethod,
            RequestPath = entity.RequestPath,
            ResourceType = parsed.Resource,
            AccessDecision = parsed.Decision,
            DecisionReason = parsed.Reason,
            ResourceId = entity.ResourceId,
            OccurredAtUtc = entity.OccurredAtUtc,
        };
    }

    private sealed record WhitelistRule(
        string AdminNtId,
        string ResourceScope,
        string TargetTenantScope,
        int Order);
}
