using CatatAja.Domain.Interface;
using Telegram.Bot.Types;
using DomainUser = CatatAja.Domain.Entities.User;

namespace CatatAja.Api.Middlewares;

public class TelegramUserMiddleware
{
    private readonly IUserService _userService;

    public TelegramUserMiddleware(IUserService userService)
    {
        _userService = userService;
    }

    public async Task<DomainUser?> EnsureUserAsync(Message message)
    {
        if (message.From is null)
        {
            return null;
        }

        var telegramId = message.From.Id.ToString();
        var username = message.From.Username;
        var firstName = message.From.FirstName;
        var lastName = message.From.LastName;

        var response = await _userService.GetOrCreateByTelegramIdAsync(telegramId, username, firstName, lastName);
        return response.Data;
    }
}
