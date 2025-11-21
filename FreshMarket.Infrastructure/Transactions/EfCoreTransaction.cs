using FreshMarket.Domain.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace FreshMarket.Infrastructure.Transactions;

public sealed class EfCoreTransaction(IDbContextTransaction inner, ILogger logger) : ITransaction
{
    private readonly IDbContextTransaction _inner = inner ?? throw new ArgumentNullException(nameof(inner));
    private readonly ILogger _logger = logger;
    private bool _committedOrRolledBack;

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        if (_committedOrRolledBack) return;
        await _inner.CommitAsync(cancellationToken);
        _committedOrRolledBack = true;
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        if (_committedOrRolledBack) return;
        await _inner.RollbackAsync(cancellationToken);
        _committedOrRolledBack = true;
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            await _inner.DisposeAsync();
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Error disposing EF transaction");
        }
    }
}
