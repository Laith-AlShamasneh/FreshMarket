using FreshMarket.Domain.Entities.FreshMarketManagement;

namespace FreshMarket.Domain.Interfaces.Repositories.FreshMarketManagement;

public interface IInventory : IRepository<Inventory>
{
    Task<bool> IsInStockAsync(long productVariantId, int quantity, CancellationToken ct = default);
    Task<int> GetAvailableStockAsync(long productVariantId, CancellationToken ct = default);
    Task ReserveStockAsync(long productVariantId, int quantity, CancellationToken ct = default);
    Task ReleaseStockAsync(long productVariantId, int quantity, CancellationToken ct = default);
    Task CommitStockAsync(long productVariantId, int quantity, CancellationToken ct = default);
}
