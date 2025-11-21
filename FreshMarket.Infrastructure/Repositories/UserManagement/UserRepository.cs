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

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        Guard.AgainstNullOrWhiteSpace(email, nameof(email));

        return await ExecutionHelper.ExecuteAsync(
            () => _context.Users
                .AsNoTracking()
                .Include(u => u.Person)
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Person.Email == email.Trim(), ct),
            logger,
            "Get User by Email",
            new { Email = email }
        );
    }

    public async Task<User?> GetByPersonIdAsync(long personId, CancellationToken ct = default)
    {
        Guard.AgainstNegativeOrZero(personId, nameof(personId));

        return await ExecutionHelper.ExecuteAsync(
            () => _context.Users
                .AsNoTracking()
                .Include(u => u.Person)
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .Include(u => u.Addresses)
                .FirstOrDefaultAsync(u => u.PersonId == personId, ct),
            logger,
            "Get User by PersonId",
            new { PersonId = personId }
        );
    }

    public async Task<IReadOnlyList<User>> GetByRoleNameAsync(string roleNameEn, CancellationToken ct = default)
    {
        Guard.AgainstNullOrWhiteSpace(roleNameEn, nameof(roleNameEn));

        return await ExecutionHelper.ExecuteAsync(
            () => _context.Users
                .AsNoTracking()
                .Include(u => u.Person)
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .Where(u => u.UserRoles.Any(ur => ur.Role.NameEn == roleNameEn.Trim()))
                .OrderBy(u => u.Person.FirstName)
                .ThenBy(u => u.Person.LastName)
                .ToListAsync(ct),
            logger,
            "Get Users by Role Name",
            new { RoleNameEn = roleNameEn }
        );
    }

    public async Task<IReadOnlyList<User>> GetUsersWithRolesAsync(CancellationToken ct = default)
    {
        return await ExecutionHelper.ExecuteAsync(
            () => _context.Users
                .AsNoTracking()
                .Include(u => u.Person)
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .OrderBy(u => u.Person.FirstName)
                .ThenBy(u => u.Person.LastName)
                .ToListAsync(ct),
            logger,
            "Get All Users with Roles"
        );
    }

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
                    await _context.SaveChangesAsync(ct);
                }
            },
            logger,
            "Update Last Login",
            new { UserId = userId }
        );
    }

    public async Task ConfirmEmailAsync(long userId, CancellationToken ct = default)
    {
        Guard.AgainstNegativeOrZero(userId, nameof(userId));

        await ExecutionHelper.ExecuteAsync(
            async () =>
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.UserId == userId, ct);

                if (user != null)
                {
                    user.IsEmailConfirmed = true;
                    await _context.SaveChangesAsync(ct);
                }
            },
            logger,
            "Confirm User Email",
            new { UserId = userId }
        );
    }

    public async Task SetPasswordHashAsync(long userId, string passwordHash, CancellationToken ct = default)
    {
        Guard.AgainstNegativeOrZero(userId, nameof(userId));
        Guard.AgainstNullOrWhiteSpace(passwordHash, nameof(passwordHash));

        await ExecutionHelper.ExecuteAsync(
            async () =>
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.UserId == userId, ct);

                if (user != null)
                {
                    user.PasswordHash = passwordHash;
                    await _context.SaveChangesAsync(ct);
                }
            },
            logger,
            "Set User Password Hash",
            new { UserId = userId }
        );
    }
}