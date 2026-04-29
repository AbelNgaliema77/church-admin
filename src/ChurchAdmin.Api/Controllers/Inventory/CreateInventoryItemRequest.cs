using ChurchAdmin.Domain.Enums;
using FluentValidation;

namespace ChurchAdmin.Api.Controllers.Inventory;

public sealed class CreateInventoryItemRequest
{
    public string Name { get; set; } = string.Empty;

    public Guid TeamId { get; set; }

    public string Description { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public InventoryCondition Condition { get; set; } = InventoryCondition.Good;

    public InventoryStatus Status { get; set; } = InventoryStatus.PendingApproval;

    public string? ImageUrl { get; set; }
}

public sealed class CreateInventoryItemRequestValidator
    : AbstractValidator<CreateInventoryItemRequest>
{
    public CreateInventoryItemRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(160);

        RuleFor(x => x.TeamId)
            .NotEmpty();

        RuleFor(x => x.Description)
            .NotEmpty()
            .MaximumLength(1000);

        RuleFor(x => x.Quantity)
            .GreaterThan(0);

        RuleFor(x => x.Condition)
            .IsInEnum();

        RuleFor(x => x.Status)
            .IsInEnum();

        RuleFor(x => x.ImageUrl)
            .Must(x => string.IsNullOrWhiteSpace(x) ||
                       Uri.TryCreate(x, UriKind.Absolute, out _))
            .WithMessage("Image URL must be a valid absolute URL.");
    }
}