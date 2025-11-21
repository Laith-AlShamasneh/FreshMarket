using FreshMarket.Domain.Entities.FreshMarketManagement;

namespace FreshMarket.Domain.Interfaces.Repositories.FreshMarketManagement;

public interface IProductMedia : IRepository<ProductMedia>
{
    Task<IReadOnlyList<ProductMedia>> GetByProductIdAsync(long productId, CancellationToken ct = default);
}
