using ChurchAdmin.Application.Common.Interfaces;
using ChurchAdmin.Domain.Entities;
using ChurchAdmin.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChurchAdmin.Api.Controllers.Reports;

[ApiController]
[Route("api/reports")]
[Authorize(Roles = "Admin,TeamLead")]
public sealed class ReportsController : ControllerBase
{
    private readonly IChurchAdminDbContext _db;

    public ReportsController(IChurchAdminDbContext db)
    {
        _db = db;
    }

    [HttpGet("attendance-trends")]
    public async Task<ActionResult<List<AttendanceTrendResponse>>> GetAttendanceTrends(
        [FromQuery] DateOnly from,
        [FromQuery] DateOnly to,
        [FromQuery] ServiceType? serviceType)
    {
        if (from == default || to == default)
        {
            return BadRequest("From and to dates are required.");
        }

        if (from > to)
        {
            return BadRequest("From date cannot be after to date.");
        }

        IQueryable<AttendanceRecord> query = _db.AttendanceRecords
            .Where(x => x.ServiceDate >= from && x.ServiceDate <= to);

        if (serviceType.HasValue)
        {
            query = query.Where(x => x.ServiceType == serviceType.Value);
        }

        List<AttendanceRecord> records = await query.ToListAsync();

        List<AttendanceTrendResponse> response = records
            .GroupBy(x => new
            {
                x.ServiceDate.Year,
                x.ServiceDate.Month
            })
            .OrderBy(x => x.Key.Year)
            .ThenBy(x => x.Key.Month)
            .Select(x => new AttendanceTrendResponse
            {
                Year = x.Key.Year,
                Month = x.Key.Month,
                MonthName = new DateTime(x.Key.Year, x.Key.Month, 1).ToString("MMM"),
                Men = x.Sum(y => y.Men),
                Women = x.Sum(y => y.Women),
                Children = x.Sum(y => y.Children),
                Visitors = x.Sum(y => y.Visitors),
                Total = x.Sum(y => y.Men + y.Women + y.Children + y.Visitors)
            })
            .ToList();

        return response;
    }

    [HttpGet("attendance-by-service-type")]
    public async Task<ActionResult<List<AttendanceServiceTypeSummaryResponse>>> GetAttendanceByServiceType(
        [FromQuery] DateOnly from,
        [FromQuery] DateOnly to)
    {
        if (from == default || to == default)
        {
            return BadRequest("From and to dates are required.");
        }

        if (from > to)
        {
            return BadRequest("From date cannot be after to date.");
        }

        List<AttendanceRecord> records = await _db.AttendanceRecords
            .Where(x => x.ServiceDate >= from && x.ServiceDate <= to)
            .ToListAsync();

        List<AttendanceServiceTypeSummaryResponse> response = records
            .GroupBy(x => x.ServiceType)
            .OrderBy(x => x.Key)
            .Select(x =>
            {
                int totalAttendance = x.Sum(y => y.Men + y.Women + y.Children + y.Visitors);

                return new AttendanceServiceTypeSummaryResponse
                {
                    ServiceType = x.Key,
                    ServicesCount = x.Count(),
                    TotalAttendance = totalAttendance,
                    AverageAttendance = x.Count() == 0
                        ? 0
                        : Math.Round((decimal)totalAttendance / x.Count(), 2)
                };
            })
            .ToList();

        return response;
    }

    [HttpGet("finance-trends")]
    public async Task<ActionResult<List<FinanceTrendResponse>>> GetFinanceTrends(
        [FromQuery] DateOnly from,
        [FromQuery] DateOnly to,
        [FromQuery] ServiceType? serviceType)
    {
        if (from == default || to == default)
        {
            return BadRequest("From and to dates are required.");
        }

        if (from > to)
        {
            return BadRequest("From date cannot be after to date.");
        }

        IQueryable<FinanceEntry> query = _db.FinanceEntries
            .Where(x => x.ServiceDate >= from && x.ServiceDate <= to);

        if (serviceType.HasValue)
        {
            query = query.Where(x => x.ServiceType == serviceType.Value);
        }

        List<FinanceEntry> entries = await query.ToListAsync();

        List<FinanceTrendResponse> response = entries
            .GroupBy(x => new
            {
                x.ServiceDate.Year,
                x.ServiceDate.Month
            })
            .OrderBy(x => x.Key.Year)
            .ThenBy(x => x.Key.Month)
            .Select(x => new FinanceTrendResponse
            {
                Year = x.Key.Year,
                Month = x.Key.Month,
                MonthName = new DateTime(x.Key.Year, x.Key.Month, 1).ToString("MMM"),
                TotalAmount = x.Sum(y => y.Amount)
            })
            .ToList();

        return response;
    }

    [HttpGet("finance-by-category")]
    public async Task<ActionResult<List<FinanceCategorySummaryResponse>>> GetFinanceByCategory(
        [FromQuery] DateOnly from,
        [FromQuery] DateOnly to,
        [FromQuery] ServiceType? serviceType)
    {
        if (from == default || to == default)
        {
            return BadRequest("From and to dates are required.");
        }

        if (from > to)
        {
            return BadRequest("From date cannot be after to date.");
        }

        IQueryable<FinanceEntry> query = _db.FinanceEntries
            .Where(x => x.ServiceDate >= from && x.ServiceDate <= to);

        if (serviceType.HasValue)
        {
            query = query.Where(x => x.ServiceType == serviceType.Value);
        }

        List<FinanceCategorySummaryResponse> response = await query
            .GroupBy(x => x.Category)
            .OrderBy(x => x.Key)
            .Select(x => new FinanceCategorySummaryResponse
            {
                Category = x.Key,
                TotalAmount = x.Sum(y => y.Amount)
            })
            .ToListAsync();

        return response;
    }
}