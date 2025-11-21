using FreshMarket.Domain.Entities.FreshMarketManagement;
using FreshMarket.Shared.Common;

namespace FreshMarket.Domain.Interfaces.Repositories.FreshMarketManagement;

public interface IOrderHistory : IRepository<OrderHistory>
{
    Task<IReadOnlyList<OrderHistory>> GetByOrderIdAsync(long orderId, CancellationToken ct = default);
    Task<IReadOnlyList<OrderHistory>> GetBySessionIdAsync(Guid sessionId, CancellationToken ct = default);
    Task<OrderHistory?> GetLatestByOrderIdAsync(long orderId, CancellationToken ct = default);
    Task<OrderHistory?> GetLatestBySessionIdAsync(Guid sessionId, CancellationToken ct = default);
    Task<OrderHistory> AddStatusChangeAsync(long orderId, OrderStatus oldStatusId, OrderStatus newStatusId, string? notes = null, CancellationToken ct = default);
}
