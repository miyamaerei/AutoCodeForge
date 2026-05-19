using __ProjectName__.Services;

namespace __ProjectName__.Api;

public static class AuthApi
{
    public static void Register(WebApplication app)
    {
        var authGroup = app.MapGroup("/api/v1/auth")
            .WithTags("Authentication");

        authGroup.MapPost("/register", async (RegisterRequest request, IAuthService authService) =>
        {
            var response = await authService.RegisterAsync(
                request.NtId,
                request.UserName,
                request.Email,
                request.Password
            );
            return Results.Ok(ApiResponse<AuthResponse>.Success(response));
        })
        .Produces<ApiResponse<AuthResponse>>(200)
        .Produces<ApiErrorResponse>(400)
        .WithName("Register")
        .WithSummary("Register a new user");

        authGroup.MapPost("/login", async (LoginRequest request, IAuthService authService) =>
        {
            var response = await authService.LoginAsync(request.NtId, request.Password);
            return Results.Ok(ApiResponse<AuthResponse>.Success(response));
        })
        .Produces<ApiResponse<AuthResponse>>(200)
        .Produces<ApiErrorResponse>(401)
        .WithName("Login")
        .WithSummary("Authenticate user and get tokens");

        authGroup.MapPost("/refresh", async (RefreshTokenRequest request, IAuthService authService) =>
        {
            var response = await authService.RefreshTokenAsync(request.RefreshToken);
            return Results.Ok(ApiResponse<AuthResponse>.Success(response));
        })
        .Produces<ApiResponse<AuthResponse>>(200)
        .Produces<ApiErrorResponse>(401)
        .WithName("RefreshToken")
        .WithSummary("Refresh access token");

        authGroup.MapGet("/me", (ClaimsPrincipal user) =>
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var ntId = user.FindFirst("ntId")?.Value;
            var userName = user.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Results.Unauthorized();
            }

            var userDto = new UserDto
            {
                Id = Guid.Parse(userId),
                NtId = ntId ?? string.Empty,
                UserName = userName ?? string.Empty
            };

            return Results.Ok(ApiResponse<UserDto>.Success(userDto));
        })
        .RequireAuthorization()
        .Produces<ApiResponse<UserDto>>(200)
        .Produces<ApiErrorResponse>(401)
        .WithName("GetCurrentUser")
        .WithSummary("Get current authenticated user");
    }
}
