using FreshMarket.Domain.Entities.FreshMarketManagement;
using FreshMarket.Domain.Interfaces.Repositories.FreshMarketManagement;
using FreshMarket.Infrastructure.Data;
using FreshMarket.Shared.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FreshMarket.Infrastructure.Repositories.FreshMarketManagement;

public class PaymentTransactionRepository(
    FreshMarketDbContext context,
    ILogger<PaymentTransactionRepository> logger)
    : Repository<PaymentTransaction>(context, logger), IPaymentTransaction
{
    private readonly FreshMarketDbContext _context = context;

    public async Task<IReadOnlyList<PaymentTransaction>> GetByOrderIdAsync(long orderId, CancellationToken ct = default)
    {
        Guard.AgainstNegativeOrZero(orderId, nameof(orderId));

        return await ExecutionHelper.ExecuteAsync(
            () => _context.PaymentTransactions
                .AsNoTracking()
                .Where(pt => pt.OrderId == orderId)
                .Include(pt => pt.PaymentMethodTypeId)
                .Include(pt => pt.PaymentStatus)
                .Include(pt => pt.Order)
                .ThenInclude(o => o.User)
                .OrderByDescending(pt => pt.CreatedAt)
                .ToListAsync(ct),
            logger,
            "Get PaymentTransactions by OrderId",
            new { OrderId = orderId }
        );
    }

    public async Task<IReadOnlyList<PaymentTransaction>> GetBySessionIdAsync(Guid sessionId, CancellationToken ct = default)
    {
        Guard.AgainstEmpty(sessionId, nameof(sessionId));

        return await ExecutionHelper.ExecuteAsync(
            () => _context.PaymentTransactions
                .AsNoTracking()
                .Where(pt => pt.Order.OrderItems.Any(oi => oi.SessionId == sessionId))
                .Include(pt => pt.PaymentMethodTypeId)
                .Include(pt => pt.PaymentStatus)
                .Include(pt => pt.Order)
                .ThenInclude(o => o.User)
                .OrderByDescending(pt => pt.CreatedAt)
                .ToListAsync(ct),
            logger,
            "Get PaymentTransactions by SessionId",
            new { SessionId = sessionId }
        );
    }

    public async Task<PaymentTransaction?> GetLatestByOrderIdAsync(long orderId, CancellationToken ct = default)
    {
        Guard.AgainstNegativeOrZero(orderId, nameof(orderId));

        return await ExecutionHelper.ExecuteAsync(
            () => _context.PaymentTransactions
                .AsNoTracking()
                .Where(pt => pt.OrderId == orderId)
                .Include(pt => pt.PaymentMethodTypeId)
                .Include(pt => pt.PaymentStatus)
                .OrderByDescending(pt => pt.CreatedAt)
                .FirstOrDefaultAsync(ct),
            logger,
            "Get Latest PaymentTransaction by OrderId",
            new { OrderId = orderId }
        );
    }

    public async Task<PaymentTransaction?> GetLatestBySessionIdAsync(Guid sessionId, CancellationToken ct = default)
    {
        Guard.AgainstEmpty(sessionId, nameof(sessionId));

        return await ExecutionHelper.ExecuteAsync(
            () => _context.PaymentTransactions
                .AsNoTracking()
                .Where(pt => pt.Order.OrderItems.Any(oi => oi.SessionId == sessionId))
                .Include(pt => pt.PaymentMethodTypeId)
                .Include(pt => pt.PaymentStatus)
                .OrderByDescending(pt => pt.CreatedAt)
                .FirstOrDefaultAsync(ct),
            logger,
            "Get Latest PaymentTransaction by SessionId",
            new { SessionId = sessionId }
        );
    }

    public async Task<IReadOnlyList<PaymentTransaction>> GetByStatusAsync(int paymentStatusId, CancellationToken ct = default)
    {
        Guard.AgainstNegativeOrZero(paymentStatusId, nameof(paymentStatusId));

        return await ExecutionHelper.ExecuteAsync(
            () => _context.PaymentTransactions
                .AsNoTracking()
                .Where(pt => pt.PaymentStatusId == paymentStatusId)
                .Include(pt => pt.PaymentMethodTypeId)
                .Include(pt => pt.PaymentStatus)
                .Include(pt => pt.Order)
                .ThenInclude(o => o.User)
                .OrderByDescending(pt => pt.CreatedAt)
                .ToListAsync(ct),
            logger,
            "Get PaymentTransactions by Status",
            new { PaymentStatusId = paymentStatusId }
        );
    }
}