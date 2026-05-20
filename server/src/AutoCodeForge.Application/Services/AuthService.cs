using AutoCodeForge.Core.DTOs.Auth;
using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Exceptions;
using AutoCodeForge.Infrastructure.Repositories;

namespace AutoCodeForge.Application.Services;

/// <summary>
/// Provides registration, login, and current-user queries.
/// </summary>
public class AuthService
{
    private const string WindowsAuthPasswordPlaceholder = "__WINDOWS_AUTH__";

    private readonly UserRepository _userRepository;
    private readonly JwtService _jwtService;
    private readonly ConfigInitializationService? _configInitializationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthService"/> class.
    /// </summary>
    /// <param name="userRepository">The user repository.</param>
    /// <param name="jwtService">The JWT service.</param>
    /// <param name="configInitializationService">The configuration initialization service (optional).</param>
    public AuthService(
        UserRepository userRepository, 
        JwtService jwtService,
        ConfigInitializationService? configInitializationService = null)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
        _configInitializationService = configInitializationService;
    }

    /// <summary>
    /// Registers a new user.
    /// </summary>
    /// <param name="request">The register request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Authentication result.</returns>
    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        ValidateRegisterRequest(request);

        var exists = await _userRepository.GetByNtIdAsync(request.NtId, cancellationToken);
        if (exists is not null)
        {
            throw new ValidationException("NtId already exists");
        }

        var user = new UserEntity
        {
            NtId = request.NtId.Trim(),
            UserName = request.UserName.Trim(),
            Email = request.Email?.Trim(),
            PasswordHash = WindowsAuthPasswordPlaceholder,
            IsDeleted = false,
        };

        await _userRepository.CreateAsync(user, cancellationToken);

        await InitializeUserConfigAsync(request.NtId.Trim(), cancellationToken);

        return BuildAuthResponse(user);
    }

    /// <summary>
    /// Signs in with a Windows identity and auto-creates the user when missing.
    /// </summary>
    /// <param name="request">The login request resolved from Windows identity context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Authentication result.</returns>
    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.NtId))
        {
            throw new ValidationException("NtId is required");
        }

        var normalizedNtId = request.NtId.Trim();
        var user = await _userRepository.GetByNtIdAsync(normalizedNtId, cancellationToken);
        var isNewUser = user is null;

        if (isNewUser)
        {
            var resolvedUserName = ResolveUserName(request.UserName, normalizedNtId);

            user = new UserEntity
            {
                NtId = normalizedNtId,
                UserName = resolvedUserName,
                Email = request.Email?.Trim(),
                PasswordHash = WindowsAuthPasswordPlaceholder,
                IsDeleted = false,
            };

            await _userRepository.CreateAsync(user, cancellationToken);

            await InitializeUserConfigAsync(normalizedNtId, cancellationToken);
        }

        return BuildAuthResponse(user);
    }

    /// <summary>
    /// Gets currently authenticated user.
    /// </summary>
    /// <param name="ntId">Current ntid.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>User entity.</returns>
    public async Task<UserEntity> GetCurrentUserAsync(string ntId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByNtIdAsync(ntId, cancellationToken);
        if (user is null)
        {
            throw new NotFoundException("User not found");
        }

        return user;
    }

    private AuthResponse BuildAuthResponse(UserEntity user)
    {
        var token = _jwtService.GenerateToken(user);
        return new AuthResponse
        {
            AccessToken = token.Token,
            ExpiresAtUtc = token.ExpiresAtUtc,
            NtId = user.NtId,
            UserName = user.UserName,
        };
    }

    /// <summary>
    /// Initializes default configurations for a newly created user.
    /// </summary>
    /// <param name="ntId">The NTID of the new user.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    private async Task InitializeUserConfigAsync(string ntId, CancellationToken cancellationToken)
    {
        if (_configInitializationService is null)
        {
            return;
        }

        try
        {
            await _configInitializationService.InitializeUserDefaultsForNtIdAsync(ntId, cancellationToken);
        }
        catch (Exception)
        {
        }
    }

    private static void ValidateRegisterRequest(RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.NtId)
            || string.IsNullOrWhiteSpace(request.UserName)
            )
        {
            throw new ValidationException("NtId and UserName are required");
        }
    }

    private static string ResolveUserName(string? requestedUserName, string ntId)
    {
        if (!string.IsNullOrWhiteSpace(requestedUserName))
        {
            return requestedUserName.Trim();
        }

        if (ntId.Contains('\\'))
        {
            var parts = ntId.Split('\\', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (parts.Length > 0)
            {
                return parts[^1];
            }
        }

        if (ntId.Contains('@'))
        {
            var parts = ntId.Split('@', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (parts.Length > 0)
            {
                return parts[0];
            }
        }

        return ntId;
    }
}
