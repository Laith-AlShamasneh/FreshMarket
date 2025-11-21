using FreshMarket.Domain.Entities.FreshMarketManagement;

namespace FreshMarket.Domain.Interfaces.Repositories.FreshMarketManagement;

public interface IWishlist : IRepository<Wishlist>
{
    Task<Wishlist?> GetByUserIdAsync(long userId, CancellationToken ct = default);
    Task<Wishlist?> GetWithItemsByUserIdAsync(long userId, CancellationToken ct = default);
    Task AddItemAsync(long userId, long productVariantId, CancellationToken ct = default);
    Task RemoveItemAsync(long userId, long productVariantId, CancellationToken ct = default);
    Task MoveToCartAsync(long userId, long productVariantId, CancellationToken ct = default);
}
