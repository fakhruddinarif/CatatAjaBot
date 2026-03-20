namespace CatatAja.Domain.Entities;

public class RecurringBill
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public Guid WalletId { get; set; }
    public Guid CategoryId { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public int DayOfMonth { get; set; } // 1-31, if the month has less days, it will be on the last day of the month
    public bool IsActive { get; set; } = true;
    public DateTime? NextBillDate { get; set; } = null; // the date when the next bill will be created
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeletedAt { get; set; } = null;

    // Navigation properties
    public User? User { get; set; } = null;
    public Wallet? Wallet { get; set; } = null;
    public Category? Category { get; set; } = null;
}