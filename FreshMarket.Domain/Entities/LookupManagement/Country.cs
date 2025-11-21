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

    public ICollection<City> Cities { get; set; } = [];
}