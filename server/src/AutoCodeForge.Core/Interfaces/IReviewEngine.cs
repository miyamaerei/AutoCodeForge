using AutoCodeForge.Core.DTOs.Review;

namespace AutoCodeForge.Core.Interfaces;

/// <summary>
/// Executes repository review rules and emits structured findings.
/// </summary>
public interface IReviewEngine
{
    /// <summary>
    /// Executes review rules against a workspace snapshot.
    /// </summary>
    /// <param name="request">The execution request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Structured review result.</returns>
    Task<ReviewExecutionResultDto> ExecuteAsync(
        ReviewExecutionRequestDto request,
        CancellationToken cancellationToken = default);
}