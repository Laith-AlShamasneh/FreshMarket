namespace FreshMarket.Domain.Interfaces.Repositories;

/// <summary>
/// Defines a generic repository contract for performing CRUD and query operations on entities of type T.
/// </summary>
public interface IRepository<T> where T : class
{
    /// <summary>Get entity by ID</summary>
    Task<T?> GetByIdAsync(object id, CancellationToken ct = default);

    /// <summary>Get all entities</summary>
    Task<IReadOnlyList<T>> ListAllAsync(CancellationToken ct = default);

    /// <summary>Get entities matching specification</summary>
    Task<IReadOnlyList<T>> ListAsync(ISpecification<T> spec, CancellationToken ct = default);

    /// <summary>Get first entity matching specification</summary>
    Task<T?> FirstOrDefaultAsync(ISpecification<T> spec, CancellationToken ct = default);

    /// <summary>Add new entity</summary>
    Task<T> AddAsync(T entity, CancellationToken ct = default);

    /// <summary>Add multiple entities</summary>
    Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default);

    /// <summary>Update existing entity</summary>
    void Update(T entity);

    /// <summary>Delete entity</summary>
    void Delete(T entity);

    /// <summary>Get count with optional filter</summary>
    Task<int> CountAsync(ISpecification<T>? spec = null, CancellationToken ct = default);
}
