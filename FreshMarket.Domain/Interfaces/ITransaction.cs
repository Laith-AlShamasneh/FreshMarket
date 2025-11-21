namespace FreshMarket.Domain.Interfaces;

/// <summary>
/// Represents a database transaction that can be committed or rolled back.
/// </summary>
public interface ITransaction : IAsyncDisposable
{
    /// <summary>
    /// Commits the transaction.
    /// </summary>
    Task CommitAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Rolls back the transaction.
    /// </summary>
    Task RollbackAsync(CancellationToken cancellationToken = default);
}
