using FreshMarket.Domain.Entities.FreshMarketManagement;
using FreshMarket.Domain.Entities.SharedManagement;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FreshMarket.Domain.Entities.LookupManagement;

public class UnitOfMeasure : Base
{
    public int UnitOfMeasureId { get; set; }

    [Required, MaxLength(50)]
    public string Code { get; set; } = null!;

    [Required, MaxLength(200)]
    public string NameEn { get; set; } = null!;

    [Required, MaxLength(200)]
    public string NameAr { get; set; } = null!;

    [MaxLength(20)]
    public string? Abbreviation { get; set; }

    public int SortOrder { get; set; }

    public ICollection<ProductVariant> ProductVariants { get; set; } = [];
}
