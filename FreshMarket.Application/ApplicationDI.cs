using FluentValidation;
using FreshMarket.Application.Services.Implementations.UserManagement;
using FreshMarket.Application.Services.Interfaces.UserManagement;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace FreshMarket.Application;

public static class ApplicationDI
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        #region User Management Services
        services.AddScoped<IAuthService, AuthService>();
        #endregion


        #region Other Services
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        #endregion

        return services;
    }
}
