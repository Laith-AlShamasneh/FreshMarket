using FreshMarket.Domain.Entities.FreshMarketManagement;

namespace FreshMarket.Domain.Interfaces.Repositories.FreshMarketManagement;

public interface ICart : IRepository<Cart>
{
    Task<Cart?> GetActiveBySessionIdAsync(Guid sessionId, CancellationToken ct = default);
    Task<Cart?> GetActiveByUserIdAsync(long? userId, CancellationToken ct = default);
    Task<Cart?> GetWithItemsBySessionIdAsync(Guid sessionId, CancellationToken ct = default);
    Task<Cart?> GetWithItemsByUserIdAsync(long? userId, CancellationToken ct = default);
    Task<Cart> GetOrCreateAsync(long? userId, Guid sessionId, CancellationToken ct = default);
}
