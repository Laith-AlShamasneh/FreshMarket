using FreshMarket.Domain.Entities.LookupManagement;
using FreshMarket.Domain.Interfaces.Repositories.LookupManagement;
using FreshMarket.Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace FreshMarket.Infrastructure.Repositories.LookupManagement;

public class OrderStatusRepository(
    FreshMarketDbContext context,
    ILogger<OrderStatusRepository> logger)
    : Repository<OrderStatus>(context, logger), IOrderStatus
{
}
