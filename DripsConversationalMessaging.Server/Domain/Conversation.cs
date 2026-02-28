namespace DripsConversationalMessaging.Server.Domain;

public class Conversation
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string ContactPhone { get; set; } = string.Empty;
    public bool IsHighPriority { get; set; }
    public bool IsOptedOut { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<Message> Messages { get; set; } = [];
}
