using Microsoft.AspNetCore.Mvc;
using CatatAja.Provider.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Telegram Bot Webhook Endpoint
var telegramSecretToken = builder.Configuration["Telegram:SecretToken"] ?? throw new InvalidOperationException("Telegram SecretToken is not configured.");

// Configure Entity Framework Core with PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options =>
{
    // Mengambil Connection String dari appsettings.json
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

var app = builder.Build();

// Endpoint to receive webhook updates from Telegram
app.MapPost("/api/telegram/webhook", async ([FromBody] dynamic update, HttpContext context) =>
{
    var requestToken = context.Request.Headers["X-Telegram-Bot-Api"].ToString();
    if (!string.Equals(requestToken, telegramSecretToken, StringComparison.Ordinal))
    {
        app.Logger.LogWarning("Unauthorized access attempt with token: {Token}", requestToken);
        return Results.Unauthorized();
    }

    app.Logger.LogInformation("Received Telegram Successfully!");

    return Results.Ok();
});

app.MapGet("/", () => "CatatAja-Bot API is running!");

await app.RunAsync();
