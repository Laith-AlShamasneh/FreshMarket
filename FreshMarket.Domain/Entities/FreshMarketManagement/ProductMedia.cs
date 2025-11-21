using FreshMarket.Domain.Entities.SharedManagement;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FreshMarket.Domain.Entities.FreshMarketManagement;

public class ProductMedia : Base
{
    public long ProductMediaId { get; set; }

    [ForeignKey(nameof(Product))]
    public long ProductId { get; set; }
    public Product Product { get; set; } = null!;

    [Required, MaxLength(1000)]
    public string Url { get; set; } = null!;  // Changed from Name

    [MaxLength(200)]
    public string? AltTextEn { get; set; }

    [MaxLength(200)]
    public string? AltTextAr { get; set; }

    public bool IsDefault { get; set; } = false;

    public int SortOrder { get; set; } = 0;

    [MaxLength(50)]
    public string? MediaType { get; set; }  // "Image", "Video"
}