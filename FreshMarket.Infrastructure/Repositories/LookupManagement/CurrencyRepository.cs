using FreshMarket.Domain.Entities.LookupManagement;
using FreshMarket.Domain.Interfaces.Repositories.LookupManagement;
using FreshMarket.Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace FreshMarket.Infrastructure.Repositories.LookupManagement;

public class CurrencyRepository(
    FreshMarketDbContext context,
    ILogger<CurrencyRepository> logger)
    : Repository<Currency>(context, logger), ICurrency
{
}
