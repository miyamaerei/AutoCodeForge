using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using __ProjectName__.Configuration;
using __ProjectName__.Entities;
using SqlSugar;

namespace __ProjectName__.Services;

public interface IAuthService
{
    Task<AuthResponse> LoginAsync(string ntId, string password);
    Task<AuthResponse> RegisterAsync(string ntId, string userName, string? email, string password);
    Task<AuthResponse> RefreshTokenAsync(string refreshToken);
    Task<bool> RevokeRefreshTokenAsync(string refreshToken);
    ClaimsPrincipal? ValidateToken(string token);
}

public class AuthService : IAuthService
{
    private readonly ISqlSugarClient _db;
    private readonly AppSettings _appSettings;
    private readonly ILogger<AuthService> _logger;

    public AuthService(ISqlSugarClient db, AppSettings appSettings, ILogger<AuthService> logger)
    {
        _db = db;
        _appSettings = appSettings;
        _logger = logger;
    }

    public async Task<AuthResponse> LoginAsync(string ntId, string password)
    {
        var user = await _db.Queryable<UserEntity>()
            .Where(u => u.NtId == ntId && !u.IsDeleted)
            .FirstAsync();

        if (user == null || !VerifyPassword(password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid credentials");
        }

        return await CreateAuthResponse(user);
    }

    public async Task<AuthResponse> RegisterAsync(string ntId, string userName, string? email, string password)
    {
        var existingUser = await _db.Queryable<UserEntity>()
            .Where(u => u.NtId == ntId)
            .FirstAsync();

        if (existingUser != null)
        {
            throw new ArgumentException("User already exists");
        }

        var user = new UserEntity
        {
            NtId = ntId,
            UserName = userName,
            Email = email,
            PasswordHash = HashPassword(password)
        };

        await _db.Insertable(user).ExecuteCommandAsync();

        return await CreateAuthResponse(user);
    }

    public async Task<AuthResponse> RefreshTokenAsync(string refreshToken)
    {
        var tokenRecord = await _db.Queryable<RefreshTokenEntity>()
            .Where(t => t.Token == refreshToken && !t.IsRevoked && t.ExpiresAt > DateTime.UtcNow)
            .FirstAsync();

        if (tokenRecord == null)
        {
            throw new UnauthorizedAccessException("Invalid or expired refresh token");
        }

        var user = await _db.Queryable<UserEntity>()
            .Where(u => u.Id == tokenRecord.UserId && !u.IsDeleted)
            .FirstAsync();

        if (user == null)
        {
            throw new UnauthorizedAccessException("User not found");
        }

        tokenRecord.IsRevoked = true;
        await _db.Updateable(tokenRecord).ExecuteCommandAsync();

        return await CreateAuthResponse(user);
    }

    public async Task<bool> RevokeRefreshTokenAsync(string refreshToken)
    {
        var tokenRecord = await _db.Queryable<RefreshTokenEntity>()
            .Where(t => t.Token == refreshToken)
            .FirstAsync();

        if (tokenRecord != null)
        {
            tokenRecord.IsRevoked = true;
            await _db.Updateable(tokenRecord).ExecuteCommandAsync();
            return true;
        }

        return false;
    }

    public ClaimsPrincipal? ValidateToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_appSettings.JwtSettings.Secret);

            var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _appSettings.JwtSettings.Issuer,
                ValidAudience = _appSettings.JwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(key)
            }, out _);

            return principal;
        }
        catch
        {
            return null;
        }
    }

    private async Task<AuthResponse> CreateAuthResponse(UserEntity user)
    {
        var accessToken = GenerateAccessToken(user);
        var refreshToken = GenerateRefreshToken();
        var expiresAt = DateTime.UtcNow.AddMinutes(_appSettings.JwtSettings.ExpiresInMinutes);

        await _db.Insertable(new RefreshTokenEntity
        {
            UserId = user.Id,
            Token = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(_appSettings.JwtSettings.RefreshTokenExpiresInDays)
        }).ExecuteCommandAsync();

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = expiresAt,
            User = new UserDto
            {
                Id = user.Id,
                NtId = user.NtId,
                UserName = user.UserName,
                Email = user.Email
            }
        };
    }

    private string GenerateAccessToken(UserEntity user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_appSettings.JwtSettings.Secret);
        var expires = DateTime.UtcNow.AddMinutes(_appSettings.JwtSettings.ExpiresInMinutes);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim("ntId", user.NtId),
                new Claim(ClaimTypes.Name, user.UserName)
            }),
            Expires = expires,
            Issuer = _appSettings.JwtSettings.Issuer,
            Audience = _appSettings.JwtSettings.Audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    private string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    private bool VerifyPassword(string password, string? hash)
    {
        if (string.IsNullOrEmpty(hash)) return false;
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }
}
