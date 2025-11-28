using FreshMarket.Application.Services.Interfaces.UserManagement;
using FreshMarket.Application.ViewModels.Request.UserManagement;
using FreshMarket.Application.ViewModels.Response.UserManagement;
using FreshMarket.Shared.Common;
using Microsoft.AspNetCore.Mvc;

namespace FreshMarket.API.Controllers.UserManagement;

[Route("api/auth")]
[ApiController]
public class AuthController(
    IAuthService authService,
    ResponseHandler responseHandler,
    IUserContext userContext) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ApiResponse<LoginResponse>> Register(
        [FromForm] RegisterRequest request,
        CancellationToken ct)
    {
        var result = await authService.RegisterAsync(request, userContext.Lang, ct);
        return responseHandler.Handle(result);
    }

    [HttpPost("login")]
    public async Task<ApiResponse<LoginResponse>> Login(
        [FromBody] LoginRequest request,
        CancellationToken ct)
    {
        var result = await authService.LoginAsync(request, userContext.Lang, ct);
        return responseHandler.Handle(result);
    }

    [HttpPost("refresh-token")]
    public async Task<ApiResponse<LoginResponse>> RefreshToken(
        [FromBody] RefreshTokenRequest request,
        CancellationToken ct)
    {
        var result = await authService.RefreshTokenAsync(request, userContext.Lang, ct);

        return responseHandler.Handle(result);
    }
}
