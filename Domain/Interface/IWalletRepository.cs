using CatatAja.Domain.Entities;

namespace CatatAja.Domain.Interface;

public interface IWalletRepository
{
    Task<IReadOnlyList<Wallet>> GetByUserIdAsync(Guid userId);
    Task<Wallet?> GetByIdAndUserIdAsync(Guid id, Guid userId);
    Task<Wallet> CreateAsync(Wallet wallet);
    Task<Wallet> UpdateAsync(Wallet wallet);
    Task DeleteAsync(Wallet wallet);
}
