using FreshMarket.Domain.Entities.UserManagement;
using FreshMarket.Domain.Interfaces.Repositories.UserManagement;
using FreshMarket.Infrastructure.Data;
using FreshMarket.Shared.Common;
using FreshMarket.Shared.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FreshMarket.Infrastructure.Repositories.UserManagement;

public class SignInLogRepository(
    FreshMarketDbContext context,
    ILogger<SignInLogRepository> logger)
    : Repository<SignInLog>(context, logger), ISignInLog
{
    private readonly FreshMarketDbContext _context = context;

    public async Task<IReadOnlyList<SignInLog>> GetByUserIdAsync(long? userId, CancellationToken ct = default)
    {
        if (!userId.HasValue || userId <= 0) return Array.Empty<SignInLog>();

        return await ExecutionHelper.ExecuteAsync(
            () => _context.SignInLogs
                .AsNoTracking()
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.AttemptedAt)
                .ToListAsync(ct),
            logger,
            "Get SignIn Logs by UserId",
            new { UserId = userId }
        );
    }

    public async Task<IReadOnlyList<SignInLog>> GetRecentAsync(int top = 100, CancellationToken ct = default)
    {
        Guard.AgainstNegative(top, nameof(top));

        return await ExecutionHelper.ExecuteAsync(
            () => _context.SignInLogs
                .AsNoTracking()
                .OrderByDescending(s => s.AttemptedAt)
                .Take(top)
                .ToListAsync(ct),
            logger,
            "Get Recent SignIn Logs",
            new { Top = top }
        );
    }

    public async Task<IReadOnlyList<SignInLog>> GetFailedAttemptsAsync(long? userId, int lastHours = 24, CancellationToken ct = default)
    {
        if (!userId.HasValue || userId <= 0) return Array.Empty<SignInLog>();
        Guard.AgainstNegative(lastHours, nameof(lastHours));

        var cutoff = DateTime.UtcNow.AddHours(-lastHours);

        return await ExecutionHelper.ExecuteAsync(
            () => _context.SignInLogs
                .AsNoTracking()
                .Where(s => s.UserId == userId && !s.IsSuccessful && s.AttemptedAt >= cutoff)
                .OrderByDescending(s => s.AttemptedAt)
                .ToListAsync(ct),
            logger,
            "Get Failed Attempts",
            new { UserId = userId, LastHours = lastHours }
        );
    }

    public async Task<IReadOnlyList<SignInLog>> GetRecentByIpAsync(string ipAddress, int top = 50, CancellationToken ct = default)
    {
        Guard.AgainstNullOrWhiteSpace(ipAddress, nameof(ipAddress));
        Guard.AgainstNegative(top, nameof(top));

        return await ExecutionHelper.ExecuteAsync(
            () => _context.SignInLogs
                .AsNoTracking()
                .Where(s => s.IpAddress == ipAddress)
                .OrderByDescending(s => s.AttemptedAt)
                .Take(top)
                .ToListAsync(ct),
            logger,
            "Get Recent Logs by IP",
            new { IpAddress = ipAddress, Top = top }
        );
    }

    public async Task<int> GetFailedStreakAsync(long? userId, CancellationToken ct = default)
    {
        if (!userId.HasValue || userId <= 0) return 0;

        return await ExecutionHelper.ExecuteAsync(
            () => _context.SignInLogs
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.AttemptedAt)
                .Select((s, index) => new { s.IsSuccessful, Index = index })
                .TakeWhile(x => !x.IsSuccessful)
                .CountAsync(ct),
            logger,
            "Get Failed Streak",
            new { UserId = userId }
        );
    }

    public async Task LogAttemptAsync(
        long? userId,
        string? ipAddress,
        bool isSuccessful,
        LoginFailureReason? failureReason = null,
        CancellationToken ct = default)
    {
        if (ipAddress is not null) Guard.AgainstNullOrWhiteSpace(ipAddress, nameof(ipAddress));

        await ExecutionHelper.ExecuteAsync(
            async () =>
            {
                var log = new SignInLog
                {
                    UserId = userId,
                    IpAddress = ipAddress?.Trim(),
                    IsSuccessful = isSuccessful,
                    FailureReason = isSuccessful ? null : failureReason,
                    AttemptedAt = DateTime.UtcNow
                };

                _context.SignInLogs.Add(log);
                await _context.SaveChangesAsync(ct);
            },
            logger,
            "Log SignIn Attempt",
            new
            {
                UserId = userId,
                IpAddress = ipAddress,
                IsSuccessful = isSuccessful,
                FailureReason = failureReason
            }
        );
    }
}
