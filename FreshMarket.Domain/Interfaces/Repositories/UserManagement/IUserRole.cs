using FreshMarket.Domain.Entities.UserManagement;

namespace FreshMarket.Domain.Interfaces.Repositories.UserManagement;

public interface IUserRole : IRepository<UserRole>
{
    Task AssignRoleAsync(long userId, long roleId, CancellationToken ct = default);
    Task RemoveRoleAsync(long userId, long roleId, CancellationToken ct = default);
    Task<bool> HasRoleAsync(long userId, long roleId, CancellationToken ct = default);
    Task<IReadOnlyList<Role>> GetRolesByUserIdAsync(long userId, CancellationToken ct = default);
    Task<IReadOnlyList<User>> GetUsersByRoleIdAsync(long roleId, CancellationToken ct = default);
    Task RemoveAllRolesAsync(long userId, CancellationToken ct = default);
}
