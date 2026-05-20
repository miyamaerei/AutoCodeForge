using AutoCodeForge.Core.DTOs.Auth;
using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Exceptions;
using AutoCodeForge.Infrastructure.Helpers;
using AutoCodeForge.Infrastructure.Repositories;

namespace AutoCodeForge.Application.Services;

/// <summary>
/// Provides registration, login, and current-user queries.
/// </summary>
public class AuthService
{
    private readonly UserRepository _userRepository;
    private readonly PasswordHelper _passwordHelper;
    private readonly JwtService _jwtService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthService"/> class.
    /// </summary>
    /// <param name="userRepository">The user repository.</param>
    /// <param name="passwordHelper">The password helper.</param>
    /// <param name="jwtService">The JWT service.</param>
    public AuthService(UserRepository userRepository, PasswordHelper passwordHelper, JwtService jwtService)
    {
        _userRepository = userRepository;
        _passwordHelper = passwordHelper;
        _jwtService = jwtService;
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
            PasswordHash = _passwordHelper.HashPassword(request.Password),
            IsDeleted = false,
        };

        await _userRepository.CreateAsync(user, cancellationToken);
        return BuildAuthResponse(user);
    }

    /// <summary>
    /// Authenticates one user.
    /// </summary>
    /// <param name="request">The login request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Authentication result.</returns>
    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.NtId) || string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ValidationException("NtId and password are required");
        }

        var user = await _userRepository.GetByNtIdAsync(request.NtId.Trim(), cancellationToken);
        if (user is null || !_passwordHelper.VerifyPassword(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedException("Invalid credentials");
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

    private static void ValidateRegisterRequest(RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.NtId)
            || string.IsNullOrWhiteSpace(request.UserName)
            || string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ValidationException("NtId, UserName and password are required");
        }
    }
}
