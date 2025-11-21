using FreshMarket.Domain.Entities.LookupManagement;
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
    public long? UserId { get; set; }  // Nullable for guest orders
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

    // Status
    [ForeignKey(nameof(OrderStatus))]
    public int OrderStatusId { get; set; }
    public OrderStatus OrderStatus { get; set; } = null!;

    [ForeignKey(nameof(PaymentStatus))]
    public int PaymentStatusId { get; set; }
    public PaymentStatus PaymentStatus { get; set; } = null!;

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

    // Navigation
    public ICollection<OrderItem> OrderItems { get; set; } = [];
    public ICollection<PaymentTransaction> PaymentTransactions { get; set; } = [];
}
