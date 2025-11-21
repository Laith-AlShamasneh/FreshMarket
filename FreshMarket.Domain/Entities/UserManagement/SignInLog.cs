using FreshMarket.Shared.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FreshMarket.Domain.Entities.UserManagement;

public class SignInLog
{
    public long SignInLogId { get; set; }

    [ForeignKey(nameof(User))]
    public long? UserId { get; set; }
    public User? User { get; set; }

    public DateTime AttemptedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(45)]
    public string? IpAddress { get; set; }

    public bool IsSuccessful { get; set; }

    [MaxLength(500)]
    public LoginFailureReason? FailureReason { get; set; }  // "InvalidCredentials", "EmailNotConfirmed", etc.
}
