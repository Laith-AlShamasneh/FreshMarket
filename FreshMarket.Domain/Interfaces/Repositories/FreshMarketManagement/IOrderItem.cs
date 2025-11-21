using FreshMarket.Domain.Entities.FreshMarketManagement;

namespace FreshMarket.Domain.Interfaces.Repositories.FreshMarketManagement;

public interface IOrderItem : IRepository<OrderItem>
{
    Task<IReadOnlyList<OrderItem>> GetByOrderIdAsync(long orderId, CancellationToken ct = default);
}
