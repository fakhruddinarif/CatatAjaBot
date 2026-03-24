using CatatAja.Domain.Interface;
using Telegram.Bot;
using Telegram.Bot.Types;
using DomainUser = CatatAja.Domain.Entities.User;

namespace CatatAja.Api.Controllers;

public class TelegramCommandController
{
    private readonly IWalletService _walletService;

    public TelegramCommandController(IWalletService walletService)
    {
        _walletService = walletService;
    }

    public async Task HandleAsync(ITelegramBotClient botClient, Message message, DomainUser user, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(message.Text))
        {
            return;
        }

        var text = message.Text.Trim();

        if (text.StartsWith("/my", StringComparison.OrdinalIgnoreCase))
        {
            await HandleMyAsync(botClient, message, user, cancellationToken);
            return;
        }

        if (text.StartsWith("/wallets", StringComparison.OrdinalIgnoreCase))
        {
            await HandleWalletsAsync(botClient, message, user, cancellationToken);
            return;
        }

        if (text.StartsWith("/new-wallet", StringComparison.OrdinalIgnoreCase))
        {
            await HandleNewWalletAsync(botClient, message, user, text, cancellationToken);
            return;
        }

        if (text.StartsWith("/update-wallet/", StringComparison.OrdinalIgnoreCase))
        {
            await HandleUpdateWalletAsync(botClient, message, user, text, cancellationToken);
            return;
        }

        if (text.StartsWith("/delete-wallet/", StringComparison.OrdinalIgnoreCase))
        {
            await HandleDeleteWalletAsync(botClient, message, user, text, cancellationToken);
        }
    }

    private static async Task HandleMyAsync(ITelegramBotClient botClient, Message message, DomainUser user, CancellationToken cancellationToken)
    {
        var result = $"ID: {user.TelegramId}\nUsername: @{user.Username ?? "N/A"}\nCreated At: {user.CreatedAt:yyyy-MM-dd HH:mm:ss} UTC";
        await botClient.SendMessage(message.Chat.Id, result, cancellationToken: cancellationToken);
    }

    private async Task HandleWalletsAsync(ITelegramBotClient botClient, Message message, DomainUser user, CancellationToken cancellationToken)
    {
        var response = await _walletService.GetWalletsByUserIdAsync(user.Id);

        if (response.Data is null || response.Data.Count == 0)
        {
            await botClient.SendMessage(message.Chat.Id, "Kamu belum punya wallet. Gunakan /new-wallet <nama> <saldo-awal>", cancellationToken: cancellationToken);
            return;
        }

        var lines = response.Data.Select(w => $"- {w.Id} | {w.Name} | Balance: {w.Balance}");
        var result = "Wallet kamu:\n" + string.Join("\n", lines);
        await botClient.SendMessage(message.Chat.Id, result, cancellationToken: cancellationToken);
    }

    private async Task HandleNewWalletAsync(ITelegramBotClient botClient, Message message, DomainUser user, string text, CancellationToken cancellationToken)
    {
        var payload = text.Replace("/new-wallet", string.Empty, StringComparison.OrdinalIgnoreCase).Trim();
        if (string.IsNullOrWhiteSpace(payload))
        {
            await botClient.SendMessage(message.Chat.Id, "Format: /new-wallet <nama> <saldo-awal>\nContoh: /new-wallet Cash 100000", cancellationToken: cancellationToken);
            return;
        }

        var parts = payload.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2 || !decimal.TryParse(parts[^1], out var initialBalance))
        {
            await botClient.SendMessage(message.Chat.Id, "Format salah. Gunakan: /new-wallet <nama> <saldo-awal>", cancellationToken: cancellationToken);
            return;
        }

        var name = string.Join(' ', parts.Take(parts.Length - 1));
        var response = await _walletService.CreateWalletAsync(user.Id, name, initialBalance);

        if (response.Data is null)
        {
            await botClient.SendMessage(message.Chat.Id, response.Message, cancellationToken: cancellationToken);
            return;
        }

        await botClient.SendMessage(message.Chat.Id, $"Wallet berhasil dibuat.\nID: {response.Data.Id}\nNama: {response.Data.Name}\nBalance: {response.Data.Balance}", cancellationToken: cancellationToken);
    }

    private async Task HandleUpdateWalletAsync(ITelegramBotClient botClient, Message message, DomainUser user, string text, CancellationToken cancellationToken)
    {
        var segments = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length < 3)
        {
            await botClient.SendMessage(message.Chat.Id, "Format: /update-wallet/<id> <nama-baru> <balance-baru>", cancellationToken: cancellationToken);
            return;
        }

        var commandPart = segments[0];
        var walletIdText = commandPart.Replace("/update-wallet/", string.Empty, StringComparison.OrdinalIgnoreCase).Trim();

        if (!Guid.TryParse(walletIdText, out var walletId))
        {
            await botClient.SendMessage(message.Chat.Id, "Wallet ID tidak valid.", cancellationToken: cancellationToken);
            return;
        }

        if (!decimal.TryParse(segments[^1], out var newBalance))
        {
            await botClient.SendMessage(message.Chat.Id, "Balance baru tidak valid.", cancellationToken: cancellationToken);
            return;
        }

        var newName = string.Join(' ', segments.Skip(1).Take(segments.Length - 2));
        var response = await _walletService.UpdateWalletAsync(user.Id, walletId, newName, newBalance);

        await botClient.SendMessage(message.Chat.Id, response.Message, cancellationToken: cancellationToken);
    }

    private async Task HandleDeleteWalletAsync(ITelegramBotClient botClient, Message message, DomainUser user, string text, CancellationToken cancellationToken)
    {
        var walletIdText = text.Replace("/delete-wallet/", string.Empty, StringComparison.OrdinalIgnoreCase).Trim();
        if (!Guid.TryParse(walletIdText, out var walletId))
        {
            await botClient.SendMessage(message.Chat.Id, "Wallet ID tidak valid.", cancellationToken: cancellationToken);
            return;
        }

        var response = await _walletService.DeleteWalletAsync(user.Id, walletId);
        await botClient.SendMessage(message.Chat.Id, response.Message, cancellationToken: cancellationToken);
    }
}
