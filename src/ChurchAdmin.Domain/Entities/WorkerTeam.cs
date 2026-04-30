using ChurchAdmin.Domain.Common;
using ChurchAdmin.Domain.Enums;

namespace ChurchAdmin.Domain.Entities;

public sealed class WorkerTeam : AuditableEntity
{
    public Guid WorkerId { get; set; }

    public Worker Worker { get; set; } = null!;

    public Guid TeamId { get; set; }

    public Team Team { get; set; } = null!;

    public RoleInTeam RoleInTeam { get; set; } = RoleInTeam.Member;

    public DateOnly StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public bool IsCurrent => EndDate is null;
}
