using ChurchAdmin.Domain.Enums;
using FluentValidation;

namespace ChurchAdmin.Api.Controllers.Workers;

public sealed class CreateWorkerRequest
{
    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public DateOnly DateOfBirth { get; set; }

    public DateOnly StartedServing { get; set; }

    public bool Baptized { get; set; }

    public string Address { get; set; } = string.Empty;

    public Guid TeamId { get; set; }

    public RoleInTeam RoleInTeam { get; set; } = RoleInTeam.Member;
}

public sealed class CreateWorkerRequestValidator
    : AbstractValidator<CreateWorkerRequest>
{
    public CreateWorkerRequestValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(256);

        RuleFor(x => x.Phone)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.DateOfBirth)
            .NotEmpty()
            .LessThan(DateOnly.FromDateTime(DateTime.UtcNow));

        RuleFor(x => x.StartedServing)
            .NotEmpty()
            .GreaterThanOrEqualTo(x => x.DateOfBirth);

        RuleFor(x => x.Address)
            .NotEmpty()
            .MaximumLength(500);

        RuleFor(x => x.TeamId)
            .NotEmpty();

        RuleFor(x => x.RoleInTeam)
            .IsInEnum();
    }
}