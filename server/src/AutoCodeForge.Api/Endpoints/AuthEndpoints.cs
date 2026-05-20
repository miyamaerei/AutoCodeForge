using System.ComponentModel.DataAnnotations;
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
}
