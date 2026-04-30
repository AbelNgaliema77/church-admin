namespace ChurchAdmin.Api.Controllers.Dashboard;

public sealed class DashboardSummaryResponse
{
    public int ActiveWorkers { get; set; }

    public int ActiveTeams { get; set; }

    public int InventoryItems { get; set; }

    public int PendingInventoryItems { get; set; }

    public int AttendanceThisMonth { get; set; }

    public decimal FinanceThisMonth { get; set; }
}