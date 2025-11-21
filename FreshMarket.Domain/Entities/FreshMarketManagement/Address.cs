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

    [Required, MaxLength(500)]
    public string Line1 { get; set; } = null!;

    [MaxLength(500)]
    public string? Line2 { get; set; }

    [MaxLength(50)]
    public string? PostalCode { get; set; }

    [MaxLength(50)]
    public string Phone { get; set; } = null!; 

    public bool IsDefault { get; set; } = true;
}