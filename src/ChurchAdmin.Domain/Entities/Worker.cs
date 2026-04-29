using ChurchAdmin.Domain.Common;
using ChurchAdmin.Domain.Enums;

namespace ChurchAdmin.Domain.Entities;

public sealed class Worker : SoftDeletableEntity
{
    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public DateOnly DateOfBirth { get; set; }

    public DateOnly StartedServing { get; set; }

    public bool Baptized { get; set; }

    public string Address { get; set; } = string.Empty;

    public WorkerStatus Status { get; set; } = WorkerStatus.Active;

    public ICollection<WorkerTeam> WorkerTeams { get; set; } = [];
}
