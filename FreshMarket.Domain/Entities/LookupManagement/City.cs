using FreshMarket.Domain.Entities.FreshMarketManagement;
using FreshMarket.Domain.Entities.SharedManagement;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FreshMarket.Domain.Entities.LookupManagement;

public class City : Base
{
    public int CityId { get; set; }

    [ForeignKey(nameof(Country))]
    public int? CountryId { get; set; }
    public Country? Country { get; set; }


    [Required, MaxLength(200)]
    public string NameEn { get; set; } = null!;

    [Required, MaxLength(200)]
    public string NameAr { get; set; } = null!;

    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }

    public ICollection<Address> Addresses { get; set; } = [];
}
