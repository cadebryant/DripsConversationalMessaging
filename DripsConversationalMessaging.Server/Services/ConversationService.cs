using DripsConversationalMessaging.Server.Data;
using DripsConversationalMessaging.Server.Domain;
using DripsConversationalMessaging.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace DripsConversationalMessaging.Server.Services;

public class ConversationService(
    MessagingDbContext db,
    IIntentAnalyzer intentAnalyzer,
    ILogger<ConversationService> logger) : IConversationService
{
    public async Task<Message> IngestMessageAsync(IngestMessageRequest request)
    {
        logger.LogInformation(
            "Ingesting message from {Sender} for contact {ContactPhone}",
            request.Sender, request.ContactPhone);

        var intent = await intentAnalyzer.AnalyzeAsync(request.Body);

        logger.LogInformation("Message classified with intent {Intent}", intent);

        var conversation = await db.Conversations
            .FirstOrDefaultAsync(c => c.ContactPhone == request.ContactPhone);

        if (conversation is null)
        {
            conversation = new Conversation { ContactPhone = request.ContactPhone };
            db.Conversations.Add(conversation);
        }

        if (intent == Intent.OptOut)
            conversation.IsOptedOut = true;

        if (intent == Intent.Frustrated)
            conversation.IsHighPriority = true;

        var message = new Message
        {
            ConversationId = conversation.Id,
            Body = request.Body,
            Sender = request.Sender,
            Intent = intent
        };

        try
        {
            db.Messages.Add(message);
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Failed to persist message for contact {ContactPhone}", request.ContactPhone);
            throw;
        }

        return message;
    }

    public async Task<List<Conversation>> GetHighPriorityConversationsAsync()
    {
        var results = await db.Conversations
            .Include(c => c.Messages)
            .Where(c => c.IsHighPriority)
            .ToListAsync();

        logger.LogInformation("Returning {Count} high-priority conversations", results.Count);
        return results;
    }
}
