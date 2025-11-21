using FreshMarket.Domain.Entities.FreshMarketManagement;
using FreshMarket.Domain.Entities.SharedManagement;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FreshMarket.Domain.Entities.UserManagement;

public class User : Base
{
    public long UserId { get; set; }

    [Required]
    [ForeignKey(nameof(Person))]
    public long PersonId { get; set; }
    public Person Person { get; set; } = null!;

    [MaxLength(256)]
    public string? PasswordHash { get; set; }

    public bool IsEmailConfirmed { get; set; } = false;
    public DateTime? LastLoginAt { get; set; }

    // Navigation
    public ICollection<UserRole> UserRoles { get; set; } = [];
    public ICollection<Order> Orders { get; set; } = [];
    public ICollection<Address> Addresses { get; set; } = [];
}
