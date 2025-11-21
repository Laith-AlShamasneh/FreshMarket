using FreshMarket.Domain.Entities.LookupManagement;
using FreshMarket.Domain.Entities.SharedManagement;
using FreshMarket.Domain.Entities.UserManagement;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FreshMarket.Domain.Entities.FreshMarketManagement;

public class Address : Base
{
    public long AddressId { get; set; }

    [ForeignKey(nameof(User))]
    public long? UserId { get; set; }
    public User? User { get; set; }

    [ForeignKey(nameof(City))]
    public int CityId { get; set; }
    public City City { get; set; } = null!;

    [ForeignKey(nameof(Country))]
    public int CountryId { get; set; }
    public Country Country { get; set; } = null!;

    [MaxLength(200)]
    public string? Label { get; set; }  // "Home", "Work", etc.

    [Required, MaxLength(500)]
    public string Line1 { get; set; } = null!;

    [MaxLength(500)]
    public string? Line2 { get; set; }

    [MaxLength(50)]
    public string? PostalCode { get; set; }

    [MaxLength(100)]
    public string? ContactName { get; set; }

    [MaxLength(50)]
    public string Phone { get; set; } = null!;  // Make required

    public bool IsDefaultShipping { get; set; } = false;
    public bool IsDefaultBilling { get; set; } = false;

    [MaxLength(500)]
    public string? DeliveryInstructions { get; set; }
}