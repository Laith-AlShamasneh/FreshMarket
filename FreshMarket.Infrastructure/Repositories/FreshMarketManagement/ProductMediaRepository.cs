using FreshMarket.Domain.Entities.FreshMarketManagement;
using FreshMarket.Domain.Interfaces.Repositories.FreshMarketManagement;
using FreshMarket.Infrastructure.Data;
using FreshMarket.Shared.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FreshMarket.Infrastructure.Repositories.FreshMarketManagement;

public class ProductMediaRepository(
    FreshMarketDbContext context,
    ILogger<ProductMediaRepository> logger)
    : Repository<ProductMedia>(context, logger), IProductMedia
{
    private readonly FreshMarketDbContext _context = context;

    public async Task<IReadOnlyList<ProductMedia>> GetByProductIdAsync(long productId, CancellationToken ct = default)
    {
        Guard.AgainstNegativeOrZero(productId, nameof(productId));

        return await ExecutionHelper.ExecuteAsync(
            () => _context.ProductMedias
                .AsNoTracking()
                .Where(pm => pm.ProductId == productId)
                .OrderBy(pm => pm.IsDefault ? 0 : 1)
                .ThenBy(pm => pm.SortOrder)
                .ToListAsync(ct),
            logger,
            "Get Media by ProductId",
            new { ProductId = productId }
        );
    }

    public async Task<IReadOnlyList<ProductMedia>> GetByProductIdsAsync(long[] productIds, CancellationToken ct = default)
    {
        if (productIds == null || productIds.Length == 0)
        {
            return [];
        }

        return await ExecutionHelper.ExecuteAsync(
            () => _context.ProductMedias
                .AsNoTracking()
                .Where(pm => productIds.Contains(pm.ProductId))
                .OrderBy(pm => pm.ProductId)
                .ThenBy(pm => pm.IsDefault ? 0 : 1)
                .ThenBy(pm => pm.SortOrder)
                .ToListAsync(ct),
            logger,
            "Get Media by ProductIds",
            new { ProductIds = productIds }
        );
    }

    public async Task<ProductMedia?> GetDefaultByProductIdAsync(long productId, CancellationToken ct = default)
    {
        Guard.AgainstNegativeOrZero(productId, nameof(productId));

        return await ExecutionHelper.ExecuteAsync(
            () => _context.ProductMedias
                .AsNoTracking()
                .FirstOrDefaultAsync(pm => pm.ProductId == productId && pm.IsDefault, ct),
            logger,
            "Get Default Media by ProductId",
            new { ProductId = productId }
        );
    }

    public async Task<IReadOnlyList<ProductMedia>> GetDefaultByProductIdsAsync(long[] productIds, CancellationToken ct = default)
    {
        if (productIds == null || productIds.Length == 0)
        {
            return [];
        }

        return await ExecutionHelper.ExecuteAsync(
            () => _context.ProductMedias
                .AsNoTracking()
                .Where(pm => productIds.Contains(pm.ProductId) && pm.IsDefault)
                .ToListAsync(ct),
            logger,
            "Get Default Media by ProductIds",
            new { ProductIds = productIds }
        );
    }
}