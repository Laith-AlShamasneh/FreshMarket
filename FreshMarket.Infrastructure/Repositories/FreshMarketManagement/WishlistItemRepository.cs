using FreshMarket.Domain.Entities.FreshMarketManagement;
using FreshMarket.Domain.Interfaces.Repositories.FreshMarketManagement;
using FreshMarket.Infrastructure.Data;
using FreshMarket.Shared.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FreshMarket.Infrastructure.Repositories.FreshMarketManagement;

public class WishlistItemRepository(
    FreshMarketDbContext context,
    ILogger<WishlistItemRepository> logger)
    : Repository<WishlistItem>(context, logger), IWishlistItem
{
    private readonly FreshMarketDbContext _context = context;

    public async Task<IReadOnlyList<WishlistItem>> GetByWishlistIdAsync(long wishlistId, CancellationToken ct = default)
    {
        Guard.AgainstNegativeOrZero(wishlistId, nameof(wishlistId));

        return await ExecutionHelper.ExecuteAsync(
            () => _context.WishlistItems
                .AsNoTracking()
                .Where(wi => wi.WishlistId == wishlistId)
                .Include(wi => wi.ProductVariant)
                .ThenInclude(pv => pv.Product)
                .ToListAsync(ct),
            logger,
            "Get WishlistItems by WishlistId",
            new { WishlistId = wishlistId }
        );
    }

    public async Task<IReadOnlyList<WishlistItem>> GetByUserIdAsync(long userId, CancellationToken ct = default)
    {
        Guard.AgainstNegativeOrZero(userId, nameof(userId));

        return await ExecutionHelper.ExecuteAsync(
            () => _context.WishlistItems
                .AsNoTracking()
                .Where(wi => wi.Wishlist.UserId == userId)
                .Include(wi => wi.ProductVariant)
                .ThenInclude(pv => pv.Product)
                .OrderByDescending(wi => wi.CreatedAt)
                .ToListAsync(ct),
            logger,
            "Get WishlistItems by UserId",
            new { UserId = userId }
        );
    }

    public async Task<IReadOnlyList<WishlistItem>> GetWithDetailsByUserIdAsync(long userId, CancellationToken ct = default)
    {
        Guard.AgainstNegativeOrZero(userId, nameof(userId));

        return await ExecutionHelper.ExecuteAsync(
            () => _context.WishlistItems
                .AsNoTracking()
                .Where(wi => wi.Wishlist.UserId == userId)
                .Include(wi => wi.ProductVariant)
                .ThenInclude(pv => pv.Product)
                .ThenInclude(p => p.ProductMedia)
                .Include(wi => wi.ProductVariant)
                .ThenInclude(pv => pv.Inventory)
                .Include(wi => wi.ProductVariant)
                .ThenInclude(pv => pv.Product)
                .ThenInclude(p => p.Category)
                .OrderByDescending(wi => wi.CreatedAt)
                .ToListAsync(ct),
            logger,
            "Get WishlistItems with Full Details by UserId",
            new { UserId = userId }
        );
    }

    public async Task<WishlistItem?> GetByWishlistIdAndVariantIdAsync(long wishlistId, long productVariantId, CancellationToken ct = default)
    {
        Guard.AgainstNegativeOrZero(wishlistId, nameof(wishlistId));
        Guard.AgainstNegativeOrZero(productVariantId, nameof(productVariantId));

        return await ExecutionHelper.ExecuteAsync(
            () => _context.WishlistItems
                .AsNoTracking()
                .FirstOrDefaultAsync(wi => wi.WishlistId == wishlistId && wi.ProductVariantId == productVariantId, ct),
            logger,
            "Get WishlistItem by WishlistId and VariantId",
            new { WishlistId = wishlistId, ProductVariantId = productVariantId }
        );
    }

    public async Task<bool> ExistsAsync(long userId, long productVariantId, CancellationToken ct = default)
    {
        Guard.AgainstNegativeOrZero(userId, nameof(userId));
        Guard.AgainstNegativeOrZero(productVariantId, nameof(productVariantId));

        return await ExecutionHelper.ExecuteAsync(
            () => _context.WishlistItems
                .AnyAsync(wi => wi.Wishlist.UserId == userId && wi.ProductVariantId == productVariantId, ct),
            logger,
            "Check if WishlistItem Exists",
            new { UserId = userId, ProductVariantId = productVariantId }
        );
    }
}
