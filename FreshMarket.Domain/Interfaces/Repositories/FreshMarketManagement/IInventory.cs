using FreshMarket.Domain.Entities.FreshMarketManagement;

namespace FreshMarket.Domain.Interfaces.Repositories.FreshMarketManagement;

public interface IInventory : IRepository<Inventory>
{
    Task<Inventory?> GetByProductVariantIdAsync(long productVariantId, CancellationToken ct = default);
    Task<bool> IsInStockAsync(long productVariantId, int quantity, CancellationToken ct = default);
    Task<int> GetAvailableStockAsync(long productVariantId, CancellationToken ct = default);
    Task<IReadOnlyList<Inventory>> GetLowStockAsync(int threshold = 10, CancellationToken ct = default);
}
