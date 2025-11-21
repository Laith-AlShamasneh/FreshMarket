using FreshMarket.Domain.Entities.FreshMarketManagement;
using FreshMarket.Domain.Interfaces.Repositories.FreshMarketManagement;
using FreshMarket.Infrastructure.Data;
using FreshMarket.Shared.Common;
using FreshMarket.Shared.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FreshMarket.Infrastructure.Repositories.FreshMarketManagement;

public class CouponRepository(
    FreshMarketDbContext context,
    ILogger<CouponRepository> logger)
    : Repository<Coupon>(context, logger), ICoupon
{
    private readonly FreshMarketDbContext _context = context;

    public async Task<Coupon?> GetActiveByCodeAsync(string code, CancellationToken ct = default)
    {
        Guard.AgainstNullOrWhiteSpace(code, nameof(code));

        return await ExecutionHelper.ExecuteAsync(
            () => _context.Coupons
                .AsNoTracking()
                .FirstOrDefaultAsync(c =>
                    c.Code == code &&
                    c.IsActive &&
                    c.StartsAt <= DateTime.UtcNow &&
                    (c.EndsAt == null || c.EndsAt >= DateTime.UtcNow) &&
                    (c.UsageLimit == null || c.UsedCount < c.UsageLimit),
                    ct),
            logger,
            "Get Active Coupon by Code",
            new { Code = code }
        );
    }

    public async Task<IReadOnlyList<Coupon>> GetActiveAsync(CancellationToken ct = default)
    {
        return await ExecutionHelper.ExecuteAsync(
            () => _context.Coupons
                .AsNoTracking()
                .Where(c =>
                    c.IsActive &&
                    c.StartsAt <= DateTime.UtcNow &&
                    (c.EndsAt == null || c.EndsAt >= DateTime.UtcNow) &&
                    (c.UsageLimit == null || c.UsedCount < c.UsageLimit))
                .OrderBy(c => c.Code)
                .ToListAsync(ct),
            logger,
            "Get All Active Coupons"
        );
    }

    public async Task<bool> IsCodeUsedByUserAsync(string code, long? userId, CancellationToken ct = default)
    {
        Guard.AgainstNullOrWhiteSpace(code, nameof(code));
        if (!userId.HasValue || userId <= 0) return false;

        return await ExecutionHelper.ExecuteAsync(
            () => _context.Orders
                .AnyAsync(o =>
                    o.CouponCode == code &&
                    o.UserId == userId &&
                    o.OrderStatusId == (int)OrderStatus.Delivered,
                    ct),
            logger,
            "Check Coupon Used by User",
            new { Code = code, UserId = userId }
        );
    }

    public async Task<bool> IsCodeUsedBySessionAsync(string code, Guid sessionId, CancellationToken ct = default)
    {
        Guard.AgainstNullOrWhiteSpace(code, nameof(code));
        Guard.AgainstEmpty(sessionId, nameof(sessionId));

        return await ExecutionHelper.ExecuteAsync(
            () => _context.OrderItems
                .AnyAsync(oi =>
                    oi.Order.CouponCode == code &&
                    oi.SessionId == sessionId &&
                    oi.Order.OrderStatusId == (int)OrderStatus.Delivered,
                    ct),
            logger,
            "Check Coupon Used by Session",
            new { Code = code, SessionId = sessionId }
        );
    }

    public async Task<Coupon?> ValidateCouponAsync(
        string code,
        decimal orderTotal,
        long? userId,
        Guid sessionId,
        CancellationToken ct = default)
    {
        Guard.AgainstNullOrWhiteSpace(code, nameof(code));
        Guard.AgainstNegativeOrZero(orderTotal, nameof(orderTotal));
        Guard.AgainstEmpty(sessionId, nameof(sessionId));

        var coupon = await GetActiveByCodeAsync(code, ct);
        if (coupon == null) return null;

        // Minimum order amount
        if (coupon.MinimumOrderAmount.HasValue && orderTotal < coupon.MinimumOrderAmount.Value)
            return null;

        // Per-user or per-session usage
        if (userId.HasValue && userId > 0)
        {
            if (await IsCodeUsedByUserAsync(code, userId, ct))
                return null;
        }
        else
        {
            if (await IsCodeUsedBySessionAsync(code, sessionId, ct))
                return null;
        }

        return coupon;
    }

    public async Task IncrementUsageAsync(long couponId, CancellationToken ct = default)
    {
        Guard.AgainstNegativeOrZero(couponId, nameof(couponId));

        await ExecutionHelper.ExecuteAsync(
            async () =>
            {
                var coupon = await _context.Coupons
                    .FirstOrDefaultAsync(c => c.CouponId == couponId, ct);

                if (coupon != null)
                {
                    coupon.UsedCount++;
                    await _context.SaveChangesAsync(ct);
                }
            },
            logger,
            "Increment Coupon Usage",
            new { CouponId = couponId }
        );
    }
}