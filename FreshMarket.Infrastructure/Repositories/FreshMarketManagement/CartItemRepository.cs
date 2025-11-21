using FreshMarket.Domain.Entities.FreshMarketManagement;
using FreshMarket.Domain.Interfaces.Repositories.FreshMarketManagement;
using FreshMarket.Infrastructure.Data;
using FreshMarket.Shared.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FreshMarket.Infrastructure.Repositories.FreshMarketManagement;

public class CartItemRepository(
    FreshMarketDbContext context,
    ILogger<CartItemRepository> logger)
    : Repository<CartItem>(context, logger), ICartItem
{
    private readonly FreshMarketDbContext _context = context;

    public async Task<IReadOnlyList<CartItem>> GetByCartIdAsync(long cartId, CancellationToken ct = default)
    {
        Guard.AgainstNegativeOrZero(cartId, nameof(cartId));

        return await ExecutionHelper.ExecuteAsync(
            () => _context.CartItems
                .AsNoTracking()
                .Where(ci => ci.CartId == cartId)
                .Include(ci => ci.ProductVariant)
                .ThenInclude(pv => pv.Product)
                .ToListAsync(ct),
            logger,
            "Get CartItems by CartId",
            new { CartId = cartId }
        );
    }

    public async Task<IReadOnlyList<CartItem>> GetBySessionIdAsync(Guid sessionId, CancellationToken ct = default)
    {
        Guard.AgainstEmpty(sessionId, nameof(sessionId));

        return await ExecutionHelper.ExecuteAsync(
            () => _context.CartItems
                .AsNoTracking()
                .Where(ci => ci.Cart.SessionId == sessionId)
                .Include(ci => ci.ProductVariant)
                .ThenInclude(pv => pv.Product)
                .ToListAsync(ct),
            logger,
            "Get CartItems by SessionId",
            new { SessionId = sessionId }
        );
    }

    public async Task<CartItem?> GetByCartIdAndProductVariantIdAsync(long cartId, long productVariantId, CancellationToken ct = default)
    {
        Guard.AgainstNegativeOrZero(cartId, nameof(cartId));
        Guard.AgainstNegativeOrZero(productVariantId, nameof(productVariantId));

        return await ExecutionHelper.ExecuteAsync(
            () => _context.CartItems
                .AsNoTracking()
                .FirstOrDefaultAsync(ci => ci.CartId == cartId && ci.ProductVariantId == productVariantId, ct),
            logger,
            "Get CartItem by CartId and ProductVariantId",
            new { CartId = cartId, ProductVariantId = productVariantId }
        );
    }

    public async Task<CartItem?> GetBySessionIdAndProductVariantIdAsync(Guid sessionId, long productVariantId, CancellationToken ct = default)
    {
        Guard.AgainstEmpty(sessionId, nameof(sessionId));
        Guard.AgainstNegativeOrZero(productVariantId, nameof(productVariantId));

        return await ExecutionHelper.ExecuteAsync(
            () => _context.CartItems
                .AsNoTracking()
                .FirstOrDefaultAsync(ci => ci.Cart.SessionId == sessionId && ci.ProductVariantId == productVariantId, ct),
            logger,
            "Get CartItem by SessionId and ProductVariantId",
            new { SessionId = sessionId, ProductVariantId = productVariantId }
        );
    }

    public async Task<IReadOnlyList<CartItem>> GetWithDetailsByCartIdAsync(long cartId, CancellationToken ct = default)
    {
        Guard.AgainstNegativeOrZero(cartId, nameof(cartId));

        return await ExecutionHelper.ExecuteAsync(
            () => _context.CartItems
                .AsNoTracking()
                .Where(ci => ci.CartId == cartId)
                .Include(ci => ci.ProductVariant)
                .ThenInclude(pv => pv.Product)
                .ThenInclude(p => p.ProductMedia)
                .Include(ci => ci.ProductVariant)
                .ThenInclude(pv => pv.Product)
                .ThenInclude(p => p.Category)
                .Include(ci => ci.ProductVariant)
                .ThenInclude(pv => pv.Inventory)
                .ToListAsync(ct),
            logger,
            "Get CartItems with Full Details by CartId",
            new { CartId = cartId }
        );
    }

    public async Task<CartItem> AddOrUpdateAsync(long cartId, long productVariantId, decimal quantity, CancellationToken ct = default)
    {
        Guard.AgainstNegativeOrZero(cartId, nameof(cartId));
        Guard.AgainstNegativeOrZero(productVariantId, nameof(productVariantId));

        return await ExecutionHelper.ExecuteAsync(
            async () =>
            {
                var item = await _context.CartItems
                    .FirstOrDefaultAsync(ci => ci.CartId == cartId && ci.ProductVariantId == productVariantId, ct);

                if (item == null)
                {
                    item = new CartItem
                    {
                        CartId = cartId,
                        ProductVariantId = productVariantId,
                        Quantity = quantity
                    };
                    _context.CartItems.Add(item);
                }
                else
                {
                    item.Quantity = quantity;
                }

                await _context.SaveChangesAsync(ct);
                return item;
            },
            logger,
            "Add or Update CartItem",
            new { CartId = cartId, ProductVariantId = productVariantId, Quantity = quantity }
        );
    }

    public async Task RemoveBySessionIdAndVariantIdAsync(Guid sessionId, long productVariantId, CancellationToken ct = default)
    {
        Guard.AgainstEmpty(sessionId, nameof(sessionId));
        Guard.AgainstNegativeOrZero(productVariantId, nameof(productVariantId));

        await ExecutionHelper.ExecuteAsync(
            async () =>
            {
                var item = await _context.CartItems
                    .FirstOrDefaultAsync(ci => ci.Cart.SessionId == sessionId && ci.ProductVariantId == productVariantId, ct);

                if (item != null)
                {
                    _context.CartItems.Remove(item);
                    await _context.SaveChangesAsync(ct);
                }
            },
            logger,
            "Remove CartItem by SessionId and VariantId",
            new { SessionId = sessionId, ProductVariantId = productVariantId }
        );
    }
}