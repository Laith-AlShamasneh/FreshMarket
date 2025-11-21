using FreshMarket.Domain.Entities.FreshMarketManagement;

namespace FreshMarket.Domain.Interfaces.Repositories.FreshMarketManagement;

public interface ICartItem : IRepository<CartItem>
{
    Task<IReadOnlyList<CartItem>> GetByCartIdAsync(long cartId, CancellationToken ct = default);
    Task<IReadOnlyList<CartItem>> GetBySessionIdAsync(Guid sessionId, CancellationToken ct = default);
    Task<CartItem?> GetByCartIdAndProductVariantIdAsync(long cartId, long productVariantId, CancellationToken ct = default);
    Task<CartItem?> GetBySessionIdAndProductVariantIdAsync(Guid sessionId, long productVariantId, CancellationToken ct = default);
    Task<IReadOnlyList<CartItem>> GetWithDetailsByCartIdAsync(long cartId, CancellationToken ct = default);
    Task<CartItem> AddOrUpdateAsync(long cartId, long productVariantId, decimal quantity, CancellationToken ct = default);
    Task RemoveBySessionIdAndVariantIdAsync(Guid sessionId, long productVariantId, CancellationToken ct = default);
}
