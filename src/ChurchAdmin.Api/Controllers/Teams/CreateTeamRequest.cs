using FluentValidation;

namespace ChurchAdmin.Api.Controllers.Teams;

public sealed class CreateTeamRequest
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
}

public sealed class CreateTeamRequestValidator : AbstractValidator<CreateTeamRequest>
{
    public CreateTeamRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(120);

        RuleFor(x => x.Description)
            .MaximumLength(500);
    }
}