using FreshMarket.Application.Services.Implementations.Specifications;
using FreshMarket.Application.Services.Implementations.UserManagement;
using FreshMarket.Application.ViewModels.Request.UserManagement;
using FreshMarket.Application.ViewModels.Response.UserManagement;
using FreshMarket.Domain.Interfaces;
using FreshMarket.Shared.Common;
using FreshMarket.Shared.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FreshMarket.Application.Services.Interfaces.UserManagement;

internal class AuthService(
    IUnitOfWork unitOfWork,
    ILogger<AuthService> logger,
    IConfiguration configuration) : IAuthService
{
    private readonly IUnitOfWork _unitOfWork = Guard.AgainstNull(unitOfWork);
    private readonly ILogger<AuthService> _logger = Guard.AgainstNull(logger);
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
        try
        {
            return await ExecutionHelper.ExecuteAsync(
                async () =>
                {
                    Guard.AgainstNull(request);
                    Guard.AgainstNullOrWhiteSpace(request.Username);
                    Guard.AgainstNullOrWhiteSpace(request.Password);

                    var spec = UserSpecifications.GetByUsernameOrEmail(request.Username);

                    var user = await _unitOfWork.UserRepository.FirstOrDefaultAsync(spec, ct);

                    if (user is null)
                    {
                        _logger.LogWarning(
                            "Login attempt with non-existent username: {Username}",
                            request.Username);

                        return ServiceResult<LoginResponse>.Failure(
                            ErrorCodes.Authentication.INVALID_CREDENTIALS,
                            Messages.Get(MessageType.InvalidUserLogin, lang));
                    }

                    if (!PasswordHelper.VerifyPassword(request.Password, user.PasswordHash ?? string.Empty))
                    {
                        _logger.LogWarning(
                            "Failed login attempt for user: {UserId}",
                            user.UserId);

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

                    // Get user roles
                    var roleIds = await _userRepository.GetUserRoleIdsAsync(user.UserId, ct);

                    // Generate tokens
                    var accessToken = JwtHandler.GenerateToken(
                        userId: user.UserId,
                        roleIds: roleIds,
                        language: lang,
                        secretKey: _jwtSecret,
                        issuer: _jwtIssuer,
                        audience: _jwtAudience,
                        expirationMinutes: _accessTokenExpirationMinutes);


                    // Update last login
                    user.LastLoginAt = DateTime.UtcNow;
                    await _userRepository.UpdateAsync(user, ct);

                    _logger.LogInformation(
                        "User {UserId} logged in successfully",
                        user.UserId);

                    var response = new LoginResponse
                    {
                        UserId = user.UserId,
                        Username = user.Person.Username,
                        Email = user.Person.Email,
                        FirstName = user.Person.FirstName,
                        LastName = user.Person.LastName,
                        AccessToken = accessToken,
                        RefreshToken = refreshToken,
                        ExpiresIn = _accessTokenExpirationMinutes * 60,
                        RoleIds = roleIds,
                        LangId = (int)lang
                    };

                    return ServiceResult<LoginResponse>.Success(response);
                },
                _logger,
                "User login",
                request,
                ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login operation failed");
            return ServiceResult<LoginResponse>.Failure(
                ErrorCodes.System.UNEXPECTED,
                Messages.Get(MessageType.SystemProblem, lang));
        }
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
