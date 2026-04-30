using ChurchAdmin.Domain.Enums;
using FluentValidation;

namespace ChurchAdmin.Api.Controllers.Workers;

public sealed class AssignWorkerTeamRequest
{
    public Guid TeamId { get; set; }

    public RoleInTeam RoleInTeam { get; set; } = RoleInTeam.Member;

    public DateOnly StartDate { get; set; }
}

public sealed class AssignWorkerTeamRequestValidator
    : AbstractValidator<AssignWorkerTeamRequest>
{
    public AssignWorkerTeamRequestValidator()
    {
        RuleFor(x => x.TeamId)
            .NotEmpty();

        RuleFor(x => x.RoleInTeam)
            .IsInEnum();

        RuleFor(x => x.StartDate)
            .NotEmpty();
    }
}