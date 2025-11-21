using FreshMarket.Domain.Interfaces;
using FreshMarket.Domain.Interfaces.Repositories.FreshMarketManagement;
using FreshMarket.Domain.Interfaces.Repositories.LookupManagement;
using FreshMarket.Domain.Interfaces.Repositories.UserManagement;
using FreshMarket.Infrastructure.Data;
using FreshMarket.Infrastructure.Repositories;
using FreshMarket.Infrastructure.Repositories.FreshMarketManagement;
using FreshMarket.Infrastructure.Repositories.LookupManagement;
using FreshMarket.Infrastructure.Repositories.UserManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FreshMarket.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<FreshMarketDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        // Register UnitOfWork
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Register all repositories
        services.AddScoped<IPerson, PersonRepository>();
        services.AddScoped<ISignInLog, SignInLogRepository>();
        services.AddScoped<IUser, UserRepository>();
        services.AddScoped<IRole, RoleRepository>();
        services.AddScoped<IUserRole, UserRoleRepository>();

        services.AddScoped<IUnitOfMeasure, UnitOfMeasureRepository>();
        services.AddScoped<ICountry, CountryRepository>();
        services.AddScoped<IPaymentStatus, PaymentStatusRepository>();
        services.AddScoped<ICity, CityRepository>();
        services.AddScoped<IOrderStatus, OrderStatusRepository>();
        services.AddScoped<IPaymentMethodType, PaymentMethodTypeRepository>();
        services.AddScoped<IShippingMethodType, ShippingMethodTypeRepository>();
        services.AddScoped<ICurrency, CurrencyRepository>();

        services.AddScoped<ICart, CartRepository>();
        services.AddScoped<ICartItem, CartItemRepository>();
        services.AddScoped<IAddress, AddressRepository>();
        services.AddScoped<IOrderItem, OrderItemRepository>();
        services.AddScoped<IInventory, InventoryRepository>();
        services.AddScoped<ICategory, CategoryRepository>();
        services.AddScoped<ICoupon, CouponRepository>();
        services.AddScoped<IOrderHistory, OrderHistoryRepository>();
        services.AddScoped<IOrder, OrderRepository>();
        services.AddScoped<IPaymentTransaction, PaymentTransactionRepository>();
        services.AddScoped<IPriceHistory, PriceHistoryRepository>();
        services.AddScoped<IProduct, ProductRepository>();
        services.AddScoped<IProductMedia, ProductMediaRepository>();
        services.AddScoped<IProductVariant, ProductVariantRepository>();
        services.AddScoped<IReview, ReviewRepository>();
        services.AddScoped<IWishlist, WishlistRepository>();
        services.AddScoped<IWishlistItem, WishlistItemRepository>();

        return services;
    }
}
