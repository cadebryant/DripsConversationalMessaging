using DripsConversationalMessaging.Server.Domain;
using DripsConversationalMessaging.Server.Models;

namespace DripsConversationalMessaging.Server.Services;

public interface IConversationService
{
    Task<Message> IngestMessageAsync(IngestMessageRequest request);
    Task<List<Conversation>> GetHighPriorityConversationsAsync();
}
