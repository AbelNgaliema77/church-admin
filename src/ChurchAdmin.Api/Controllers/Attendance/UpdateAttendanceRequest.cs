using ChurchAdmin.Domain.Enums;
using FluentValidation;

namespace ChurchAdmin.Api.Controllers.Attendance;

public sealed class UpdateAttendanceRequest
{
    public DateOnly ServiceDate { get; set; }

    public ServiceType ServiceType { get; set; }

    public int Men { get; set; }

    public int Women { get; set; }

    public int Children { get; set; }

    public int Visitors { get; set; }

    public string? Notes { get; set; }
}

public sealed class UpdateAttendanceRequestValidator
    : AbstractValidator<UpdateAttendanceRequest>
{
    public UpdateAttendanceRequestValidator()
    {
        RuleFor(x => x.ServiceDate)
            .NotEmpty()
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow));

        RuleFor(x => x.ServiceType)
            .IsInEnum();

        RuleFor(x => x.Men)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.Women)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.Children)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.Visitors)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x)
            .Must(x => x.Men + x.Women + x.Children + x.Visitors > 0)
            .WithMessage("At least one attendance count must be greater than zero.");

        RuleFor(x => x.Notes)
            .MaximumLength(1000);
    }
}