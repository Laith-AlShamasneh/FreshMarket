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

        Cart? cart = null;

        if (sessionId != Guid.Empty)
        {
            cart = await _context.Carts
                .FirstOrDefaultAsync(c => c.SessionId == sessionId && c.ExpiresAt > DateTime.UtcNow, ct);
        }

        if (cart == null && userId.HasValue && userId > 0)
        {
            cart = await _context.Carts
                .FirstOrDefaultAsync(c => c.UserId == userId && c.ExpiresAt > DateTime.UtcNow, ct);
        }

        if (cart == null)
        {
            cart = Cart.Create(userId, sessionId);
            await _context.Carts.AddAsync(cart, ct);
        }
        else if (userId.HasValue && userId > 0 && !cart.UserId.HasValue)
        {
            cart.AssignUser(userId.Value);
            _context.Carts.Update(cart);
        }

        return cart;
    }
}