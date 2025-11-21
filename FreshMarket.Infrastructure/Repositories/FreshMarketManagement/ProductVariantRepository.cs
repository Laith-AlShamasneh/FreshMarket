using FreshMarket.Domain.Entities.FreshMarketManagement;
using FreshMarket.Domain.Interfaces.Repositories.FreshMarketManagement;
using FreshMarket.Infrastructure.Data;
using FreshMarket.Shared.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FreshMarket.Infrastructure.Repositories.FreshMarketManagement;

public class ProductVariantRepository(
    FreshMarketDbContext context,
    ILogger<ProductVariantRepository> logger)
    : Repository<ProductVariant>(context, logger), IProductVariant
{
    private readonly FreshMarketDbContext _context = context;

    public async Task<IReadOnlyList<ProductVariant>> GetByProductIdAsync(long productId, CancellationToken ct = default)
    {
        Guard.AgainstNegativeOrZero(productId, nameof(productId));

        return await ExecutionHelper.ExecuteAsync(
            () => _context.ProductVariants
                .AsNoTracking()
                .Where(pv => pv.ProductId == productId && pv.IsActive)
                .OrderBy(pv => pv.IsDefault ? 0 : 1)
                .ThenBy(pv => pv.Sku)
                .ToListAsync(ct),
            logger,
            "Get Variants by ProductId",
            new { ProductId = productId }
        );
    }

    public async Task<IReadOnlyList<ProductVariant>> GetByProductIdsAsync(long[] productIds, CancellationToken ct = default)
    {
        if (productIds == null || productIds.Length == 0)
        {
            return [];
        }

        return await ExecutionHelper.ExecuteAsync(
            () => _context.ProductVariants
                .AsNoTracking()
                .Where(pv => productIds.Contains(pv.ProductId) && pv.IsActive)
                .OrderBy(pv => pv.ProductId)
                .ThenBy(pv => pv.IsDefault ? 0 : 1)
                .ThenBy(pv => pv.Sku)
                .ToListAsync(ct),
            logger,
            "Get Variants by ProductIds",
            new { ProductIds = productIds }
        );
    }

    public async Task<ProductVariant?> GetDefaultByProductIdAsync(long productId, CancellationToken ct = default)
    {
        Guard.AgainstNegativeOrZero(productId, nameof(productId));

        return await ExecutionHelper.ExecuteAsync(
            () => _context.ProductVariants
                .AsNoTracking()
                .Include(pv => pv.Inventory)
                .Include(pv => pv.UnitOfMeasure)
                .FirstOrDefaultAsync(pv => pv.ProductId == productId && pv.IsDefault && pv.IsActive, ct),
            logger,
            "Get Default Variant by ProductId",
            new { ProductId = productId }
        );
    }

    public async Task<IReadOnlyList<ProductVariant>> GetBySessionIdAsync(Guid sessionId, CancellationToken ct = default)
    {
        Guard.AgainstEmpty(sessionId, nameof(sessionId));

        var variantIdsFromCart = _context.CartItems
            .Where(ci => ci.Cart.SessionId == sessionId)
            .Select(ci => ci.ProductVariantId);

        var variantIdsFromOrder = _context.OrderItems
            .Where(oi => oi.SessionId == sessionId)
            .Select(oi => oi.ProductVariantId);

        var variantIds = variantIdsFromCart.Union(variantIdsFromOrder);

        return await ExecutionHelper.ExecuteAsync(
            () => _context.ProductVariants
                .AsNoTracking()
                .Include(pv => pv.Inventory)
                .Include(pv => pv.UnitOfMeasure)
                .Where(pv => pv.IsActive && variantIds.Contains(pv.ProductVariantId))
                .OrderBy(pv => pv.Sku)
                .ToListAsync(ct),
            logger,
            "Get Variants by SessionId (Cart + Order)",
            new { SessionId = sessionId }
        );
    }

    public async Task<ProductVariant?> GetBySkuAsync(string sku, CancellationToken ct = default)
    {
        Guard.AgainstNullOrWhiteSpace(sku, nameof(sku));

        return await ExecutionHelper.ExecuteAsync(
            () => _context.ProductVariants
                .AsNoTracking()
                .Include(pv => pv.Inventory)
                .Include(pv => pv.UnitOfMeasure)
                .Include(pv => pv.Product)
                .FirstOrDefaultAsync(pv => pv.Sku == sku && pv.IsActive, ct),
            logger,
            "Get Variant by SKU",
            new { Sku = sku }
        );
    }

    public async Task<IReadOnlyList<ProductVariant>> GetBySkusAsync(string[] skus, CancellationToken ct = default)
    {
        Guard.AgainstNullOrEmpty(skus, nameof(skus));

        return await ExecutionHelper.ExecuteAsync(
            () => _context.ProductVariants
                .AsNoTracking()
                .Include(pv => pv.Inventory)
                .Where(pv => skus.Contains(pv.Sku) && pv.IsActive)
                .ToListAsync(ct),
            logger,
            "Get Variants by SKUs",
            new { Skus = skus }
        );
    }

    public async Task<IReadOnlyList<ProductVariant>> GetInStockByProductIdAsync(long productId, CancellationToken ct = default)
    {
        Guard.AgainstNegativeOrZero(productId, nameof(productId));

        return await ExecutionHelper.ExecuteAsync(
            () => _context.ProductVariants
                .AsNoTracking()
                .Include(pv => pv.Inventory)
                .Where(pv => pv.ProductId == productId && pv.IsActive && pv.Inventory.Quantity > pv.Inventory.ReservedQuantity)
                .OrderBy(pv => pv.IsDefault ? 0 : 1)
                .ThenBy(pv => pv.Sku)
                .ToListAsync(ct),
            logger,
            "Get In-Stock Variants by ProductId",
            new { ProductId = productId }
        );
    }

    public async Task<IReadOnlyList<ProductVariant>> GetInStockWithDetailsAsync(long productId, CancellationToken ct = default)
    {
        Guard.AgainstNegativeOrZero(productId, nameof(productId));

        return await ExecutionHelper.ExecuteAsync(
            () => _context.ProductVariants
                .AsNoTracking()
                .Include(pv => pv.Inventory)
                .Include(pv => pv.UnitOfMeasure)
                .Include(pv => pv.Product)
                .Where(pv => pv.ProductId == productId && pv.IsActive && pv.Inventory.Quantity > pv.Inventory.ReservedQuantity)
                .OrderBy(pv => pv.IsDefault ? 0 : 1)
                .ThenBy(pv => pv.Sku)
                .ToListAsync(ct),
            logger,
            "Get In-Stock Variants with Full Details",
            new { ProductId = productId }
        );
    }
}