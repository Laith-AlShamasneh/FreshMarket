using FreshMarket.Domain.Entities.SharedManagement;
using FreshMarket.Domain.Entities.UserManagement;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FreshMarket.Domain.Entities.FreshMarketManagement;

public class Review : Base
{
    public long ReviewId { get; set; }

    [ForeignKey(nameof(Product))]
    public long ProductId { get; set; }
    public Product Product { get; set; } = null!;

    [ForeignKey(nameof(User))]
    public long UserId { get; set; }
    public User User { get; set; } = null!;

    [Range(1, 5)]
    public int Rating { get; set; }

    [MaxLength(200)]
    public string? Title { get; set; }

    [MaxLength(2000)]
    public string? Comment { get; set; }

    public bool IsApproved { get; set; } = false;

    public long? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }

    public bool IsVerifiedPurchase { get; set; } = false;
}
