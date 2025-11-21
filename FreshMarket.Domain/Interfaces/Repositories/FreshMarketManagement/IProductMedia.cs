using FreshMarket.Domain.Entities.FreshMarketManagement;

namespace FreshMarket.Domain.Interfaces.Repositories.FreshMarketManagement;

public interface IProductMedia : IRepository<ProductMedia>
{
    Task<IReadOnlyList<ProductMedia>> GetByProductIdAsync(long productId, CancellationToken ct = default);
    Task<IReadOnlyList<ProductMedia>> GetByProductIdsAsync(long[] productIds, CancellationToken ct = default);
    Task<ProductMedia?> GetDefaultByProductIdAsync(long productId, CancellationToken ct = default);
    Task<IReadOnlyList<ProductMedia>> GetDefaultByProductIdsAsync(long[] productIds, CancellationToken ct = default);
}
