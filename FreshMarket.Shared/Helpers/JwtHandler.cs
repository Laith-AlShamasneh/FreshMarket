using FreshMarket.Shared.Common;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;

namespace FreshMarket.Shared.Helpers;

/// <summary>
/// Create and extracts and validates claims from JWT tokens.
/// Populates user context with authenticated user information.
/// </summary>
public static class JwtHandler
{
    /// <summary>
    /// Generates a JWT token with user claims.
    /// </summary>
    public static string GenerateToken(
        int userId,
        IReadOnlyList<int> roleIds,
        Lang language,
        string secretKey,
        string? issuer = null,
        string? audience = null,
        int expirationDays = 1)
    {
        Guard.AgainstNegativeOrZero(userId, nameof(userId));
        Guard.AgainstNullOrEmpty(roleIds, nameof(roleIds));
        Guard.AgainstNullOrWhiteSpace(secretKey, nameof(secretKey));
        Guard.AgainstNegativeOrZero(expirationDays, nameof(expirationDays));

        try
        {
            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, userId.ToString()),
                new("LangId", ((int)language).ToString()),
                new("RoleIds", string.Join(",", roleIds))
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddDays(expirationDays),
                signingCredentials: credentials);

            var tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.WriteToken(token);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to generate JWT token.", ex);
        }
    }

    /// <summary>
    /// Extracts claims from a JWT token string.
    /// Returns a dictionary of claim type-value pairs.
    /// </summary>
    public static Dictionary<string, string> ExtractClaimsFromToken(string token)
    {
        Guard.AgainstNullOrWhiteSpace(token, nameof(token));

        var claims = new Dictionary<string, string>();

        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            foreach (var claim in jwtToken.Claims)
            {
                claims[claim.Type] = claim.Value;
            }

            return claims;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to extract claims from JWT token.", ex);
        }
    }

    /// <summary>
    /// Extracts a specific claim value from the JWT token.
    /// Returns null if the claim doesn't exist.
    /// </summary>
    public static string? ExtractClaimValue(string token, string claimType)
    {
        Guard.AgainstNullOrWhiteSpace(token, nameof(token));
        Guard.AgainstNullOrWhiteSpace(claimType, nameof(claimType));

        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            return jwtToken.Claims.FirstOrDefault(c => c.Type == claimType)?.Value;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Extracts user ID from JWT token (expects NameIdentifier claim).
    /// </summary>
    public static int? ExtractUserId(string token)
    {
        var userIdStr = ExtractClaimValue(token, ClaimTypes.NameIdentifier);
        return int.TryParse(userIdStr, out var userId) ? userId : null;
    }

    /// <summary>
    /// Extracts language preference from JWT token.
    /// </summary>
    public static Lang? ExtractLanguage(string token)
    {
        var langStr = ExtractClaimValue(token, "LangId");
        if (int.TryParse(langStr, out var langId) && Enum.IsDefined(typeof(Lang), langId))
        {
            return (Lang)langId;
        }
        return null;
    }

    /// <summary>
    /// Extracts role IDs from JWT token (expects comma-separated values).
    /// </summary>
    public static IReadOnlyList<int> ExtractRoleIds(string token)
    {
        var rolesStr = ExtractClaimValue(token, "RoleIds");

        if (string.IsNullOrWhiteSpace(rolesStr))
            return [];

        return [.. rolesStr
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(s => int.TryParse(s, out var id) ? (int?)id : null)
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()];
    }

    /// <summary>
    /// Validates JWT token signature using a public key.
    /// Throws exception if token is invalid.
    /// </summary>
    public static bool ValidateTokenSignature(string token, string publicKey)
    {
        Guard.AgainstNullOrWhiteSpace(token, nameof(token));
        Guard.AgainstNullOrWhiteSpace(publicKey, nameof(publicKey));

        try
        {
            var handler = new JwtSecurityTokenHandler();
            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(publicKey));

            handler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            return validatedToken is JwtSecurityToken;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Checks if a JWT token is expired.
    /// </summary>
    public static bool IsTokenExpired(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            return jwtToken.ValidTo < DateTime.UtcNow;
        }
        catch
        {
            return true;
        }
    }
}
