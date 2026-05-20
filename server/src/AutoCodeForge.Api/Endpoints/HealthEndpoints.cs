using AutoCodeForge.Core.Models;
using SqlSugar;

namespace AutoCodeForge.Api.Endpoints;

/// <summary>
/// Registers health check endpoints for monitoring system status.
/// </summary>
public static class HealthEndpoints
{
    /// <summary>
    /// Maps health check endpoints.
    /// </summary>
    /// <param name="app">The route builder.</param>
    /// <returns>The route builder.</returns>
    public static IEndpointRouteBuilder MapHealthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/health", async (ISqlSugarClient db, CancellationToken cancellationToken) =>
        {
            try
            {
                _ = cancellationToken;
                await db.Ado.ExecuteCommandAsync("SELECT 1");

                return Results.Ok(new
                {
                    status = "healthy",
                    timestamp = DateTime.UtcNow,
                    checks = new
                    {
                        database = "ok",
                    },
                });
            }
            catch (Exception ex)
            {
                return Results.Json(new
                {
                    status = "unhealthy",
                    timestamp = DateTime.UtcNow,
                    error = ex.Message,
                    checks = new
                    {
                        database = "failed",
                    },
                }, statusCode: 503);
            }
        }).AllowAnonymous();

        app.MapGet("/health/live", (CancellationToken cancellationToken) =>
        {
            _ = cancellationToken;
            return Results.Ok(new
            {
                status = "alive",
                timestamp = DateTime.UtcNow,
            });
        }).AllowAnonymous();

        app.MapGet("/health/ready", async (ISqlSugarClient db, CancellationToken cancellationToken) =>
        {
            try
            {
                _ = cancellationToken;
                await db.Ado.ExecuteCommandAsync("SELECT 1");

                return Results.Ok(new
                {
                    status = "ready",
                    timestamp = DateTime.UtcNow,
                });
            }
            catch (Exception ex)
            {
                return Results.Json(new
                {
                    status = "not_ready",
                    timestamp = DateTime.UtcNow,
                    error = ex.Message,
                }, statusCode: 503);
            }
        }).AllowAnonymous();

        return app;
    }
}
