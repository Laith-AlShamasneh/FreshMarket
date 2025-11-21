using FreshMarket.Domain.Entities.UserManagement;

namespace FreshMarket.Domain.Interfaces.Repositories.UserManagement;

public interface IRole : IRepository<Role>
{
    Task<Role?> GetByNameAsync(string nameEn, CancellationToken ct = default);
    Task<Role?> GetDefaultRoleAsync(CancellationToken ct = default);
}