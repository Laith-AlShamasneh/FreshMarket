namespace FreshMarket.Domain.Interfaces;

/// <summary>
/// Represents a database transaction that can be committed or rolled back.
/// </summary>
public interface ITransaction : IAsyncDisposable
{
    /// <summary>Commit the transaction</summary>
    Task CommitAsync(CancellationToken ct = default);

    /// <summary>Rollback the transaction</summary>
    Task RollbackAsync(CancellationToken ct = default);
}
