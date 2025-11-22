using FreshMarket.Domain.Entities.UserManagement;
using FreshMarket.Shared.Common;

namespace FreshMarket.Domain.Interfaces.Repositories.UserManagement;

public interface ISignInLog : IRepository<SignInLog>
{
    Task LogAttemptAsync(long? userId, string? ipAddress, bool isSuccessful, CancellationToken ct = default);
}
