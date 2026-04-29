using ChurchAdmin.Domain.Common;
using ChurchAdmin.Domain.Enums;

namespace ChurchAdmin.Domain.Entities;

public sealed class FinanceEntry : SoftDeletableEntity
{
    public DateOnly ServiceDate { get; set; }

    public ServiceType ServiceType { get; set; }

    public FinanceCategory Category { get; set; }

    public decimal Amount { get; set; }

    public PaymentMethod PaymentMethod { get; set; }

    public bool IsVerified { get; set; }

    public DateTimeOffset? VerifiedAt { get; set; }

    public string? VerifiedBy { get; set; }

    public Guid? CorrectionOfFinanceEntryId { get; set; }

    public FinanceEntry? CorrectionOfFinanceEntry { get; set; }

    public string? Notes { get; set; }
}
