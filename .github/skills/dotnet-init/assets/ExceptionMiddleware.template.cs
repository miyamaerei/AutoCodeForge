using System.Net;
using System.Text.Json;

namespace __ProjectName__.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var requestId = context.TraceIdentifier;
        var path = context.Request.Path;
        var timestamp = DateTime.UtcNow;

        _logger.LogError(exception, "Unhandled exception occurred. RequestId: {RequestId}, Path: {Path}", requestId, path);

        var errorResponse = new ApiErrorResponse
        {
            Code = (int)HttpStatusCode.InternalServerError,
            Message = "An unexpected error occurred",
            Detail = exception.Message,
            RequestId = requestId,
            Path = path,
            Timestamp = timestamp,
            Errors = null
        };

        // Handle specific exception types
        if (exception is UnauthorizedAccessException)
        {
            errorResponse.Code = (int)HttpStatusCode.Unauthorized;
            errorResponse.Message = "Unauthorized access";
        }
        else if (exception is KeyNotFoundException)
        {
            errorResponse.Code = (int)HttpStatusCode.NotFound;
            errorResponse.Message = "Resource not found";
        }
        else if (exception is ArgumentException || exception is ValidationException)
        {
            errorResponse.Code = (int)HttpStatusCode.BadRequest;
            errorResponse.Message = "Invalid request";
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = errorResponse.Code;

        return context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse));
    }
}

public class ValidationException : Exception
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException(IDictionary<string, string[]> errors)
        : base("Validation failed")
    {
        Errors = errors;
    }
}
