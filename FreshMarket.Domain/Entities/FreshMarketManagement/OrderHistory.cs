using FreshMarket.Domain.Common;
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

    public OrderStatus OldStatus { get; set; } = OrderStatus.Pending;
    public OrderStatus NewStatus { get; set; } = OrderStatus.Pending;

    [MaxLength(500)]
    public string? Notes { get; set; }
}
