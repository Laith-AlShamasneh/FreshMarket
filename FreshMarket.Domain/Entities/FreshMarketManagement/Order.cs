using FreshMarket.Domain.Common;
using FreshMarket.Domain.Entities.SharedManagement;
using FreshMarket.Domain.Entities.UserManagement;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FreshMarket.Domain.Entities.FreshMarketManagement;

public class Order : Base
{
    public long OrderId { get; set; }

    [Required, MaxLength(100)]
    public string OrderNumber { get; set; } = null!;

    [ForeignKey(nameof(User))]
    public long? UserId { get; set; }
    public User? User { get; set; }

    // Addresses
    [ForeignKey(nameof(BillingAddress))]
    public long? BillingAddressId { get; set; }
    public Address? BillingAddress { get; set; }

    [ForeignKey(nameof(ShippingAddress))]
    public long? ShippingAddressId { get; set; }
    public Address? ShippingAddress { get; set; }

    // Totals
    [Column(TypeName = "decimal(6,2)")]
    public decimal SubTotal { get; set; } = 0m;

    [Column(TypeName = "decimal(6,2)")]
    public decimal DiscountTotal { get; set; } = 0m;

    [Column(TypeName = "decimal(6,2)")]
    public decimal ShippingTotal { get; set; } = 0m;

    [Column(TypeName = "decimal(6,2)")]
    public decimal TaxTotal { get; set; } = 0m;

    [Column(TypeName = "decimal(6,2)")]
    public decimal GrandTotal { get; set; } = 0m;

    // Coupon
    [ForeignKey(nameof(Coupon))]
    public long? CouponId { get; set; }
    public Coupon? Coupon { get; set; }

    [MaxLength(100)]
    public string? CouponCode { get; set; }

    // Shipping
    [MaxLength(200)]
    public string? ShippingCarrier { get; set; }

    [MaxLength(200)]
    public string? ShippingTrackingNumber { get; set; }

    // Timestamps
    public DateTime PlacedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }

    public OrderStatus OrderStatus { get; set; } = OrderStatus.Pending;
    public PaymentMethodType PaymentMethod { get; set; } = PaymentMethodType.CreditCard;
    public ShippingMethodType ShippingMethod { get; set; } = ShippingMethodType.Standard;

    // Navigation
    public ICollection<OrderItem> OrderItems { get; set; } = [];
}
