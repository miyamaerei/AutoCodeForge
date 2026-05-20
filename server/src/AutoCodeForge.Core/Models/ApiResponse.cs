namespace AutoCodeForge.Core.Models;

/// <summary>
/// Represents a standard API response envelope.
/// </summary>
public class ApiResponse<T>
{
    /// <summary>
    /// Gets or sets a value indicating whether the request completed successfully.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets the response message.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the response payload.
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// Gets or sets the trace identifier associated with the request.
    /// </summary>
    public string? TraceId { get; set; }

    /// <summary>
    /// Creates a successful response.
    /// </summary>
    /// <param name="data">The payload to include.</param>
    /// <param name="message">The success message.</param>
    /// <returns>A successful response instance.</returns>
    public static ApiResponse<T> Ok(T? data, string message = "OK")
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data,
        };
    }

    /// <summary>
    /// Creates a failed response.
    /// </summary>
    /// <param name="message">The failure message.</param>
    /// <param name="traceId">The request trace identifier.</param>
    /// <returns>A failed response instance.</returns>
    public static ApiResponse<T> Fail(string message, string? traceId = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            TraceId = traceId,
        };
    }
}