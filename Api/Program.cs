using CatatAja.Api.Controllers;
using CatatAja.Api.Middlewares;
using CatatAja.Application.Repositories;
using CatatAja.Application.Services;
using CatatAja.Domain.Interface;
using CatatAja.Provider.Data;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

var builder = WebApplication.CreateBuilder(args);

// Telegram Bot Webhook Endpoint
var telegramBotToken = builder.Configuration["Telegram:BotToken"] ?? throw new InvalidOperationException("Telegram BotToken is not configured.");

// Configure Entity Framework Core with PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options =>
{
    // Mengambil Connection String dari appsettings.json
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IWalletRepository, WalletRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IWalletService, WalletService>();
builder.Services.AddScoped<TelegramUserMiddleware>();
builder.Services.AddScoped<TelegramCommandController>();

var app = builder.Build();
var serviceScopeFactory = app.Services.GetRequiredService<IServiceScopeFactory>();

using var cts = new CancellationTokenSource();
var bot = new TelegramBotClient(telegramBotToken);
var me = await bot.GetMe();
Console.WriteLine($"@{me.Username} is running...");

// Handle updates
async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
{
    // Only process message updates
    if (update.Message is not { } message || string.IsNullOrWhiteSpace(message.Text))
        return;

    using var scope = serviceScopeFactory.CreateScope();
    var userMiddleware = scope.ServiceProvider.GetRequiredService<TelegramUserMiddleware>();
    var commandController = scope.ServiceProvider.GetRequiredService<TelegramCommandController>();

    var user = await userMiddleware.EnsureUserAsync(message);
    if (user is not { } ensuredUser)
    {
        await botClient.SendMessage(message.Chat.Id, "Tidak bisa memproses user Telegram.", cancellationToken: cancellationToken);
        return;
    }

    var userId = message.From?.Id;
    var username = message.From?.Username ?? "N/A";
    Console.WriteLine($"Received message '{message.Text}' from User ID: {userId}, Username: @{username}");

    await commandController.HandleAsync(botClient, message, ensuredUser, cancellationToken);
}

// Handle errors
async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
{
    Console.WriteLine($"Error: {exception.Message}");
    await Task.CompletedTask;
}

// Start polling for updates
var receiverOptions = new ReceiverOptions() { AllowedUpdates = [] };
_ = bot.ReceiveAsync(
    updateHandler: HandleUpdateAsync,
    errorHandler: HandleErrorAsync,
    receiverOptions: receiverOptions,
    cancellationToken: cts.Token
);

app.MapGet("/", () => {
    return Results.Ok(new { message = "CatatAja API is running!" });
});

await app.RunAsync();
