using DripsConversationalMessaging.Server.Data;
using DripsConversationalMessaging.Server.Endpoints;
using DripsConversationalMessaging.Server.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddOpenApi();

builder.Services.AddDbContext<MessagingDbContext>(options =>
    options.UseInMemoryDatabase("MessagingDb"));

builder.Services.AddScoped<IConversationService, ConversationService>();

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
