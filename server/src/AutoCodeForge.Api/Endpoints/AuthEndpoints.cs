using System.ComponentModel.DataAnnotations;
using System.Security.Principal;
using AutoCodeForge.Application.Services;
using AutoCodeForge.Core.DTOs.Auth;
using AutoCodeForge.Core.Models;

namespace AutoCodeForge.Api.Endpoints;

/// <summary>
/// Registers auth related API endpoints.
/// </summary>
public static class AuthEndpoints
{
    /// <summary>
    /// Maps auth endpoints.
    /// </summary>
    /// <param name="app">The web application.</param>
    /// <returns>The route builder.</returns>
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/auth");

        group.MapPost("/register", async (RegisterRequest request, AuthService authService, CancellationToken cancellationToken) =>
        {
            ValidateModel(request);
            var response = await authService.RegisterAsync(request, cancellationToken);
            return Results.Ok(ApiResponse<AuthResponse>.Ok(response, "Register success"));
        });

        group.MapPost("/login", async (LoginRequest request, AuthService authService, CancellationToken cancellationToken) =>
        {
            ValidateModel(request);
            var response = await authService.LoginAsync(request, cancellationToken);
            return Results.Ok(ApiResponse<AuthResponse>.Ok(response, "Login success"));
        });

        group.MapPost("/windows-login", async (HttpContext context, AuthService authService, CancellationToken cancellationToken) =>
        {
            var loginRequest = ResolveWindowsLoginRequest(context);
            ValidateModel(loginRequest);
            var response = await authService.LoginAsync(loginRequest, cancellationToken);
            return Results.Ok(ApiResponse<AuthResponse>.Ok(response, "Windows login success"));
        });

        group.MapGet("/me", async (HttpContext context, AuthService authService, CancellationToken cancellationToken) =>
        {
            var ntId = context.User.FindFirst("NtId")?.Value;
            if (string.IsNullOrWhiteSpace(ntId))
            {
                return Results.Unauthorized();
            }

            var user = await authService.GetCurrentUserAsync(ntId, cancellationToken);
            return Results.Ok(ApiResponse<object>.Ok(new
            {
                user.Id,
                user.NtId,
                user.UserName,
                user.Email,
            }));
        });

        return app;
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

    private static LoginRequest ResolveWindowsLoginRequest(HttpContext context)
    {
        var rawIdentity = context.User?.Identity?.Name;

        if (string.IsNullOrWhiteSpace(rawIdentity)
            && context.Request.Headers.TryGetValue("X-Windows-User", out var windowsUserHeader))
        {
            rawIdentity = windowsUserHeader.ToString();
        }

        if (string.IsNullOrWhiteSpace(rawIdentity)
            && context.Request.Headers.TryGetValue("X-NtId", out var ntIdHeader))
        {
            rawIdentity = ntIdHeader.ToString();
        }

        if (string.IsNullOrWhiteSpace(rawIdentity))
        {
            rawIdentity = GetLocalWindowsIdentity();
        }

        var normalizedNtId = NormalizeNtId(rawIdentity);
        var userName = ResolveDisplayName(rawIdentity, normalizedNtId);

        return new LoginRequest
        {
            NtId = normalizedNtId,
            UserName = userName,
        };
    }

    private static string? GetLocalWindowsIdentity()
    {
        if (!OperatingSystem.IsWindows())
        {
            return null;
        }

        try
        {

            var name = WindowsIdentity.GetCurrent();
            return WindowsIdentity.GetCurrent()?.Name;
        }
        catch
        {
            return null;
        }
    }

    private static string? NormalizeNtId(string? rawIdentity)
    {
        if (string.IsNullOrWhiteSpace(rawIdentity))
        {
            return null;
        }

        var normalized = rawIdentity.Trim();
        if (normalized.Contains('\\'))
        {
            var parts = normalized.Split('\\', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            return parts.Length > 0 ? parts[^1] : normalized;
        }

        if (normalized.Contains('@'))
        {
            var parts = normalized.Split('@', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            return parts.Length > 0 ? parts[0] : normalized;
        }

        return normalized;
    }

    private static string? ResolveDisplayName(string? rawIdentity, string? ntId)
    {
        if (!string.IsNullOrWhiteSpace(rawIdentity) && rawIdentity.Contains('\\'))
        {
            var parts = rawIdentity.Split('\\', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (parts.Length > 0)
            {
                return parts[^1];
            }
        }

        return ntId;
    }
}
