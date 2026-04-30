using System.Security.Claims;
using ChurchAdmin.Application.Common.Interfaces;

namespace ChurchAdmin.Api.Common;

public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string UserId =>
        _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? "system";

    public string Email =>
        _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Email)
        ?? _httpContextAccessor.HttpContext?.User.Identity?.Name
        ?? "system";

    public Guid? ChurchId
    {
        get
        {
            string? raw = _httpContextAccessor.HttpContext?.User.FindFirstValue("churchId");
            return Guid.TryParse(raw, out Guid id) ? id : null;
        }
    }
}
