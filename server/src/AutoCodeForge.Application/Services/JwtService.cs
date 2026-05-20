using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AutoCodeForge.Application.Services;

/// <summary>
/// Provides JWT generation and validation.
/// </summary>
public class JwtService
{
    private readonly JwtOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="JwtService"/> class.
    /// </summary>
    /// <param name="options">JWT options.</param>
    public JwtService(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    /// <summary>
    /// Generates JWT token for one user.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns>Token and expiration UTC.</returns>
    public (string Token, DateTime ExpiresAtUtc) GenerateToken(UserEntity user)
    {
        var now = DateTime.UtcNow;
        var expiresAtUtc = now.AddMinutes(_options.ExpireMinutes);
        var claims = new List<Claim>
        {
            new("NtId", user.NtId),
            new(ClaimTypes.NameIdentifier, user.NtId),
            new(ClaimTypes.Name, user.UserName),
            new("IsAdmin", user.IsAdmin ? "true" : "false"),
        };

        var credentials = new SigningCredentials(GetSecurityKey(), SecurityAlgorithms.HmacSha256);
        var jwt = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: now,
            expires: expiresAtUtc,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(jwt), expiresAtUtc);
    }

    /// <summary>
    /// Validates token and returns principal.
    /// </summary>
    /// <param name="token">The token.</param>
    /// <returns>Claims principal when valid; null otherwise.</returns>
    public ClaimsPrincipal? ValidateToken(string token)
    {
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = _options.Issuer,
            ValidateAudience = true,
            ValidAudience = _options.Audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = GetSecurityKey(),
            ClockSkew = TimeSpan.FromMinutes(1),
        };

        try
        {
            return new JwtSecurityTokenHandler().ValidateToken(token, validationParameters, out _);
        }
        catch
        {
            return null;
        }
    }

    private SymmetricSecurityKey GetSecurityKey()
    {
        return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));
    }
}
