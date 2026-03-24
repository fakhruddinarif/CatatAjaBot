using CatatAja.Domain.Entities;
using CatatAja.Domain.Interface;
using CatatAja.Provider.Data;
using Microsoft.EntityFrameworkCore;

namespace CatatAja.Application.Repositories;

public class WalletRepository : IWalletRepository
{
    private readonly AppDbContext _context;

    public WalletRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Wallet>> GetByUserIdAsync(Guid userId)
    {
        return await _context.Wallets
            .Where(w => w.UserId == userId)
            .OrderBy(w => w.CreatedAt)
            .ToListAsync();
    }

    public async Task<Wallet?> GetByIdAndUserIdAsync(Guid id, Guid userId)
    {
        return await _context.Wallets.FirstOrDefaultAsync(w => w.Id == id && w.UserId == userId);
    }

    public async Task<Wallet> CreateAsync(Wallet wallet)
    {
        _context.Wallets.Add(wallet);
        await _context.SaveChangesAsync();
        return wallet;
    }

    public async Task<Wallet> UpdateAsync(Wallet wallet)
    {
        _context.Wallets.Update(wallet);
        await _context.SaveChangesAsync();
        return wallet;
    }

    public async Task DeleteAsync(Wallet wallet)
    {
        _context.Wallets.Remove(wallet);
        await _context.SaveChangesAsync();
    }
}
