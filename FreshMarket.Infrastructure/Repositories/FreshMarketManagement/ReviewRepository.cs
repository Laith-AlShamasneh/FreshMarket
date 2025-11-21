using FreshMarket.Domain.Entities.FreshMarketManagement;
using FreshMarket.Domain.Interfaces.Repositories.FreshMarketManagement;
using FreshMarket.Infrastructure.Data;
using FreshMarket.Shared.Common;
using FreshMarket.Shared.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FreshMarket.Infrastructure.Repositories.FreshMarketManagement;

public class ReviewRepository(
    FreshMarketDbContext context,
    ILogger<ReviewRepository> logger,
    IUserContext userContext)
    : Repository<Review>(context, logger), IReview
{
    private readonly FreshMarketDbContext _context = context;
    private readonly IUserContext _userContext = userContext;

    public async Task<IReadOnlyList<Review>> GetByProductIdAsync(long productId, CancellationToken ct = default)
    {
        Guard.AgainstNegativeOrZero(productId, nameof(productId));

        return await ExecutionHelper.ExecuteAsync(
            () => _context.Reviews
                .AsNoTracking()
                .Where(r => r.ProductId == productId)
                .Include(r => r.User)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync(ct),
            logger,
            "Get Reviews by ProductId",
            new { ProductId = productId }
        );
    }

    public async Task<IReadOnlyList<Review>> GetApprovedByProductIdAsync(long productId, CancellationToken ct = default)
    {
        Guard.AgainstNegativeOrZero(productId, nameof(productId));

        return await ExecutionHelper.ExecuteAsync(
            () => _context.Reviews
                .AsNoTracking()
                .Where(r => r.ProductId == productId && r.IsApproved)
                .Include(r => r.User)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync(ct),
            logger,
            "Get Approved Reviews by ProductId",
            new { ProductId = productId }
        );
    }

    public async Task<IPaginatedList<Review>> GetApprovedPaginatedAsync(long productId, PaginationRequest request, CancellationToken ct = default)
    {
        Guard.AgainstNegativeOrZero(productId, nameof(productId));

        var query = _context.Reviews
            .AsNoTracking()
            .Where(r => r.ProductId == productId && r.IsApproved)
            .Include(r => r.User);

        return await ExecutionHelper.ExecuteAsync(
            () => query
                .OrderByDescending(r => r.CreatedAt)
                .ToPaginatedListAsync(request, ct),
            logger,
            "Get Paginated Approved Reviews",
            new { ProductId = productId, Request = request }
        );
    }

    public async Task<IReadOnlyList<Review>> GetVerifiedPurchaseReviewsAsync(long productId, CancellationToken ct = default)
    {
        Guard.AgainstNegativeOrZero(productId, nameof(productId));

        return await ExecutionHelper.ExecuteAsync(
            () => _context.Reviews
                .AsNoTracking()
                .Where(r => r.ProductId == productId && r.IsApproved && r.IsVerifiedPurchase)
                .Include(r => r.User)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync(ct),
            logger,
            "Get Verified Purchase Reviews",
            new { ProductId = productId }
        );
    }

    public async Task<double> GetAverageRatingAsync(long productId, CancellationToken ct = default)
    {
        Guard.AgainstNegativeOrZero(productId, nameof(productId));

        var avg = await ExecutionHelper.ExecuteAsync(
            () => _context.Reviews
                .Where(r => r.ProductId == productId && r.IsApproved)
                .AverageAsync(r => (double?)r.Rating),
            logger,
            "Get Average Rating",
            new { ProductId = productId }
        );

        return avg ?? 0.0;
    }

    public async Task<bool> HasUserReviewedAsync(long productId, long userId, CancellationToken ct = default)
    {
        Guard.AgainstNegativeOrZero(productId, nameof(productId));
        Guard.AgainstNegativeOrZero(userId, nameof(userId));

        return await ExecutionHelper.ExecuteAsync(
            () => _context.Reviews
                .AnyAsync(r => r.ProductId == productId && r.UserId == userId, ct),
            logger,
            "Check if User Reviewed",
            new { ProductId = productId, UserId = userId }
        );
    }

    public async Task ApproveReviewAsync(long reviewId, long approvedBy, CancellationToken ct = default)
    {
        Guard.AgainstNegativeOrZero(reviewId, nameof(reviewId));
        Guard.AgainstNegativeOrZero(approvedBy, nameof(approvedBy));

        await ExecutionHelper.ExecuteAsync(
            async () =>
            {
                var review = await _context.Reviews
                    .FirstOrDefaultAsync(r => r.ReviewId == reviewId, ct);

                if (review != null)
                {
                    review.IsApproved = true;
                    review.ApprovedBy = approvedBy;
                    review.ApprovedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync(ct);
                }
            },
            logger,
            "Approve Review",
            new { ReviewId = reviewId, ApprovedBy = approvedBy }
        );
    }

    public async Task MarkAsVerifiedPurchaseAsync(long reviewId, CancellationToken ct = default)
    {
        Guard.AgainstNegativeOrZero(reviewId, nameof(reviewId));

        await ExecutionHelper.ExecuteAsync(
            async () =>
            {
                var review = await _context.Reviews
                    .FirstOrDefaultAsync(r => r.ReviewId == reviewId, ct);

                if (review != null)
                {
                    review.IsVerifiedPurchase = true;
                    await _context.SaveChangesAsync(ct);
                }
            },
            logger,
            "Mark Review as Verified Purchase",
            new { ReviewId = reviewId }
        );
    }
}
