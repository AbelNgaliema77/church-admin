using ChurchAdmin.Domain.Entities;

namespace ChurchAdmin.Api.Controllers.Teams;

public sealed class TeamResponse
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public static TeamResponse FromEntity(Team team)
    {
        return new TeamResponse
        {
            Id = team.Id,
            Name = team.Name,
            Description = team.Description,
            IsActive = team.IsActive
        };
    }
}