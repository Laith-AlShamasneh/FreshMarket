using FreshMarket.Application.Helpers;
using FreshMarket.Application.Services.Implementations.Specifications;
using FreshMarket.Application.Services.Interfaces.UserManagement;
using FreshMarket.Application.ViewModels.Request.UserManagement;
using FreshMarket.Application.ViewModels.Response.UserManagement;
using FreshMarket.Domain.Interfaces;
using FreshMarket.Shared.Common;
using FreshMarket.Shared.Helpers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using static FreshMarket.Application.Helpers.StorageUtilityHelper;

namespace FreshMarket.Application.Services.Implementations.UserManagement;

internal class AuthService(
    IUnitOfWork unitOfWork,
    ILogger<AuthService> logger,
    IConfiguration configuration,
    IHostingEnvironment hostingEnvironment) : IAuthService
{
    private readonly IUnitOfWork _unitOfWork = Guard.AgainstNull(unitOfWork);
    private readonly ILogger<AuthService> _logger = Guard.AgainstNull(logger);
    private readonly IHostingEnvironment _hostingEnvironment = Guard.AgainstNull(hostingEnvironment);

    private readonly string _baseUrl = Guard.AgainstNullOrWhiteSpace(
            configuration["Settings:BaseUrl"]);
    private readonly string _jwtSecret = Guard.AgainstNullOrWhiteSpace(
            configuration["Jwt:Secret"]);
    private readonly string _jwtIssuer = Guard.AgainstNullOrWhiteSpace(
            configuration["Jwt:Issuer"] ?? "FreshMarket");
    private readonly string _jwtAudience = Guard.AgainstNullOrWhiteSpace(
            configuration["Jwt:Audience"] ?? "FreshMarketClients");
    private readonly int _accessTokenExpirationMinutes = int.TryParse(
            configuration["Jwt:AccessTokenExpirationMinutes"], out var atExp) ? atExp : 60;
    private readonly int _refreshTokenExpirationDays = int.TryParse(
            configuration["Jwt:RefreshTokenExpirationDays"], out var rtExp) ? rtExp : 7;

    public async Task<ServiceResult<LoginResponse>> LoginAsync(LoginRequest request, Lang lang, CancellationToken ct = default)
    {
        return await ExecutionHelper.ExecuteAsync(
            async () =>
            {
                Guard.AgainstNull(request, nameof(request));

                var userExistSpec = UserSpecifications.GetByUsernameOrEmail(request.Username);
                var user = await _unitOfWork.UserRepository.FirstOrDefaultAsync(userExistSpec, ct);

                if (user is null)
                {
                    _logger.LogWarning("Login attempt with non-existent username: {Username}", request.Username);
                    return ServiceResult<LoginResponse>.Failure(
                        ErrorCodes.Authentication.INVALID_CREDENTIALS,
                        Messages.Get(MessageType.InvalidUserLogin, lang),
                        HttpResponseStatus.Unauthorized);
                }

                if (!PasswordHelper.VerifyPassword(request.Password, user.PasswordHash ?? string.Empty))
                {
                    _logger.LogWarning("Failed login attempt for user: {UserId}", user.UserId);
                    return ServiceResult<LoginResponse>.Failure(
                        ErrorCodes.Authentication.PASSWORD_INCORRECT,
                        Messages.Get(MessageType.PasswordIncorrect, lang),
                        HttpResponseStatus.BadRequest);
                }

                if (!user.IsActive)
                {
                    return ServiceResult<LoginResponse>.Failure(
                        ErrorCodes.Authentication.ACCOUNT_DISABLED,
                        Messages.Get(MessageType.InvalidUserLogin, lang),
                        HttpResponseStatus.Forbidden);
                }

                var userRoleSpec = UserRoleSpecification.GetByUserId(user.UserId);
                var userRoles = await _unitOfWork.UserRoleRepository.ListAsync(userRoleSpec, ct);
                var roleIdsForToken = userRoles.Select(r => r.RoleId).ToArray();

                var accessToken = JwtHandler.GenerateToken(
                    user.UserId, roleIdsForToken, lang, _jwtSecret, _jwtIssuer, _jwtAudience, _accessTokenExpirationMinutes);
                var refreshToken = JwtHandler.GenerateRefreshToken();

                user.RecordLoginSuccess();
                user.SetRefreshToken(refreshToken, _refreshTokenExpirationDays);

                _unitOfWork.UserRepository.Update(user);
                await _unitOfWork.SaveChangesAsync(ct);

                var profilePictureFileResult = await StorageUtilityHelper.GetFileAsync(
                    _hostingEnvironment.WebRootPath,
                    FolderPathNameDictionary.GetValueOrDefault(FolderPathName.UsersImages, string.Empty),
                    user.Person.ProfilePictureUrl ?? string.Empty,
                    _baseUrl,
                    ct);

                var resultData = new LoginResponse
                {
                    Username = user.Username ?? string.Empty,
                    FullName = $"{user.Person.FirstName} {user.Person.LastName}",
                    PersonalImage = profilePictureFileResult?.Url,
                    AccessToken = accessToken,
                    RefreshToken = refreshToken
                };

                return ServiceResult<LoginResponse>.Success(
                    resultData,
                    Messages.Get(MessageType.UserLoginSuccess, lang),
                    HttpResponseStatus.OK);
            },
            _logger, "User login", request, ct);
    }

    public async Task<ServiceResult<LoginResponse>> RefreshTokenAsync(RefreshTokenRequest request, Lang lang, CancellationToken ct = default)
    {
        return await ExecutionHelper.ExecuteAsync(
            async () =>
            {
                Guard.AgainstNull(request);

                var principal = JwtHandler.GetPrincipalFromExpiredToken(request.AccessToken, _jwtSecret);
                if (principal == null)
                {
                    return ServiceResult<LoginResponse>.Failure(
                        ErrorCodes.Authentication.TOKEN_INVALID,
                        Messages.Get(MessageType.InvalidToken, lang),
                        HttpResponseStatus.BadRequest);
                }

                var userIdStr = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!long.TryParse(userIdStr, out var userId))
                {
                    return ServiceResult<LoginResponse>.Failure(
                        ErrorCodes.Authentication.TOKEN_INVALID,
                        Messages.Get(MessageType.InvalidToken, lang),
                        HttpResponseStatus.BadRequest);
                }

                var user = await _unitOfWork.UserRepository.GetByIdAsync(userId, ct);
                if (user == null || user.RefreshToken != request.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
                {
                    return ServiceResult<LoginResponse>.Failure(
                        ErrorCodes.Authentication.TOKEN_EXPIRED,
                        Messages.Get(MessageType.TokenExpired, lang),
                        HttpResponseStatus.Unauthorized);
                }

                var userRoleSpec = UserRoleSpecification.GetByUserId(user.UserId);
                var userRoles = await _unitOfWork.UserRoleRepository.ListAsync(userRoleSpec, ct);
                var roleIdsForToken = userRoles.Select(r => r.RoleId).ToArray();

                var newAccessToken = JwtHandler.GenerateToken(
                    user.UserId, roleIdsForToken, lang, _jwtSecret, _jwtIssuer, _jwtAudience, _accessTokenExpirationMinutes);
                var newRefreshToken = JwtHandler.GenerateRefreshToken();

                user.SetRefreshToken(newRefreshToken, _refreshTokenExpirationDays);

                _unitOfWork.UserRepository.Update(user);
                await _unitOfWork.SaveChangesAsync(ct);

                return ServiceResult<LoginResponse>.Success(
                    new LoginResponse { AccessToken = newAccessToken, RefreshToken = newRefreshToken },
                    Messages.Get(MessageType.RetrieveSuccessfully, lang),
                    HttpResponseStatus.OK);
            },
            _logger, "Refresh Token", request, ct);
    }

    public Task<ServiceResult<bool>> ChangePasswordAsync(int userId, ChangePasswordRequest request, Lang lang, CancellationToken ct = default)
        => throw new NotImplementedException();
    public Task<ServiceResult<bool>> LogoutAsync(int userId, CancellationToken ct = default)
        => throw new NotImplementedException();
    public Task<ServiceResult<LoginResponse>> RegisterAsync(RegisterRequest request, Lang lang, CancellationToken ct = default)
        => throw new NotImplementedException();
}
