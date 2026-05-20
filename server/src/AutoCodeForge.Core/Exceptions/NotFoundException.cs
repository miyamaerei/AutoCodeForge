namespace AutoCodeForge.Core.Exceptions;

/// <summary>
/// Represents a missing resource error.
/// </summary>
public sealed class NotFoundException : CustomException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NotFoundException"/> class.
    /// </summary>
    /// <param name="message">The resource-not-found message.</param>
    public NotFoundException(string message)
        : base(message, 404)
    {
    }
}