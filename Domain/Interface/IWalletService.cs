using CatatAja.Domain.DTOs;
using CatatAja.Domain.Entities;

namespace CatatAja.Domain.Interface;

public interface IWalletService
{
    Task<ApiResponse<IReadOnlyList<Wallet>>> GetWalletsByUserIdAsync(Guid userId);
    Task<ApiResponse<Wallet>> CreateWalletAsync(Guid userId, string name, decimal initialBalance);
    Task<ApiResponse<Wallet>> UpdateWalletAsync(Guid userId, Guid walletId, string name, decimal balance);
    Task<ApiResponse<bool>> DeleteWalletAsync(Guid userId, Guid walletId);
}
