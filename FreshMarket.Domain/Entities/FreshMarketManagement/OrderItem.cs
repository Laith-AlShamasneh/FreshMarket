using FreshMarket.Domain.Entities.LookupManagement;
using FreshMarket.Domain.Entities.SharedManagement;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FreshMarket.Domain.Entities.FreshMarketManagement;

public class OrderItem : Base
{
    public long OrderItemId { get; set; }

    [ForeignKey(nameof(Order))]
    public long OrderId { get; set; }
    public Order Order { get; set; } = null!;

    [ForeignKey(nameof(ProductVariant))]
    public long ProductVariantId { get; set; }
    public ProductVariant ProductVariant { get; set; } = null!;

    public decimal Quantity { get; set; } = 1m;

    [Column(TypeName = "decimal(6,2)")]
    public decimal UnitPrice { get; set; } = 0m;

    [Column(TypeName = "decimal(6,2)")]
    public decimal DiscountAmount { get; set; } = 0m;

    [Column(TypeName = "decimal(6,2)")]
    public decimal TaxAmount { get; set; } = 0m;

    [Column(TypeName = "decimal(6,2)")]
    public decimal LineTotal { get; set; } = 0m;

    // Snapshot fields
    [MaxLength(200)]
    public string Sku { get; set; } = null!;

    [MaxLength(500)]
    public string NameEn { get; set; } = null!;

    [MaxLength(500)]
    public string NameAr { get; set; } = null!;

    [MaxLength(1000)]
    public string? ImageUrl { get; set; }

    // GUEST TRACKING
    public Guid SessionId { get; set; }
}
