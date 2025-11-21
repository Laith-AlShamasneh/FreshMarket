using FreshMarket.Domain.Entities.LookupManagement;
using FreshMarket.Domain.Interfaces.Repositories.LookupManagement;
using FreshMarket.Infrastructure.Data;
using FreshMarket.Shared.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FreshMarket.Infrastructure.Repositories.LookupManagement;

public class CityRepository(
    FreshMarketDbContext context,
    ILogger<CityRepository> logger)
    : Repository<City>(context, logger), ICity
{
    private readonly FreshMarketDbContext _context = context;

    public async Task<IReadOnlyList<City>> GetByCountryIdAsync(int countryId, CancellationToken ct = default)
    {
        Guard.AgainstNegativeOrZero(countryId, nameof(countryId));

        return await ExecutionHelper.ExecuteAsync(
            () => _context.Cities
                .AsNoTracking()
                .Where(c => c.CountryId == countryId)
                .OrderBy(c => c.NameEn)
                .ToListAsync(ct),
            logger,
            "Get Cities by CountryId",
            new { CountryId = countryId }
        );
    }
}
