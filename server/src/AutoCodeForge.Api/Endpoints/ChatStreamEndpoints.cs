using AutoCodeForge.Application.Services;
using AutoCodeForge.Core.DTOs.Chat;
using AutoCodeForge.Core.Helpers;
using Microsoft.Extensions.Logging;

namespace AutoCodeForge.Api.Endpoints;

/// <summary>
/// Registers SSE chat streaming endpoints.
/// Supports streaming message tokens with proper error handling for tool execution.
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
            ILogger<Program> logger,
            CancellationToken cancellationToken) =>
        {
            try
            {
                context.Response.Headers.Append("Cache-Control", "no-cache");
                context.Response.Headers.Append("Connection", "keep-alive");
                context.Response.ContentType = "text/event-stream";

                await WriteEventAsync(context, "start", JsonHelper.Serialize(new { sessionId = id }), cancellationToken);

                SendMessageResponse result;
                try
                {
                    result = await chatService.SendMessageAsync(id, request, cancellationToken);
                }
                catch (Exception ex)
                {
                    // Tool execution or other service errors are logged but don't interrupt stream
                    logger.LogError(ex, "Error executing agent for session {SessionId}", id);
                    await WriteEventAsync(
                        context,
                        "error",
                        JsonHelper.Serialize(new { message = ex.Message }),
                        cancellationToken);
                    await WriteEventAsync(context, "done", "{}", cancellationToken);
                    return Results.Empty;
                }

                // Stream the assistant message content in chunks
                if (!string.IsNullOrEmpty(result.AssistantMessage?.Content))
                {
                    foreach (var chunk in Chunk(result.AssistantMessage.Content, 12))
                    {
                        if (cancellationToken.IsCancellationRequested || context.RequestAborted.IsCancellationRequested)
                        {
                            logger.LogInformation("Stream cancelled for session {SessionId}", id);
                            break;
                        }

                        try
                        {
                            await WriteEventAsync(context, "token", chunk, cancellationToken);
                        }
                        catch (IOException ex)
                        {
                            // Client disconnected during streaming
                            logger.LogWarning(ex, "Client disconnected during stream for session {SessionId}", id);
                            break;
                        }
                    }
                }

                await WriteEventAsync(context, "done", JsonHelper.Serialize(result), cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error in streaming endpoint for session {SessionId}", id);
                try
                {
                    await context.Response.WriteAsync(
                        $"event: error\ndata: {JsonHelper.Serialize(new { message = "Internal server error" })}\n\n",
                        cancellationToken);
                }
                catch
                {
                    // Response already sent or client disconnected
                }
            }

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
