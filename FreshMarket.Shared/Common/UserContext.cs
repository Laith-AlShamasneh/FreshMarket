using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace FreshMarket.Shared.Common;

public interface IUserContext
{
    int UserId { get; }
    int LangId { get; }
    Lang Lang { get; }
    bool IsAuthenticated { get; }
    IReadOnlyList<int> RoleIds { get; }
    bool HasRole(int roleId);
}

public class HttpUserContext(IHttpContextAccessor accessor) : IUserContext
{
    private readonly HttpContext? _ctx = accessor.HttpContext;
    private int? _cachedLangId;

    public bool IsAuthenticated => _ctx?.User.Identity?.IsAuthenticated == true;

    public int UserId
    {
        get
        {
            if (!IsAuthenticated) return 0;
            var raw = _ctx!.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(raw, out var id) ? id : 0;
        }
    }

    public int LangId
    {
        get
        {
            if (_cachedLangId.HasValue)
                return _cachedLangId.Value;

            var headerRaw = _ctx?.Request.Headers["accepted-lang"].FirstOrDefault();
            if (TryParseLang(headerRaw, out var langFromHeader))
            {
                _cachedLangId = (int)langFromHeader;
                return _cachedLangId.Value;
            }

            if (IsAuthenticated)
            {
                var claimRaw = _ctx!.User.FindFirst("LangId")?.Value;
                if (TryParseLang(claimRaw, out var langFromClaim))
                {
                    _cachedLangId = (int)langFromClaim;
                    return _cachedLangId.Value;
                }
            }

            _cachedLangId = (int)Lang.Ar;
            return _cachedLangId.Value;
        }
    }

    public Lang Lang => Enum.IsDefined(typeof(Lang), LangId) ? (Lang)LangId : Lang.En;

    public IReadOnlyList<int> RoleIds
    {
        get
        {
            if (!IsAuthenticated) return [];

            var raw = _ctx!.User.FindFirst("RoleIds")?.Value;
            if (!string.IsNullOrWhiteSpace(raw))
            {
                return [.. raw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                          .Select(s => int.TryParse(s, out var v) ? v : (int?)null)
                          .Where(v => v.HasValue)
                          .Select(v => v!.Value)
                          .Distinct()];
            }

            var singleRole = _ctx.User.FindFirst(ClaimTypes.Role)?.Value;
            if (int.TryParse(singleRole, out var roleId))
                return [roleId];

            return [];
        }
    }

    public bool HasRole(int roleId) => RoleIds.Contains(roleId);

    private static bool TryParseLang(string? raw, out Lang lang)
    {
        lang = default;

        if (string.IsNullOrWhiteSpace(raw))
            return false;

        raw = raw.Trim();

        // Numeric form
        if (int.TryParse(raw, out var num) && Enum.IsDefined(typeof(Lang), num))
        {
            lang = (Lang)num;
            return true;
        }

        // Normalize locale patterns like en-US, ar-JO
        if (raw.Contains('-'))
            raw = raw.Split('-')[0];

        // Code form
        switch (raw.ToLowerInvariant())
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
