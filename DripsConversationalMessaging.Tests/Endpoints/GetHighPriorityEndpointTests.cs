using System.Net;
using System.Net.Http.Json;
using DripsConversationalMessaging.Server.Domain;
using DripsConversationalMessaging.Server.Services;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;

namespace DripsConversationalMessaging.Tests.Endpoints;

[TestFixture]
public class GetHighPriorityEndpointTests
{
    private WebApplicationFactory<Program> _factory = null!;
    private Mock<IConversationService> _serviceMock = null!;
    private HttpClient _client = null!;

    [SetUp]
    public void SetUp()
    {
        _serviceMock = new Mock<IConversationService>();
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(b => b.ConfigureTestServices(services =>
            {
                services.Replace(ServiceDescriptor.Scoped<IConversationService>(
                    _ => _serviceMock.Object));
            }));
        _client = _factory.CreateClient();
    }

    [TearDown]
    public async Task TearDown()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
    }

    [Test]
    public async Task Get_Returns200Ok()
    {
        _serviceMock.Setup(s => s.GetHighPriorityConversationsAsync())
            .ReturnsAsync([]);

        var response = await _client.GetAsync("/api/conversations/priority");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task Get_ReturnsConversationsFromService()
    {
        var conversations = TestData.Conversations(isHighPriority: true).Generate(3);
        _serviceMock.Setup(s => s.GetHighPriorityConversationsAsync())
            .ReturnsAsync(conversations);

        var response = await _client.GetAsync("/api/conversations/priority");
        var body = await response.Content.ReadFromJsonAsync<List<Conversation>>();

        Assert.Multiple(() =>
        {
            Assert.That(body, Is.Not.Null);
            Assert.That(body!, Has.Count.EqualTo(3));
            Assert.That(body.Select(c => c.Id),
                Is.EquivalentTo(conversations.Select(c => c.Id)));
        });
    }

    [Test]
    public async Task Get_EmptyList_ReturnsOkWithEmptyArray()
    {
        _serviceMock.Setup(s => s.GetHighPriorityConversationsAsync())
            .ReturnsAsync([]);

        var response = await _client.GetAsync("/api/conversations/priority");
        var body = await response.Content.ReadFromJsonAsync<List<Conversation>>();

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(body, Is.Not.Null);
            Assert.That(body!, Is.Empty);
        });
    }

    [Test]
    public async Task Get_CallsServiceExactlyOnce()
    {
        _serviceMock.Setup(s => s.GetHighPriorityConversationsAsync())
            .ReturnsAsync([]);

        await _client.GetAsync("/api/conversations/priority");

        _serviceMock.Verify(s => s.GetHighPriorityConversationsAsync(), Times.Once);
    }

    [Test]
    public async Task Get_ServiceThrows_Returns500()
    {
        _serviceMock.Setup(s => s.GetHighPriorityConversationsAsync())
            .ThrowsAsync(new Exception("Database unavailable"));

        var response = await _client.GetAsync("/api/conversations/priority");

        Assert.That((int)response.StatusCode, Is.EqualTo(500));
    }

    [Test]
    public async Task Get_ReturnsOnlyHighPriorityConversations()
    {
        var highPriority = TestData.Conversations(isHighPriority: true).Generate(2);
        _serviceMock.Setup(s => s.GetHighPriorityConversationsAsync())
            .ReturnsAsync(highPriority);

        var response = await _client.GetAsync("/api/conversations/priority");
        var body = await response.Content.ReadFromJsonAsync<List<Conversation>>();

        Assert.That(body!.All(c => c.IsHighPriority), Is.True);
    }
}
