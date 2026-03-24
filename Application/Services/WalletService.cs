using CatatAja.Domain.DTOs;
using CatatAja.Domain.Entities;
using CatatAja.Domain.Interface;

namespace CatatAja.Application.Services;

public class WalletService : IWalletService
{
    private readonly IWalletRepository _walletRepository;

    public WalletService(IWalletRepository walletRepository)
    {
        _walletRepository = walletRepository;
    }

    public async Task<ApiResponse<IReadOnlyList<Wallet>>> GetWalletsByUserIdAsync(Guid userId)
    {
        var wallets = await _walletRepository.GetByUserIdAsync(userId);
        return ApiResponse<IReadOnlyList<Wallet>>.Success(200, wallets, "Wallet list retrieved.");
    }

    public async Task<ApiResponse<Wallet>> CreateWalletAsync(Guid userId, string name, decimal initialBalance)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
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
        return ApiResponse<Wallet>.Success(201, createdWallet, "Wallet created.");
    }

    public async Task<ApiResponse<Wallet>> UpdateWalletAsync(Guid userId, Guid walletId, string name, decimal balance)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return ApiResponse<Wallet>.Fail(400, "Wallet name is required.");
        }

        var wallet = await _walletRepository.GetByIdAndUserIdAsync(walletId, userId);
        if (wallet is null)
        {
            return ApiResponse<Wallet>.Fail(404, "Wallet not found.");
        }

        wallet.Name = name.Trim();
        wallet.Balance = balance;
        wallet.UpdatedAt = DateTime.UtcNow;

        var updatedWallet = await _walletRepository.UpdateAsync(wallet);
        return ApiResponse<Wallet>.Success(200, updatedWallet, "Wallet updated.");
    }

    public async Task<ApiResponse<bool>> DeleteWalletAsync(Guid userId, Guid walletId)
    {
        var wallet = await _walletRepository.GetByIdAndUserIdAsync(walletId, userId);
        if (wallet is null)
        {
            return ApiResponse<bool>.Fail(404, "Wallet not found.");
        }

        await _walletRepository.DeleteAsync(wallet);
        return ApiResponse<bool>.Success(200, true, "Wallet deleted.");
    }
}
