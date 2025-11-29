using FreshMarket.Domain.Interfaces;
using FreshMarket.Domain.Interfaces.Repositories.FreshMarketManagement;
using FreshMarket.Domain.Interfaces.Repositories.LookupManagement;
using FreshMarket.Domain.Interfaces.Repositories.UserManagement;
using FreshMarket.Infrastructure.Data;
using FreshMarket.Infrastructure.Transactions;
using FreshMarket.Shared.Helpers;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace FreshMarket.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly FreshMarketDbContext _context;
    private readonly ILogger<UnitOfWork> _logger;

    // Repositories (injected)
    public IPerson PersonRepository { get; }
    public ISignInLog SignInLogRepository { get; }
    public IUser UserRepository { get; }
    public IRole RoleRepository { get; }
    public IUserRole UserRoleRepository { get; }

    public ICountry CountryRepository { get; }
    public ICity CityRepository { get; }
    public ICurrency CurrencyRepository { get; }
    public IUnitOfMeasure UnitOfMeasureRepository { get; }

    public ICategory CategoryRepository { get; }
    public IProduct ProductRepository { get; }
    public IProductVariant ProductVariantRepository { get; }
    public IProductMedia ProductMediaRepository { get; }
    public IInventory InventoryRepository { get; }

    public ICart CartRepository { get; }
    public ICartItem CartItemRepository { get; }
    public IAddress AddressRepository { get; }

    public IOrder OrderRepository { get; }
    public IOrderItem OrderItemRepository { get; }
    public IOrderHistory OrderHistoryRepository { get; }

    public ICoupon CouponRepository { get; }
    public IReview ReviewRepository { get; }

    public UnitOfWork(
        FreshMarketDbContext context,
        ILogger<UnitOfWork> logger,
        IPerson personRepository,
        ISignInLog signInLogRepository,
        IUser userRepository,
        IRole roleRepository,
        IUserRole userRoleRepository,
        IUnitOfMeasure unitOfMeasureRepository,
        ICountry countryRepository,
        ICity cityRepository,
        ICurrency currencyRepository,
        ICart cartRepository,
        ICartItem cartItemRepository,
        IAddress addressRepository,
        IOrderItem orderItemRepository,
        IInventory inventoryRepository,
        ICategory categoryRepository,
        ICoupon couponRepository,
        IOrderHistory orderHistoryRepository,
        IOrder orderRepository,
        IProduct productRepository,
        IProductMedia productMediaRepository,
        IProductVariant productVariantRepository,
        IReview reviewRepository)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        PersonRepository = personRepository;
        SignInLogRepository = signInLogRepository;
        UserRepository = userRepository;
        RoleRepository = roleRepository;
        UserRoleRepository = userRoleRepository;

        UnitOfMeasureRepository = unitOfMeasureRepository;
        CountryRepository = countryRepository;
        CityRepository = cityRepository;
        CurrencyRepository = currencyRepository;

        CartRepository = cartRepository;
        CartItemRepository = cartItemRepository;
        AddressRepository = addressRepository;
        OrderItemRepository = orderItemRepository;
        InventoryRepository = inventoryRepository;
        CategoryRepository = categoryRepository;
        CouponRepository = couponRepository;
        OrderHistoryRepository = orderHistoryRepository;
        OrderRepository = orderRepository;
        ProductRepository = productRepository;
        ProductMediaRepository = productMediaRepository;
        ProductVariantRepository = productVariantRepository;
        ReviewRepository = reviewRepository;
    }

    public int SaveChanges()
        =>  Task.FromResult(_context.SaveChanges()).GetAwaiter().GetResult();

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        => await _context.SaveChangesAsync(ct);

    /// <summary>
    /// Begins a new EF Core transaction and returns an ITransaction wrapper.
    /// Use transaction.CommitAsync() to commit, or RollbackAsync() to rollback.
    /// </summary>
    public async Task<ITransaction> BeginTransactionAsync(CancellationToken ct = default)
    {
        var efTx = await _context.Database.BeginTransactionAsync(ct);
        return new EfCoreTransaction(efTx, _logger);
    }

    /// <summary>
    /// Dispose the underlying DbContext when the UnitOfWork is disposed.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        try
        {
            await _context.DisposeAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error disposing FreshMarketDbContext in UnitOfWork");
        }
    }
}