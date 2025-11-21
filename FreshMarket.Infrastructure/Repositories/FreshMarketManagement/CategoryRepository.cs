using FreshMarket.Domain.Entities.FreshMarketManagement;
using FreshMarket.Domain.Interfaces.Repositories.FreshMarketManagement;
using FreshMarket.Infrastructure.Data;
using FreshMarket.Shared.Common;
using FreshMarket.Shared.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FreshMarket.Infrastructure.Repositories.FreshMarketManagement;

public class CategoryRepository(
    FreshMarketDbContext context,
    ILogger<CategoryRepository> logger,
    IUserContext userContext)
    : Repository<Category>(context, logger), ICategory
{
    private readonly FreshMarketDbContext _context = context;
    private readonly IUserContext _userContext = userContext;
    private readonly bool _isArabic = userContext.LangId == (int)Lang.Ar;

    public async Task<IReadOnlyList<Category>> GetActiveAsync(CancellationToken ct = default)
    {
        return await ExecutionHelper.ExecuteAsync(
            () => _context.Categories
                .AsNoTracking()
                .Where(c => c.IsActive)
                .OrderBy(c => c.SortOrder)
                .ToListAsync(ct),
            logger,
            "Get Active Categories"
        );
    }

    public async Task<IReadOnlyList<Category>> GetActiveLocalizedAsync(CancellationToken ct = default)
    {
        return await ExecutionHelper.ExecuteAsync(
            () => _context.Categories
                .AsNoTracking()
                .Where(c => c.IsActive)
                .OrderBy(c => c.SortOrder)
                .Select(c => new Category
                {
                    CategoryId = c.CategoryId,
                    NameEn = c.NameEn,
                    NameAr = c.NameAr,
                    Slug = c.Slug,
                    DescriptionEn = c.DescriptionEn,
                    DescriptionAr = c.DescriptionAr,
                    ImageUrl = c.ImageUrl,
                    SeoTitle = c.SeoTitle,
                    SeoDescription = c.SeoDescription,
                    SortOrder = c.SortOrder,
                    IsActive = c.IsActive,
                    ParentCategoryId = c.ParentCategoryId
                })
                .ToListAsync(ct),
            logger,
            "Get Active Categories (Localized)"
        );
    }

    public async Task<IReadOnlyList<Category>> GetWithProductsAsync(CancellationToken ct = default)
    {
        return await ExecutionHelper.ExecuteAsync(
            () => _context.Categories
                .AsNoTracking()
                .Where(c => c.IsActive)
                .Include(c => c.Products.Where(p => p.IsActive))
                .ThenInclude(p => p.ProductVariants.Where(pv => pv.IsActive))
                .Include(c => c.Products.Where(p => p.IsActive))
                .ThenInclude(p => p.ProductMedia)
                .OrderBy(c => c.SortOrder)
                .ToListAsync(ct),
            logger,
            "Get Categories with Active Products"
        );
    }

    public async Task<IReadOnlyList<Category>> GetWithProductsLocalizedAsync(CancellationToken ct = default)
    {
        return await ExecutionHelper.ExecuteAsync(
            () => _context.Categories
                .AsNoTracking()
                .Where(c => c.IsActive)
                .Include(c => c.Products.Where(p => p.IsActive))
                .ThenInclude(p => p.ProductVariants.Where(pv => pv.IsActive))
                .Include(c => c.Products.Where(p => p.IsActive))
                .ThenInclude(p => p.ProductMedia)
                .OrderBy(c => c.SortOrder)
                .ToListAsync(ct),
            logger,
            "Get Categories with Products (Localized)"
        );
    }

    public async Task<IReadOnlyList<Category>> GetTreeAsync(CancellationToken ct = default)
    {
        return await ExecutionHelper.ExecuteAsync(
            () => _context.Categories
                .AsNoTracking()
                .Where(c => c.IsActive && c.ParentCategoryId == null)
                .Include(c => c.Children.Where(ch => ch.IsActive))
                .ThenInclude(ch => ch.Children.Where(gch => gch.IsActive))
                .OrderBy(c => c.SortOrder)
                .ToListAsync(ct),
            logger,
            "Get Category Tree"
        );
    }

    public async Task<Category?> GetBySlugAsync(string slug, CancellationToken ct = default)
    {
        Guard.AgainstNullOrWhiteSpace(slug, nameof(slug));

        return await ExecutionHelper.ExecuteAsync(
            () => _context.Categories
                .AsNoTracking()
                .Include(c => c.Products.Where(p => p.IsActive))
                .ThenInclude(p => p.ProductVariants.Where(pv => pv.IsActive))
                .Include(c => c.Products.Where(p => p.IsActive))
                .ThenInclude(p => p.ProductMedia)
                .FirstOrDefaultAsync(c => c.Slug == slug && c.IsActive, ct),
            logger,
            "Get Category by Slug",
            new { Slug = slug }
        );
    }

    public async Task<IReadOnlyList<Category>> SearchAsync(string query, CancellationToken ct = default)
    {
        Guard.AgainstNullOrWhiteSpace(query, nameof(query));
        var search = query.Trim();

        return await ExecutionHelper.ExecuteAsync(
            () => _context.Categories
                .AsNoTracking()
                .Where(c => c.IsActive && (
                    EF.Functions.Like(c.Slug, $"%{search}%") ||
                    (_isArabic && (
                        EF.Functions.Like(c.NameAr, $"%{search}%") ||
                        EF.Functions.Like(c.DescriptionAr ?? "", $"%{search}%")
                    )) ||
                    (!_isArabic && (
                        EF.Functions.Like(c.NameEn, $"%{search}%") ||
                        EF.Functions.Like(c.DescriptionEn ?? "", $"%{search}%")
                    ))
                ))
                .OrderBy(c => c.SortOrder)
                .ToListAsync(ct),
            logger,
            "Search Categories (Localized)",
            new { Query = search, LangId = _userContext.LangId }
        );
    }
}