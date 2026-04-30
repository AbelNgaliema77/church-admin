using ChurchAdmin.Domain.Common;
using ChurchAdmin.Domain.Enums;

namespace ChurchAdmin.Domain.Entities;

public sealed class User : SoftDeletableEntity
{
    public string Email { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string? PasswordHash { get; set; }

    public string? InviteTokenHash { get; set; }

    public DateTimeOffset? InviteTokenExpiresAt { get; set; }

    public DateTimeOffset? InviteAcceptedAt { get; set; }

    public DateTimeOffset? LastLoginAt { get; set; }

    public string? ExternalProvider { get; set; }

    public string? ExternalProviderUserId { get; set; }

    public UserRole Role { get; set; } = UserRole.Pending;

    public bool IsActive { get; set; } = true;
    public Guid ChurchId { get; set; }
    public Church Church { get; set; } = null!;
}