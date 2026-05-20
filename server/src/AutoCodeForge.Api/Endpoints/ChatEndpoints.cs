using System.ComponentModel.DataAnnotations;
using AutoCodeForge.Application.Services;
using AutoCodeForge.Core.DTOs.Chat;
using AutoCodeForge.Core.Models;

namespace AutoCodeForge.Api.Endpoints;

/// <summary>
/// Registers chat session API endpoints.
/// </summary>
public static class ChatEndpoints
{
    /// <summary>
    /// Maps chat endpoints.
    /// </summary>
    /// <param name="app">The route builder.</param>
    /// <returns>The route builder.</returns>
    public static IEndpointRouteBuilder MapChatEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/chat/sessions");

        group.MapPost("/", async (CreateSessionRequest request, ChatService service, CancellationToken cancellationToken) =>
        {
            ValidateModel(request);
            var result = await service.CreateSessionAsync(request, cancellationToken);
            return Results.Ok(ApiResponse<ChatSessionResponse>.Ok(result, "Session created"));
        });

        group.MapGet("/", async (int page, int pageSize, ChatService service, CancellationToken cancellationToken) =>
        {
            var result = await service.GetSessionsAsync(page <= 0 ? 1 : page, pageSize <= 0 ? 20 : pageSize, cancellationToken);
            return Results.Ok(ApiResponse<PagedResult<ChatSessionResponse>>.Ok(result));
        });

        group.MapGet("/{id:guid}", async (Guid id, ChatService service, CancellationToken cancellationToken) =>
        {
            var result = await service.GetSessionAsync(id, cancellationToken);
            return Results.Ok(ApiResponse<ChatSessionResponse>.Ok(result));
        });

        group.MapGet("/{id:guid}/messages", async (Guid id, ChatService service, CancellationToken cancellationToken) =>
        {
            var result = await service.GetMessagesAsync(id, cancellationToken);
            return Results.Ok(ApiResponse<List<ChatMessageResponse>>.Ok(result));
        });

        group.MapPost("/{id:guid}/messages", async (
            Guid id,
            SendMessageRequest request,
            ChatService service,
            CancellationToken cancellationToken) =>
        {
            ValidateModel(request);
            var result = await service.SendMessageAsync(id, request, cancellationToken);
            return Results.Ok(ApiResponse<SendMessageResponse>.Ok(result));
        });

        group.MapDelete("/{id:guid}", async (Guid id, ChatService service, CancellationToken cancellationToken) =>
        {
            await service.DeleteSessionAsync(id, cancellationToken);
            return Results.Ok(ApiResponse<object?>.Ok(null, "Session deleted"));
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
