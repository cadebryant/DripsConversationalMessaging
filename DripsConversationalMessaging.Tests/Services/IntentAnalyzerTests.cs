using DripsConversationalMessaging.Server.Domain;
using DripsConversationalMessaging.Server.Services;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Moq;

namespace DripsConversationalMessaging.Tests.Services;

[TestFixture]
public class IntentAnalyzerTests
{
    private Mock<IChatClient> _chatClientMock = null!;
    private IntentAnalyzer _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _chatClientMock = new Mock<IChatClient>();
        _sut = new IntentAnalyzer(
            _chatClientMock.Object,
            new Mock<ILogger<IntentAnalyzer>>().Object);
    }

    private void SetupAiResponse(string text) =>
        _chatClientMock
            .Setup(c => c.GetResponseAsync(
                It.IsAny<IEnumerable<ChatMessage>>(),
                It.IsAny<ChatOptions?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ChatResponse(new ChatMessage(ChatRole.Assistant, text)));

    private void SetupAiException(Exception exception) =>
        _chatClientMock
            .Setup(c => c.GetResponseAsync(
                It.IsAny<IEnumerable<ChatMessage>>(),
                It.IsAny<ChatOptions?>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

    // ── AI success path ───────────────────────────────────────────────────────

    [TestCase("Interested", Intent.Interested)]
    [TestCase("Confused",   Intent.Confused)]
    [TestCase("Frustrated", Intent.Frustrated)]
    [TestCase("OptOut",     Intent.OptOut)]
    [TestCase("Opt Out",    Intent.OptOut)]
    [TestCase("Opt-Out",    Intent.OptOut)]
    public async Task AnalyzeAsync_ReturnsExpectedIntent_ForRecognizedAiResponse(
        string aiResponse, Intent expected)
    {
        SetupAiResponse(aiResponse);

        var result = await _sut.AnalyzeAsync("any message body");

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public async Task AnalyzeAsync_TrimsWhitespace_BeforeClassifying()
    {
        SetupAiResponse("  Frustrated  ");

        var result = await _sut.AnalyzeAsync("any message body");

        Assert.That(result, Is.EqualTo(Intent.Frustrated));
    }

    // ── Keyword fallback (unrecognized AI response) ───────────────────────────

    [TestCase("Please stop messaging me",         Intent.OptOut)]
    [TestCase("I want to unsubscribe",            Intent.OptOut)]
    [TestCase("I am angry about this",            Intent.Frustrated)]
    [TestCase("This is terrible service",         Intent.Frustrated)]
    [TestCase("I hate these messages",            Intent.Frustrated)]
    [TestCase("I am confused about the offer",    Intent.Confused)]
    [TestCase("This is unclear to me",            Intent.Confused)]
    [TestCase("I am not sure what you mean",      Intent.Confused)]
    [TestCase("Sounds interesting, tell me more", Intent.Interested)]
    public async Task AnalyzeAsync_AppliesKeywordFallback_WhenAiReturnsUnrecognizedValue(
        string messageBody, Intent expected)
    {
        SetupAiResponse("INVALID_RESPONSE");

        var result = await _sut.AnalyzeAsync(messageBody);

        Assert.That(result, Is.EqualTo(expected));
    }

    // ── AI exception path ─────────────────────────────────────────────────────

    [Test]
    public async Task AnalyzeAsync_FallsBackToKeywords_WhenAiClientThrows()
    {
        SetupAiException(new HttpRequestException("Connection refused"));

        var result = await _sut.AnalyzeAsync("please stop");

        Assert.That(result, Is.EqualTo(Intent.OptOut));
    }

    [Test]
    public async Task AnalyzeAsync_ReturnsInterested_WhenAiThrowsAndNoKeywordMatches()
    {
        SetupAiException(new HttpRequestException("Connection refused"));

        var result = await _sut.AnalyzeAsync("Sounds great, tell me more!");

        Assert.That(result, Is.EqualTo(Intent.Interested));
    }
}
