using System.Security.Claims;
using AutoCodeForge.Core.Interfaces;
using Microsoft.AspNetCore.Http;

namespace AutoCodeForge.Infrastructure.Services;

/// <summary>
/// Resolves the current NtId from the active HTTP request context.
/// </summary>
public sealed class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="CurrentUser"/> class.
    /// </summary>
    /// <param name="httpContextAccessor">The HTTP context accessor.</param>
    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Gets the current request NtId from claims first, then from the X-NtId header.
    /// </summary>
    /// <returns>The current NtId, or <see langword="null"/> when unavailable.</returns>
    public string? GetCurrentNtId()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context is null)
        {
            return null;
        }

        if (context.Items.TryGetValue("NtId", out var fromItems)
            && fromItems is string ntIdFromItems
            && !string.IsNullOrWhiteSpace(ntIdFromItems))
        {
            return ntIdFromItems;
        }

        var fromClaims = context.User?.FindFirst("NtId")?.Value
            ?? context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!string.IsNullOrWhiteSpace(fromClaims))
        {
            return fromClaims;
        }

        if (context.Request.Headers.TryGetValue("X-NtId", out var headerValue))
        {
            return headerValue.ToString();
        }

        return null;
    }
}