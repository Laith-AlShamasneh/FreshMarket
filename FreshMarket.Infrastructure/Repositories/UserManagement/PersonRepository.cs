using FreshMarket.Domain.Entities.UserManagement;
using FreshMarket.Domain.Interfaces.Repositories.UserManagement;
using FreshMarket.Infrastructure.Data;
using FreshMarket.Shared.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FreshMarket.Infrastructure.Repositories.UserManagement;

public class PersonRepository(
    FreshMarketDbContext context,
    ILogger<PersonRepository> logger)
    : Repository<Person>(context, logger), IPerson
{
    private readonly FreshMarketDbContext _context = context;

    public async Task<Person?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        Guard.AgainstNullOrWhiteSpace(email, nameof(email));

        return await ExecutionHelper.ExecuteAsync(
            () => _context.Persons
                .AsNoTracking()
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Email == email, ct),
            logger,
            "Get Person by Email",
            new { Email = email }
        );
    }

    public async Task<Person?> GetByUserIdAsync(long userId, CancellationToken ct = default)
    {
        Guard.AgainstNegativeOrZero(userId, nameof(userId));

        return await ExecutionHelper.ExecuteAsync(
            () => _context.Persons
                .AsNoTracking()
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.User!.UserId == userId, ct),
            logger,
            "Get Person by UserId",
            new { UserId = userId }
        );
    }

    public async Task<Person?> GetByPhoneNumberAsync(string phoneNumber, CancellationToken ct = default)
    {
        Guard.AgainstNullOrWhiteSpace(phoneNumber, nameof(phoneNumber));

        return await ExecutionHelper.ExecuteAsync(
            () => _context.Persons
                .AsNoTracking()
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.PhoneNumber == phoneNumber, ct),
            logger,
            "Get Person by PhoneNumber",
            new { PhoneNumber = phoneNumber }
        );
    }

    public async Task<IReadOnlyList<Person>> SearchByNameAsync(string query, CancellationToken ct = default)
    {
        Guard.AgainstNullOrWhiteSpace(query, nameof(query));
        var search = query.Trim();

        return await ExecutionHelper.ExecuteAsync(
            () => _context.Persons
                .AsNoTracking()
                .Include(p => p.User)
                .Where(p =>
                    EF.Functions.Like(p.FirstName, $"%{search}%") ||
                    EF.Functions.Like(p.LastName, $"%{search}%") ||
                    EF.Functions.Like(p.FirstName + " " + p.LastName, $"%{search}%"))
                .OrderBy(p => p.FirstName)
                .ThenBy(p => p.LastName)
                .ToListAsync(ct),
            logger,
            "Search Persons by Name",
            new { Query = search }
        );
    }

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default)
    {
        Guard.AgainstNullOrWhiteSpace(email, nameof(email));

        return await ExecutionHelper.ExecuteAsync(
            () => _context.Persons
                .AnyAsync(p => p.Email == email, ct),
            logger,
            "Check Person exists by Email",
            new { Email = email }
        );
    }
}
