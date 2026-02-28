using DripsConversationalMessaging.Server.Domain;
using DripsConversationalMessaging.Server.Models;
using DripsConversationalMessaging.Server.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace DripsConversationalMessaging.Server.Endpoints;

public static class ConversationEndpoints
{
    public static void MapConversationEndpoints(this WebApplication app)
    {
        var messages = app.MapGroup("/api/messages").WithTags("Messages");
        var conversations = app.MapGroup("/api/conversations").WithTags("Conversations");

        messages.MapPost("/ingest", IngestMessageAsync);
        conversations.MapGet("/priority", GetHighPriorityAsync);
    }

    private static async Task<Results<Created<Message>, BadRequest<string>>> IngestMessageAsync(
        IngestMessageRequest request,
        IConversationService service,
        ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger(nameof(ConversationEndpoints));
        logger.LogInformation(
            "POST /api/messages/ingest received for contact {ContactPhone}",
            request.ContactPhone);

        var message = await service.IngestMessageAsync(request);
        return TypedResults.Created($"/api/messages/{message.Id}", message);
    }

    private static async Task<Ok<List<Conversation>>> GetHighPriorityAsync(
        IConversationService service,
        ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger(nameof(ConversationEndpoints));
        logger.LogInformation("GET /api/conversations/priority called");

        var conversations = await service.GetHighPriorityConversationsAsync();
        return TypedResults.Ok(conversations);
    }
}
