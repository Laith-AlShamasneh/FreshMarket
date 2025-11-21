using FreshMarket.Domain.Entities.FreshMarketManagement;

namespace FreshMarket.Domain.Interfaces.Repositories.FreshMarketManagement;

public interface ICoupon : IRepository<Coupon>
{
    Task<Coupon?> GetActiveByCodeAsync(string code, CancellationToken ct = default);
    Task<IReadOnlyList<Coupon>> GetActiveAsync(CancellationToken ct = default);
}
