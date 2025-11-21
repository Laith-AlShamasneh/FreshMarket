using FreshMarket.Domain.Entities.SharedManagement;
using FreshMarket.Domain.Entities.UserManagement;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FreshMarket.Domain.Entities.FreshMarketManagement;

public class WishlistItem : Base
{
    public long WishlistItemId { get; set; }

    [ForeignKey(nameof(Wishlist))]
    public long WishlistId { get; set; }
    public Wishlist Wishlist { get; set; } = null!;

    [ForeignKey(nameof(ProductVariant))]
    public long ProductVariantId { get; set; }
    public ProductVariant ProductVariant { get; set; } = null!;

}