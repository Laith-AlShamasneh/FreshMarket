using FreshMarket.Domain.Entities.LookupManagement;
using FreshMarket.Domain.Interfaces.Repositories.LookupManagement;
using FreshMarket.Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace FreshMarket.Infrastructure.Repositories.LookupManagement;

public class UnitOfMeasureRepository(
    FreshMarketDbContext context,
    ILogger<UnitOfMeasureRepository> logger)
    : Repository<UnitOfMeasure>(context, logger), IUnitOfMeasure
{
}
