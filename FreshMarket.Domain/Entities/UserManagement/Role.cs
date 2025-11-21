using FreshMarket.Domain.Entities.SharedManagement;
using System.ComponentModel.DataAnnotations;

namespace FreshMarket.Domain.Entities.UserManagement;

public class Role : Base
{
    public long RoleId { get; set; }

    [Required, MaxLength(100)]
    public string NameEn { get; set; } = null!;

    [Required, MaxLength(100)]
    public string NameAr { get; set; } = null!;

    [MaxLength(500)]
    public string? Description { get; set; }

    public bool IsDefault { get; set; } = false;

    // Navigation
    public ICollection<UserRole> UserRoles { get; set; } = [];
}