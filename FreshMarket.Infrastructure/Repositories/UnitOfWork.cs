using FreshMarket.Domain.Interfaces;
using FreshMarket.Domain.Interfaces.Repositories.FreshMarketManagement;
using FreshMarket.Domain.Interfaces.Repositories.LookupManagement;
using FreshMarket.Domain.Interfaces.Repositories.UserManagement;
using FreshMarket.Infrastructure.Data;
using FreshMarket.Infrastructure.Transactions;
using FreshMarket.Shared.Helpers;
using Microsoft.Extensions.Logging;

namespace FreshMarket.Infrastructure.Repositories;

public class UnitOfWork(
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
    : IUnitOfWork
{
    public IPerson PersonRepository => personRepository;
    public ISignInLog SignInLogRepository => signInLogRepository;
    public IUser UserRepository => userRepository;
    public IRole RoleRepository => roleRepository;
    public IUserRole UserRoleRepository => userRoleRepository;
    public IUnitOfMeasure UnitOfMeasureRepository => unitOfMeasureRepository;
    public ICountry CountryRepository => countryRepository;
    public ICity CityRepository => cityRepository;
    public ICurrency CurrencyRepository => currencyRepository;
    public ICart CartRepository => cartRepository;
    public ICartItem CartItemRepository => cartItemRepository;
    public IAddress AddressRepository => addressRepository;
    public IOrderItem OrderItemRepository => orderItemRepository;
    public IInventory InventoryRepository => inventoryRepository;
    public ICategory CategoryRepository => categoryRepository;
    public ICoupon CouponRepository => couponRepository;
    public IOrderHistory OrderHistoryRepository => orderHistoryRepository;
    public IOrder OrderRepository => orderRepository;
    public IProduct ProductRepository => productRepository;
    public IProductMedia ProductMediaRepository => productMediaRepository;
    public IProductVariant ProductVariantRepository => productVariantRepository;
    public IReview ReviewRepository => reviewRepository;

    public int SaveChanges()
        => ExecutionHelper.ExecuteAsync(
            () => Task.FromResult(context.SaveChanges()),
            logger,
            "SaveChanges (sync)"
        ).GetAwaiter().GetResult();

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        => await ExecutionHelper.ExecuteAsync(
            () => context.SaveChangesAsync(ct),
            logger,
            "SaveChanges (async)"
        );

    public async Task<ITransaction> BeginTransactionAsync(CancellationToken ct = default)
        => await ExecutionHelper.ExecuteAsync(
            async () =>
            {
                var tx = await context.Database.BeginTransactionAsync(ct);
                return new EfCoreTransaction(tx);
            },
            logger,
            "BeginTransaction"
        );

    public async ValueTask DisposeAsync()
    {
        await context.DisposeAsync();
    }
}
