using FreshMarket.Domain.Entities.UserManagement;

namespace FreshMarket.Domain.Interfaces.Repositories.UserManagement;

public interface IUser : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<User?> GetByPersonIdAsync(long personId, CancellationToken ct = default);
    Task<IReadOnlyList<User>> GetByRoleNameAsync(string roleNameEn, CancellationToken ct = default);
    Task<IReadOnlyList<User>> GetUsersWithRolesAsync(CancellationToken ct = default);
    Task UpdateLastLoginAsync(long userId, CancellationToken ct = default);
    Task ConfirmEmailAsync(long userId, CancellationToken ct = default);
    Task SetPasswordHashAsync(long userId, string passwordHash, CancellationToken ct = default);
}
