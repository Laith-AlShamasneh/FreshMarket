using FreshMarket.Shared.Common;
using System.Globalization;
using System.Security.Claims;
using System.Text.Json;

namespace FreshMarket.API.Middleware;

/// <summary>
/// Populates IUserContext from HttpContext.User (JWT claims) and extracts language from headers.
/// Must run AFTER UseAuthentication() so HttpContext.User is filled by JwtBearer.
/// </summary>
public class UserContextMiddleware(RequestDelegate next, ILogger<UserContextMiddleware> logger)
{
    private readonly RequestDelegate _next = next ?? throw new ArgumentNullException(nameof(next));
    private readonly ILogger<UserContextMiddleware> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private const string RoleIdsClaimName = "RoleIds";
    private const string LangIdClaimName = "LangId";
    private const string AcceptedLangHeader = "accepted-lang";

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            var user = context.User;

            TryPopulateLangFromHeader(context);

            if (user?.Identity?.IsAuthenticated == true)
            {
                EnsureNameIdentifierClaim(user);
                EnsureRoleIdsClaim(user);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "UserContextMiddleware failed to normalize claims");
        }

        await _next(context);
    }

    private void TryPopulateLangFromHeader(HttpContext context)
    {
        try
        {
            if (context.Request.Headers.TryGetValue(AcceptedLangHeader, out var headerValues))
            {
                var raw = headerValues.FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(raw))
                {
                    if (TryParseLang(raw, out var lang))
                    {
                        AddOrReplaceClaim(context.User, LangIdClaimName, ((int)lang).ToString());

                        try
                        {
                            var culture = lang == Lang.Ar ? "ar" : "en";
                            CultureInfo.CurrentCulture = new CultureInfo(culture);
                            CultureInfo.CurrentUICulture = new CultureInfo(culture);
                        }
                        catch {}
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "failed to populate language from header");
        }
    }

    private void EnsureNameIdentifierClaim(ClaimsPrincipal user)
    {
        // If NameIdentifier exists, nothing to do
        if (user.HasClaim(c => c.Type == ClaimTypes.NameIdentifier)) return;

        // Try fallbacks commonly used in tokens
        var fallback = user.FindFirst("sub") ?? user.FindFirst("id") ?? user.FindFirst("userId") ?? user.FindFirst("UserId");
        if (fallback != null)
        {
            AddOrReplaceClaim(user, ClaimTypes.NameIdentifier, fallback.Value);
        }
    }

    private void EnsureRoleIdsClaim(ClaimsPrincipal user)
    {
        try
        {
            // If RoleIds claim already exists, keep it
            if (user.HasClaim(c => c.Type == RoleIdsClaimName)) return;

            var numericRoleIds = new List<int>();

            // 1) Inspect ClaimTypes.Role claims - may be textual or numeric
            var roleClaims = user.FindAll(ClaimTypes.Role).Select(c => c.Value).Where(v => !string.IsNullOrWhiteSpace(v));
            foreach (var rc in roleClaims)
            {
                if (int.TryParse(rc, out var rid))
                    numericRoleIds.Add(rid);
            }

            // 2) Inspect "roles" / "roleIds" / "RoleIds" custom claims (could be CSV or JSON array)
            var candidateNames = new[] { "roles", "roleIds", "RoleIds", "rolesIds" };
            foreach (var name in candidateNames)
            {
                foreach (var claim in user.FindAll(name))
                {
                    var val = claim.Value?.Trim();
                    if (string.IsNullOrEmpty(val)) continue;

                    // JSON array? e.g. ["1","2"] or [1,2]
                    if ((val.StartsWith("[") && val.EndsWith(")")) || (val.StartsWith("[") && val.EndsWith("]")))
                    {
                        try
                        {
                            using var doc = JsonDocument.Parse(val);
                            if (doc.RootElement.ValueKind == JsonValueKind.Array)
                            {
                                foreach (var el in doc.RootElement.EnumerateArray())
                                {
                                    if (el.ValueKind == JsonValueKind.Number && el.TryGetInt32(out var n))
                                        numericRoleIds.Add(n);
                                    else if (el.ValueKind == JsonValueKind.String)
                                    {
                                        var s = el.GetString();
                                        if (int.TryParse(s, out var n2)) numericRoleIds.Add(n2);
                                    }
                                }
                            }
                        }
                        catch
                        {
                            // ignore parse errors
                        }
                    }
                    else
                    {
                        // treat as CSV or single value
                        var parts = val.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                        foreach (var p in parts)
                        {
                            if (int.TryParse(p, out var n)) numericRoleIds.Add(n);
                        }
                    }
                }
            }

            // If we found numeric role ids, add RoleIds claim (CSV) and also add ClaimTypes.Role with numeric string values
            if (numericRoleIds.Any())
            {
                var unique = numericRoleIds.Distinct().ToArray();
                var csv = string.Join(',', unique);
                AddOrReplaceClaim(user, RoleIdsClaimName, csv);

                // also add ClaimTypes.Role claims (useful if other parts expect them)
                var roleClaimsToAdd = unique.Select(id => new Claim(ClaimTypes.Role, id.ToString())).ToList();
                if (roleClaimsToAdd.Count > 0)
                {
                    // add as a supplemental identity so we don't mutate original identity
                    var identity = new ClaimsIdentity(roleClaimsToAdd, "UserContextRoleAugmentation");
                    user.AddIdentity(identity);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "failed to normalize roles into RoleIds claim");
        }
    }

    private static void AddOrReplaceClaim(ClaimsPrincipal principal, string claimType, string claimValue)
    {
        if (principal == null) return;
        try
        {
            // We cannot directly replace claims in an existing identity reliably,
            // so add a new identity carrying the claim. HttpUserContext will read first matching claim.
            var newClaim = new Claim(claimType, claimValue);
            var identity = new ClaimsIdentity(new[] { newClaim }, "UserContextAugmentation");
            principal.AddIdentity(identity);
        }
        catch
        {
            // best effort only
        }
    }

    private static bool TryParseLang(string raw, out Lang lang)
    {
        lang = default;
        if (string.IsNullOrWhiteSpace(raw)) return false;

        raw = raw.Trim();
        // numeric
        if (int.TryParse(raw, out var n) && Enum.IsDefined(typeof(Lang), n))
        {
            lang = (Lang)n;
            return true;
        }

        // normalize variants like en-US -> en
        var first = raw.Split(';')[0].Split(',')[0].Trim();
        if (first.Contains('-')) first = first.Split('-')[0];

        switch (first.ToLowerInvariant())
        {
            case "ar":
                lang = Lang.Ar;
                return true;
            case "en":
                lang = Lang.En;
                return true;
            default:
                return false;
        }
    }
}

