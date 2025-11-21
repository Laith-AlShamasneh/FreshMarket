using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace FreshMarket.Shared.Common;

public static class IQueryableExtensions
{
    public static async Task<IPaginatedList<T>> ToPaginatedListAsync<T>(
        this IQueryable<T> source,
        PaginationRequest request,
        CancellationToken ct = default) where T : class
    {
        if (!string.IsNullOrWhiteSpace(request.SearchValue))
        {
            var search = request.SearchValue.Trim();
            source = source.Where(x =>
                EF.Functions.Like(EF.Property<string>(x, "Name") ?? "", $"%{search}%") ||
                EF.Functions.Like(EF.Property<string>(x, "Description") ?? "", $"%{search}%"));
        }

        var count = await source.CountAsync(ct);

        if (!string.IsNullOrWhiteSpace(request.SortBy))
        {
            var direction = request.SortDirection == SortDirection.Ascending ? "ASC" : "DESC";
            source = source.OrderBy($"{request.SortBy} {direction}");
        }

        var items = await source.Skip(request.Skip).Take(request.Take).ToListAsync(ct);

        return new PaginatedList<T>(items, request.PageNumber, request.PageSize, count);
    }
}
