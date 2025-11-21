namespace FreshMarket.Shared.Common;

public record PaginationRequest(
    int PageNumber = 1,
    int PageSize = 10,
    string? SearchValue = null,
    string? SortBy = null,
    SortDirection SortDirection = SortDirection.Ascending)
{
    public int Skip => (PageNumber - 1) * PageSize;
    public int Take => PageSize;
}

public interface IPaginatedList<T>
{
    IReadOnlyList<T> Items { get; }
    int PageNumber { get; }
    int PageSize { get; }
    int TotalCount { get; }
    int TotalPages { get; }
    bool HasPreviousPage { get; }
    bool HasNextPage { get; }
}

public record PaginatedList<T>(
    IReadOnlyList<T> Items,
    int PageNumber,
    int PageSize,
    int TotalCount) : IPaginatedList<T>
{
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}
