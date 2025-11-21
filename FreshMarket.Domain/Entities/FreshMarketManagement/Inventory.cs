using FreshMarket.Domain.Entities.SharedManagement;
using System.ComponentModel.DataAnnotations.Schema;

namespace FreshMarket.Domain.Entities.FreshMarketManagement;

public class Inventory : Base
{
    public long InventoryId { get; set; }

    [ForeignKey(nameof(ProductVariant))]
    public long ProductVariantId { get; set; }
    public ProductVariant ProductVariant { get; set; } = null!;

    public int Quantity { get; set; }
    public int ReservedQuantity { get; set; }  // For pending orders
    public int? ReorderLevel { get; set; }     // When to reorder
}
