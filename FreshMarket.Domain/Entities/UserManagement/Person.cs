using FreshMarket.Domain.Entities.FreshMarketManagement;
using FreshMarket.Domain.Entities.LookupManagement;
using FreshMarket.Domain.Entities.SharedManagement;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FreshMarket.Domain.Entities.UserManagement;

public class Person : Base
{
    public long PersonId { get; set; }

    [Required, MaxLength(100)]
    public string FirstName { get; set; } = null!;

    [Required, MaxLength(100)]
    public string LastName { get; set; } = null!;

    [Required, MaxLength(256)]
    public string Email { get; set; } = null!;

    [MaxLength(20)]
    public string? PhoneNumber { get; set; }

    [MaxLength(500)]
    public string? ProfilePictureUrl { get; set; }

    // Navigation
    public User? User { get; set; }
}
