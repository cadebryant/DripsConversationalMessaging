using DripsConversationalMessaging.Server.Data;
using DripsConversationalMessaging.Server.Domain;
using DripsConversationalMessaging.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace DripsConversationalMessaging.Server.Services;

public class ConversationService(MessagingDbContext db, ILogger<ConversationService> logger) : IConversationService
{
    private static readonly string[] OptOutKeywords =
        ["stop", "unsubscribe"];

    private static readonly string[] FrustratedKeywords =
        ["angry", "frustrated", "terrible", "awful", "horrible", "worst", "hate", "useless", "unacceptable", "ridiculous", "furious"];

    private static readonly string[] ConfusedKeywords =
        ["confused", "confusing", "unclear", "not sure", "don't understand", "what does", "how do"];

    private static readonly string[] InterestedKeywords =
        ["interested", "tell me more", "sounds good", "yes", "great", "awesome", "love", "want", "sign me up"];

    public async Task<Message> IngestMessageAsync(IngestMessageRequest request)
    {
        logger.LogInformation(
            "Ingesting message from {Sender} for contact {ContactPhone}",
            request.Sender, request.ContactPhone);

        var intent = ClassifyIntent(request.Body);

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

        db.Messages.Add(message);
        await db.SaveChangesAsync();

        return message;
    }

    public async Task<List<Conversation>> GetHighPriorityConversationsAsync()
    {
        return await db.Conversations
            .Include(c => c.Messages)
            .Where(c => c.IsHighPriority)
            .ToListAsync();
    }

    private static Intent ClassifyIntent(string body)
    {
        var lower = body.ToLowerInvariant();

        if (OptOutKeywords.Any(kw => lower.Contains(kw)))
            return Intent.OptOut;

        if (FrustratedKeywords.Any(kw => lower.Contains(kw)))
            return Intent.Frustrated;

        if (ConfusedKeywords.Any(kw => lower.Contains(kw)))
            return Intent.Confused;

        if (InterestedKeywords.Any(kw => lower.Contains(kw)))
            return Intent.Interested;

        return Intent.Interested;
    }
}
