using FreshMarket.Domain.Entities.SharedManagement;
using System.ComponentModel.DataAnnotations;

namespace FreshMarket.Domain.Entities.LookupManagement;

public class Currency : Base
{
    public int CurrencyId { get; set; }

    [Required, MaxLength(3)]
    public string IsoCode { get; set; } = null!;

    [Required, MaxLength(200)]
    public string NameEn { get; set; } = null!;

    [Required, MaxLength(200)]
    public string NameAr { get; set; } = null!;

    [MaxLength(10)]
    public string? Symbol { get; set; }            // e.g. "$", "د.ا"

    public int DecimalPlaces { get; set; } = 2;   // common precision, e.g. 2 for cents

    public bool IsDefault { get; set; } = false;


    [MaxLength(50)]
    public string? Culture { get; set; }          // e.g., "en-US", "ar-JO" for formatting


    // Navigation
    public ICollection<Country> Countries { get; set; } = [];
}
