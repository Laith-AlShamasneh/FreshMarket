using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace FreshMarket.Shared.Common;

public interface IUserContext
{
    int UserId { get; }
    int LangId { get; }
    bool IsAuthenticated { get; }
    string? Role { get; }
}

public class HttpUserContext(IHttpContextAccessor accessor) : IUserContext
{
    private readonly HttpContext? _ctx = accessor.HttpContext;

    public bool IsAuthenticated => _ctx?.User.Identity?.IsAuthenticated == true;

    public int UserId => IsAuthenticated
        ? int.Parse(_ctx!.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0")
        : 0;

    public int LangId => IsAuthenticated
        ? int.Parse(_ctx!.User.FindFirst("LangId")?.Value ?? "1")
        : 1;

    public string? Role => _ctx?.User.FindFirst(ClaimTypes.Role)?.Value;
}
