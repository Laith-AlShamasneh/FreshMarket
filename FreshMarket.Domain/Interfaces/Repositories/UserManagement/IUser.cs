using FreshMarket.Domain.Entities.UserManagement;

namespace FreshMarket.Domain.Interfaces.Repositories.UserManagement;

public interface IUser : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<User?> GetByPersonIdAsync(long personId, CancellationToken ct = default);
    Task UpdateLastLoginAsync(long userId, CancellationToken ct = default);
}
