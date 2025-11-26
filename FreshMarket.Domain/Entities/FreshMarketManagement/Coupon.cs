using FreshMarket.Domain.Entities.LookupManagement;
using FreshMarket.Domain.Entities.SharedManagement;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FreshMarket.Domain.Entities.FreshMarketManagement;

public class Coupon : Base
{
    public long CouponId { get; set; }

    [Required, MaxLength(100)]
    public string Code { get; set; } = null!;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required, MaxLength(50)]
    public string DiscountType { get; set; } = "Fixed";  // "Fixed" or "Percentage"

    [Column(TypeName = "decimal(6,2)")]
    public decimal DiscountValue { get; set; }

    [Column(TypeName = "decimal(6,2)")]
    public decimal? MinimumOrderAmount { get; set; }

    public DateTime? StartsAt { get; set; }
    public DateTime? EndsAt { get; set; }

    public int? UsageLimit { get; set; }  // Total uses allowed
    public int UsedCount { get; private set; } = 0; // Private setter

    public ICollection<Order> Orders { get; set; } = [];

    // Domain Logic: Validate if active
    public bool IsValid()
    {
        var now = DateTime.UtcNow;
        if (!IsActive) return false;
        if (StartsAt.HasValue && StartsAt > now) return false;
        if (EndsAt.HasValue && EndsAt < now) return false;
        if (UsageLimit.HasValue && UsedCount >= UsageLimit) return false;

        return true;
    }

    // Domain Logic: Record usage
    public void RecordUsage()
    {
        if (UsageLimit.HasValue && UsedCount >= UsageLimit)
            throw new InvalidOperationException("Coupon usage limit reached.");

        UsedCount++;
    }
}
