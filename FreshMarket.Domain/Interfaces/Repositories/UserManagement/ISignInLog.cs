using FreshMarket.Domain.Entities.UserManagement;
using FreshMarket.Shared.Common;

namespace FreshMarket.Domain.Interfaces.Repositories.UserManagement;

public interface ISignInLog : IRepository<SignInLog>
{
    Task<IReadOnlyList<SignInLog>> GetByUserIdAsync(long? userId, CancellationToken ct = default);
    Task<IReadOnlyList<SignInLog>> GetFailedAttemptsAsync(long? userId, int lastHours = 24, CancellationToken ct = default);
    Task LogAttemptAsync(long? userId, string? ipAddress, bool isSuccessful, LoginFailureReason? failureReason = null, CancellationToken ct = default);
}
