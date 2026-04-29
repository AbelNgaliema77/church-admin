using FluentValidation;

namespace ChurchAdmin.Api.Controllers.Teams;

public sealed class UpdateTeamRequest
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsActive { get; set; }
}

public sealed class UpdateTeamRequestValidator : AbstractValidator<UpdateTeamRequest>
{
    public UpdateTeamRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(120);

        RuleFor(x => x.Description)
            .MaximumLength(500);
    }
}