using FreshMarket.Domain.Entities.FreshMarketManagement;
using FreshMarket.Domain.Entities.LookupManagement;
using FreshMarket.Domain.Entities.SharedManagement;
using FreshMarket.Domain.Entities.UserManagement;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace FreshMarket.Infrastructure.Data;

/// <summary>
/// Entity Framework Core DbContext for FreshMarket application.
/// Manages all entity configurations, migrations, and database interactions.
/// Includes automatic audit timestamp management and soft delete filtering.
/// </summary>
public class FreshMarketDbContext(DbContextOptions<FreshMarketDbContext> options) : DbContext(options)
{
    // ────────────────────────────── User Management ──────────────────────────────
    public DbSet<Person> Persons => Set<Person>();
    public DbSet<SignInLog> SignInLogs => Set<SignInLog>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();

    // ────────────────────────────── Lookup Management ──────────────────────────────
    public DbSet<UnitOfMeasure> UnitOfMeasures => Set<UnitOfMeasure>();
    public DbSet<Country> Countries => Set<Country>();
    public DbSet<City> Cities => Set<City>();
    public DbSet<Currency> Currencies => Set<Currency>();

    // ────────────────────────────── FreshMarket Management ──────────────────────────────
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Address> Addresses => Set<Address>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Inventory> Inventories => Set<Inventory>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Coupon> Coupons => Set<Coupon>();
    public DbSet<OrderHistory> OrderHistories => Set<OrderHistory>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductMedia> ProductMedias => Set<ProductMedia>();
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
    public DbSet<Review> Reviews => Set<Review>();

    /// <summary>
    /// Configures entity relationships, constraints, and database mappings.
    /// Applies all FluentAPI configurations from the assembly.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all entity configurations from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FreshMarketDbContext).Assembly);

        // Configure global query filter for soft deletes
        ConfigureSoftDeleteFilter(modelBuilder);
    }

    /// <summary>
    /// Overrides SaveChangesAsync to automatically manage audit timestamps.
    /// Sets CreatedAt for new entities and UpdatedAt for modified entities.
    /// </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SetAuditTimestamps();
        return await base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Overrides SaveChanges (sync version) to automatically manage audit timestamps.
    /// </summary>
    public override int SaveChanges()
    {
        SetAuditTimestamps();
        return base.SaveChanges();
    }

    /// <summary>
    /// Sets CreatedAt and UpdatedAt timestamps for audit tracking.
    /// </summary>
    private void SetAuditTimestamps()
    {
        var entries = ChangeTracker.Entries();

        foreach (var entry in entries)
        {
            if (entry.Entity is not Base entity) continue;

            switch (entry.State)
            {
                case EntityState.Added:
                    entity.CreatedAt = DateTime.UtcNow;
                    break;
                case EntityState.Modified:
                    entity.UpdatedAt = DateTime.UtcNow;
                    break;
            }
        }
    }

    /// <summary>
    /// Configures global query filters for soft delete functionality.
    /// Automatically filters out soft-deleted records from all queries.
    /// </summary>
    private static void ConfigureSoftDeleteFilter(ModelBuilder modelBuilder)
    {
        var baseType = typeof(Base);
        var types = modelBuilder.Model.GetEntityTypes()
            .Where(t => t.ClrType.IsAssignableTo(baseType))
            .ToList();

        foreach (var entityType in types)
        {
            var parameter = Expression.Parameter(entityType.ClrType);
            var isDeletedProperty = Expression.Property(parameter, nameof(Base.IsDeleted));
            var filter = Expression.Lambda(Expression.Not(isDeletedProperty), parameter);

            modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filter);
        }
    }
}
