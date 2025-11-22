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

    public async Task<Cart> GetOrCreateAsync(long? userId, Guid sessionId, CancellationToken ct = default)
    {
        Guard.AgainstEmptyGuid(sessionId, nameof(sessionId));

        // Try to find existing cart
        Cart? cart = null;

        // Try session first (anonymous users)
        if (sessionId != Guid.Empty)
        {
            cart = await _context.Carts
                .FirstOrDefaultAsync(c => c.SessionId == sessionId && c.ExpiresAt > DateTime.UtcNow, ct);
        }

        // Then try user cart if logged in
        if (cart == null && userId.HasValue && userId > 0)
        {
            cart = await _context.Carts
                .FirstOrDefaultAsync(c => c.UserId == userId && c.ExpiresAt > DateTime.UtcNow, ct);
        }

        // Create new cart if not found
        if (cart == null)
        {
            cart = new Cart
            {
                UserId = userId,
                SessionId = sessionId,
                CartNumber = $"CART-{DateTime.UtcNow:yyyy}-{new Random().Next(1, 9999):D4}",
                ExpiresAt = DateTime.UtcNow.AddHours(userId.HasValue && userId > 0 ? 72 : 24)
            };

            await _context.Carts.AddAsync(cart, ct);
        }

        return cart;
    }
}