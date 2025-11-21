using FreshMarket.Domain.Entities.LookupManagement;
using FreshMarket.Domain.Interfaces.Repositories.LookupManagement;
using FreshMarket.Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace FreshMarket.Infrastructure.Repositories.LookupManagement;

public class PaymentStatusRepository(
    FreshMarketDbContext context,
    ILogger<PaymentStatusRepository> logger)
    : Repository<PaymentStatus>(context, logger), IPaymentStatus
{
}
