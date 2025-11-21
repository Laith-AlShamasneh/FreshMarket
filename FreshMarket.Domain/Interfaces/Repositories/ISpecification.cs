using System.Linq.Expressions;

namespace FreshMarket.Domain.Interfaces.Repositories;

/// <summary>
/// Defines a specification pattern for querying entities with criteria, includes, sorting, and paging.
/// </summary>
public interface ISpecification<T>
{
    /// <summary>Filter criteria (WHERE clause)</summary>
    Expression<Func<T, bool>>? Criteria { get; }

    /// <summary>Related entities to include (eager loading)</summary>
    IReadOnlyList<Expression<Func<T, object>>> Includes { get; }

    /// <summary>String-based include paths for complex scenarios</summary>
    IReadOnlyList<string> IncludeStrings { get; }

    /// <summary>Sort ascending</summary>
    Expression<Func<T, object>>? OrderBy { get; }

    /// <summary>Sort descending</summary>
    Expression<Func<T, object>>? OrderByDescending { get; }

    /// <summary>Number of records to return (LIMIT)</summary>
    int? Take { get; }

    /// <summary>Number of records to skip (OFFSET)</summary>
    int? Skip { get; }

    /// <summary>Is paging enabled?</summary>
    bool IsPagingEnabled { get; }
}
