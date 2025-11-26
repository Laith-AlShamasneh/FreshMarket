using FreshMarket.Domain.Entities.SharedManagement;

namespace FreshMarket.Domain.Entities.FreshMarketManagement;

public class Cart : Base
{
    public long CartId { get; set; }
    public long? UserId { get; private set; } // private setter
    public Guid SessionId { get; private set; }
    public DateTime? ExpiresAt { get; private set; }
    public string CartNumber { get; set; } = null!;

    public ICollection<CartItem> Items { get; set; } = [];

    // Static Factory Method to encapsulate creation logic
    public static Cart Create(long? userId, Guid sessionId)
    {
        var cart = new Cart
        {
            UserId = userId,
            SessionId = sessionId == Guid.Empty ? Guid.NewGuid() : sessionId,
            CartNumber = $"CART-{DateTime.UtcNow:yyyy}-{new Random().Next(1, 9999):D4}",
            CreatedAt = DateTime.UtcNow
        };

        cart.RecalculateExpiration();
        return cart;
    }

    // Domain Logic: Expiration Rules
    public void RecalculateExpiration()
    {
        // Rule: Registered users get 72h, Anonymous get 24h
        int hours = (UserId.HasValue && UserId > 0) ? 72 : 24;
        ExpiresAt = DateTime.UtcNow.AddHours(hours);
    }

    public void AssignUser(long userId)
    {
        if (UserId.HasValue) return; // Already assigned
        UserId = userId;
        RecalculateExpiration(); // Extend time when user logs in
    }
}
