using FreshMarket.Domain.Entities.FreshMarketManagement;
using FreshMarket.Shared.Common;

namespace FreshMarket.Domain.Interfaces.Repositories.FreshMarketManagement;

public interface IOrderHistory : IRepository<OrderHistory>
{
    Task<IReadOnlyList<OrderHistory>> GetByOrderIdAsync(long orderId, CancellationToken ct = default);
}
