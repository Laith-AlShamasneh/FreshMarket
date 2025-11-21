using FreshMarket.Domain.Entities.FreshMarketManagement;
using FreshMarket.Domain.Entities.LookupManagement;
using FreshMarket.Domain.Entities.UserManagement;
using Microsoft.EntityFrameworkCore;

namespace FreshMarket.Infrastructure.Data;

public class FreshMarketDbContext(DbContextOptions<FreshMarketDbContext> options) : DbContext(options)
{
    // ──────────────────────────────
    // User Management
    // ──────────────────────────────
    public DbSet<Person> Persons => Set<Person>();
    public DbSet<SignInLog> SignInLogs => Set<SignInLog>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();

    // ──────────────────────────────
    // Lookup Management
    // ──────────────────────────────
    public DbSet<UnitOfMeasure> UnitOfMeasures => Set<UnitOfMeasure>();
    public DbSet<Country> Countries => Set<Country>();
    public DbSet<PaymentStatus> PaymentStatuses => Set<PaymentStatus>();
    public DbSet<City> Cities => Set<City>();
    public DbSet<OrderStatus> OrderStatuses => Set<OrderStatus>();
    public DbSet<PaymentMethodType> PaymentMethodTypes => Set<PaymentMethodType>();
    public DbSet<ShippingMethodType> ShippingMethodTypes => Set<ShippingMethodType>();
    public DbSet<Currency> Currencies => Set<Currency>();

    // ──────────────────────────────
    // FreshMarket Management
    // ──────────────────────────────
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Address> Addresses => Set<Address>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Inventory> Inventories => Set<Inventory>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Coupon> Coupons => Set<Coupon>();
    public DbSet<OrderHistory> OrderHistories => Set<OrderHistory>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<PaymentTransaction> PaymentTransactions => Set<PaymentTransaction>();
    public DbSet<PriceHistory> PriceHistories => Set<PriceHistory>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductMedia> ProductMedias => Set<ProductMedia>();
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<Wishlist> Wishlists => Set<Wishlist>();
    public DbSet<WishlistItem> WishlistItems => Set<WishlistItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FreshMarketDbContext).Assembly);
    }
}
