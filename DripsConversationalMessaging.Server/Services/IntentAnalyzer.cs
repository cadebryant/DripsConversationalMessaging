using DripsConversationalMessaging.Server.Domain;
using Microsoft.Extensions.AI;

namespace DripsConversationalMessaging.Server.Services;

public class IntentAnalyzer(IChatClient chatClient, ILogger<IntentAnalyzer> logger) : IIntentAnalyzer
{
    private const string SystemPrompt = """
        You are a customer intent classifier for a conversational messaging platform.
        Analyze the following customer message and classify it as exactly one of:

        - Interested : The customer shows curiosity, positivity, or wants to learn more.
        - Confused   : The customer is asking for clarification or seems uncertain.
        - Frustrated : The customer expresses anger, dissatisfaction, or strong negativity.
        - OptOut     : The customer explicitly wants to stop receiving messages or unsubscribe.

        Respond with ONLY the single intent word. No explanation. No punctuation.
        """;

    private static readonly ChatOptions ClassificationOptions = new()
    {
        Temperature = 0.1f,   // low temperature = consistent, deterministic output
        MaxOutputTokens = 10  // we only need a single word back
    };

    public async Task<Intent> AnalyzeAsync(string messageBody, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Sending message body to AI for intent classification");

        var response = await chatClient.GetResponseAsync(
            [
                new ChatMessage(ChatRole.System, SystemPrompt),
                new ChatMessage(ChatRole.User, messageBody)
            ],
            ClassificationOptions,
            cancellationToken);

        var classification = response.Text?.Trim() ?? string.Empty;

        logger.LogInformation("AI returned intent classification: {Classification}", classification);

        return classification.ToUpperInvariant() switch
        {
            "INTERESTED"                        => Intent.Interested,
            "CONFUSED"                          => Intent.Confused,
            "FRUSTRATED"                        => Intent.Frustrated,
            "OPTOUT" or "OPT OUT" or "OPT-OUT" => Intent.OptOut,
            _                                   => ApplyKeywordFallback(messageBody, classification)
        };
    }

    // Guards against malformed model output — prevents silent data loss
    private Intent ApplyKeywordFallback(string body, string rawResponse)
    {
        logger.LogWarning(
            "AI returned unrecognized value '{Response}'. Applying keyword fallback.",
            rawResponse);

        var lower = body.ToLowerInvariant();

        if (lower.Contains("stop") || lower.Contains("unsubscribe"))         return Intent.OptOut;
        if (new[] { "angry", "frustrated", "terrible", "awful", "hate" }.Any(lower.Contains)) return Intent.Frustrated;
        if (new[] { "confused", "unclear", "not sure" }.Any(lower.Contains))                  return Intent.Confused;

        return Intent.Interested;
    }
}