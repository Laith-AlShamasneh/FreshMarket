namespace FreshMarket.Domain.Interfaces.Repositories;

/// <summary>
/// Defines a generic repository contract for performing CRUD and query operations on entities of type T.
/// </summary>
public interface IRepository<T> where T : class
{
    /// <summary>
    /// Retrieves an entity by its unique identifier.
    /// </summary>
    Task<T?> GetByIdAsync(
        object id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all entities of type T.
    /// </summary>
    Task<IReadOnlyList<T>> ListAllAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a list of entities that match the provided specification.
    /// </summary>
    Task<IReadOnlyList<T>> ListAsync(
        ISpecification<T> specification,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the first entity that matches the specification, or null if none.
    /// </summary>
    Task<T?> FirstOrDefaultAsync(
        ISpecification<T> specification,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new entity to the repository.
    /// </summary>
    Task<T> AddAsync(
        T entity,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds multiple entities to the repository.
    /// </summary>
    Task AddRangeAsync(
        IEnumerable<T> entities,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks an existing entity for update in the current unit of work.
    /// </summary>
    void Update(T entity);

    /// <summary>
    /// Marks an entity for deletion in the current unit of work.
    /// </summary>
    void Delete(T entity);

    /// <summary>
    /// Returns the total count of entities, optionally filtered by a specification.
    /// </summary>
    Task<int> CountAsync(
        ISpecification<T>? specification = null,
        CancellationToken cancellationToken = default);
}
