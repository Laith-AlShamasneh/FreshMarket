using FreshMarket.Domain.Entities.FreshMarketManagement;

namespace FreshMarket.Domain.Interfaces.Repositories.FreshMarketManagement;

public interface IProductVariant : IRepository<ProductVariant>
{
    Task<IReadOnlyList<ProductVariant>> GetByProductIdAsync(long productId, CancellationToken ct = default);
    Task<ProductVariant?> GetDefaultByProductIdAsync(long productId, CancellationToken ct = default);
    Task<ProductVariant?> GetBySkuAsync(string sku, CancellationToken ct = default);
    Task<IReadOnlyList<ProductVariant>> GetInStockByProductIdAsync(long productId, CancellationToken ct = default);
}
