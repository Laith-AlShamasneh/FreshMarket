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
    private readonly int _accessTokenExpirationDays = int.TryParse(
            configuration["Jwt:AccessTokenExpirationMinutes"],
            out var atExp) ? atExp : 60;

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

                var roleIdsForToken = userRoles
                    .Select(r => r.RoleId)
                    .ToArray();

                var accessToken = JwtHandler.GenerateToken(
                    userId: user.UserId,
                    roleIds: roleIdsForToken,
                    language: lang,
                    secretKey: _jwtSecret,
                    issuer: _jwtIssuer,
                    audience: _jwtAudience,
                    expirationDays: _accessTokenExpirationDays);

                user.LastLoginAt = DateTime.UtcNow;
                await _unitOfWork.UserRepository.UpdateLastLoginAsync(user.UserId, ct);

                _logger.LogInformation("User {UserId} logged in successfully", user.UserId);

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
                    PersonalImage = user.Person.ProfilePictureUrl,
                    AccessToken = accessToken,
                };

                return ServiceResult<LoginResponse>.Success(serviceResult);
            },
            _logger,
            "User login",
            request,
            ct);
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
