using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using DripsConversationalMessaging.Server.Domain;
using DripsConversationalMessaging.Server.Models;
using DripsConversationalMessaging.Server.Services;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;

namespace DripsConversationalMessaging.Tests.Endpoints;

[TestFixture]
public class IngestMessageEndpointTests
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
    public async Task Post_ValidRequest_Returns201Created()
    {
        var request = TestData.MessageRequests().Generate();
        _serviceMock.Setup(s => s.IngestMessageAsync(It.IsAny<IngestMessageRequest>()))
            .ReturnsAsync(TestData.Messages().Generate());

        var response = await _client.PostAsJsonAsync("/api/messages/ingest", request);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
    }

    [Test]
    public async Task Post_ValidRequest_ReturnsMessageInBody()
    {
        var request = TestData.MessageRequests().Generate();
        var expected = TestData.Messages().Generate();
        _serviceMock.Setup(s => s.IngestMessageAsync(It.IsAny<IngestMessageRequest>()))
            .ReturnsAsync(expected);

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter() }
        };

        var response = await _client.PostAsJsonAsync("/api/messages/ingest", request);
        var body = await response.Content.ReadFromJsonAsync<Message>(jsonOptions);

        Assert.Multiple(() =>
        {
            Assert.That(body, Is.Not.Null);
            Assert.That(body!.Id, Is.EqualTo(expected.Id));
            Assert.That(body.Body, Is.EqualTo(expected.Body));
            Assert.That(body.Sender, Is.EqualTo(expected.Sender));
            Assert.That(body.Intent, Is.EqualTo(expected.Intent));
        });
    }

    [Test]
    public async Task Post_ValidRequest_SetsLocationHeaderToMessageUri()
    {
        var request = TestData.MessageRequests().Generate();
        var message = TestData.Messages().Generate();
        _serviceMock.Setup(s => s.IngestMessageAsync(It.IsAny<IngestMessageRequest>()))
            .ReturnsAsync(message);

        var response = await _client.PostAsJsonAsync("/api/messages/ingest", request);

        Assert.That(response.Headers.Location?.ToString(),
            Does.Contain($"/api/messages/{message.Id}"));
    }

    [Test]
    public async Task Post_ValidRequest_PassesCorrectDataToService()
    {
        var request = TestData.MessageRequests().Generate();
        _serviceMock.Setup(s => s.IngestMessageAsync(It.IsAny<IngestMessageRequest>()))
            .ReturnsAsync(TestData.Messages().Generate());

        await _client.PostAsJsonAsync("/api/messages/ingest", request);

        _serviceMock.Verify(s => s.IngestMessageAsync(
            It.Is<IngestMessageRequest>(r =>
                r.ContactPhone == request.ContactPhone &&
                r.Sender == request.Sender &&
                r.Body == request.Body)),
            Times.Once);
    }

    [Test]
    public async Task Post_ServiceThrows_Returns500()
    {
        var request = TestData.MessageRequests().Generate();
        _serviceMock.Setup(s => s.IngestMessageAsync(It.IsAny<IngestMessageRequest>()))
            .ThrowsAsync(new InvalidOperationException("Unexpected storage failure"));

        var response = await _client.PostAsJsonAsync("/api/messages/ingest", request);

        Assert.That((int)response.StatusCode, Is.EqualTo(500));
    }

    [Test]
    public async Task Post_MalformedJson_Returns400()
    {
        var content = new StringContent("this is not json", Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/messages/ingest", content);

        Assert.That((int)response.StatusCode, Is.EqualTo(400));
    }
}
