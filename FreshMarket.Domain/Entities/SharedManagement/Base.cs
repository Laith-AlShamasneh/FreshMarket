namespace FreshMarket.Domain.Entities.SharedManagement;

/// <summary>
/// Represents a base model containing common properties for tracking entity metadata.
/// </summary>
public class Base
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; } = false;
}
