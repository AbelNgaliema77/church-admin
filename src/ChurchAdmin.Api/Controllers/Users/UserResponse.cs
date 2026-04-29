using ChurchAdmin.Domain.Entities;
using ChurchAdmin.Domain.Enums;

namespace ChurchAdmin.Api.Controllers.Users;

public sealed class UserResponse
{
    public Guid Id { get; set; }

    public string Email { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string? ExternalProvider { get; set; }

    public UserRole Role { get; set; }

    public string RoleName { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public string? InviteLink { get; set; }

    public static UserResponse FromEntity(User user)
    {
        return new UserResponse
        {
            Id = user.Id,
            Email = user.Email,
            DisplayName = user.DisplayName,
            ExternalProvider = user.ExternalProvider,
            Role = user.Role,
            RoleName = user.Role.ToString(),
            IsActive = user.IsActive,
            InviteLink = null
        };
    }
}