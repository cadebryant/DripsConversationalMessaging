namespace DripsConversationalMessaging.Server.Models;

public record IngestMessageRequest(string ContactPhone, string Sender, string Body);
