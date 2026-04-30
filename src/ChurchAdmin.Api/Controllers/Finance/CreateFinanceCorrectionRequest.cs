using ChurchAdmin.Domain.Enums;
using FluentValidation;

namespace ChurchAdmin.Api.Controllers.Finance;

public sealed class CreateFinanceCorrectionRequest
{
    public FinanceCategory Category { get; set; }

    public decimal Amount { get; set; }

    public PaymentMethod PaymentMethod { get; set; }

    public string Reason { get; set; } = string.Empty;
}

public sealed class CreateFinanceCorrectionRequestValidator
    : AbstractValidator<CreateFinanceCorrectionRequest>
{
    public CreateFinanceCorrectionRequestValidator()
    {
        RuleFor(x => x.Category)
            .IsInEnum();

        RuleFor(x => x.Amount)
            .NotEqual(0);

        RuleFor(x => x.PaymentMethod)
            .IsInEnum();

        RuleFor(x => x.Reason)
            .NotEmpty()
            .MaximumLength(1000);
    }
}