using ChurchAdmin.Domain.Enums;

namespace ChurchAdmin.Api.Controllers.Reports;

public sealed class FinanceCategorySummaryResponse
{
    public FinanceCategory Category { get; set; }

    public decimal TotalAmount { get; set; }
}