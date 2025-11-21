using FreshMarket.Domain.Entities.SharedManagement;
using FreshMarket.Domain.Entities.UserManagement;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FreshMarket.Domain.Entities.FreshMarketManagement;

public class Cart : Base
{
    public long CartId { get; set; }

    [ForeignKey(nameof(User))]
    public long? UserId { get; set; }
    public User? User { get; set; }

    public Guid SessionId { get; set; } = Guid.NewGuid(); // For anonymous

    [MaxLength(50)]
    public string CartNumber { get; set; } = null!; // e.g., CART-2025-0001

    public DateTime? ExpiresAt { get; set; } // 24h for anonymous, longer for logged-in

    public ICollection<CartItem> Items { get; set; } = [];
}
