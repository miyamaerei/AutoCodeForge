namespace AutoCodeForge.Core.Interfaces;

/// <summary>
/// Provides access to the current user identity without coupling to transport concerns.
/// </summary>
public interface ICurrentUser
{
    /// <summary>
    /// Gets the current user's NtId, or <see langword="null"/> when unavailable.
    /// </summary>
    /// <returns>The current NtId, if available.</returns>
    string? GetCurrentNtId();

    /// <summary>
    /// Gets a value indicating whether the current user has administrator privileges.
    /// </summary>
    /// <returns><see langword="true"/> when the current user is an admin; otherwise <see langword="false"/>.</returns>
    bool IsAdmin();
}