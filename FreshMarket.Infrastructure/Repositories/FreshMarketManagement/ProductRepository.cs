using FreshMarket.Domain.Entities.FreshMarketManagement;
using FreshMarket.Domain.Interfaces.Repositories.FreshMarketManagement;
using FreshMarket.Infrastructure.Data;
using FreshMarket.Shared.Common;
using FreshMarket.Shared.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FreshMarket.Infrastructure.Repositories.FreshMarketManagement;

public class ProductRepository(
    FreshMarketDbContext context,
    ILogger<ProductRepository> logger,
    IUserContext userContext)
    : Repository<Product>(context, logger), IProduct
{
    private readonly FreshMarketDbContext _context = context;
    private readonly IUserContext _userContext = userContext;

    public async Task<IReadOnlyList<Product>> GetActiveAsync(CancellationToken ct = default)
    {
        return await ExecutionHelper.ExecuteAsync(
            () => _context.Products
                .AsNoTracking()
                .Where(p => p.IsPublished)
                .OrderBy(p => p.NameEn)
                .ToListAsync(ct),
            logger,
            "Get Active Products"
        );
    }

    public async Task<IReadOnlyList<Product>> GetByCategoryIdAsync(long categoryId, CancellationToken ct = default)
    {
        Guard.AgainstNegativeOrZero(categoryId, nameof(categoryId));

        return await ExecutionHelper.ExecuteAsync(
            () => _context.Products
                .AsNoTracking()
                .Where(p => p.CategoryId == categoryId && p.IsPublished)
                .OrderBy(p => p.NameEn)
                .ToListAsync(ct),
            logger,
            "Get Products by CategoryId",
            new { CategoryId = categoryId }
        );
    }

    public async Task<IReadOnlyList<Product>> GetFeaturedAsync(CancellationToken ct = default)
    {
        return await ExecutionHelper.ExecuteAsync(
            () => _context.Products
                .AsNoTracking()
                .Where(p => p.IsPublished && p.IsFeatured)
                .OrderByDescending(p => p.AverageRating)
                .ThenBy(p => p.NameEn)
                .ToListAsync(ct),
            logger,
            "Get Featured Products"
        );
    }

    public async Task<IReadOnlyList<Product>> GetSimilarByCategoryAsync(long productId, int count = 5, CancellationToken ct = default)
    {
        Guard.AgainstNegativeOrZero(productId, nameof(productId));
        Guard.AgainstNegativeOrZero(count, nameof(count));

        return await ExecutionHelper.ExecuteAsync(
            () => _context.Products
                .AsNoTracking()
                .Where(p => p.ProductId != productId && p.IsPublished && p.CategoryId == _context.Products
                    .Where(pr => pr.ProductId == productId)
                    .Select(pr => pr.CategoryId)
                    .FirstOrDefault())
                .OrderByDescending(p => p.AverageRating)
                .Take(count)
                .ToListAsync(ct),
            logger,
            "Get Similar Products by Category",
            new { ProductId = productId, Count = count }
        );
    }

    public async Task<IReadOnlyList<Product>> GetByIdsAsync(long[] productIds, CancellationToken ct = default)
    {
        if (productIds == null || productIds.Length == 0)
        {
            return [];
        }

        return await ExecutionHelper.ExecuteAsync(
            () => _context.Products
                .AsNoTracking()
                .Where(p => productIds.Contains(p.ProductId) && p.IsPublished)
                .ToListAsync(ct),
            logger,
            "Get Products by IDs",
            new { ProductIds = productIds }
        );
    }

    public async Task<Product?> GetWithDetailsAsync(long productId, CancellationToken ct = default)
    {
        Guard.AgainstNegativeOrZero(productId, nameof(productId));

        return await ExecutionHelper.ExecuteAsync(
            () => _context.Products
                .AsNoTracking()
                .Include(p => p.ProductVariants.Where(pv => pv.IsActive))
                .Include(p => p.ProductMedia)
                .Include(p => p.Category)
                .Include(p => p.Reviews.Where(r => r.IsApproved))
                .FirstOrDefaultAsync(p => p.ProductId == productId && p.IsPublished, ct),
            logger,
            "Get Product with Details",
            new { ProductId = productId }
        );
    }

    public async Task<IReadOnlyList<Product>> SearchAsync(string query, CancellationToken ct = default)
    {
        Guard.AgainstNullOrWhiteSpace(query, nameof(query));
        var search = query.Trim();
        var isArabic = _userContext.LangId == (int)Lang.Ar;

        var dbQuery = _context.Products
            .AsNoTracking()
            .Where(p => p.IsPublished && (
                EF.Functions.Like(p.Slug, $"%{search}%") ||
                EF.Functions.Like(p.Sku, $"%{search}%") ||
                (isArabic && (
                    EF.Functions.Like(p.NameAr, $"%{search}%") ||
                    EF.Functions.Like(p.DescriptionAr ?? "", $"%{search}%")
                )) ||
                (!isArabic && (
                    EF.Functions.Like(p.NameEn, $"%{search}%") ||
                    EF.Functions.Like(p.DescriptionEn ?? "", $"%{search}%")
                ))
            ));

        return await ExecutionHelper.ExecuteAsync(
            () => dbQuery
                .OrderBy(p => isArabic ? p.NameAr : p.NameEn)
                .ToListAsync(ct),
            logger,
            "Search Products (Localized)",
            new { Query = search, LangId = _userContext.LangId }
        );
    }

    public async Task<Product?> GetBySlugAsync(string slug, CancellationToken ct = default)
    {
        Guard.AgainstNullOrWhiteSpace(slug, nameof(slug));

        return await ExecutionHelper.ExecuteAsync(
            () => _context.Products
                .AsNoTracking()
                .Include(p => p.ProductVariants.Where(pv => pv.IsActive))
                .Include(p => p.ProductMedia)
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Slug == slug && p.IsPublished, ct),
            logger,
            "Get Product by Slug",
            new { Slug = slug }
        );
    }

    public async Task<IPaginatedList<Product>> GetPaginatedAsync(PaginationRequest request, CancellationToken ct = default)
    {
        var langId = _userContext.LangId;
        var isArabic = langId == (int)Lang.Ar;

        var query = _context.Products
            .AsNoTracking()
            .Where(p => p.IsPublished);

        // Localized Search
        if (!string.IsNullOrWhiteSpace(request.SearchValue))
        {
            var search = request.SearchValue.Trim();
            query = query.Where(p =>
                EF.Functions.Like(p.Slug, $"%{search}%") ||
                EF.Functions.Like(p.Sku, $"%{search}%") ||
                (isArabic && (
                    EF.Functions.Like(p.NameAr, $"%{search}%") ||
                    EF.Functions.Like(p.DescriptionAr ?? "", $"%{search}%")
                )) ||
                (!isArabic && (
                    EF.Functions.Like(p.NameEn, $"%{search}%") ||
                    EF.Functions.Like(p.DescriptionEn ?? "", $"%{search}%")
                ))
            );
        }

        // Sorting
        query = (request.SortBy?.Trim().ToLower()) switch
        {
            "name" => isArabic
                ? query.OrderBy(p => p.NameAr)
                : query.OrderBy(p => p.NameEn),
            "price" => request.SortDirection == SortDirection.Ascending
                ? query.OrderBy(p => p.Price)
                : query.OrderByDescending(p => p.Price),
            "createdat" => request.SortDirection == SortDirection.Ascending
                ? query.OrderBy(p => p.CreatedAt)
                : query.OrderByDescending(p => p.CreatedAt),
            "rating" => request.SortDirection == SortDirection.Ascending
                ? query.OrderBy(p => p.AverageRating)
                : query.OrderByDescending(p => p.AverageRating),
            _ => isArabic
                ? query.OrderBy(p => p.NameAr)
                : query.OrderBy(p => p.NameEn)
        };

        return await ExecutionHelper.ExecuteAsync(
            () => query.ToPaginatedListAsync(request, ct),
            logger,
            "Get Paginated Products (Localized)",
            new
            {
                Page = request.PageNumber,
                Size = request.PageSize,
                Search = request.SearchValue,
                SortBy = request.SortBy,
                LangId = langId
            }
        );
    }
}
