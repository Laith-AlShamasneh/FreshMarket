using FreshMarket.Application.Helpers;
using FreshMarket.Application.Services.Implementations.Specifications;
using FreshMarket.Application.Services.Interfaces.UserManagement;
using FreshMarket.Application.ViewModels.Request.UserManagement;
using FreshMarket.Application.ViewModels.Response.UserManagement;
using FreshMarket.Domain.Entities.UserManagement;
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

    public async Task<ServiceResult<LoginResponse>> RegisterAsync(RegisterRequest request, Lang lang, CancellationToken ct = default)
    {
        return await ExecutionHelper.ExecuteAsync(async () =>
        {
            Guard.AgainstNull(request);

            var existingUserSpec = UserSpecifications.GetByUsernameOrEmail(request.Email);
            var exists = await _unitOfWork.UserRepository.CountAsync(existingUserSpec, ct) > 0;

            if (exists)
            {
                return ServiceResult<LoginResponse>.Failure(
                    ErrorCodes.User.EMAIL_IN_USE,
                    Messages.Get(MessageType.EmailAlreadyExists, lang),
                    HttpResponseStatus.Conflict);
            }

            await using var transaction = await _unitOfWork.BeginTransactionAsync(ct);
            try
            {
                var profilePicture = request.ProfilePicture is not null ? await StorageUtilityHelper.SaveFileAsync(_hostingEnvironment.WebRootPath,
                    FolderPathNameDictionary.GetValueOrDefault(FolderPathName.UsersImages,
                    string.Empty),
                    request.ProfilePicture,
                    FileUploadType.UserProfileImage,
                    null,
                    false,
                    ct) : null;

                var person = new Person
                {
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Email = request.Email,
                    PhoneNumber = request.PhoneNumber,
                    ProfilePictureUrl = profilePicture?.FileName ?? null,
                    IsActive = true
                };

                var user = new User
                {
                    Person = person,
                    Username = request.Username,
                    PasswordHash = PasswordHelper.HashPassword(request.Password),
                    IsActive = true,
                    IsEmailConfirmed = false
                };

                var userRoleSpec = RoleSpecification.GetDefaultRole();
                var defaultRole = await _unitOfWork.RoleRepository
                    .FirstOrDefaultAsync(userRoleSpec, ct);

                if (defaultRole != null)
                {
                    user.UserRoles.Add(new UserRole { Role = defaultRole });
                }

                await _unitOfWork.UserRepository.AddAsync(user, ct);
                await _unitOfWork.SaveChangesAsync(ct);

                var authResponse = await GenerateAuthResponseAsync(user, lang, false, null, ct);

                await transaction.CommitAsync(ct);

                return ServiceResult<LoginResponse>.Success(
                    authResponse,
                    Messages.Get(MessageType.SaveSuccessfully, lang),
                    HttpResponseStatus.Created);
            }
            catch
            {
                await transaction.RollbackAsync(ct);
                throw;
            }

        }, _logger, "User registration", request, ct);
    }

    public async Task<ServiceResult<LoginResponse>> LoginAsync(LoginRequest request, Lang lang, CancellationToken ct = default)
    {
        return await ExecutionHelper.ExecuteAsync(async () =>
        {
            Guard.AgainstNull(request);

            var userExistSpec = UserSpecifications.GetByUsernameOrEmail(request.Username);
            var user = await _unitOfWork.UserRepository.FirstOrDefaultAsync(userExistSpec, ct);

            if (user is null)
            {
                _logger.LogWarning("Login attempt with non-existent username: {Username}", request.Username);
                return ServiceResult<LoginResponse>.Failure(
                    ErrorCodes.Authentication.INVALID_CREDENTIALS,
                    Messages.Get(MessageType.InvalidUserLogin, lang),
                    HttpResponseStatus.BadRequest);
            }

            if (!PasswordHelper.VerifyPassword(request.Password, user.PasswordHash ?? string.Empty))
            {
                _logger.LogWarning("Login failed: Incorrect password for user ID {UserId}", user.UserId);
                return ServiceResult<LoginResponse>.Failure(
                    ErrorCodes.Authentication.INVALID_CREDENTIALS,
                    Messages.Get(MessageType.InvalidUserLogin, lang),
                    HttpResponseStatus.BadRequest);
            }

            if (!user.IsActive)
            {
                return ServiceResult<LoginResponse>.Failure(
                    ErrorCodes.Authentication.ACCOUNT_DISABLED,
                    Messages.Get(MessageType.AccountLocked, lang),
                    HttpResponseStatus.Forbidden);
            }

            var authResponse = await GenerateAuthResponseAsync(user, lang, false, null, ct);

            return ServiceResult<LoginResponse>.Success(
                authResponse,
                Messages.Get(MessageType.UserLoginSuccess, lang),
                HttpResponseStatus.OK);

        }, _logger, "User login", request, ct);
    }

    public async Task<ServiceResult<LoginResponse>> RefreshTokenAsync(RefreshTokenRequest request, Lang lang, CancellationToken ct = default)
    {
        return await ExecutionHelper.ExecuteAsync(async () =>
        {
            Guard.AgainstNull(request);

            var principal = JwtHandler.GetPrincipalFromExpiredToken(request.AccessToken, _jwtSecret);
            if (principal == null)
                return ServiceResult<LoginResponse>.Failure(ErrorCodes.Authentication.TOKEN_INVALID, Messages.Get(MessageType.InvalidToken, lang), HttpResponseStatus.BadRequest);

            var userIdStr = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!long.TryParse(userIdStr, out var userId))
                return ServiceResult<LoginResponse>.Failure(ErrorCodes.Authentication.TOKEN_INVALID, Messages.Get(MessageType.InvalidToken, lang), HttpResponseStatus.BadRequest);

            var user = await _unitOfWork.UserRepository.GetByIdAsync(userId, ct);

            if (user == null || user.RefreshToken != request.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                return ServiceResult<LoginResponse>.Failure(
                    ErrorCodes.Authentication.TOKEN_EXPIRED,
                    Messages.Get(MessageType.TokenExpired, lang),
                    HttpResponseStatus.Unauthorized);
            }

            var authResponse = await GenerateAuthResponseAsync(user, lang, true, null, ct);

            return ServiceResult<LoginResponse>.Success(
                authResponse,
                Messages.Get(MessageType.RetrieveSuccessfully, lang),
                HttpResponseStatus.OK);

        }, _logger, "Refresh Token", request, ct);
    }

    public Task<ServiceResult<bool>> ChangePasswordAsync(int userId, ChangePasswordRequest request, Lang lang, CancellationToken ct = default)
        => throw new NotImplementedException();
    public Task<ServiceResult<bool>> LogoutAsync(int userId, CancellationToken ct = default)
        => throw new NotImplementedException();

    #region Private Helpers
    private async Task<LoginResponse> GenerateAuthResponseAsync(User user, Lang lang, bool isRefreshTokenGenerate, string? ipAddress, CancellationToken ct)
    {
        var userRoleSpec = UserRoleSpecification.GetByUserId(user.UserId);
        var userRoles = await _unitOfWork.UserRoleRepository.ListAsync(userRoleSpec, ct);
        var roleIdsForToken = userRoles.Select(r => r.RoleId).ToArray();

        var accessToken = JwtHandler.GenerateToken(
            user.UserId,
            roleIdsForToken,
            lang,
            _jwtSecret,
            _jwtIssuer,
            _jwtAudience,
            _accessTokenExpirationMinutes);

        var refreshToken = JwtHandler.GenerateRefreshToken();

        user.RecordLoginSuccess();
        user.SetRefreshToken(refreshToken, _refreshTokenExpirationDays);

        await _unitOfWork.SignInLogRepository.LogAttemptAsync(user.UserId, ipAddress, true, ct);
        _unitOfWork.UserRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(ct);

        string? imageUrl = null;

        if (!isRefreshTokenGenerate)
        {
            var profilePictureFileResult = await StorageUtilityHelper.GetFileAsync(
            _hostingEnvironment.WebRootPath,
            FolderPathNameDictionary.GetValueOrDefault(FolderPathName.UsersImages, string.Empty),
            user.Person.ProfilePictureUrl ?? string.Empty,
            _baseUrl,
            ct);

            imageUrl = profilePictureFileResult?.Url;
        }

        return new LoginResponse
        {
            Username = user.Username ?? string.Empty,
            FullName = $"{user.Person.FirstName} {user.Person.LastName}",
            PersonalImage = imageUrl,
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };
    }
    #endregion
}
