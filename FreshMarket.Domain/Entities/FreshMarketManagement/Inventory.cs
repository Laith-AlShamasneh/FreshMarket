using FreshMarket.Domain.Entities.SharedManagement;
using FreshMarket.Shared.Helpers;
using System.ComponentModel.DataAnnotations.Schema;

namespace FreshMarket.Domain.Entities.FreshMarketManagement;

public class Inventory : Base
{
    public long InventoryId { get; set; }

    [ForeignKey(nameof(ProductVariant))]
    public long ProductVariantId { get; set; }
    public ProductVariant ProductVariant { get; set; } = null!;

    public int Quantity { get; private set; } // Set setter to private
    public int ReservedQuantity { get; private set; }
    public int? ReorderLevel { get; set; }

    // Domain Logic: specific method to calculate what is actually available
    public int AvailableStock => Quantity - ReservedQuantity;

    public void ReserveStock(int quantity)
    {
        Guard.AgainstNegativeOrZero(quantity, nameof(quantity));

        // Enforce Invariant: Cannot reserve more than available
        if (AvailableStock < quantity)
            throw new InvalidOperationException($"Insufficient stock. Requested: {quantity}, Available: {AvailableStock}");

        ReservedQuantity += quantity;
    }

    public void ReleaseStock(int quantity)
    {
        Guard.AgainstNegativeOrZero(quantity, nameof(quantity));
        ReservedQuantity = Math.Max(0, ReservedQuantity - quantity);
    }

    public void CommitStock(int quantity)
    {
        Guard.AgainstNegativeOrZero(quantity, nameof(quantity));

        // When committing (shipping), we reduce the physical quantity
        // and reduce the reservation associated with it.
        Quantity -= quantity;
        ReservedQuantity = Math.Max(0, ReservedQuantity - quantity);

        // Safety check for data integrity
        if (Quantity < 0)
            throw new InvalidOperationException("Inventory quantity cannot be negative after commit.");
    }

    // For adding new stock (restocking)
    public void AddStock(int quantity)
    {
        Guard.AgainstNegativeOrZero(quantity, nameof(quantity));
        Quantity += quantity;
    }
}
