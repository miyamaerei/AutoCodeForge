namespace AutoCodeForge.Core.Exceptions;

/// <summary>
/// Represents a domain or application exception with an associated HTTP status code.
/// </summary>
public class CustomException : Exception
{
    /// <summary>
    /// Gets the HTTP status code to use when the exception is returned to the client.
    /// </summary>
    public int StatusCode { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CustomException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="statusCode">The HTTP status code.</param>
    public CustomException(string message, int statusCode = 400)
        : base(message)
    {
        StatusCode = statusCode;
    }
}