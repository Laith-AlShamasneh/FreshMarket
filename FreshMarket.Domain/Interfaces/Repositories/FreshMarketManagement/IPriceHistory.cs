using FreshMarket.Domain.Entities.FreshMarketManagement;

namespace FreshMarket.Domain.Interfaces.Repositories.FreshMarketManagement;

public interface IPriceHistory : IRepository<PriceHistory>
{
    Task<IReadOnlyList<PriceHistory>> GetByProductIdAsync(long productId, CancellationToken ct = default);
    Task<IReadOnlyList<PriceHistory>> GetByProductVariantIdAsync(long productVariantId, CancellationToken ct = default);
    Task<PriceHistory?> GetLatestByProductVariantIdAsync(long productVariantId, CancellationToken ct = default);
    Task<IReadOnlyList<PriceHistory>> GetPriceHistoryRangeAsync(
        long? productId,
        long? productVariantId,
        DateTime? from,
        DateTime? to,
        CancellationToken ct = default);
    Task<PriceHistory> LogPriceChangeAsync(
        long? productId,
        long? productVariantId,
        decimal oldPrice,
        decimal newPrice,
        int? currencyId = null,
        string? reason = null,
        string? notes = null,
        CancellationToken ct = default);
}
