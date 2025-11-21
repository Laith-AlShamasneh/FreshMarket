using FreshMarket.Domain.Entities.UserManagement;
using FreshMarket.Domain.Interfaces.Repositories.UserManagement;
using FreshMarket.Infrastructure.Data;
using FreshMarket.Shared.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FreshMarket.Infrastructure.Repositories.UserManagement;

public class UserRoleRepository(
    FreshMarketDbContext context,
    ILogger<UserRoleRepository> logger)
    : Repository<UserRole>(context, logger), IUserRole
{
    private readonly FreshMarketDbContext _context = context;

    public async Task AssignRoleAsync(long userId, long roleId, CancellationToken ct = default)
    {
        Guard.AgainstNegativeOrZero(userId, nameof(userId));
        Guard.AgainstNegativeOrZero(roleId, nameof(roleId));

        await ExecutionHelper.ExecuteAsync(
            async () =>
            {
                if (!await _context.UserRoles.AnyAsync(ur => ur.UserId == userId && ur.RoleId == roleId, ct))
                {
                    var userRole = new UserRole
                    {
                        UserId = userId,
                        RoleId = roleId
                    };
                    _context.UserRoles.Add(userRole);
                    await _context.SaveChangesAsync(ct);
                }
            },
            logger,
            "Assign Role to User",
            new { UserId = userId, RoleId = roleId }
        );
    }

    public async Task RemoveRoleAsync(long userId, long roleId, CancellationToken ct = default)
    {
        Guard.AgainstNegativeOrZero(userId, nameof(userId));
        Guard.AgainstNegativeOrZero(roleId, nameof(roleId));

        await ExecutionHelper.ExecuteAsync(
            async () =>
            {
                var userRole = await _context.UserRoles
                    .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId, ct);

                if (userRole != null)
                {
                    _context.UserRoles.Remove(userRole);
                    await _context.SaveChangesAsync(ct);
                }
            },
            logger,
            "Remove Role from User",
            new { UserId = userId, RoleId = roleId }
        );
    }

    public async Task<bool> HasRoleAsync(long userId, long roleId, CancellationToken ct = default)
    {
        Guard.AgainstNegativeOrZero(userId, nameof(userId));
        Guard.AgainstNegativeOrZero(roleId, nameof(roleId));

        return await ExecutionHelper.ExecuteAsync(
            () => _context.UserRoles
                .AnyAsync(ur => ur.UserId == userId && ur.RoleId == roleId, ct),
            logger,
            "Check User Has Role",
            new { UserId = userId, RoleId = roleId }
        );
    }

    public async Task<IReadOnlyList<Role>> GetRolesByUserIdAsync(long userId, CancellationToken ct = default)
    {
        Guard.AgainstNegativeOrZero(userId, nameof(userId));

        return await ExecutionHelper.ExecuteAsync(
            () => _context.UserRoles
                .AsNoTracking()
                .Where(ur => ur.UserId == userId)
                .Include(ur => ur.Role)
                .Select(ur => ur.Role)
                .OrderBy(r => r.NameEn)
                .ToListAsync(ct),
            logger,
            "Get Roles by UserId",
            new { UserId = userId }
        );
    }

    public async Task<IReadOnlyList<User>> GetUsersByRoleIdAsync(long roleId, CancellationToken ct = default)
    {
        Guard.AgainstNegativeOrZero(roleId, nameof(roleId));

        return await ExecutionHelper.ExecuteAsync(
            () => _context.UserRoles
                .AsNoTracking()
                .Where(ur => ur.RoleId == roleId)
                .Include(ur => ur.User)
                .ThenInclude(u => u.Person)
                .Select(ur => ur.User)
                .OrderBy(u => u.Person.FirstName)
                .ThenBy(u => u.Person.LastName)
                .ToListAsync(ct),
            logger,
            "Get Users by RoleId",
            new { RoleId = roleId }
        );
    }

    public async Task RemoveAllRolesAsync(long userId, CancellationToken ct = default)
    {
        Guard.AgainstNegativeOrZero(userId, nameof(userId));

        await ExecutionHelper.ExecuteAsync(
            async () =>
            {
                var userRoles = await _context.UserRoles
                    .Where(ur => ur.UserId == userId)
                    .ToListAsync(ct);

                if (userRoles.Any())
                {
                    _context.UserRoles.RemoveRange(userRoles);
                    await _context.SaveChangesAsync(ct);
                }
            },
            logger,
            "Remove All Roles from User",
            new { UserId = userId }
        );
    }
}
