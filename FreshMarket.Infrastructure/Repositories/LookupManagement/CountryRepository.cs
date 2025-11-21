using FreshMarket.Domain.Entities.LookupManagement;
using FreshMarket.Domain.Interfaces.Repositories.LookupManagement;
using FreshMarket.Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace FreshMarket.Infrastructure.Repositories.LookupManagement;

public class CountryRepository(
    FreshMarketDbContext context,
    ILogger<CountryRepository> logger)
    : Repository<Country>(context, logger), ICountry
{
}
