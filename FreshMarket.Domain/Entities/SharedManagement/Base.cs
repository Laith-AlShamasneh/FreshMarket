namespace FreshMarket.Domain.Entities.SharedManagement;

/// <summary>
/// Represents a base model containing common properties for tracking entity metadata.
/// </summary>
public class Base
{
    public long CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;  // Add default
    public long? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsActive { get; set; } = true;  // Add default
    public bool IsDeleted { get; set; } = false;  // Add default (soft delete)
}
