using FreshMarket.Domain.Entities.LookupManagement;

namespace FreshMarket.Domain.Interfaces.Repositories.LookupManagement;

public interface ICity : IRepository<City>
{
    Task<IReadOnlyList<City>> GetByCountryIdAsync(int countryId, CancellationToken ct = default);
}
