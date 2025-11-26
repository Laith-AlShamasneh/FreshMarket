using FluentValidation;
using FreshMarket.Application.Services.Implementations.UserManagement;
using FreshMarket.Application.Services.Interfaces.UserManagement;
using FreshMarket.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace FreshMarket.Application;

public static class ApplicationDI
{
    public static IServiceCollection AddApplication(this IServiceCollection services,
        IConfiguration configuration)
    {
        #region User Management Services
        services.AddScoped<IAuthService, AuthService>();
        #endregion


        #region Other Services
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        #endregion

        services.AddInfrastructure(configuration);
        return services;
    }
}
