using ChurchAdmin.Domain.Common;

namespace ChurchAdmin.Domain.Entities;

public sealed class Team : SoftDeletableEntity
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<WorkerTeam> WorkerTeams { get; set; } = [];
    public Guid ChurchId { get; set; }
    public Church Church { get; set; } = null!;
}
