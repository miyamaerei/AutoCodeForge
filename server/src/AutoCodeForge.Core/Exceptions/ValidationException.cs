namespace AutoCodeForge.Core.Exceptions;

/// <summary>
/// Represents a validation failure.
/// </summary>
public sealed class ValidationException : CustomException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationException"/> class.
    /// </summary>
    /// <param name="message">The validation error message.</param>
    public ValidationException(string message)
        : base(message, 400)
    {
    }
}