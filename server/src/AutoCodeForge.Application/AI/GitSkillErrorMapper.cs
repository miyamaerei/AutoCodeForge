using AutoCodeForge.Core.Exceptions;

namespace AutoCodeForge.Application.AI;

/// <summary>
/// Maps Git tool exceptions to stable error code and actionable suggestion.
/// </summary>
public class GitSkillErrorMapper
{
    /// <summary>
    /// Maps one exception to error code and suggestion.
    /// </summary>
    /// <param name="exception">The source exception.</param>
    /// <returns>Tuple with error code and suggestion.</returns>
    public (string errorCode, string suggestion) Map(Exception exception)
    {
        return exception switch
        {
            ForbiddenException => (
                "GIT_PERMISSION_DENIED",
                "Request Write permission in Git skill policy, or switch to a read-only operation."),
            ValidationException => (
                "GIT_INVALID_INPUT",
                "Check repositoryId and required operation arguments, then retry."),
            TimeoutException => (
                "GIT_TIMEOUT",
                "Network may be unstable. Retry later or reduce payload size."),
            HttpRequestException => (
                "GIT_NETWORK_ERROR",
                "Verify repository endpoint reachability and token validity."),
            CustomException custom when custom.StatusCode == 504 => (
                "GIT_TOOL_TIMEOUT",
                "Tool execution timed out. Retry with simpler request."),
            _ => (
                "GIT_TOOL_FAILED",
                "Check operation arguments and repository authorization, then retry.")
        };
    }
}
