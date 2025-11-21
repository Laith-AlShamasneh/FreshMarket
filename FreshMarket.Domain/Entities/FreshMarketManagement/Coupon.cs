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
    public int UsedCount { get; set; } = 0;

    public ICollection<Order> Orders { get; set; } = [];
}
