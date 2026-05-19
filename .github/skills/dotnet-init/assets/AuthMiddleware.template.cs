using System.Security.Claims;
using __ProjectName__.Services;

namespace __ProjectName__.Middleware;

public class AuthMiddleware
{
    private readonly RequestDelegate _next;

    public AuthMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IAuthService authService)
    {
        var token = context.Request.Headers.Authorization.FirstOrDefault()?.Split(" ").Last();

        if (!string.IsNullOrEmpty(token))
        {
            try
            {
                var claims = authService.ValidateToken(token);
                if (claims != null)
                {
                    var identity = new ClaimsIdentity(claims, "jwt");
                    context.User = new ClaimsPrincipal(identity);
                }
            }
            catch
            {
                // Token validation failed, continue without setting user
            }
        }

        await _next(context);
    }
}
