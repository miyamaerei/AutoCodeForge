using AutoCodeForge.Application.Services;

namespace AutoCodeForge.Api.Middleware;

/// <summary>
/// Validates bearer token and sets user principal for authenticated routes.
/// </summary>
public class JwtAuthMiddleware
{
    private static readonly HashSet<string> AnonymousPaths = new(StringComparer.OrdinalIgnoreCase)
    {
        "/",
        "/health",
        "/swagger",
        "/swagger/index.html",
        "/api/v1/auth/login",
        "/api/v1/auth/register",
        "/api/v1/auth/windows-login",
    };

    private readonly RequestDelegate _next;

    /// <summary>
    /// Initializes a new instance of the <see cref="JwtAuthMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next pipeline delegate.</param>
    public JwtAuthMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    /// <summary>
    /// Validates JWT for protected requests.
    /// </summary>
    /// <param name="context">The current http context.</param>
    /// <param name="jwtService">JWT service.</param>
    /// <returns>A task representing asynchronous execution.</returns>
    public async Task InvokeAsync(HttpContext context, JwtService jwtService)
    {
        if (ShouldSkip(context.Request.Path))
        {
            await _next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue("Authorization", out var header))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { success = false, message = "Missing authorization token" });
            return;
        }

        var token = header.ToString().Replace("Bearer ", string.Empty, StringComparison.OrdinalIgnoreCase).Trim();
        if (string.IsNullOrWhiteSpace(token))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { success = false, message = "Invalid authorization token" });
            return;
        }

        var principal = jwtService.ValidateToken(token);
        if (principal is null)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { success = false, message = "Token validation failed" });
            return;
        }

        context.User = principal;
        var ntId = principal.FindFirst("NtId")?.Value;
        if (!string.IsNullOrWhiteSpace(ntId))
        {
            context.Items["NtId"] = ntId;
        }

        var isAdminClaim = principal.FindFirst("IsAdmin")?.Value;
        context.Items["IsAdmin"] = string.Equals(isAdminClaim, "true", StringComparison.OrdinalIgnoreCase);

        await _next(context);
    }

    private static bool ShouldSkip(PathString path)
    {
        var value = path.Value ?? string.Empty;
        if (value.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return AnonymousPaths.Contains(value);
    }
}
