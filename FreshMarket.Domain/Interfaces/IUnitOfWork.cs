using FreshMarket.Domain.Interfaces.Repositories.FreshMarketManagement;
using FreshMarket.Domain.Interfaces.Repositories.LookupManagement;
using FreshMarket.Domain.Interfaces.Repositories.UserManagement;

namespace FreshMarket.Domain.Interfaces;

/// <summary>
/// Coordinates access to all domain repositories and manages transactional persistence within a single unit of work.
/// Ensures atomicity across multiple repository operations and supports asynchronous persistence.
/// </summary>
public interface IUnitOfWork : IAsyncDisposable
{
    // ──────────────────────────────
    // User Management Repositories
    // ──────────────────────────────
    IPerson PersonRepository { get; }
    ISignInLog SignInLogRepository { get; }
    IUser UserRepository { get; }
    IRole RoleRepository { get; }
    IUserRole UserRoleRepository { get; }

    // ──────────────────────────────
    // Lookup Management Repositories
    // ──────────────────────────────
    IUnitOfMeasure UnitOfMeasureRepository { get; }
    ICountry CountryRepository { get; }
    IPaymentStatus PaymentStatusRepository { get; }
    ICity CityRepository { get; }
    IOrderStatus OrderStatusRepository { get; }
    IPaymentMethodType PaymentMethodTypeRepository { get; }
    IShippingMethodType ShippingMethodTypeRepository { get; }
    ICurrency CurrencyRepository { get; }

    // ──────────────────────────────
    // FreshMarket Management Repositories
    // ──────────────────────────────
    ICart CartRepository { get; }
    ICartItem CartItemRepository { get; }
    IAddress AddressRepository { get; }
    IOrderItem OrderItemRepository { get; }
    IInventory InventoryRepository { get; }
    ICategory CategoryRepository { get; }
    ICoupon CouponRepository { get; }
    IOrderHistory OrderHistoryRepository { get; }
    IOrder OrderRepository { get; }
    IPaymentTransaction PaymentTransactionRepository { get; }
    IPriceHistory PriceHistoryRepository { get; }
    IProduct ProductRepository { get; }
    IProductMedia ProductMediaRepository { get; }
    IProductVariant ProductVariantRepository { get; }
    IReview ReviewRepository { get; }
    IWishlist WishlistRepository { get; }
    IWishlistItem WishlistItemRepository { get; }

    // ──────────────────────────────
    // Persistence & Transaction Control
    // ──────────────────────────────

    /// <summary>
    /// Commits all changes made in this unit of work to the database.
    /// </summary>
    int SaveChanges();

    /// <summary>
    /// Asynchronously commits all changes made in this unit of work.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Begins a new database transaction.
    /// </summary>
    Task<ITransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
}
