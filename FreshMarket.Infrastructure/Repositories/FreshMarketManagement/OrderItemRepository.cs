using FreshMarket.Domain.Entities.FreshMarketManagement;
using FreshMarket.Domain.Interfaces.Repositories.FreshMarketManagement;
using FreshMarket.Infrastructure.Data;
using FreshMarket.Shared.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FreshMarket.Infrastructure.Repositories.FreshMarketManagement;

public class OrderItemRepository(
    FreshMarketDbContext context,
    ILogger<OrderItemRepository> logger)
    : Repository<OrderItem>(context, logger), IOrderItem
{
    private readonly FreshMarketDbContext _context = context;

    public async Task<IReadOnlyList<OrderItem>> GetByOrderIdAsync(long orderId, CancellationToken ct = default)
    {
        Guard.AgainstNegativeOrZero(orderId, nameof(orderId));

        return await ExecutionHelper.ExecuteAsync(
            () => _context.OrderItems
                .AsNoTracking()
                .Where(oi => oi.OrderId == orderId)
                .Include(oi => oi.ProductVariant)
                .ThenInclude(pv => pv.Product)
                .ToListAsync(ct),
            logger,
            "Get OrderItems by OrderId",
            new { OrderId = orderId }
        );
    }

    public async Task<IReadOnlyList<OrderItem>> GetBySessionIdAsync(Guid sessionId, CancellationToken ct = default)
    {
        Guard.AgainstEmpty(sessionId, nameof(sessionId));

        return await ExecutionHelper.ExecuteAsync(
            () => _context.OrderItems
                .AsNoTracking()
                .Where(oi => oi.SessionId == sessionId)
                .Include(oi => oi.ProductVariant)
                .ThenInclude(pv => pv.Product)
                .ToListAsync(ct),
            logger,
            "Get OrderItems by SessionId",
            new { SessionId = sessionId }
        );
    }

    public async Task<IReadOnlyList<OrderItem>> GetWithDetailsByOrderIdAsync(long orderId, CancellationToken ct = default)
    {
        Guard.AgainstNegativeOrZero(orderId, nameof(orderId));

        return await ExecutionHelper.ExecuteAsync(
            () => _context.OrderItems
                .AsNoTracking()
                .Where(oi => oi.OrderId == orderId)
                .Include(oi => oi.ProductVariant)
                .ThenInclude(pv => pv.Product)
                .ThenInclude(p => p.ProductMedia)
                .Include(oi => oi.ProductVariant)
                .ThenInclude(pv => pv.Product)
                .ThenInclude(p => p.Category)
                .Include(oi => oi.ProductVariant)
                .ThenInclude(pv => pv.Inventory)
                .ToListAsync(ct),
            logger,
            "Get OrderItems with Full Details by OrderId",
            new { OrderId = orderId }
        );
    }

    public async Task<OrderItem?> GetByOrderIdAndProductVariantIdAsync(long orderId, long productVariantId, CancellationToken ct = default)
    {
        Guard.AgainstNegativeOrZero(orderId, nameof(orderId));
        Guard.AgainstNegativeOrZero(productVariantId, nameof(productVariantId));

        return await ExecutionHelper.ExecuteAsync(
            () => _context.OrderItems
                .AsNoTracking()
                .FirstOrDefaultAsync(oi => oi.OrderId == orderId && oi.ProductVariantId == productVariantId, ct),
            logger,
            "Get OrderItem by OrderId and ProductVariantId",
            new { OrderId = orderId, ProductVariantId = productVariantId }
        );
    }

    public async Task<OrderItem?> GetBySessionIdAndProductVariantIdAsync(Guid sessionId, long productVariantId, CancellationToken ct = default)
    {
        Guard.AgainstEmpty(sessionId, nameof(sessionId));
        Guard.AgainstNegativeOrZero(productVariantId, nameof(productVariantId));

        return await ExecutionHelper.ExecuteAsync(
            () => _context.OrderItems
                .AsNoTracking()
                .FirstOrDefaultAsync(oi => oi.SessionId == sessionId && oi.ProductVariantId == productVariantId, ct),
            logger,
            "Get OrderItem by SessionId and ProductVariantId",
            new { SessionId = sessionId, ProductVariantId = productVariantId }
        );
    }
}
