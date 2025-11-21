using FreshMarket.Domain.Entities.LookupManagement;
using FreshMarket.Domain.Entities.SharedManagement;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FreshMarket.Domain.Entities.FreshMarketManagement;

public class PaymentTransaction : Base
{
    public long PaymentTransactionId { get; set; }

    [ForeignKey(nameof(Order))]
    public long OrderId { get; set; }
    public Order Order { get; set; } = null!;

    [ForeignKey(nameof(PaymentMethodType))]
    public int PaymentMethodTypeId { get; set; }
    public PaymentMethodType PaymentMethodType { get; set; } = null!;

    [ForeignKey(nameof(PaymentStatus))]
    public int PaymentStatusId { get; set; }
    public PaymentStatus PaymentStatus { get; set; } = null!;


    [Column(TypeName = "decimal(6,2)")]
    public decimal Amount { get; set; }

    [MaxLength(200)]
    public string? TransactionId { get; set; }  // Gateway transaction ID

    [MaxLength(200)]
    public string? Provider { get; set; }  // "Stripe", "PayPal", etc.

    public bool IsSuccessful { get; set; } = false;

    public DateTime? CompletedAt { get; set; }

    [MaxLength(1000)]
    public string? FailureReason { get; set; }

    [MaxLength(20)]
    public string? MaskedCardNumber { get; set; }  // Last 4 digits
}