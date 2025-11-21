using FreshMarket.Domain.Entities.SharedManagement;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FreshMarket.Domain.Entities.LookupManagement;

public class Country : Base
{
    public int CountryId { get; set; }

    [ForeignKey(nameof(Currency))]
    public int? CurrencyId { get; set; }
    public Currency? Currency { get; set; }


    [Required, MaxLength(200)]
    public string NameEn { get; set; } = null!;

    [Required, MaxLength(200)]
    public string NameAr { get; set; } = null!;

    [MaxLength(3)]
    public string? Iso2 { get; set; } 

    [MaxLength(20)]
    public string? CallingCode { get; set; } 

    [MaxLength(200)]
    public string? TimeZone { get; set; }

    [MaxLength(200)]
    public string? Slug { get; set; }

    public string? Metadata { get; set; }

    public ICollection<City> Cities { get; set; } = [];
}