using FreshMarket.Domain.Entities.FreshMarketManagement;

namespace FreshMarket.Domain.Interfaces.Repositories.FreshMarketManagement;

public interface ICoupon : IRepository<Coupon>
{
    Task<Coupon?> GetActiveByCodeAsync(string code, CancellationToken ct = default);
    Task<bool> IsCodeUsedByUserAsync(string code, long? userId, CancellationToken ct = default);
    Task<bool> IsCodeUsedBySessionAsync(string code, Guid sessionId, CancellationToken ct = default);
    Task IncrementUsageAsync(long couponId, CancellationToken ct = default);
}
