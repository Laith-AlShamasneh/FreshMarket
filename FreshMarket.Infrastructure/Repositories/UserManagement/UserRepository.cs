using FreshMarket.Domain.Entities.UserManagement;
using FreshMarket.Domain.Interfaces.Repositories.UserManagement;
using FreshMarket.Infrastructure.Data;
using FreshMarket.Shared.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FreshMarket.Infrastructure.Repositories.UserManagement;

public class UserRepository(
    FreshMarketDbContext context,
    ILogger<UserRepository> logger)
    : Repository<User>(context, logger), IUser
{
    private readonly FreshMarketDbContext _context = context;

    public async Task UpdateLastLoginAsync(long userId, CancellationToken ct = default)
    {
        Guard.AgainstNegativeOrZero(userId, nameof(userId));

        await ExecutionHelper.ExecuteAsync(
            async () =>
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.UserId == userId, ct);

                if (user != null)
                {
                    user.LastLoginAt = DateTime.UtcNow;
                    _context.Users.Update(user);
                }
            },
            logger,
            "Update User Last Login",
            new { UserId = userId }
        );
    }
}