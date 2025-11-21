using FreshMarket.Domain.Entities.FreshMarketManagement;

namespace FreshMarket.Domain.Interfaces.Repositories.FreshMarketManagement;

public interface ICartItem : IRepository<CartItem>
{
    Task<CartItem?> GetByCartAndVariantAsync(long cartId, long productVariantId, CancellationToken ct = default);
}
