using FreshMarket.Domain.Entities.FreshMarketManagement;

namespace FreshMarket.Domain.Interfaces.Repositories.FreshMarketManagement;

public interface IOrderItem : IRepository<OrderItem>
{
    Task<IReadOnlyList<OrderItem>> GetByOrderIdAsync(long orderId, CancellationToken ct = default);
    Task<IReadOnlyList<OrderItem>> GetBySessionIdAsync(Guid sessionId, CancellationToken ct = default);
    Task<IReadOnlyList<OrderItem>> GetWithDetailsByOrderIdAsync(long orderId, CancellationToken ct = default);
    Task<OrderItem?> GetByOrderIdAndProductVariantIdAsync(long orderId, long productVariantId, CancellationToken ct = default);
    Task<OrderItem?> GetBySessionIdAndProductVariantIdAsync(Guid sessionId, long productVariantId, CancellationToken ct = default);
}
