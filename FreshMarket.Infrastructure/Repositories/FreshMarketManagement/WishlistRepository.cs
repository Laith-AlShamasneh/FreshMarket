using FreshMarket.Domain.Entities.FreshMarketManagement;
using FreshMarket.Domain.Interfaces.Repositories.FreshMarketManagement;
using FreshMarket.Infrastructure.Data;
using FreshMarket.Shared.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FreshMarket.Infrastructure.Repositories.FreshMarketManagement;

public class WishlistRepository(
    FreshMarketDbContext context,
    ILogger<WishlistRepository> logger)
    : Repository<Wishlist>(context, logger), IWishlist
{
    private readonly FreshMarketDbContext _context = context;

    public async Task<Wishlist?> GetByUserIdAsync(long userId, CancellationToken ct = default)
    {
        Guard.AgainstNegativeOrZero(userId, nameof(userId));

        return await ExecutionHelper.ExecuteAsync(
            () => _context.Wishlists
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.UserId == userId, ct),
            logger,
            "Get Wishlist by UserId",
            new { UserId = userId }
        );
    }

    public async Task<Wishlist?> GetWithItemsByUserIdAsync(long userId, CancellationToken ct = default)
    {
        Guard.AgainstNegativeOrZero(userId, nameof(userId));

        return await ExecutionHelper.ExecuteAsync(
            () => _context.Wishlists
                .AsNoTracking()
                .Include(w => w.Items)
                .ThenInclude(wi => wi.ProductVariant)
                .ThenInclude(pv => pv.Product)
                .ThenInclude(p => p.ProductMedia)
                .Include(w => w.Items)
                .ThenInclude(wi => wi.ProductVariant)
                .ThenInclude(pv => pv.Inventory)
                .Include(w => w.Items)
                .ThenInclude(wi => wi.ProductVariant)
                .ThenInclude(pv => pv.Product)
                .ThenInclude(p => p.Category)
                .FirstOrDefaultAsync(w => w.UserId == userId, ct),
            logger,
            "Get Wishlist with Full Items",
            new { UserId = userId }
        );
    }

    public async Task AddItemAsync(long userId, long productVariantId, CancellationToken ct = default)
    {
        Guard.AgainstNegativeOrZero(userId, nameof(userId));
        Guard.AgainstNegativeOrZero(productVariantId, nameof(productVariantId));

        await ExecutionHelper.ExecuteAsync(
            async () =>
            {
                var wishlist = await _context.Wishlists
                    .Include(w => w.Items)
                    .FirstOrDefaultAsync(w => w.UserId == userId, ct);

                if (wishlist == null)
                {
                    wishlist = new Wishlist
                    {
                        UserId = userId,
                        Items = new List<WishlistItem>()
                    };
                    _context.Wishlists.Add(wishlist);
                }

                if (!wishlist.Items.Any(i => i.ProductVariantId == productVariantId))
                {
                    wishlist.Items.Add(new WishlistItem
                    {
                        ProductVariantId = productVariantId,
                        CreatedAt = DateTime.UtcNow
                    });
                }

                await _context.SaveChangesAsync(ct);
            },
            logger,
            "Add Wishlist Item",
            new { UserId = userId, ProductVariantId = productVariantId }
        );
    }

    public async Task RemoveItemAsync(long userId, long productVariantId, CancellationToken ct = default)
    {
        Guard.AgainstNegativeOrZero(userId, nameof(userId));
        Guard.AgainstNegativeOrZero(productVariantId, nameof(productVariantId));

        await ExecutionHelper.ExecuteAsync(
            async () =>
            {
                var wishlist = await _context.Wishlists
                    .Include(w => w.Items)
                    .FirstOrDefaultAsync(w => w.UserId == userId, ct);

                if (wishlist != null)
                {
                    var item = wishlist.Items.FirstOrDefault(i => i.ProductVariantId == productVariantId);
                    if (item != null)
                    {
                        wishlist.Items.Remove(item);
                        await _context.SaveChangesAsync(ct);
                    }
                }
            },
            logger,
            "Remove Wishlist Item",
            new { UserId = userId, ProductVariantId = productVariantId }
        );
    }

    public async Task MoveToCartAsync(long userId, long productVariantId, CancellationToken ct = default)
    {
        Guard.AgainstNegativeOrZero(userId, nameof(userId));
        Guard.AgainstNegativeOrZero(productVariantId, nameof(productVariantId));

        await ExecutionHelper.ExecuteAsync(
            async () =>
            {
                var wishlist = await _context.Wishlists
                    .Include(w => w.Items)
                    .FirstOrDefaultAsync(w => w.UserId == userId, ct);

                if (wishlist != null)
                {
                    var item = wishlist.Items.FirstOrDefault(i => i.ProductVariantId == productVariantId);
                    if (item != null)
                    {
                        // Add to cart
                        var cart = await _context.Carts
                            .Include(c => c.Items)
                            .FirstOrDefaultAsync(c => c.UserId == userId, ct);

                        if (cart == null)
                        {
                            cart = new Cart { UserId = userId };
                            _context.Carts.Add(cart);
                            await _context.SaveChangesAsync(ct);
                        }

                        if (!cart.Items.Any(ci => ci.ProductVariantId == productVariantId))
                        {
                            cart.Items.Add(new CartItem
                            {
                                ProductVariantId = productVariantId,
                                Quantity = 1
                            });
                        }

                        // Remove from wishlist
                        wishlist.Items.Remove(item);
                        await _context.SaveChangesAsync(ct);
                    }
                }
            },
            logger,
            "Move Wishlist Item to Cart",
            new { UserId = userId, ProductVariantId = productVariantId }
        );
    }
}
