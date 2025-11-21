using FreshMarket.Domain.Entities.UserManagement;
using FreshMarket.Shared.Common;

namespace FreshMarket.Domain.Interfaces.Repositories.UserManagement;

public interface ISignInLog : IRepository<SignInLog>
{
    Task<IReadOnlyList<SignInLog>> GetByUserIdAsync(long? userId, CancellationToken ct = default);
    Task<IReadOnlyList<SignInLog>> GetRecentAsync(int top = 100, CancellationToken ct = default);
    Task<IReadOnlyList<SignInLog>> GetFailedAttemptsAsync(long? userId, int lastHours = 24, CancellationToken ct = default);
    Task<IReadOnlyList<SignInLog>> GetRecentByIpAsync(string ipAddress, int top = 50, CancellationToken ct = default);
    Task<int> GetFailedStreakAsync(long? userId, CancellationToken ct = default);
    Task LogAttemptAsync(long? userId, string? ipAddress, bool isSuccessful, LoginFailureReason? failureReason = null, CancellationToken ct = default);
}
