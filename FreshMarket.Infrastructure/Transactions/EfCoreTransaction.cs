using FreshMarket.Domain.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace FreshMarket.Infrastructure.Transactions;

public sealed class EfCoreTransaction(IDbContextTransaction transaction) : ITransaction
{
    public async Task CommitAsync(CancellationToken ct = default)
        => await transaction.CommitAsync(ct);

    public async Task RollbackAsync(CancellationToken ct = default)
        => await transaction.RollbackAsync(ct);

    public ValueTask DisposeAsync()
        => transaction.DisposeAsync();
}
