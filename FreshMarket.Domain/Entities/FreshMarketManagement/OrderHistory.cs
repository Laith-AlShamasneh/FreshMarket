using FreshMarket.Domain.Entities.LookupManagement;
using FreshMarket.Domain.Entities.SharedManagement;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FreshMarket.Domain.Entities.FreshMarketManagement;

public class OrderHistory : Base
{
    public long OrderHistoryId { get; set; }

    [ForeignKey(nameof(Order))]
    public long OrderId { get; set; }
    public Order Order { get; set; } = null!;

    [ForeignKey(nameof(OrderStatus))]
    public int? OldStatusId { get; set; }
    public OrderStatus? OldStatus { get; set; }

    [ForeignKey(nameof(OrderStatus))]
    public int NewStatusId { get; set; }
    public OrderStatus NewStatus { get; set; } = null!;

    [MaxLength(500)]
    public string? Notes { get; set; }
}
