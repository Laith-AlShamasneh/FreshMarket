using FreshMarket.Domain.Entities.UserManagement;

namespace FreshMarket.Domain.Interfaces.Repositories.UserManagement;

public interface IUser : IRepository<User>
{
    Task UpdateLastLoginAsync(long userId, CancellationToken ct = default);
}
