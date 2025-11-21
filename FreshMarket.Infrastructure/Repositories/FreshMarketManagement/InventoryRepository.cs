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

    public async Task<Inventory?> GetByProductVariantIdAsync(long productVariantId, CancellationToken ct = default)
    {
        Guard.AgainstNegativeOrZero(productVariantId, nameof(productVariantId));

        return await ExecutionHelper.ExecuteAsync(
            () => _context.Inventories
                .FirstOrDefaultAsync(i => i.ProductVariantId == productVariantId, ct),
            logger,
            "Get Inventory by ProductVariantId",
            new { ProductVariantId = productVariantId }
        );
    }

    public async Task<int> GetAvailableStockAsync(long productVariantId, CancellationToken ct = default)
    {
        Guard.AgainstNegativeOrZero(productVariantId, nameof(productVariantId));

        return await ExecutionHelper.ExecuteAsync(
            () => _context.Inventories
                .Where(i => i.ProductVariantId == productVariantId)
                .Select(i => i.Quantity - i.ReservedQuantity)
                .FirstOrDefaultAsync(ct),
            logger,
            "Get Available Stock",
            new { ProductVariantId = productVariantId }
        );
    }

    public async Task<bool> IsInStockAsync(long productVariantId, int quantity, CancellationToken ct = default)
    {
        Guard.AgainstNegativeOrZero(productVariantId, nameof(productVariantId));
        Guard.AgainstNegative(quantity, nameof(quantity));

        return await GetAvailableStockAsync(productVariantId, ct) >= quantity;
    }

    public async Task<IReadOnlyList<Inventory>> GetLowStockAsync(int threshold = 10, CancellationToken ct = default)
    {
        Guard.AgainstNegative(threshold, nameof(threshold));

        return await ExecutionHelper.ExecuteAsync(
            () => _context.Inventories
                .AsNoTracking()
                .Where(i => i.Quantity - i.ReservedQuantity <= threshold)
                .ToListAsync(ct),
            logger,
            "Get Low Stock",
            new { Threshold = threshold }
        );
    }

    public async Task<IReadOnlyList<Inventory>> GetLowStockWithDetailsAsync(int threshold = 10, CancellationToken ct = default)
    {
        Guard.AgainstNegative(threshold, nameof(threshold));

        return await ExecutionHelper.ExecuteAsync(
            () => _context.Inventories
                .AsNoTracking()
                .Where(i => i.Quantity - i.ReservedQuantity <= threshold)
                .Include(i => i.ProductVariant)
                .ThenInclude(pv => pv.Product)
                .ThenInclude(p => p.ProductMedia)
                .OrderBy(i => i.Quantity - i.ReservedQuantity)
                .ToListAsync(ct),
            logger,
            "Get Low Stock with Details",
            new { Threshold = threshold }
        );
    }

    public async Task UpdateStockAsync(long productVariantId, int quantityChange, CancellationToken ct = default)
    {
        Guard.AgainstNegativeOrZero(productVariantId, nameof(productVariantId));

        await ExecutionHelper.ExecuteAsync(
            async () =>
            {
                var inventory = await _context.Inventories
                    .FirstOrDefaultAsync(i => i.ProductVariantId == productVariantId, ct);

                if (inventory != null)
                {
                    inventory.Quantity = Math.Max(0, inventory.Quantity + quantityChange);
                    await _context.SaveChangesAsync(ct);
                }
            },
            logger,
            "Update Inventory Stock",
            new { ProductVariantId = productVariantId, QuantityChange = quantityChange }
        );
    }

    public async Task ReserveStockAsync(long productVariantId, int quantity, CancellationToken ct = default)
    {
        Guard.AgainstNegativeOrZero(productVariantId, nameof(productVariantId));
        Guard.AgainstNegative(quantity, nameof(quantity));

        await ExecutionHelper.ExecuteAsync(
            async () =>
            {
                var inventory = await _context.Inventories
                    .FirstOrDefaultAsync(i => i.ProductVariantId == productVariantId, ct);

                if (inventory != null)
                {
                    var available = inventory.Quantity - inventory.ReservedQuantity;
                    if (available < quantity)
                        throw new InvalidOperationException($"Not enough stock to reserve. Available: {available}");

                    inventory.ReservedQuantity += quantity;
                    await _context.SaveChangesAsync(ct);
                }
            },
            logger,
            "Reserve Stock",
            new { ProductVariantId = productVariantId, Quantity = quantity }
        );
    }

    public async Task ReleaseStockAsync(long productVariantId, int quantity, CancellationToken ct = default)
    {
        Guard.AgainstNegativeOrZero(productVariantId, nameof(productVariantId));
        Guard.AgainstNegative(quantity, nameof(quantity));

        await ExecutionHelper.ExecuteAsync(
            async () =>
            {
                var inventory = await _context.Inventories
                    .FirstOrDefaultAsync(i => i.ProductVariantId == productVariantId, ct);

                if (inventory != null)
                {
                    inventory.ReservedQuantity = Math.Max(0, inventory.ReservedQuantity - quantity);
                    await _context.SaveChangesAsync(ct);
                }
            },
            logger,
            "Release Reserved Stock",
            new { ProductVariantId = productVariantId, Quantity = quantity }
        );
    }

    public async Task CommitStockAsync(long productVariantId, int quantity, CancellationToken ct = default)
    {
        Guard.AgainstNegativeOrZero(productVariantId, nameof(productVariantId));
        Guard.AgainstNegative(quantity, nameof(quantity));

        await ExecutionHelper.ExecuteAsync(
            async () =>
            {
                var inventory = await _context.Inventories
                    .FirstOrDefaultAsync(i => i.ProductVariantId == productVariantId, ct);

                if (inventory != null && inventory.ReservedQuantity >= quantity)
                {
                    inventory.ReservedQuantity -= quantity;
                    inventory.Quantity -= quantity;
                    if (inventory.Quantity < 0) inventory.Quantity = 0;
                    await _context.SaveChangesAsync(ct);
                }
            },
            logger,
            "Commit Reserved Stock",
            new { ProductVariantId = productVariantId, Quantity = quantity }
        );
    }
}