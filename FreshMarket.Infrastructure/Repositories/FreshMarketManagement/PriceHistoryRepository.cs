using FreshMarket.Domain.Entities.FreshMarketManagement;
using FreshMarket.Domain.Interfaces.Repositories.FreshMarketManagement;
using FreshMarket.Infrastructure.Data;
using FreshMarket.Shared.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FreshMarket.Infrastructure.Repositories.FreshMarketManagement;

public class PriceHistoryRepository(
    FreshMarketDbContext context,
    ILogger<PriceHistoryRepository> logger)
    : Repository<PriceHistory>(context, logger), IPriceHistory
{
    private readonly FreshMarketDbContext _context = context;

    public async Task<IReadOnlyList<PriceHistory>> GetByProductIdAsync(long productId, CancellationToken ct = default)
    {
        Guard.AgainstNegativeOrZero(productId, nameof(productId));

        return await ExecutionHelper.ExecuteAsync(
            () => _context.PriceHistories
                .AsNoTracking()
                .Where(ph => ph.ProductId == productId)
                .Include(ph => ph.Product)
                .Include(ph => ph.ProductVariant)
                .Include(ph => ph.Currency)
                .OrderByDescending(ph => ph.CreatedAt)
                .ToListAsync(ct),
            logger,
            "Get PriceHistory by ProductId",
            new { ProductId = productId }
        );
    }

    public async Task<IReadOnlyList<PriceHistory>> GetByProductVariantIdAsync(long productVariantId, CancellationToken ct = default)
    {
        Guard.AgainstNegativeOrZero(productVariantId, nameof(productVariantId));

        return await ExecutionHelper.ExecuteAsync(
            () => _context.PriceHistories
                .AsNoTracking()
                .Where(ph => ph.ProductVariantId == productVariantId)
                .Include(ph => ph.Product)
                .Include(ph => ph.ProductVariant)
                .Include(ph => ph.Currency)
                .OrderByDescending(ph => ph.CreatedAt)
                .ToListAsync(ct),
            logger,
            "Get PriceHistory by ProductVariantId",
            new { ProductVariantId = productVariantId }
        );
    }

    public async Task<PriceHistory?> GetLatestByProductVariantIdAsync(long productVariantId, CancellationToken ct = default)
    {
        Guard.AgainstNegativeOrZero(productVariantId, nameof(productVariantId));

        return await ExecutionHelper.ExecuteAsync(
            () => _context.PriceHistories
                .AsNoTracking()
                .Where(ph => ph.ProductVariantId == productVariantId)
                .Include(ph => ph.Currency)
                .OrderByDescending(ph => ph.CreatedAt)
                .FirstOrDefaultAsync(ct),
            logger,
            "Get Latest PriceHistory by ProductVariantId",
            new { ProductVariantId = productVariantId }
        );
    }

    public async Task<IReadOnlyList<PriceHistory>> GetPriceHistoryRangeAsync(
        long? productId,
        long? productVariantId,
        DateTime? from,
        DateTime? to,
        CancellationToken ct = default)
    {
        if (productId.HasValue) Guard.AgainstNegativeOrZero(productId.Value, nameof(productId));
        if (productVariantId.HasValue) Guard.AgainstNegativeOrZero(productVariantId.Value, nameof(productVariantId));

        var query = _context.PriceHistories.AsNoTracking().AsQueryable();

        if (productId.HasValue)
            query = query.Where(ph => ph.ProductId == productId);
        if (productVariantId.HasValue)
            query = query.Where(ph => ph.ProductVariantId == productVariantId);
        if (from.HasValue)
            query = query.Where(ph => ph.CreatedAt >= from.Value);
        if (to.HasValue)
            query = query.Where(ph => ph.CreatedAt <= to.Value);

        return await ExecutionHelper.ExecuteAsync(
            () => query
                .Include(ph => ph.Product)
                .Include(ph => ph.ProductVariant)
                .Include(ph => ph.Currency)
                .OrderByDescending(ph => ph.CreatedAt)
                .ToListAsync(ct),
            logger,
            "Get PriceHistory Range",
            new { ProductId = productId, ProductVariantId = productVariantId, From = from, To = to }
        );
    }

    public async Task<PriceHistory> LogPriceChangeAsync(
        long? productId,
        long? productVariantId,
        decimal oldPrice,
        decimal newPrice,
        int? currencyId = null,
        string? reason = null,
        string? notes = null,
        CancellationToken ct = default)
    {
        if (productId.HasValue) Guard.AgainstNegativeOrZero(productId.Value, nameof(productId));
        if (productVariantId.HasValue) Guard.AgainstNegativeOrZero(productVariantId.Value, nameof(productVariantId));
        if (currencyId.HasValue) Guard.AgainstNegativeOrZero(currencyId.Value, nameof(currencyId));
        Guard.AgainstNegativeOrZero(oldPrice, nameof(oldPrice));
        Guard.AgainstNegativeOrZero(newPrice, nameof(newPrice));

        var history = new PriceHistory
        {
            ProductId = productId,
            ProductVariantId = productVariantId,
            OldPrice = oldPrice,
            NewPrice = newPrice,
            CurrencyId = currencyId,
            Reason = reason?.Trim(),
            Notes = notes?.Trim()
        };

        return await ExecutionHelper.ExecuteAsync(
            async () =>
            {
                _context.PriceHistories.Add(history);
                await _context.SaveChangesAsync(ct);
                return history;
            },
            logger,
            "Log Price Change",
            new
            {
                ProductId = productId,
                ProductVariantId = productVariantId,
                OldPrice = oldPrice,
                NewPrice = newPrice,
                CurrencyId = currencyId,
                Reason = reason,
                Notes = notes?.Length > 50 ? notes.Substring(0, 47) + "..." : notes
            }
        );
    }
}
