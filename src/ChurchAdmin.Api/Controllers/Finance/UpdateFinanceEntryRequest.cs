using ChurchAdmin.Domain.Enums;
using FluentValidation;

namespace ChurchAdmin.Api.Controllers.Finance;

public sealed class UpdateFinanceEntryRequest
{
    public DateOnly ServiceDate { get; set; }

    public ServiceType ServiceType { get; set; }

    public FinanceCategory Category { get; set; }

    public decimal Amount { get; set; }

    public PaymentMethod PaymentMethod { get; set; }

    public string? Notes { get; set; }
}

public sealed class UpdateFinanceEntryRequestValidator
    : AbstractValidator<UpdateFinanceEntryRequest>
{
    public UpdateFinanceEntryRequestValidator()
    {
        RuleFor(x => x.ServiceDate)
            .NotEmpty()
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow));

        RuleFor(x => x.ServiceType)
            .IsInEnum();

        RuleFor(x => x.Category)
            .IsInEnum();

        RuleFor(x => x.Amount)
            .GreaterThan(0);

        RuleFor(x => x.PaymentMethod)
            .IsInEnum();

        RuleFor(x => x.Notes)
            .MaximumLength(1000);
    }
}