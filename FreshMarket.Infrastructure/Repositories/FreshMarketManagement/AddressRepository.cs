using FreshMarket.Domain.Entities.FreshMarketManagement;
using FreshMarket.Domain.Interfaces.Repositories.FreshMarketManagement;
using FreshMarket.Infrastructure.Data;
using FreshMarket.Shared.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FreshMarket.Infrastructure.Repositories.FreshMarketManagement;

public class AddressRepository(
    FreshMarketDbContext context,
    ILogger<AddressRepository> logger)
    : Repository<Address>(context, logger), IAddress
{
    private readonly FreshMarketDbContext _context = context;

    public async Task<IReadOnlyList<Address>> GetByUserIdAsync(long? userId, CancellationToken ct = default)
    {
        if (userId <= 0) return [];

        return await ExecutionHelper.ExecuteAsync(
            () => _context.Addresses
                .AsNoTracking()
                .Where(a => a.UserId == userId)
                .Include(a => a.City)
                .ThenInclude(a => a.Country)
                .Include(a => a.User)
                .OrderBy(a => a.IsDefault ? 0 : a.IsDefault ? 1 : 2)
                .ThenBy(a => a.AddressId)
                .ToListAsync(ct),
            logger,
            "Get Addresses by UserId",
            new { UserId = userId }
        );
    }

    public async Task<Address?> GetDefaultBillingAsync(long? userId, CancellationToken ct = default)
    {
        if (userId <= 0) return null;

        return await ExecutionHelper.ExecuteAsync(
            () => _context.Addresses
                .AsNoTracking()
                .Include(a => a.City)
                .ThenInclude(a => a.Country)
                .FirstOrDefaultAsync(a => a.UserId == userId && a.IsDefault, ct),
            logger,
            "Get Default Billing Address",
            new { UserId = userId }
        );
    }

    public async Task<Address?> GetDefaultShippingAsync(long? userId, CancellationToken ct = default)
    {
        if (userId <= 0) return null;

        return await ExecutionHelper.ExecuteAsync(
            () => _context.Addresses
                .AsNoTracking()
                .Include(a => a.City)
                .FirstOrDefaultAsync(a => a.UserId == userId && a.IsDefault, ct),
            logger,
            "Get Default Shipping Address",
            new { UserId = userId }
        );
    }

    public async Task<Address?> GetByIdWithDetailsAsync(long addressId, CancellationToken ct = default)
    {
        Guard.AgainstNegativeOrZero(addressId, nameof(addressId));

        return await ExecutionHelper.ExecuteAsync(
            () => _context.Addresses
                .AsNoTracking()
                .Include(a => a.City)
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.AddressId == addressId, ct),
            logger,
            "Get Address by Id with Details",
            new { AddressId = addressId }
        );
    }
}