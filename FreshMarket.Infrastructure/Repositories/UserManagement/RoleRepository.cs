using FreshMarket.Domain.Entities.UserManagement;
using FreshMarket.Domain.Interfaces.Repositories.UserManagement;
using FreshMarket.Infrastructure.Data;
using FreshMarket.Shared.Common;
using FreshMarket.Shared.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FreshMarket.Infrastructure.Repositories.UserManagement;

public class RoleRepository(
    FreshMarketDbContext context,
    ILogger<RoleRepository> logger,
    IUserContext userContext)
    : Repository<Role>(context, logger), IRole
{
    private readonly FreshMarketDbContext _context = context;
    private readonly IUserContext _userContext = userContext;

    private bool IsArabic => _userContext.LangId == (int)Lang.Ar;

    public async Task<Role?> GetByNameAsync(string name, CancellationToken ct = default)
    {
        Guard.AgainstNullOrWhiteSpace(name, nameof(name));
        var search = name.Trim();

        return await ExecutionHelper.ExecuteAsync(
            () => _context.Roles
                .AsNoTracking()
                .FirstOrDefaultAsync(r =>
                    (IsArabic && r.NameAr == search) ||
                    (!IsArabic && r.NameEn == search), ct),
            logger,
            "Get Role by Localized Name",
            new { Name = search, LangId = _userContext.LangId }
        );
    }

    public async Task<Role?> GetDefaultRoleAsync(CancellationToken ct = default)
    {
        return await ExecutionHelper.ExecuteAsync(
            () => _context.Roles
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.IsDefault, ct),
            logger,
            "Get Default Role"
        );
    }

    public async Task<IReadOnlyList<Role>> GetByUserIdAsync(long userId, CancellationToken ct = default)
    {
        Guard.AgainstNegativeOrZero(userId, nameof(userId));

        return await ExecutionHelper.ExecuteAsync(
            () => _context.Roles
                .AsNoTracking()
                .Where(r => r.UserRoles.Any(ur => ur.UserId == userId))
                .OrderBy(r => IsArabic ? r.NameAr : r.NameEn)
                .ToListAsync(ct),
            logger,
            "Get Roles by UserId (Localized)",
            new { UserId = userId, LangId = _userContext.LangId }
        );
    }

    public async Task<IReadOnlyList<Role>> GetAllAsync(CancellationToken ct = default)
    {
        return await ExecutionHelper.ExecuteAsync(
            () => _context.Roles
                .AsNoTracking()
                .Include(r => r.UserRoles)
                .ThenInclude(ur => ur.User)
                .ThenInclude(u => u.Person)
                .OrderBy(r => IsArabic ? r.NameAr : r.NameEn)
                .ToListAsync(ct),
            logger,
            "Get All Roles (Localized)",
            new { LangId = _userContext.LangId }
        );
    }
}
