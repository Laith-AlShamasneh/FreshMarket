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
    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<LoginResponse>>> Login(
        [FromBody] LoginRequest request,
        CancellationToken ct)
    {
        var result = await authService.LoginAsync(request, userContext.Lang, ct);

        var response = responseHandler.Handle(
            result,
            successMessage: MessageType.UserLoginSuccess,
            failureMessage: MessageType.InvalidUserLogin,
            defaultErrorCode: ErrorCodes.Authentication.INVALID_CREDENTIALS);

        return response;
    }

    [HttpPost("refresh-token")]
    public async Task<ApiResponse<LoginResponse>> RefreshToken(
        [FromBody] RefreshTokenRequest request,
        CancellationToken ct)
    {
        var result = await authService.RefreshTokenAsync(request, userContext.Lang, ct);

        var response = responseHandler.Handle(
            result,
            successMessage: MessageType.RetrieveSuccessfully,
            failureMessage: MessageType.InvalidToken,
            defaultErrorCode: ErrorCodes.Authentication.TOKEN_INVALID);

        return response;
    }
}
