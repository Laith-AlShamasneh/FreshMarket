using FreshMarket.Application.ViewModels.Request.UserManagement;
using FreshMarket.Application.ViewModels.Response.UserManagement;
using FreshMarket.Shared.Common;

namespace FreshMarket.Application.Services.Interfaces.UserManagement;

public interface IAuthService
{
    Task<ServiceResult<LoginResponse>> LoginAsync(
        LoginRequest request,
        Lang lang,
        CancellationToken ct = default);

    Task<ServiceResult<LoginResponse>> RegisterAsync(
        RegisterRequest request,
        Lang lang,
        CancellationToken ct = default);

    Task<ServiceResult<bool>> ChangePasswordAsync(
        int userId,
        ChangePasswordRequest request,
        Lang lang,
        CancellationToken ct = default);

    Task<ServiceResult<bool>> LogoutAsync(
        int userId,
        CancellationToken ct = default);
}
