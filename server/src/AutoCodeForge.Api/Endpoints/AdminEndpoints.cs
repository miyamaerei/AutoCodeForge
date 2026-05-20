using AutoCodeForge.Application.Services;
using AutoCodeForge.Core.DTOs.Admin;
using AutoCodeForge.Core.Models;

namespace AutoCodeForge.Api.Endpoints;

/// <summary>
/// Registers admin-only endpoints for cross-tenant operations and audit log management.
/// </summary>
public static class AdminEndpoints
{
    /// <summary>
    /// Maps admin endpoints.
    /// </summary>
    /// <param name="app">The route builder.</param>
    /// <returns>The route builder.</returns>
    public static IEndpointRouteBuilder MapAdminEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/admin");

        // GET /api/v1/admin/audit-logs — paged audit log
        group.MapGet("/audit-logs", async (
            int page,
            int pageSize,
            string? adminNtId,
            string? targetNtId,
            AdminAuditService adminAuditService,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            await adminAuditService.AuthorizeCrossTenantAsync(
                httpContext.Request.Method,
                httpContext.Request.Path,
                "AdminAuditLogs",
                targetNtId,
                cancellationToken: cancellationToken);
            var result = await adminAuditService.GetAuditLogsAsync(
                page <= 0 ? 1 : page,
                pageSize <= 0 ? 20 : pageSize,
                adminNtId,
                cancellationToken);
            return Results.Ok(ApiResponse<PagedResult<AdminAuditLogDto>>.Ok(result));
        });

        // GET /api/v1/admin/whitelist — current runtime whitelist
        group.MapGet("/whitelist", async (
            AdminAuditService adminAuditService,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            await adminAuditService.AuthorizeCrossTenantAsync(
                httpContext.Request.Method,
                httpContext.Request.Path,
                "AdminWhitelistRead",
                cancellationToken: cancellationToken);
            var whitelist = await adminAuditService.GetWhitelistAsync(cancellationToken);
            return Results.Ok(ApiResponse<string>.Ok(whitelist));
        });

        // PUT /api/v1/admin/whitelist — update runtime whitelist
        group.MapPut("/whitelist", async (
            UpdateWhitelistRequest request,
            AdminAuditService adminAuditService,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            await adminAuditService.AuthorizeCrossTenantAsync(
                httpContext.Request.Method,
                httpContext.Request.Path,
                "AdminWhitelistWrite",
                allowBootstrapWhenWhitelistEmpty: true,
                cancellationToken: cancellationToken);
            await adminAuditService.SetWhitelistAsync(request.CommaSeparatedNtIds ?? string.Empty, cancellationToken);
            return Results.Ok(ApiResponse<string>.Ok("Whitelist updated"));
        });

        return app;
    }
}

/// <summary>
/// Request body for updating the admin whitelist.
/// </summary>
public sealed class UpdateWhitelistRequest
{
    /// <summary>Gets or sets comma-separated NtIds.</summary>
    public string? CommaSeparatedNtIds { get; set; }
}
