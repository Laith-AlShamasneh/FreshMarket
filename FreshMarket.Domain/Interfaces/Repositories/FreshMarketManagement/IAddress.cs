using FreshMarket.Domain.Entities.FreshMarketManagement;

namespace FreshMarket.Domain.Interfaces.Repositories.FreshMarketManagement;

public interface IAddress : IRepository<Address>
{
    Task<IReadOnlyList<Address>> GetByUserIdAsync(long? userId, CancellationToken ct = default);
    Task<Address?> GetDefaultBillingAsync(long? userId, CancellationToken ct = default);
    Task<Address?> GetDefaultShippingAsync(long? userId, CancellationToken ct = default);
    Task<Address?> GetByIdWithDetailsAsync(long addressId, CancellationToken ct = default);
}
