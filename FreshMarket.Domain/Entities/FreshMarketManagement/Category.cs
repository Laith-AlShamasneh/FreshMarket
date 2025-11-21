using FreshMarket.Domain.Entities.SharedManagement;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FreshMarket.Domain.Entities.FreshMarketManagement;

public class Category : Base
{
    public long CategoryId { get; set; }

    [Required, MaxLength(200)]
    public string NameEn { get; set; } = null!;

    [Required, MaxLength(200)]
    public string NameAr { get; set; } = null!;

    [MaxLength(200)]
    public string? Slug { get; set; }

    [MaxLength(1000)]
    public string? DescriptionEn { get; set; }

    [MaxLength(1000)]
    public string? DescriptionAr { get; set; }

    [ForeignKey(nameof(ParentCategory))]
    public long? ParentCategoryId { get; set; }
    public Category? ParentCategory { get; set; }

    public ICollection<Category> Children { get; set; } = [];


    [MaxLength(500)]
    public string? ImageUrl { get; set; }

    [MaxLength(200)]
    public string? SeoTitle { get; set; }

    [MaxLength(500)]
    public string? SeoDescription { get; set; }

    public int SortOrder { get; set; }

    public ICollection<Product> Products { get; set; } = [];
}
