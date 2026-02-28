namespace DripsConversationalMessaging.Server.Domain;

public class Message
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ConversationId { get; set; }
    public string Body { get; set; } = string.Empty;
    public string Sender { get; set; } = string.Empty;
    public Intent Intent { get; set; }
    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
}

