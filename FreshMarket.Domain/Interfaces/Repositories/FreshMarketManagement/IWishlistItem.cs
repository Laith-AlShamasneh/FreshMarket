using FreshMarket.Domain.Entities.FreshMarketManagement;

namespace FreshMarket.Domain.Interfaces.Repositories.FreshMarketManagement;

public interface IWishlistItem : IRepository<WishlistItem>
{
    Task<IReadOnlyList<WishlistItem>> GetByWishlistIdAsync(long wishlistId, CancellationToken ct = default);
    Task<IReadOnlyList<WishlistItem>> GetByUserIdAsync(long userId, CancellationToken ct = default);
    Task<IReadOnlyList<WishlistItem>> GetWithDetailsByUserIdAsync(long userId, CancellationToken ct = default);
    Task<WishlistItem?> GetByWishlistIdAndVariantIdAsync(long wishlistId, long productVariantId, CancellationToken ct = default);
    Task<bool> ExistsAsync(long userId, long productVariantId, CancellationToken ct = default);
}
