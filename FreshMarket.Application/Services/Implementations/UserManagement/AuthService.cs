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
    //private readonly IConfiguration _configuration = Guard.AgainstNull(configuration);
    private readonly IHostingEnvironment _hostingEnvironment = Guard.AgainstNull(hostingEnvironment);

    private readonly string _baseUrl = Guard.AgainstNullOrWhiteSpace(
            configuration["Settings:BaseUrl"]);
    private readonly string _jwtSecret = Guard.AgainstNullOrWhiteSpace(
            configuration["Jwt:Secret"],
            "Jwt:Secret configuration is missing");
    private readonly string _jwtIssuer = Guard.AgainstNullOrWhiteSpace(
            configuration["Jwt:Issuer"] ?? "FreshMarket",
            "Jwt:Issuer");
    private readonly string _jwtAudience = Guard.AgainstNullOrWhiteSpace(
            configuration["Jwt:Audience"] ?? "FreshMarketClients",
            "Jwt:Audience");
    private readonly int _accessTokenExpirationMinutes = int.TryParse(
            configuration["Jwt:AccessTokenExpirationMinutes"],
            out var atExp) ? atExp : 60;

    private readonly int _refreshTokenExpirationDays = int.TryParse(
            configuration["Jwt:RefreshTokenExpirationDays"],
            out var rtExp) ? rtExp : 7;

    public Task<ServiceResult<bool>> ChangePasswordAsync(int userId, ChangePasswordRequest request, Lang lang, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public async Task<ServiceResult<LoginResponse>> LoginAsync(LoginRequest request, Lang lang, CancellationToken ct = default)
    {
        return await ExecutionHelper.ExecuteAsync(
            async () =>
            {
                Guard.AgainstNull(request);
                Guard.AgainstNullOrWhiteSpace(request.Username);
                Guard.AgainstNullOrWhiteSpace(request.Password);

                var userExistSpec = UserSpecifications.GetByUsernameOrEmail(request.Username);
                var user = await _unitOfWork.UserRepository.FirstOrDefaultAsync(userExistSpec, ct);

                if (user is null)
                {
                    _logger.LogWarning("Login attempt with non-existent username: {Username}", request.Username);

                    return ServiceResult<LoginResponse>.Failure(
                        ErrorCodes.Authentication.INVALID_CREDENTIALS,
                        Messages.Get(MessageType.InvalidUserLogin, lang));
                }

                if (!PasswordHelper.VerifyPassword(request.Password, user.PasswordHash ?? string.Empty))
                {
                    _logger.LogWarning("Failed login attempt for user: {UserId}", user.UserId);

                    return ServiceResult<LoginResponse>.Failure(
                        ErrorCodes.Authentication.PASSWORD_INCORRECT,
                        Messages.Get(MessageType.PasswordIncorrect, lang));
                }

                if (!user.IsActive)
                {
                    return ServiceResult<LoginResponse>.Failure(
                        ErrorCodes.Authentication.ACCOUNT_DISABLED,
                        Messages.Get(MessageType.InvalidUserLogin, lang));
                }

                var userRoleSpec = UserRoleSpecification.GetByUserId(user.UserId);
                var userRoles = await _unitOfWork.UserRoleRepository.ListAsync(userRoleSpec, ct);
                var roleIdsForToken = userRoles.Select(r => r.RoleId).ToArray();

                // 1. Generate Access Token
                var accessToken = JwtHandler.GenerateToken(
                    userId: user.UserId,
                    roleIds: roleIdsForToken,
                    language: lang,
                    secretKey: _jwtSecret,
                    issuer: _jwtIssuer,
                    audience: _jwtAudience,
                    expirationDays: _accessTokenExpirationMinutes);

                // 2. Generate Refresh Token
                var refreshToken = JwtHandler.GenerateRefreshToken();

                // 3. Save to User Entity (Domain Logic)
                user.RecordLoginSuccess();
                user.SetRefreshToken(refreshToken, _refreshTokenExpirationDays);
                                                                                                                                                          
                _unitOfWork.UserRepository.Update(user);
                await _unitOfWork.SaveChangesAsync(ct);

                var profilePictureFileResult = await StorageUtilityHelper.GetFileAsync(
                    rootPath: _hostingEnvironment.WebRootPath,
                    pathOrRelativeFolder: FolderPathNameDictionary.TryGetValue(FolderPathName.UsersImages, out var folder) ? folder : string.Empty,
                    fileNameWithExtension: user.Person.ProfilePictureUrl ?? string.Empty,
                    baseUrl: _baseUrl,
                    cancellationToken: ct);

                var serviceResult = new LoginResponse
                {
                    Username = user.Username ?? string.Empty,
                    FullName = $"{user.Person.FirstName} {user.Person.LastName}",
                    PersonalImage = profilePictureFileResult?.Url,
                    AccessToken = accessToken,
                };

                return ServiceResult<LoginResponse>.Success(serviceResult);
            },
            _logger,
            "User login",
            request,
            ct);
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
                        Messages.Get(MessageType.InvalidToken));
                }

                var userIdStr = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!long.TryParse(userIdStr, out var userId))
                {
                    return ServiceResult<LoginResponse>.Failure(
                        ErrorCodes.Authentication.TOKEN_INVALID,
                        Messages.Get(MessageType.InvalidToken));
                }

                // 2. Fetch User with Refresh Token
                var user = await _unitOfWork.UserRepository.GetByIdAsync(userId, ct);
                if (user == null || user.RefreshToken != request.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
                {
                    return ServiceResult<LoginResponse>.Failure(
                        ErrorCodes.Authentication.TOKEN_EXPIRED,
                        Messages.Get(MessageType.TokenExpired));
                }

                // 3. Generate New Tokens
                var userRoleSpec = UserRoleSpecification.GetByUserId(user.UserId);
                var userRoles = await _unitOfWork.UserRoleRepository.ListAsync(userRoleSpec, ct);
                var roleIdsForToken = userRoles.Select(r => r.RoleId).ToArray();

                var newAccessToken = JwtHandler.GenerateToken(
                    userId: user.UserId,
                    roleIds: roleIdsForToken,
                    language: lang,
                    secretKey: _jwtSecret,
                    issuer: _jwtIssuer,
                    audience: _jwtAudience,
                    expirationDays: _accessTokenExpirationMinutes);

                var newRefreshToken = JwtHandler.GenerateRefreshToken();

                // 4. Rotate Refresh Token (Security Best Practice)
                user.SetRefreshToken(newRefreshToken, _refreshTokenExpirationDays);

                _unitOfWork.UserRepository.Update(user);
                await _unitOfWork.SaveChangesAsync(ct);

                return ServiceResult<LoginResponse>.Success(new LoginResponse
                {
                    AccessToken = newAccessToken,
                    RefreshToken = newRefreshToken,
                });
            },
            _logger, "Refresh Token", request, ct);
    }

    public Task<ServiceResult<bool>> LogoutAsync(int userId, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<ServiceResult<LoginResponse>> RegisterAsync(RegisterRequest request, Lang lang, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}
