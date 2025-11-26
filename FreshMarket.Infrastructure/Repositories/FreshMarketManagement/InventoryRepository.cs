using FreshMarket.Domain.Entities.FreshMarketManagement;
using FreshMarket.Domain.Interfaces.Repositories.FreshMarketManagement;
using FreshMarket.Infrastructure.Data;
using FreshMarket.Shared.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FreshMarket.Infrastructure.Repositories.FreshMarketManagement;

public class InventoryRepository(
    FreshMarketDbContext context,
    ILogger<InventoryRepository> logger)
    : Repository<Inventory>(context, logger), IInventory
{
    private readonly FreshMarketDbContext _context = context;

    public async Task<bool> IsInStockAsync(long productVariantId, int quantity, CancellationToken ct = default)
    {
        Guard.AgainstNegativeOrZero(productVariantId, nameof(productVariantId));
        Guard.AgainstNegativeOrZero(quantity, nameof(quantity));

        var inventory = await _context.Inventories
            .FirstOrDefaultAsync(i => i.ProductVariantId == productVariantId, ct);

        if (inventory == null) return false;

        // Use the Domain property
        return inventory.AvailableStock >= quantity;
    }

    public async Task<int> GetAvailableStockAsync(long productVariantId, CancellationToken ct = default)
    {
        Guard.AgainstNegativeOrZero(productVariantId, nameof(productVariantId));

        var inventory = await _context.Inventories
            .FirstOrDefaultAsync(i => i.ProductVariantId == productVariantId, ct);

        // Use the Domain property
        return inventory?.AvailableStock ?? 0;
    }

    public async Task ReserveStockAsync(long productVariantId, int quantity, CancellationToken ct = default)
    {
        Guard.AgainstNegativeOrZero(productVariantId, nameof(productVariantId));

        var inventory = await _context.Inventories
            .FirstOrDefaultAsync(i => i.ProductVariantId == productVariantId, ct)
            ?? throw new InvalidOperationException($"Inventory not found for variant {productVariantId}");

        inventory.ReserveStock(quantity);

        _context.Inventories.Update(inventory);
    }

    public async Task ReleaseStockAsync(long productVariantId, int quantity, CancellationToken ct = default)
    {
        Guard.AgainstNegativeOrZero(productVariantId, nameof(productVariantId));

        var inventory = await _context.Inventories
            .FirstOrDefaultAsync(i => i.ProductVariantId == productVariantId, ct)
            ?? throw new InvalidOperationException($"Inventory not found for variant {productVariantId}");

        inventory.ReleaseStock(quantity);

        _context.Inventories.Update(inventory);
    }

    public async Task CommitStockAsync(long productVariantId, int quantity, CancellationToken ct = default)
    {
        Guard.AgainstNegativeOrZero(productVariantId, nameof(productVariantId));

        var inventory = await _context.Inventories
            .FirstOrDefaultAsync(i => i.ProductVariantId == productVariantId, ct)
            ?? throw new InvalidOperationException($"Inventory not found for variant {productVariantId}");

        inventory.CommitStock(quantity);

        _context.Inventories.Update(inventory);
    }
}