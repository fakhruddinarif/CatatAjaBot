using CatatAja.Domain.DTOs;
using CatatAja.Domain.Entities;

namespace CatatAja.Domain.Interface;

public interface IUserService
{
    Task<ApiResponse<User>> CreateUserAsync(User user);
    Task<ApiResponse<User>> GetUserByTelegramIdAsync(string telegramId);
    Task<ApiResponse<User>> GetOrCreateByTelegramIdAsync(string telegramId, string? username, string? firstName, string? lastName);
    Task<ApiResponse<User>> UpdateUserAsync(User user);
    Task<ApiResponse<bool>> DeleteUserAsync(Guid id);
}