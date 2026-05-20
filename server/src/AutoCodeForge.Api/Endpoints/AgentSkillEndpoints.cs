using System.ComponentModel.DataAnnotations;
using AutoCodeForge.Application.Services;
using AutoCodeForge.Core.DTOs.AI.GitTools;
using AutoCodeForge.Core.Models;

namespace AutoCodeForge.Api.Endpoints;

/// <summary>
/// Registers endpoints for agent skill policy management.
/// </summary>
public static class AgentSkillEndpoints
{
    /// <summary>
    /// Maps agent skill policy endpoints.
    /// </summary>
    /// <param name="app">The route builder.</param>
    /// <returns>The route builder.</returns>
    public static IEndpointRouteBuilder MapAgentSkillEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/agent-skills/git");

        group.MapGet("/grants/{repositoryId:guid}", async (
            Guid repositoryId,
            GitSkillPolicyService service,
            CancellationToken cancellationToken) =>
        {
            var result = await service.GetByRepositoryAsync(repositoryId, cancellationToken);
            return Results.Ok(ApiResponse<GitSkillGrantResponse>.Ok(result));
        });

        group.MapPut("/grants/{repositoryId:guid}", async (
            Guid repositoryId,
            UpdateGitSkillGrantRequest request,
            GitSkillPolicyService service,
            CancellationToken cancellationToken) =>
        {
            ValidateModel(request);
            var result = await service.UpsertAsync(repositoryId, request, cancellationToken);
            return Results.Ok(ApiResponse<GitSkillGrantResponse>.Ok(result, "Git skill grant updated"));
        });

        return app;
    }

    private static void ValidateModel(object request)
    {
        var context = new ValidationContext(request);
        var results = new List<ValidationResult>();
        if (!Validator.TryValidateObject(request, context, results, true))
        {
            var message = string.Join("; ", results.Select(result => result.ErrorMessage));
            throw new AutoCodeForge.Core.Exceptions.ValidationException(message);
        }
    }
}
