using CatatAja.Domain.DTOs;
using CatatAja.Domain.Entities;
using CatatAja.Domain.Interface;
using CatatAja.Provider.Data;

namespace CatatAja.Application.Services;

public class WalletService : IWalletService
{
    private readonly IWalletRepository _walletRepository;
    private readonly AppDbContext _dbContext;

    public WalletService(IWalletRepository walletRepository, AppDbContext dbContext)
    {
        _walletRepository = walletRepository;
        _dbContext = dbContext;
    }

    public async Task<ApiResponse<IReadOnlyList<Wallet>>> GetWalletsByUserIdAsync(Guid userId)
    {
        var wallets = await _walletRepository.GetByUserIdAsync(userId);
        return ApiResponse<IReadOnlyList<Wallet>>.Success(200, wallets, "Wallet list retrieved.");
    }

    public async Task<ApiResponse<Wallet>> CreateWalletAsync(Guid userId, string name, decimal initialBalance)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                await transaction.RollbackAsync();
                return ApiResponse<Wallet>.Fail(400, "Wallet name is required.");
            }

            var wallet = new Wallet
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Name = name.Trim(),
                Balance = initialBalance,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createdWallet = await _walletRepository.CreateAsync(wallet);
            await transaction.CommitAsync();
            return ApiResponse<Wallet>.Success(201, createdWallet, "Wallet created.");
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<ApiResponse<Wallet>> UpdateWalletAsync(Guid userId, Guid walletId, string name, decimal balance)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                await transaction.RollbackAsync();
                return ApiResponse<Wallet>.Fail(400, "Wallet name is required.");
            }

            var wallet = await _walletRepository.GetByIdAndUserIdAsync(walletId, userId);
            if (wallet is null)
            {
                await transaction.RollbackAsync();
                return ApiResponse<Wallet>.Fail(404, "Wallet not found.");
            }

            wallet.Name = name.Trim();
            wallet.Balance = balance;
            wallet.UpdatedAt = DateTime.UtcNow;

            var updatedWallet = await _walletRepository.UpdateAsync(wallet);
            await transaction.CommitAsync();
            return ApiResponse<Wallet>.Success(200, updatedWallet, "Wallet updated.");
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<ApiResponse<bool>> DeleteWalletAsync(Guid userId, Guid walletId)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            var wallet = await _walletRepository.GetByIdAndUserIdAsync(walletId, userId);
            if (wallet is null)
            {
                await transaction.RollbackAsync();
                return ApiResponse<bool>.Fail(404, "Wallet not found.");
            }

            await _walletRepository.DeleteAsync(wallet);
            await transaction.CommitAsync();
            return ApiResponse<bool>.Success(200, true, "Wallet deleted.");
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
