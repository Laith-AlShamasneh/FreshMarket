using FreshMarket.Domain.Entities.SharedManagement;
using System.ComponentModel.DataAnnotations;

namespace FreshMarket.Domain.Entities.LookupManagement;

public class ShippingMethodType : Base
{
    public int ShippingMethodTypeId { get; set; }

    [Required, MaxLength(50)]
    public string Code { get; set; } = null!;

    [Required, MaxLength(200)]
    public string NameEn { get; set; } = null!;

    [Required, MaxLength(200)]
    public string NameAr { get; set; } = null!;

    [MaxLength(500)]
    public string? Description { get; set; }

    public decimal Cost { get; set; }  // Simple flat rate

    public decimal? FreeShippingThreshold { get; set; }  // Free shipping over X amount

    public int EstimatedDays { get; set; }  // Delivery estimate

    public int SortOrder { get; set; }
}
