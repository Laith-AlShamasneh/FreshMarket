using FreshMarket.Domain.Entities.FreshMarketManagement;
using FreshMarket.Domain.Entities.SharedManagement;
using System.ComponentModel.DataAnnotations;

namespace FreshMarket.Domain.Entities.LookupManagement;

public class PaymentStatus : Base
{
    public int PaymentStatusId { get; set; }

    [Required, MaxLength(50)]
    public string Code { get; set; } = null!;

    [Required, MaxLength(200)]
    public string NameEn { get; set; } = null!;

    [Required, MaxLength(200)]
    public string NameAr { get; set; } = null!;

    [MaxLength(500)]
    public string? Description { get; set; }

    public int SortOrder { get; set; }

    public ICollection<PaymentTransaction> PaymentTransactions { get; set; } = [];
}
