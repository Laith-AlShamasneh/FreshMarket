using FreshMarket.Domain.Entities.FreshMarketManagement;

namespace FreshMarket.Domain.Interfaces.Repositories.FreshMarketManagement;

public interface ICategory : IRepository<Category>
{
    Task<IReadOnlyList<Category>> GetActiveAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Category>> GetActiveLocalizedAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Category>> GetWithProductsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Category>> GetWithProductsLocalizedAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Category>> GetTreeAsync(CancellationToken ct = default);
    Task<Category?> GetBySlugAsync(string slug, CancellationToken ct = default);
    Task<IReadOnlyList<Category>> SearchAsync(string query, CancellationToken ct = default);
}
