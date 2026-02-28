using DripsConversationalMessaging.Server.Domain;

namespace DripsConversationalMessaging.Server.Services;

public interface IIntentAnalyzer
{
    Task<Intent> AnalyzeAsync(string messageBody, CancellationToken cancellationToken = default);
}