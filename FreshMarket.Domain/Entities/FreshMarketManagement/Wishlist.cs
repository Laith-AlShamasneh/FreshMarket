using FreshMarket.Domain.Entities.SharedManagement;
using FreshMarket.Domain.Entities.UserManagement;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FreshMarket.Domain.Entities.FreshMarketManagement;

public class Wishlist : Base
{
    public long WishlistId { get; set; }

    [ForeignKey(nameof(User))]
    public long UserId { get; set; }  // Make required
    public User User { get; set; } = null!;

    [Required, MaxLength(200)]
    public string Name { get; set; } = "My Wishlist";

    public bool IsDefault { get; set; } = false;

    [MaxLength(500)]
    public string? Notes { get; set; }

    public ICollection<WishlistItem> Items { get; set; } = [];
}
