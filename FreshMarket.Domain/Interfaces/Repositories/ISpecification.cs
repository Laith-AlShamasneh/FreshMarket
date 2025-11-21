using System.Linq.Expressions;

namespace FreshMarket.Domain.Interfaces.Repositories;

/// <summary>
/// Defines a specification pattern for querying entities with criteria, includes, sorting, and paging.
/// </summary>
public interface ISpecification<T>
{
    /// <summary>
    /// The filtering criteria (WHERE clause) applied to the query.
    /// </summary>
    Expression<Func<T, bool>>? Criteria { get; }

    /// <summary>
    /// A list of related entities to include via navigation properties (eager loading).
    /// </summary>
    IReadOnlyList<Expression<Func<T, object>>> Includes { get; }

    /// <summary>
    /// A list of string-based include paths for complex or ThenInclude scenarios.
    /// </summary>
    IReadOnlyList<string> IncludeStrings { get; }

    /// <summary>
    /// The expression used to sort results in ascending order.
    /// </summary>
    Expression<Func<T, object>>? OrderBy { get; }

    /// <summary>
    /// The expression used to sort results in descending order.
    /// </summary>
    Expression<Func<T, object>>? OrderByDescending { get; }

    /// <summary>
    /// The maximum number of records to return (LIMIT).
    /// </summary>
    int? Take { get; }

    /// <summary>
    /// The number of records to skip before returning results (OFFSET).
    /// </summary>
    int? Skip { get; }

    /// <summary>
    /// Indicates whether paging (Take/Skip) is enabled in this specification.
    /// </summary>
    bool IsPagingEnabled { get; }
}
