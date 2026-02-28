using System.Text.Json;
using System.Text.Json.Serialization;
using DripsConversationalMessaging.Server.Data;
using DripsConversationalMessaging.Server.Endpoints;
using DripsConversationalMessaging.Server.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using OllamaSharp;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddOpenApi();

builder.Services.AddDbContext<MessagingDbContext>(options =>
    options.UseInMemoryDatabase("MessagingDb"));

builder.Services.AddScoped<IConversationService, ConversationService>();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// ── AI Intent Analysis ────────────────────────────────────────────────────────
// Option A: Ollama (local, free). Install Ollama, then: ollama pull llama3.2
builder.Services.AddSingleton<IChatClient>(_ =>
{
    var ollama = new OllamaApiClient(
        new Uri(builder.Configuration["Ollama:Endpoint"] ?? "http://localhost:11434"));
    ollama.SelectedModel = builder.Configuration["Ollama:Model"] ?? "llama3.2";
    return ollama;
});

// Option B: Azure OpenAI — install Microsoft.Extensions.AI.OpenAI, then swap the above for:
// builder.Services.AddSingleton<IChatClient>(_ =>
//     new AzureOpenAIClient(
//         new Uri(builder.Configuration["AzureOpenAI:Endpoint"]!),
//         new AzureKeyCredential(builder.Configuration["AzureOpenAI:Key"]!))
//     .AsChatClient(builder.Configuration["AzureOpenAI:Deployment"]!));

builder.Services.AddSingleton<IIntentAnalyzer, IntentAnalyzer>();
// ─────────────────────────────────────────────────────────────────────────────

var app = builder.Build();

app.MapDefaultEndpoints();

app.UseDefaultFiles();
app.MapStaticAssets();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapConversationEndpoints();

app.MapFallbackToFile("/index.html");

app.Run();

public partial class Program { }
