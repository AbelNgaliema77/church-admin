using ChurchAdmin.Domain.Entities;
using ChurchAdmin.Domain.Enums;

namespace ChurchAdmin.Api.Controllers.Workers;

public sealed class WorkerResponse
{
    public Guid Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public DateOnly DateOfBirth { get; set; }

    public DateOnly StartedServing { get; set; }

    public bool Baptized { get; set; }

    public string Address { get; set; } = string.Empty;

    public WorkerStatus Status { get; set; }

    public List<WorkerTeamResponse> Teams { get; set; } = [];

    public static WorkerResponse FromEntity(Worker worker)
    {
        return new WorkerResponse
        {
            Id = worker.Id,
            FullName = worker.FullName,
            Email = worker.Email,
            Phone = worker.Phone,
            DateOfBirth = worker.DateOfBirth,
            StartedServing = worker.StartedServing,
            Baptized = worker.Baptized,
            Address = worker.Address,
            Status = worker.Status,
            Teams = worker.WorkerTeams
                .Where(x => x.EndDate is null)
                .Select(x => new WorkerTeamResponse
                {
                    TeamId = x.TeamId,
                    TeamName = x.Team.Name,
                    RoleInTeam = x.RoleInTeam,
                    StartDate = x.StartDate
                })
                .ToList()
        };
    }
}

public sealed class WorkerTeamResponse
{
    public Guid TeamId { get; set; }

    public string TeamName { get; set; } = string.Empty;

    public RoleInTeam RoleInTeam { get; set; }

    public DateOnly StartDate { get; set; }
}