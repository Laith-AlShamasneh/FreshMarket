using FreshMarket.Domain.Entities.FreshMarketManagement;

namespace FreshMarket.Domain.Interfaces.Repositories.FreshMarketManagement;

public interface IPaymentTransaction : IRepository<PaymentTransaction>
{
    Task<IReadOnlyList<PaymentTransaction>> GetByOrderIdAsync(long orderId, CancellationToken ct = default);
    Task<IReadOnlyList<PaymentTransaction>> GetBySessionIdAsync(Guid sessionId, CancellationToken ct = default);
    Task<PaymentTransaction?> GetLatestByOrderIdAsync(long orderId, CancellationToken ct = default);
    Task<PaymentTransaction?> GetLatestBySessionIdAsync(Guid sessionId, CancellationToken ct = default);
    Task<IReadOnlyList<PaymentTransaction>> GetByStatusAsync(int paymentStatusId, CancellationToken ct = default);
}
