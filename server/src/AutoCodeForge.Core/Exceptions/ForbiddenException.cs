namespace AutoCodeForge.Core.Exceptions;

/// <summary>
/// Represents a forbidden access error where the caller is authenticated but lacks permission.
/// </summary>
public sealed class ForbiddenException : CustomException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ForbiddenException"/> class.
    /// </summary>
    /// <param name="message">The forbidden message.</param>
    public ForbiddenException(string message = "Forbidden")
        : base(message, 403)
    {
    }
}
