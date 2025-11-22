namespace FreshMarket.Shared.Common;

/// <summary>
/// Request parameters for paging, searching and sorting.
/// Includes normalization logic to keep values in safe bounds.
/// </summary>
public record PaginationRequest(
    int PageNumber = 1,
    int PageSize = 10,
    string? SearchValue = null,
    string? SortBy = null,
    SortDirection SortDirection = SortDirection.Ascending,
    bool IncludeTotalCount = true,   // Set false if you want to skip COUNT(*) for performance
    bool IgnorePaging = false)       // Set true for exports (returns all)
{
    public const int MinPageNumber = 1;
    public const int MinPageSize = 1;
    public const int MaxPageSize = 200;

    /// <summary>True if a non‑blank search string was provided.</summary>
    public bool IsSearching => !string.IsNullOrWhiteSpace(SearchValue);

    /// <summary>Sanitized page number (never < 1).</summary>
    public int SanitizedPageNumber => PageNumber < MinPageNumber ? MinPageNumber : PageNumber;

    /// <summary>Sanitized page size (clamped between MinPageSize and MaxPageSize).</summary>
    public int SanitizedPageSize =>
        PageSize < MinPageSize ? MinPageSize :
        PageSize > MaxPageSize ? MaxPageSize :
        PageSize;

    /// <summary>Number of items to skip. Zero if IgnorePaging is true.</summary>
    public int Skip => IgnorePaging ? 0 : (SanitizedPageNumber - 1) * SanitizedPageSize;

    /// <summary>Number of items to take. int.MaxValue if IgnorePaging is true.</summary>
    public int Take => IgnorePaging ? int.MaxValue : SanitizedPageSize;

    /// <summary>
    /// Returns a normalized copy (clamps PageNumber/PageSize).
    /// Call this before using Skip/Take if external input supplied.
    /// </summary>
    public PaginationRequest Normalize() =>
        this with
        {
            PageNumber = SanitizedPageNumber,
            PageSize = SanitizedPageSize
        };
}

/// <summary>
/// Abstraction returned from paginated queries.
/// </summary>
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

/// <summary>
/// Concrete implementation of IPaginatedList.
/// </summary>
public record PaginatedList<T>(
    IReadOnlyList<T> Items,
    int PageNumber,
    int PageSize,
    int TotalCount) : IPaginatedList<T>
{
    private int SafePageSize => PageSize <= 0 ? 1 : PageSize;

    public int TotalPages => TotalCount == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)SafePageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}