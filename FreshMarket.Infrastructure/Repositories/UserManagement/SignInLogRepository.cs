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

    public async Task LogAttemptAsync(
        long? userId,
        string? ipAddress,
        bool isSuccessful,
        LoginFailureReason? failureReason = null,
        CancellationToken ct = default)
    {
        if (!userId.HasValue || userId <= 0) return;

        var log = new SignInLog
        {
            UserId = userId,
            IpAddress = ipAddress,
            IsSuccessful = isSuccessful,
            FailureReason = failureReason,
            AttemptedAt = DateTime.UtcNow
        };

        await ExecutionHelper.ExecuteAsync(
            async () =>
            {
                await _context.SignInLogs.AddAsync(log, ct);
                await _context.SaveChangesAsync(ct);
            },
            logger,
            "Log SignIn Attempt",
            new { UserId = userId, IsSuccessful = isSuccessful }
        );
    }
}
