using FreshMarket.Domain.Entities.FreshMarketManagement;
using FreshMarket.Shared.Common;

namespace FreshMarket.Domain.Interfaces.Repositories.FreshMarketManagement;

public interface IReview : IRepository<Review>
{
    Task<IReadOnlyList<Review>> GetByProductIdAsync(long productId, CancellationToken ct = default);
    Task<IReadOnlyList<Review>> GetApprovedByProductIdAsync(long productId, CancellationToken ct = default);
}
