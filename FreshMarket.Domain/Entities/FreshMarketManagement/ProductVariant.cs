using FreshMarket.Domain.Entities.LookupManagement;
using FreshMarket.Domain.Entities.SharedManagement;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FreshMarket.Domain.Entities.FreshMarketManagement;

public class ProductVariant : Base
{
    public long ProductVariantId { get; set; }

    [ForeignKey(nameof(Product))]
    public long ProductId { get; set; }
    public Product Product { get; set; } = null!;

    [Required, MaxLength(100)]
    public string Sku { get; set; } = null!;  // Variant-specific SKU

    [MaxLength(200)]
    public string? NameEn { get; set; }  // e.g., "Small", "Red", "500g"

    [MaxLength(200)]
    public string? NameAr { get; set; }

    [Column(TypeName = "decimal(6,2)")]
    public decimal Price { get; set; }

    [Column(TypeName = "decimal(6,2)")]
    public decimal? Cost { get; set; }

    [ForeignKey(nameof(UnitOfMeasure))]
    public int? UnitOfMeasureId { get; set; }
    public UnitOfMeasure? UnitOfMeasure { get; set; }

    [Column(TypeName = "decimal(6,3)")]
    public decimal? Weight { get; set; }

    [MaxLength(1000)]
    public string? ImageUrl { get; set; }

    public bool IsDefault { get; set; } = false;

    // Navigation
    public Inventory Inventory { get; set; } = new();
    public ICollection<CartItem> CartItems { get; set; } = [];
    public ICollection<OrderItem> OrderItems { get; set; } = [];
}