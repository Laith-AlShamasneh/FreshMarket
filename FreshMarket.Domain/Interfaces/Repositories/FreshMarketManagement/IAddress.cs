using FreshMarket.Domain.Entities.FreshMarketManagement;

namespace FreshMarket.Domain.Interfaces.Repositories.FreshMarketManagement;

public interface IAddress : IRepository<Address>
{
    Task<IReadOnlyList<Address>> GetByUserIdAsync(long? userId, CancellationToken ct = default);
    Task<Address?> GetDefaultAsync(long? userId, CancellationToken ct = default);
}
