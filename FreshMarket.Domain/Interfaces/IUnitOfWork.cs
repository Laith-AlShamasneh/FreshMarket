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
    // ────────────────────────────── User Management ──────────────────────────────
    IPerson PersonRepository { get; }
    IUser UserRepository { get; }
    IRole RoleRepository { get; }
    IUserRole UserRoleRepository { get; }
    ISignInLog SignInLogRepository { get; }

    // ────────────────────────────── Lookups ──────────────────────────────
    ICountry CountryRepository { get; }
    ICity CityRepository { get; }
    ICurrency CurrencyRepository { get; }
    IUnitOfMeasure UnitOfMeasureRepository { get; }

    // ────────────────────────────── FreshMarket ──────────────────────────────
    ICategory CategoryRepository { get; }
    IProduct ProductRepository { get; }
    IProductVariant ProductVariantRepository { get; }
    IProductMedia ProductMediaRepository { get; }
    IInventory InventoryRepository { get; }

    ICart CartRepository { get; }
    ICartItem CartItemRepository { get; }
    IAddress AddressRepository { get; }

    IOrder OrderRepository { get; }
    IOrderItem OrderItemRepository { get; }
    IOrderHistory OrderHistoryRepository { get; }

    ICoupon CouponRepository { get; }
    IReview ReviewRepository { get; }

    // ────────────────────────────── Persistence ──────────────────────────────
    int SaveChanges();
    Task<int> SaveChangesAsync(CancellationToken ct = default);
    Task<ITransaction> BeginTransactionAsync(CancellationToken ct = default);
}
