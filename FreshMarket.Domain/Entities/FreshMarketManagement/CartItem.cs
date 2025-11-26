using FreshMarket.Domain.Entities.LookupManagement;
using FreshMarket.Domain.Entities.SharedManagement;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FreshMarket.Domain.Entities.FreshMarketManagement;

public class CartItem : Base
{
    public long CartItemId { get; set; }

    [ForeignKey(nameof(Cart))]
    public long CartId { get; set; }
    public Cart Cart { get; set; } = null!;

    [ForeignKey(nameof(ProductVariant))]
    public long ProductVariantId { get; set; }
    public ProductVariant ProductVariant { get; set; } = null!;

    [Column(TypeName = "decimal(18,2)")]
    public decimal Quantity { get; set; } = 1m;
}