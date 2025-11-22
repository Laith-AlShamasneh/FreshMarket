using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace FreshMarket.Shared.Common;

public static class IQueryableExtensions
{
    /// <summary>
    /// Applies search (optional), sorting (optional) and pagination to a queryable source,
    /// returning a paginated list abstraction.
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    /// <param name="source">Base query</param>
    /// <param name="request">Pagination, search and sorting parameters</param>
    /// <param name="searchSelectors">
    /// Optional list of string property names to search (e.g. new[] { "Name", "Description" }).
    /// Only used if request.SearchValue is provided.
    /// </param>
    public static async Task<IPaginatedList<T>> ToPaginatedListAsync<T>(
        this IQueryable<T> source,
        PaginationRequest request,
        IEnumerable<string>? searchSelectors = null,
        CancellationToken ct = default) where T : class
    {
        // Validate pagination
        var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize <= 0 ? 10 : request.PageSize;

        // Apply search if requested
        if (!string.IsNullOrWhiteSpace(request.SearchValue) && searchSelectors is not null)
        {
            var search = request.SearchValue.Trim();

            // Build a dynamic OR predicate: e.g. Name.Contains(search) OR Description.Contains(search)
            // Using EF.Functions.Like for database-side pattern matching.
            var predicates = searchSelectors
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Select(p => $"{p}.Contains(@0)")
                .ToList();

            if (predicates.Count > 0)
            {
                var fullPredicate = string.Join(" OR ", predicates);
                source = source.Where(fullPredicate, search);
            }
        }

        var totalCount = await source.CountAsync(ct);

        // Sorting
        if (!string.IsNullOrWhiteSpace(request.SortBy))
        {
            var direction = request.SortDirection == SortDirection.Descending ? "DESC" : "ASC";

            try
            {
                // Use dynamic ordering; if property name invalid it will throw => we catch & ignore
                source = source.OrderBy($"{request.SortBy} {direction}");
            }
            catch
            {
                // Invalid SortBy supplied; ignore sorting
            }
        }

        var skip = (pageNumber - 1) * pageSize;

        var items = pageSize == 0
            ? []
            : await source
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync(ct);

        return new PaginatedList<T>(items, pageNumber, pageSize, totalCount);
    }
}
