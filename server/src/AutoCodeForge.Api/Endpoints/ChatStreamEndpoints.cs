using AutoCodeForge.Application.Services;
using AutoCodeForge.Core.DTOs.Chat;
using AutoCodeForge.Core.Helpers;

namespace AutoCodeForge.Api.Endpoints;

/// <summary>
/// Registers SSE chat streaming endpoints.
/// </summary>
public static class ChatStreamEndpoints
{
    /// <summary>
    /// Maps chat SSE endpoints.
    /// </summary>
    /// <param name="app">The route builder.</param>
    /// <returns>The route builder.</returns>
    public static IEndpointRouteBuilder MapChatStreamEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/chat/sessions");

        group.MapPost("/{id:guid}/stream", async (
            Guid id,
            SendMessageRequest request,
            ChatService chatService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            context.Response.Headers.Append("Cache-Control", "no-cache");
            context.Response.Headers.Append("Connection", "keep-alive");
            context.Response.ContentType = "text/event-stream";

            await WriteEventAsync(context, "start", JsonHelper.Serialize(new { sessionId = id }), cancellationToken);

            var result = await chatService.SendMessageAsync(id, request, cancellationToken);
            foreach (var chunk in Chunk(result.AssistantMessage.Content, 12))
            {
                if (cancellationToken.IsCancellationRequested || context.RequestAborted.IsCancellationRequested)
                {
                    break;
                }

                await WriteEventAsync(context, "token", chunk, cancellationToken);
            }

            await WriteEventAsync(context, "done", JsonHelper.Serialize(result), cancellationToken);
            return Results.Empty;
        });

        return app;
    }

    private static async Task WriteEventAsync(
        HttpContext context,
        string eventName,
        string data,
        CancellationToken cancellationToken)
    {
        await context.Response.WriteAsync($"event: {eventName}\n", cancellationToken);
        await context.Response.WriteAsync($"data: {data}\n\n", cancellationToken);
        await context.Response.Body.FlushAsync(cancellationToken);
    }

    private static IEnumerable<string> Chunk(string text, int size)
    {
        if (string.IsNullOrEmpty(text))
        {
            yield break;
        }

        var index = 0;
        while (index < text.Length)
        {
            var length = Math.Min(size, text.Length - index);
            yield return text.Substring(index, length);
            index += length;
        }
    }
}
