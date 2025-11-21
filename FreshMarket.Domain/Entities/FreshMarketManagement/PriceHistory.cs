using FreshMarket.Domain.Entities.LookupManagement;
using FreshMarket.Domain.Entities.SharedManagement;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FreshMarket.Domain.Entities.FreshMarketManagement;

public class PriceHistory : Base
{
    public long PriceHistoryId { get; set; }

    /// <summary>
    /// Link to the product (optional if tracked at variant level)
    /// </summary>
    public long? ProductId { get; set; }
    public Product? Product { get; set; }

    /// <summary>
    /// Link to the specific product variant (preferred for SKU-level pricing)
    /// </summary>
    public long? ProductVariantId { get; set; }
    public ProductVariant? ProductVariant { get; set; }

    /// <summary>
    /// Previous price snapshot
    /// </summary>
    [Column(TypeName = "decimal(6,2)")]
    public decimal OldPrice { get; set; } = 0m;

    /// <summary>
    /// New price snapshot
    /// </summary>
    [Column(TypeName = "decimal(6,2)")]
    public decimal NewPrice { get; set; } = 0m;

    /// <summary>
    /// Currency snapshot id (nullable to fall back to store default)
    /// </summary>
    [ForeignKey(nameof(Currency))]
    public int? CurrencyId { get; set; }
    public Currency? Currency { get; set; }

    /// <summary>
    /// Reason for the change (e.g., "Promotion", "Cost update", "Manual adjustment")
    /// </summary>
    [MaxLength(500)]
    public string? Reason { get; set; }

    /// <summary>
    /// Optional note or admin comment (longer freeform)
    /// </summary>
    [MaxLength(2000)]
    public string? Notes { get; set; }
}
