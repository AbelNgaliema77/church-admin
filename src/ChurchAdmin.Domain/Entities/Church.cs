using ChurchAdmin.Domain.Common;

namespace ChurchAdmin.Domain.Entities;

public sealed class Church : SoftDeletableEntity
{
    public string Name { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public string? LogoUrl { get; set; }

    public string PrimaryColor { get; set; } = "#111827";

    public string SecondaryColor { get; set; } = "#F9FAFB";

    public string? WelcomeText { get; set; }

    public bool IsActive { get; set; } = true;
}