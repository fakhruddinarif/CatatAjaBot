using CatatAja.Domain.Entities;

namespace CatatAja.Domain.Interface;
public interface IUserRepository
{
    Task<User?> GetByTelegramIdAsync(string telegramId);
    Task<User> CreateAsync(User user);
    Task<User> UpdateAsync(User user);
    Task DeleteAsync(Guid id);
}