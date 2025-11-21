using FreshMarket.Domain.Entities.FreshMarketManagement;
using FreshMarket.Shared.Common;

namespace FreshMarket.Domain.Interfaces.Repositories.FreshMarketManagement;

public interface IReview : IRepository<Review>
{
    Task<IReadOnlyList<Review>> GetByProductIdAsync(long productId, CancellationToken ct = default);
    Task<IReadOnlyList<Review>> GetApprovedByProductIdAsync(long productId, CancellationToken ct = default);
    Task<IPaginatedList<Review>> GetApprovedPaginatedAsync(long productId, PaginationRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<Review>> GetVerifiedPurchaseReviewsAsync(long productId, CancellationToken ct = default);
    Task<double> GetAverageRatingAsync(long productId, CancellationToken ct = default);
    Task<bool> HasUserReviewedAsync(long productId, long userId, CancellationToken ct = default);
    Task ApproveReviewAsync(long reviewId, long approvedBy, CancellationToken ct = default);
    Task MarkAsVerifiedPurchaseAsync(long reviewId, CancellationToken ct = default);
}
