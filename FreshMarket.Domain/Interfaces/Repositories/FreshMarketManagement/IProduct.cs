using FreshMarket.Domain.Entities.FreshMarketManagement;
using FreshMarket.Shared.Common;

namespace FreshMarket.Domain.Interfaces.Repositories.FreshMarketManagement;

public interface IProduct : IRepository<Product>
{
    Task<IReadOnlyList<Product>> GetActiveAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Product>> GetByCategoryIdAsync(long categoryId, CancellationToken ct = default);
    Task<Product?> GetWithDetailsAsync(long productId, CancellationToken ct = default);
    Task<Product?> GetBySlugAsync(string slug, CancellationToken ct = default);
    Task<IReadOnlyList<Product>> SearchAsync(string query, CancellationToken ct = default);
}
