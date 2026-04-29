using ChurchAdmin.Application.Common.Interfaces;
using ChurchAdmin.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChurchAdmin.Api.Controllers.Dashboard;

[ApiController]
[Route("api/dashboard")]
[Authorize]
public sealed class DashboardController : ControllerBase
{
    private readonly IChurchAdminDbContext _db;

    public DashboardController(IChurchAdminDbContext db)
    {
        _db = db;
    }

    [HttpGet("summary")]
    public async Task<ActionResult<DashboardSummaryResponse>> GetSummary()
    {
        DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);
        DateOnly firstDayOfMonth = new DateOnly(today.Year, today.Month, 1);

        int attendanceThisMonth = await _db.AttendanceRecords
            .Where(x => x.ServiceDate >= firstDayOfMonth &&
                        x.ServiceDate <= today)
            .SumAsync(x => x.Men + x.Women + x.Children + x.Visitors);

        decimal financeThisMonth = await _db.FinanceEntries
            .Where(x => x.ServiceDate >= firstDayOfMonth &&
                        x.ServiceDate <= today)
            .SumAsync(x => x.Amount);

        DashboardSummaryResponse response = new DashboardSummaryResponse
        {
            ActiveWorkers = await _db.Workers
                .CountAsync(x => x.Status == WorkerStatus.Active),

            ActiveTeams = await _db.Teams
                .CountAsync(x => x.IsActive),

            InventoryItems = await _db.InventoryItems
                .CountAsync(),

            PendingInventoryItems = await _db.InventoryItems
                .CountAsync(x => x.Status == InventoryStatus.PendingApproval),

            AttendanceThisMonth = attendanceThisMonth,

            FinanceThisMonth = financeThisMonth
        };

        return response;
    }
}