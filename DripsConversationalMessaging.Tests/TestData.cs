using Bogus;
using DripsConversationalMessaging.Server.Domain;
using DripsConversationalMessaging.Server.Models;

namespace DripsConversationalMessaging.Tests;

internal static class TestData
{
    internal static Faker<IngestMessageRequest> MessageRequests() =>
        new Faker<IngestMessageRequest>()
            .CustomInstantiator(f => new IngestMessageRequest(
                f.Phone.PhoneNumber("##########"),
                f.Name.FullName(),
                f.Lorem.Sentence()));

    internal static Faker<Message> Messages() =>
        new Faker<Message>()
            .RuleFor(m => m.Id, f => f.Random.Guid())
            .RuleFor(m => m.ConversationId, f => f.Random.Guid())
            .RuleFor(m => m.Body, f => f.Lorem.Sentence())
            .RuleFor(m => m.Sender, f => f.Name.FullName())
            .RuleFor(m => m.Intent, f => f.PickRandom<Intent>())
            .RuleFor(m => m.ReceivedAt, f => f.Date.Recent());

    internal static Faker<Conversation> Conversations(bool isHighPriority = true) =>
        new Faker<Conversation>()
            .RuleFor(c => c.Id, f => f.Random.Guid())
            .RuleFor(c => c.ContactPhone, f => f.Phone.PhoneNumber("##########"))
            .RuleFor(c => c.IsHighPriority, _ => isHighPriority)
            .RuleFor(c => c.IsOptedOut, _ => false)
            .RuleFor(c => c.CreatedAt, f => f.Date.Past())
            .RuleFor(c => c.Messages, _ => new List<Message>());
}
