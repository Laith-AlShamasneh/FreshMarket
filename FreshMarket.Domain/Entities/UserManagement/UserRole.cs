using FreshMarket.Domain.Entities.SharedManagement;
using System.ComponentModel.DataAnnotations.Schema;

namespace FreshMarket.Domain.Entities.UserManagement;

public class UserRole : Base
{
    public long UserRoleId { get; set; }

    [ForeignKey(nameof(User))]
    public long UserId { get; set; }
    public User User { get; set; } = null!;

    [ForeignKey(nameof(Role))]
    public long RoleId { get; set; }
    public Role Role { get; set; } = null!;
}
