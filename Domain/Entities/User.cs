using System.Collections;

namespace CatatAja.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string TelegramId { get; set; } = string.Empty;
    public string? Username { get; set; } = null;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeletedAt { get; set; } = null;

    // Navigation properties
    public ICollection<Wallet> Wallets { get; set; } = new List<Wallet>();
}