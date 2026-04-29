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

    public static AuthResponse FromUser(User user, string token)
    {
        return new AuthResponse
        {
            Token = token,
            UserId = user.Id,
            Email = user.Email,
            DisplayName = user.DisplayName,
            Role = user.Role,
            IsActive = user.IsActive
        };
    }
}