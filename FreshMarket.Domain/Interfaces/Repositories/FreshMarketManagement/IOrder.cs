using FreshMarket.Domain.Entities.FreshMarketManagement;
using FreshMarket.Shared.Common;

namespace FreshMarket.Domain.Interfaces.Repositories.FreshMarketManagement;

public interface IOrder : IRepository<Order>
{
    Task<IReadOnlyList<Order>> GetByUserIdAsync(long? userId, CancellationToken ct = default);
    Task<IReadOnlyList<Order>> GetBySessionIdAsync(Guid sessionId, CancellationToken ct = default);
    Task<Order?> GetWithDetailsAsync(long orderId, CancellationToken ct = default);
    Task<Order?> GetWithDetailsBySessionIdAsync(Guid sessionId, CancellationToken ct = default);
    Task<IReadOnlyList<Order>> GetPendingPaymentAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Order>> GetByStatusAsync(int orderStatusId, CancellationToken ct = default);
    Task<IPaginatedList<Order>> GetPaginatedByUserIdAsync(long? userId, PaginationRequest request, CancellationToken ct = default);
    Task<IPaginatedList<Order>> GetPaginatedBySessionIdAsync(Guid sessionId, PaginationRequest request, CancellationToken ct = default);
}
