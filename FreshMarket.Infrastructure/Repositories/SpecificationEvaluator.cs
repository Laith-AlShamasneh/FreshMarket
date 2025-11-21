using FreshMarket.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FreshMarket.Infrastructure.Repositories;

public static class SpecificationEvaluator<T> where T : class
{
    public static IQueryable<T> GetQuery(IQueryable<T> input, ISpecification<T>? spec)
    {
        var query = input;

        if (spec is null)
            return query;

        if (spec.Criteria is not null)
            query = query.Where(spec.Criteria);

        // Apply expression includes (if any)
        if (spec.Includes is not null && spec.Includes.Count > 0)
            query = spec.Includes.Aggregate(query, (current, include) => current.Include(include));

        // Apply string-based include paths (if any)
        if (spec.IncludeStrings is not null && spec.IncludeStrings.Count > 0)
            query = spec.IncludeStrings.Aggregate(query, (current, include) => current.Include(include));

        // Ordering - prefer OrderBy (ascending) when provided, otherwise OrderByDescending
        if (spec.OrderBy is not null)
        {
            query = query.OrderBy(spec.OrderBy);
            // Support additional ordering in future by exposing OrderByThen / OrderByDescendingThen in spec
        }
        else if (spec.OrderByDescending is not null)
        {
            query = query.OrderByDescending(spec.OrderByDescending);
        }

        // Paging
        if (spec.IsPagingEnabled)
            query = query.Skip(spec.Skip ?? 0).Take(spec.Take ?? int.MaxValue);

        return query;
    }
}
