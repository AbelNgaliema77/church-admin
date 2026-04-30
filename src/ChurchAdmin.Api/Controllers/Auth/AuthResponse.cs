using ChurchAdmin.Domain.Entities;
using ChurchAdmin.Domain.Enums;

namespace ChurchAdmin.Api.Controllers.Auth;

public sealed class AuthResponse
{
    public string Token { get; set; } = string.Empty;

    public Guid UserId { get; set; }

    public string Email { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public UserRole Role { get; set; }

    public bool IsActive { get; set; }

    public Guid? ChurchId { get; set; }

    public string? ChurchSlug { get; set; }

    public string? ChurchName { get; set; }

    public static AuthResponse FromUser(User user, string token)
    {
        return new AuthResponse
        {
            Token = token,
            UserId = user.Id,
            Email = user.Email,
            DisplayName = user.DisplayName,
            Role = user.Role,
            IsActive = user.IsActive,
            ChurchId = user.ChurchId == Guid.Empty ? null : user.ChurchId,
            ChurchSlug = user.Church?.Slug,
            ChurchName = user.Church?.Name
        };
    }
}
