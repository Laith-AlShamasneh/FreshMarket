using FreshMarket.Domain.Entities.LookupManagement;
using FreshMarket.Domain.Interfaces.Repositories.LookupManagement;
using FreshMarket.Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace FreshMarket.Infrastructure.Repositories.LookupManagement;

public class PaymentMethodTypeRepository(
    FreshMarketDbContext context,
    ILogger<PaymentMethodTypeRepository> logger)
    : Repository<PaymentMethodType>(context, logger), IPaymentMethodType
{
}
