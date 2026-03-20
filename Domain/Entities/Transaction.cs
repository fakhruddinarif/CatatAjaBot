using CatatAja.Domain.Enums;

namespace CatatAja.Domain.Entities;

public class Transaction
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid WalletId { get; set; }
    public Guid CategoryId { get; set; }
    public decimal Amount { get; set; }
    public TransactionType Type { get; set; }
    public string? Description { get; set; } = null;
    public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
    public string? ReceiptImageUrl { get; set; } = null;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeletedAt { get; set; } = null;

    // Navigation properties
    public Wallet? Wallet { get; set; } = null;
    public Category? Category { get; set; } = null;
}