namespace AutoCodeForge.Core.Exceptions;

/// <summary>
/// Represents an unauthorized access error.
/// </summary>
public sealed class UnauthorizedException : CustomException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UnauthorizedException"/> class.
    /// </summary>
    /// <param name="message">The unauthorized message.</param>
    public UnauthorizedException(string message = "Unauthorized")
        : base(message, 401)
    {
    }
}