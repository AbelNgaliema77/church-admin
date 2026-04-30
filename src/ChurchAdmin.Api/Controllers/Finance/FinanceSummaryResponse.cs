using ChurchAdmin.Domain.Enums;

namespace ChurchAdmin.Api.Controllers.Finance;

public sealed class FinanceSummaryResponse
{
    public DateOnly From { get; set; }

    public DateOnly To { get; set; }

    public ServiceType? ServiceType { get; set; }

    public decimal TotalAmount { get; set; }

    public int EntriesCount { get; set; }

    public Dictionary<FinanceCategory, decimal> TotalsByCategory { get; set; } = [];

    public Dictionary<PaymentMethod, decimal> TotalsByPaymentMethod { get; set; } = [];
}