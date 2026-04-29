using ChurchAdmin.Domain.Entities;
using ChurchAdmin.Domain.Enums;

namespace ChurchAdmin.Api.Controllers.Finance;

public sealed class FinanceEntryResponse
{
    public Guid Id { get; set; }

    public DateOnly ServiceDate { get; set; }

    public ServiceType ServiceType { get; set; }

    public FinanceCategory Category { get; set; }

    public decimal Amount { get; set; }

    public PaymentMethod PaymentMethod { get; set; }

    public bool IsVerified { get; set; }

    public DateTimeOffset? VerifiedAt { get; set; }

    public string? VerifiedBy { get; set; }

    public Guid? CorrectionOfFinanceEntryId { get; set; }

    public string? Notes { get; set; }

    public static FinanceEntryResponse FromEntity(FinanceEntry entry)
    {
        return new FinanceEntryResponse
        {
            Id = entry.Id,
            ServiceDate = entry.ServiceDate,
            ServiceType = entry.ServiceType,
            Category = entry.Category,
            Amount = entry.Amount,
            PaymentMethod = entry.PaymentMethod,
            IsVerified = entry.IsVerified,
            VerifiedAt = entry.VerifiedAt,
            VerifiedBy = entry.VerifiedBy,
            CorrectionOfFinanceEntryId = entry.CorrectionOfFinanceEntryId,
            Notes = entry.Notes
        };
    }
}