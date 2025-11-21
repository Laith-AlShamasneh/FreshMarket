using FreshMarket.Domain.Entities.FreshMarketManagement;

namespace FreshMarket.Domain.Interfaces.Repositories.FreshMarketManagement;

public interface ICart : IRepository<Cart>
{
    Task<Cart> GetOrCreateAsync(long? userId, Guid sessionId, CancellationToken ct = default);
}
