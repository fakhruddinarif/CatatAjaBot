using Microsoft.AspNetCore.Mvc;
using CatatAja.Provider.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.Json.Serialization;

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
app.MapPost("/api/webhook/telegram", async (HttpContext context) =>
{
    // Ambil token dari header dan trim whitespace
    var requestToken = context.Request.Headers["X-Telegram-Bot-Api-Secret-Token"].ToString().Trim();
    
    // Debug logging
    Console.WriteLine($"[DEBUG] Received token: '{requestToken}'");
    Console.WriteLine($"[DEBUG] Expected token: '{telegramSecretToken}'");
    Console.WriteLine($"[DEBUG] Token length received: {requestToken.Length}");
    Console.WriteLine($"[DEBUG] Token length expected: {telegramSecretToken.Length}");
    
    if (string.IsNullOrEmpty(requestToken))
    {
        app.Logger.LogWarning("Secret token header is missing!");
        Console.WriteLine("[WARN] X-Telegram-Bot-Api-Secret-Token header tidak ditemukan");
        return Results.BadRequest("Missing secret token");
    }
    
    if (!string.Equals(requestToken, telegramSecretToken, StringComparison.Ordinal))
    {
        app.Logger.LogWarning($"Unauthorized access attempt. Received: '{requestToken}', Expected: '{telegramSecretToken}'");
        return Results.Unauthorized();
    }

    try
    {
        // Baca raw body dari request
        using (var reader = new StreamReader(context.Request.Body))
        {
            var jsonPayload = await reader.ReadToEndAsync();
            
            Console.WriteLine("=== New Message Received ===");
            Console.WriteLine($"Raw JSON: {jsonPayload}");
            
            // Parse JSON
            using (var doc = JsonDocument.Parse(jsonPayload))
            {
                var root = doc.RootElement;
                
                // Ekstrak message object
                if (root.TryGetProperty("message", out var messageElement))
                {
                    var from = messageElement.GetProperty("from");
                    var userId = from.GetProperty("id").GetInt64();
                    var username = from.TryGetProperty("username", out var usernameElement) ? usernameElement.GetString() : "N/A";
                    var firstName = from.TryGetProperty("first_name", out var firstNameElement) ? firstNameElement.GetString() : "";
                    var messageText = messageElement.TryGetProperty("text", out var textElement) ? textElement.GetString() : "";
                    
                    Console.WriteLine($"User ID: {userId}");
                    Console.WriteLine($"Username: {username}");
                    Console.WriteLine($"First Name: {firstName}");
                    Console.WriteLine($"Message: {messageText}");
                }
                else if (root.TryGetProperty("edited_message", out _))
                {
                    Console.WriteLine("Edited message received");
                }
                else
                {
                    Console.WriteLine("Unknown update type received, not a message or edited_message");
                }
            }
            
            Console.WriteLine("================================");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error parsing update: {ex.Message}");
        app.Logger.LogError(ex, "Error parsing telegram update");
    }

    return Results.Ok();
});

app.MapGet("/", () => {
    return Results.Ok("Hello, CatatAja API is running!");
});

await app.RunAsync();
