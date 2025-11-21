using FreshMarket.Domain.Entities.FreshMarketManagement;
using FreshMarket.Domain.Interfaces.Repositories.FreshMarketManagement;
using FreshMarket.Infrastructure.Data;
using FreshMarket.Shared.Common;
using FreshMarket.Shared.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FreshMarket.Infrastructure.Repositories.FreshMarketManagement;

public class OrderHistoryRepository(
    FreshMarketDbContext context,
    ILogger<OrderHistoryRepository> logger)
    : Repository<OrderHistory>(context, logger), IOrderHistory
{
    private readonly FreshMarketDbContext _context = context;

    public async Task<IReadOnlyList<OrderHistory>> GetByOrderIdAsync(long orderId, CancellationToken ct = default)
    {
        Guard.AgainstNegativeOrZero(orderId, nameof(orderId));

        return await ExecutionHelper.ExecuteAsync(
            () => _context.OrderHistories
                .AsNoTracking()
                .Where(oh => oh.OrderId == orderId)
                .Include(oh => oh.OldStatus)
                .Include(oh => oh.NewStatus)
                .Include(oh => oh.Order)
                .ThenInclude(o => o.User)
                .OrderByDescending(oh => oh.UpdatedAt)
                .ToListAsync(ct),
            logger,
            "Get OrderHistory by OrderId",
            new { OrderId = orderId }
        );
    }

    public async Task<IReadOnlyList<OrderHistory>> GetBySessionIdAsync(Guid sessionId, CancellationToken ct = default)
    {
        Guard.AgainstEmpty(sessionId, nameof(sessionId));

        return await ExecutionHelper.ExecuteAsync(
            () => _context.OrderHistories
                .AsNoTracking()
                .Where(oh => oh.Order.OrderItems.Any(oi => oi.SessionId == sessionId))
                .Include(oh => oh.OldStatus)
                .Include(oh => oh.NewStatus)
                .Include(oh => oh.Order)
                .ThenInclude(o => o.User)
                .OrderByDescending(oh => oh.UpdatedAt)
                .ToListAsync(ct),
            logger,
            "Get OrderHistory by SessionId",
            new { SessionId = sessionId }
        );
    }

    public async Task<OrderHistory?> GetLatestByOrderIdAsync(long orderId, CancellationToken ct = default)
    {
        Guard.AgainstNegativeOrZero(orderId, nameof(orderId));

        return await ExecutionHelper.ExecuteAsync(
            () => _context.OrderHistories
                .AsNoTracking()
                .Where(oh => oh.OrderId == orderId)
                .Include(oh => oh.NewStatus)
                .OrderByDescending(oh => oh.UpdatedAt)
                .FirstOrDefaultAsync(ct),
            logger,
            "Get Latest OrderHistory by OrderId",
            new { OrderId = orderId }
        );
    }

    public async Task<OrderHistory?> GetLatestBySessionIdAsync(Guid sessionId, CancellationToken ct = default)
    {
        Guard.AgainstEmpty(sessionId, nameof(sessionId));

        return await ExecutionHelper.ExecuteAsync(
            () => _context.OrderHistories
                .AsNoTracking()
                .Where(oh => oh.Order.OrderItems.Any(oi => oi.SessionId == sessionId))
                .Include(oh => oh.NewStatus)
                .OrderByDescending(oh => oh.UpdatedAt)
                .FirstOrDefaultAsync(ct),
            logger,
            "Get Latest OrderHistory by SessionId",
            new { SessionId = sessionId }
        );
    }

    public async Task<OrderHistory> AddStatusChangeAsync(
        long orderId,
        OrderStatus oldStatus,
        OrderStatus newStatus,
        string? notes = null,
        CancellationToken ct = default)
    {
        if (notes is not null) Guard.AgainstNullOrWhiteSpace(notes, nameof(notes));

        var history = new OrderHistory
        {
            OrderId = orderId,
            OldStatus = oldStatus,
            NewStatus = newStatus,
            Notes = notes?.Trim()
        };

        return await ExecutionHelper.ExecuteAsync(
            async () =>
            {
                _context.OrderHistories.Add(history);
                await _context.SaveChangesAsync(ct);
                return history;
            },
            logger,
            "Add Order Status Change",
            new { OrderId = orderId, OldStatus = oldStatus, NewStatus = newStatus, Notes = notes }
        );
    }
}
