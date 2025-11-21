using FreshMarket.Domain.Entities.LookupManagement;
using FreshMarket.Domain.Entities.SharedManagement;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FreshMarket.Domain.Entities.FreshMarketManagement;

public class Product : Base
{
    public long ProductId { get; set; }

    [Required, MaxLength(100)]
    public string Sku { get; set; } = null!;

    [Required, MaxLength(200)]
    public string NameEn { get; set; } = null!;

    [Required, MaxLength(200)]
    public string NameAr { get; set; } = null!;

    [MaxLength(200)]
    public string? Slug { get; set; }  // SEO-friendly URL

    public string? DescriptionEn { get; set; }
    public string? DescriptionAr { get; set; }

    [ForeignKey(nameof(Category))]
    public long? CategoryId { get; set; }
    public Category? Category { get; set; }

    [Column(TypeName = "decimal(6,3)")]
    public decimal? Weight { get; set; }

    [Column(TypeName = "decimal(6,2)")]
    public decimal Price { get; set; }

    [MaxLength(1000)]
    public string? DefaultImageUrl { get; set; }

    public bool IsPublished { get; set; } = false;
    public bool IsFeatured { get; set; } = false;

    [Column(TypeName = "decimal(3,2)")]
    public decimal? AverageRating { get; set; }

    public int TotalReviews { get; set; } = 0;

    // SEO fields
    [MaxLength(200)]
    public string? SeoTitle { get; set; }

    [MaxLength(500)]
    public string? SeoDescription { get; set; }

    // Navigation
    public ICollection<PriceHistory> PriceHistories { get; set; } = [];
    public ICollection<ProductMedia> ProductMedia { get; set; } = [];
    public ICollection<ProductVariant> ProductVariants { get; set; } = [];
    public ICollection<Review> Reviews { get; set; } = [];
}
