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
}