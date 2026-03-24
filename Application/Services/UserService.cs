using CatatAja.Domain.DTOs;
using CatatAja.Domain.Entities;
using CatatAja.Domain.Interface;
using CatatAja.Provider.Data;

namespace CatatAja.Application.Services;

public class UserService : IUserService
{
	private readonly IUserRepository _userRepository;
	private readonly AppDbContext _dbContext;

	public UserService(IUserRepository userRepository, AppDbContext dbContext)
	{
		_userRepository = userRepository;
		_dbContext = dbContext;
	}

	public async Task<ApiResponse<User>> CreateUserAsync(User user)
	{
		await using var transaction = await _dbContext.Database.BeginTransactionAsync();
		try
		{
			if (string.IsNullOrWhiteSpace(user.TelegramId))
			{
				await transaction.RollbackAsync();
				return ApiResponse<User>.Fail(400, "Telegram ID is required.");
			}

			var existingUser = await _userRepository.GetByTelegramIdAsync(user.TelegramId);
			if (existingUser is not null)
			{
				await transaction.RollbackAsync();
				return ApiResponse<User>.Fail(409, "User already exists.");
			}

			user.Id = user.Id == Guid.Empty ? Guid.NewGuid() : user.Id;
			user.CreatedAt = DateTime.UtcNow;
			user.UpdatedAt = DateTime.UtcNow;

			var createdUser = await _userRepository.CreateAsync(user);
			await transaction.CommitAsync();
			return ApiResponse<User>.Success(201, createdUser, "User created.");
		}
		catch
		{
			await transaction.RollbackAsync();
			throw;
		}
	}

	public async Task<ApiResponse<User>> GetUserByTelegramIdAsync(string telegramId)
	{
		var user = await _userRepository.GetByTelegramIdAsync(telegramId);
		if (user is null)
		{
			return ApiResponse<User>.Fail(404, "User not found.");
		}

		return ApiResponse<User>.Success(200, user, "User found.");
	}

	public async Task<ApiResponse<User>> GetOrCreateByTelegramIdAsync(string telegramId, string? username, string? firstName, string? lastName)
	{
		await using var transaction = await _dbContext.Database.BeginTransactionAsync();
		try
		{
			if (string.IsNullOrWhiteSpace(telegramId))
			{
				await transaction.RollbackAsync();
				return ApiResponse<User>.Fail(400, "Telegram ID is required.");
			}

			var existingUser = await _userRepository.GetByTelegramIdAsync(telegramId);
			if (existingUser is not null)
			{
				if (!string.Equals(existingUser.Username, username, StringComparison.Ordinal))
				{
					existingUser.Username = username;
					existingUser.UpdatedAt = DateTime.UtcNow;
					var updatedUser = await _userRepository.UpdateAsync(existingUser);
					await transaction.CommitAsync();
					return ApiResponse<User>.Success(200, updatedUser, "User found and updated.");
				}

				await transaction.CommitAsync();
				return ApiResponse<User>.Success(200, existingUser, "User found.");
			}

			var derivedUsername = username;
			if (string.IsNullOrWhiteSpace(derivedUsername))
			{
				derivedUsername = BuildDisplayName(firstName, lastName);
			}

			var newUser = new User
			{
				Id = Guid.NewGuid(),
				TelegramId = telegramId,
				Username = string.IsNullOrWhiteSpace(derivedUsername) ? null : derivedUsername,
				CreatedAt = DateTime.UtcNow,
				UpdatedAt = DateTime.UtcNow
			};

			var createdUser = await _userRepository.CreateAsync(newUser);
			await transaction.CommitAsync();
			return ApiResponse<User>.Success(201, createdUser, "User created.");
		}
		catch
		{
			await transaction.RollbackAsync();
			throw;
		}
	}

	public async Task<ApiResponse<User>> UpdateUserAsync(User user)
	{
		await using var transaction = await _dbContext.Database.BeginTransactionAsync();
		try
		{
			if (user.Id == Guid.Empty)
			{
				await transaction.RollbackAsync();
				return ApiResponse<User>.Fail(400, "User ID is required.");
			}

			var existingUser = await _userRepository.GetByTelegramIdAsync(user.TelegramId);
			if (existingUser is null)
			{
				await transaction.RollbackAsync();
				return ApiResponse<User>.Fail(404, "User not found.");
			}

			existingUser.Username = user.Username;
			existingUser.UpdatedAt = DateTime.UtcNow;

			var updatedUser = await _userRepository.UpdateAsync(existingUser);
			await transaction.CommitAsync();
			return ApiResponse<User>.Success(200, updatedUser, "User updated.");
		}
		catch
		{
			await transaction.RollbackAsync();
			throw;
		}
	}

	public async Task<ApiResponse<bool>> DeleteUserAsync(Guid id)
	{
		await using var transaction = await _dbContext.Database.BeginTransactionAsync();
		try
		{
			if (id == Guid.Empty)
			{
				await transaction.RollbackAsync();
				return ApiResponse<bool>.Fail(400, "User ID is required.");
			}

			await _userRepository.DeleteAsync(id);
			await transaction.CommitAsync();
			return ApiResponse<bool>.Success(200, true, "User deleted.");
		}
		catch
		{
			await transaction.RollbackAsync();
			throw;
		}
	}

	private static string? BuildDisplayName(string? firstName, string? lastName)
	{
		var fullName = string.Join(" ", new[] { firstName, lastName }.Where(x => !string.IsNullOrWhiteSpace(x))).Trim();
		return string.IsNullOrWhiteSpace(fullName) ? null : fullName;
	}
}
