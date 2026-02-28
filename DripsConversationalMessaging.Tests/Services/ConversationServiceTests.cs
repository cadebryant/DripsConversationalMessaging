using DripsConversationalMessaging.Server.Data;
using DripsConversationalMessaging.Server.Domain;
using DripsConversationalMessaging.Server.Models;
using DripsConversationalMessaging.Server.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace DripsConversationalMessaging.Tests.Services;

[TestFixture]
public class ConversationServiceTests
{
    private MessagingDbContext _db = null!;
    private Mock<IIntentAnalyzer> _intentAnalyzerMock = null!;
    private ConversationService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<MessagingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new MessagingDbContext(options);
        _intentAnalyzerMock = new Mock<IIntentAnalyzer>();
        _sut = new ConversationService(
            _db,
            _intentAnalyzerMock.Object,
            new Mock<ILogger<ConversationService>>().Object);
    }

    [TearDown]
    public void TearDown() => _db.Dispose();

    private void SetupIntent(Intent intent) =>
        _intentAnalyzerMock
            .Setup(a => a.AnalyzeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(intent);

    // ── IngestMessageAsync ────────────────────────────────────────────────────

    [Test]
    public async Task IngestMessageAsync_CreatesNewConversation_ForNewContact()
    {
        SetupIntent(Intent.Interested);
        var request = TestData.MessageRequests().Generate();

        await _sut.IngestMessageAsync(request);

        var conversation = await _db.Conversations
            .FirstOrDefaultAsync(c => c.ContactPhone == request.ContactPhone);
        Assert.That(conversation, Is.Not.Null);
    }

    [Test]
    public async Task IngestMessageAsync_ReusesExistingConversation_ForKnownContact()
    {
        SetupIntent(Intent.Interested);
        var request = TestData.MessageRequests().Generate();

        await _sut.IngestMessageAsync(request);
        await _sut.IngestMessageAsync(request);

        var count = await _db.Conversations
            .CountAsync(c => c.ContactPhone == request.ContactPhone);
        Assert.That(count, Is.EqualTo(1));
    }

    [Test]
    public async Task IngestMessageAsync_SetsIsOptedOut_WhenIntentIsOptOut()
    {
        SetupIntent(Intent.OptOut);
        var request = TestData.MessageRequests().Generate();

        await _sut.IngestMessageAsync(request);

        var conversation = await _db.Conversations
            .FirstAsync(c => c.ContactPhone == request.ContactPhone);
        Assert.That(conversation.IsOptedOut, Is.True);
    }

    [Test]
    public async Task IngestMessageAsync_SetsIsHighPriority_WhenIntentIsFrustrated()
    {
        SetupIntent(Intent.Frustrated);
        var request = TestData.MessageRequests().Generate();

        await _sut.IngestMessageAsync(request);

        var conversation = await _db.Conversations
            .FirstAsync(c => c.ContactPhone == request.ContactPhone);
        Assert.That(conversation.IsHighPriority, Is.True);
    }

    [TestCase(Intent.Interested)]
    [TestCase(Intent.Confused)]
    public async Task IngestMessageAsync_DoesNotSetIsHighPriority_ForNonFrustratedIntent(Intent intent)
    {
        SetupIntent(intent);
        var request = TestData.MessageRequests().Generate();

        await _sut.IngestMessageAsync(request);

        var conversation = await _db.Conversations
            .FirstAsync(c => c.ContactPhone == request.ContactPhone);
        Assert.That(conversation.IsHighPriority, Is.False);
    }

    [TestCase(Intent.Interested)]
    [TestCase(Intent.Confused)]
    [TestCase(Intent.Frustrated)]
    public async Task IngestMessageAsync_DoesNotSetIsOptedOut_ForNonOptOutIntent(Intent intent)
    {
        SetupIntent(intent);
        var request = TestData.MessageRequests().Generate();

        await _sut.IngestMessageAsync(request);

        var conversation = await _db.Conversations
            .FirstAsync(c => c.ContactPhone == request.ContactPhone);
        Assert.That(conversation.IsOptedOut, Is.False);
    }

    [Test]
    public async Task IngestMessageAsync_PersistsMessage_WithCorrectProperties()
    {
        SetupIntent(Intent.Confused);
        var request = TestData.MessageRequests().Generate();

        await _sut.IngestMessageAsync(request);

        var message = await _db.Messages.FirstOrDefaultAsync();
        Assert.Multiple(() =>
        {
            Assert.That(message, Is.Not.Null);
            Assert.That(message!.Body, Is.EqualTo(request.Body));
            Assert.That(message.Sender, Is.EqualTo(request.Sender));
            Assert.That(message.Intent, Is.EqualTo(Intent.Confused));
        });
    }

    [Test]
    public async Task IngestMessageAsync_ReturnsPersistedMessage()
    {
        SetupIntent(Intent.Interested);
        var request = TestData.MessageRequests().Generate();

        var result = await _sut.IngestMessageAsync(request);

        Assert.Multiple(() =>
        {
            Assert.That(result.Id, Is.Not.EqualTo(Guid.Empty));
            Assert.That(result.Body, Is.EqualTo(request.Body));
            Assert.That(result.Sender, Is.EqualTo(request.Sender));
            Assert.That(result.Intent, Is.EqualTo(Intent.Interested));
        });
    }

    [Test]
    public async Task IngestMessageAsync_LinksMessageToConversation()
    {
        SetupIntent(Intent.Interested);
        var request = TestData.MessageRequests().Generate();

        var message = await _sut.IngestMessageAsync(request);

        var conversation = await _db.Conversations
            .FirstAsync(c => c.ContactPhone == request.ContactPhone);
        Assert.That(message.ConversationId, Is.EqualTo(conversation.Id));
    }

    [Test]
    public async Task IngestMessageAsync_SecondMessage_AppendedToSameConversation()
    {
        SetupIntent(Intent.Interested);
        var request = TestData.MessageRequests().Generate();

        var first = await _sut.IngestMessageAsync(request);
        var second = await _sut.IngestMessageAsync(request);

        Assert.That(second.ConversationId, Is.EqualTo(first.ConversationId));
    }

    // ── GetHighPriorityConversationsAsync ─────────────────────────────────────

    [Test]
    public async Task GetHighPriorityConversationsAsync_ReturnsOnlyHighPriorityConversations()
    {
        _db.Conversations.AddRange(TestData.Conversations(isHighPriority: true).Generate(2));
        _db.Conversations.AddRange(TestData.Conversations(isHighPriority: false).Generate(3));
        await _db.SaveChangesAsync();

        var result = await _sut.GetHighPriorityConversationsAsync();

        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result.All(c => c.IsHighPriority), Is.True);
        });
    }

    [Test]
    public async Task GetHighPriorityConversationsAsync_ReturnsEmptyList_WhenNoneAreHighPriority()
    {
        _db.Conversations.AddRange(TestData.Conversations(isHighPriority: false).Generate(3));
        await _db.SaveChangesAsync();

        var result = await _sut.GetHighPriorityConversationsAsync();

        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task GetHighPriorityConversationsAsync_IncludesMessages_InResult()
    {
        SetupIntent(Intent.Frustrated);
        var request = TestData.MessageRequests().Generate();
        await _sut.IngestMessageAsync(request);

        var result = await _sut.GetHighPriorityConversationsAsync();

        Assert.That(result[0].Messages, Has.Count.EqualTo(1));
    }
}
