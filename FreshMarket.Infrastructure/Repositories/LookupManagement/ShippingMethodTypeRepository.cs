using FreshMarket.Domain.Entities.LookupManagement;
using FreshMarket.Domain.Interfaces.Repositories.LookupManagement;
using FreshMarket.Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace FreshMarket.Infrastructure.Repositories.LookupManagement;

public class ShippingMethodTypeRepository(
    FreshMarketDbContext context,
    ILogger<ShippingMethodTypeRepository> logger)
    : Repository<ShippingMethodType>(context, logger), IShippingMethodType
{
}
