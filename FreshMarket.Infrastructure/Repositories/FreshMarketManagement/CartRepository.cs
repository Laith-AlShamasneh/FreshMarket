using FreshMarket.Domain.Entities.FreshMarketManagement;
using FreshMarket.Domain.Interfaces.Repositories.FreshMarketManagement;
using FreshMarket.Infrastructure.Data;
using FreshMarket.Shared.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FreshMarket.Infrastructure.Repositories.FreshMarketManagement;

public class CartRepository(
    FreshMarketDbContext context,
    ILogger<CartRepository> logger)
    : Repository<Cart>(context, logger), ICart
{
    private readonly FreshMarketDbContext _context = context;

    private static string GenerateCartNumber() =>
        $"CART-{DateTime.UtcNow:yyyy}-{new Random().Next(1, 9999):D4}";

    public async Task<Cart?> GetActiveBySessionIdAsync(Guid sessionId, CancellationToken ct = default)
    {
        Guard.AgainstEmpty(sessionId, nameof(sessionId));

        return await ExecutionHelper.ExecuteAsync(
            () => _context.Carts
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.SessionId == sessionId && c.ExpiresAt > DateTime.UtcNow, ct),
            logger,
            "Get Active Cart by SessionId",
            new { SessionId = sessionId }
        );
    }

    public async Task<Cart?> GetActiveByUserIdAsync(long? userId, CancellationToken ct = default)
    {
        if (!userId.HasValue || userId <= 0) return null;

        return await ExecutionHelper.ExecuteAsync(
            () => _context.Carts
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.UserId == userId && c.ExpiresAt > DateTime.UtcNow, ct),
            logger,
            "Get Active Cart by UserId",
            new { UserId = userId }
        );
    }

    public async Task<Cart?> GetWithItemsBySessionIdAsync(Guid sessionId, CancellationToken ct = default)
    {
        Guard.AgainstEmpty(sessionId, nameof(sessionId));

        return await ExecutionHelper.ExecuteAsync(
            () => _context.Carts
                .AsNoTracking()
                .Include(c => c.Items)
                .ThenInclude(ci => ci.ProductVariant)
                .ThenInclude(pv => pv.Product)
                .ThenInclude(p => p.ProductMedia)
                .Include(c => c.Items)
                .ThenInclude(ci => ci.ProductVariant)
                .ThenInclude(pv => pv.Product)
                .ThenInclude(p => p.Category)
                .FirstOrDefaultAsync(c => c.SessionId == sessionId && c.ExpiresAt > DateTime.UtcNow, ct),
            logger,
            "Get Cart with Items by SessionId",
            new { SessionId = sessionId }
        );
    }

    public async Task<Cart?> GetWithItemsByUserIdAsync(long? userId, CancellationToken ct = default)
    {
        if (!userId.HasValue || userId <= 0) return null;

        return await ExecutionHelper.ExecuteAsync(
            () => _context.Carts
                .AsNoTracking()
                .Include(c => c.Items)
                .ThenInclude(ci => ci.ProductVariant)
                .ThenInclude(pv => pv.Product)
                .ThenInclude(p => p.ProductMedia)
                .Include(c => c.Items)
                .ThenInclude(ci => ci.ProductVariant)
                .ThenInclude(pv => pv.Product)
                .ThenInclude(p => p.Category)
                .FirstOrDefaultAsync(c => c.UserId == userId && c.ExpiresAt > DateTime.UtcNow, ct),
            logger,
            "Get Cart with Items by UserId",
            new { UserId = userId }
        );
    }

    public async Task<Cart> GetOrCreateAsync(long? userId, Guid sessionId, CancellationToken ct = default)
    {
        Guard.AgainstEmpty(sessionId, nameof(sessionId));

        // Try session first (anonymous)
        var cart = await GetActiveBySessionIdAsync(sessionId, ct);

        // Then user (if logged in)
        if (cart == null && userId.HasValue && userId > 0)
            cart = await GetActiveByUserIdAsync(userId, ct);

        if (cart != null) return cart;

        // Create new
        var newCart = new Cart
        {
            UserId = userId,
            SessionId = sessionId,
            CartNumber = GenerateCartNumber(),
            ExpiresAt = userId.HasValue
                ? DateTime.UtcNow.AddDays(20)  // Logged-in: 20 days
                : DateTime.UtcNow.AddHours(24) // Anonymous: 24h
        };

        await ExecutionHelper.ExecuteAsync(
            () =>
            {
                _context.Carts.Add(newCart);
                return _context.SaveChangesAsync(ct);
            },
            logger,
            "Create New Cart (Anonymous or User)",
            new { UserId = userId, SessionId = sessionId, CartNumber = newCart.CartNumber }
        );

        return newCart;
    }

    public async Task ClearExpiredCartsAsync(CancellationToken ct = default)
    {
        await ExecutionHelper.ExecuteAsync(
            async () =>
            {
                var expired = await _context.Carts
                    .Where(c => c.ExpiresAt <= DateTime.UtcNow)
                    .ToListAsync(ct);

                if (expired.Any())
                {
                    _context.Carts.RemoveRange(expired);
                    await _context.SaveChangesAsync(ct);
                }
            },
            logger,
            "Clear Expired Carts",
            new { ExpiredCount = "unknown" }
        );
    }
}