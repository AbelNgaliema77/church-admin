using ChurchAdmin.Api.Common;
using ChurchAdmin.Application.Common.Interfaces;
using ChurchAdmin.Domain.Entities;
using ChurchAdmin.Domain.Enums;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChurchAdmin.Api.Controllers.Attendance;

[ApiController]
[Route("api/attendance")]
[Authorize(Roles = "Admin,TeamLead")]
public sealed class AttendanceController : ControllerBase
{
    private readonly IChurchAdminDbContext _db;
    private readonly IAuditService _auditService;
    private readonly IValidator<CreateAttendanceRequest> _createValidator;
    private readonly IValidator<UpdateAttendanceRequest> _updateValidator;

    public AttendanceController(
        IChurchAdminDbContext db,
        IAuditService auditService,
        IValidator<CreateAttendanceRequest> createValidator,
        IValidator<UpdateAttendanceRequest> updateValidator)
    {
        _db = db;
        _auditService = auditService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpGet]
    public async Task<ActionResult<List<AttendanceResponse>>> GetAll(
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        [FromQuery] ServiceType? serviceType)
    {
        IQueryable<AttendanceRecord> query = _db.AttendanceRecords;

        if (from.HasValue)
        {
            query = query.Where(x => x.ServiceDate >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(x => x.ServiceDate <= to.Value);
        }

        if (serviceType.HasValue)
        {
            query = query.Where(x => x.ServiceType == serviceType.Value);
        }

        List<AttendanceRecord> records = await query
            .OrderByDescending(x => x.ServiceDate)
            .ThenBy(x => x.ServiceType)
            .ToListAsync();

        return records.Select(AttendanceResponse.FromEntity).ToList();
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AttendanceResponse>> GetById(Guid id)
    {
        AttendanceRecord? record = await _db.AttendanceRecords
            .FirstOrDefaultAsync(x => x.Id == id);

        if (record is null)
        {
            return NotFound();
        }

        return AttendanceResponse.FromEntity(record);
    }

    [HttpGet("summary")]
    public async Task<ActionResult<AttendanceSummaryResponse>> GetSummary(
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

        if (records.Count == 0)
        {
            return new AttendanceSummaryResponse
            {
                From = from,
                To = to,
                ServiceType = serviceType
            };
        }

        AttendanceRecord highest = records.OrderByDescending(x => x.Total).First();
        AttendanceRecord lowest = records.OrderBy(x => x.Total).First();

        int totalAttendance = records.Sum(x => x.Total);

        return new AttendanceSummaryResponse
        {
            From = from,
            To = to,
            ServiceType = serviceType,
            ServicesCount = records.Count,
            TotalMen = records.Sum(x => x.Men),
            TotalWomen = records.Sum(x => x.Women),
            TotalChildren = records.Sum(x => x.Children),
            TotalVisitors = records.Sum(x => x.Visitors),
            TotalAttendance = totalAttendance,
            AverageAttendance = Math.Round((decimal)totalAttendance / records.Count, 2),
            HighestAttendanceDate = highest.ServiceDate,
            HighestAttendanceTotal = highest.Total,
            LowestAttendanceDate = lowest.ServiceDate,
            LowestAttendanceTotal = lowest.Total
        };
    }

    [HttpPost]
    public async Task<ActionResult<AttendanceResponse>> Create(
        CreateAttendanceRequest request)
    {
        FluentValidation.Results.ValidationResult validation =
            await _createValidator.ValidateAsync(request);

        if (!validation.IsValid)
        {
            return validation.ToBadRequest();
        }

        bool duplicateExists = await _db.AttendanceRecords.AnyAsync(x =>
            x.ServiceDate == request.ServiceDate &&
            x.ServiceType == request.ServiceType);

        if (duplicateExists)
        {
            return BadRequest("Attendance already exists for this service date and service type.");
        }

        AttendanceRecord record = new AttendanceRecord
        {
            Id = Guid.NewGuid(),
            ServiceDate = request.ServiceDate,
            ServiceType = request.ServiceType,
            Men = request.Men,
            Women = request.Women,
            Children = request.Children,
            Visitors = request.Visitors,
            Notes = request.Notes?.Trim()
        };

        _db.AttendanceRecords.Add(record);

        await _db.SaveChangesAsync();

        AttendanceResponse response = AttendanceResponse.FromEntity(record);

        await _auditService.LogAsync(
            "AttendanceRecord",
            record.Id,
            AuditAction.Created,
            after: response);

        return CreatedAtAction(
            nameof(GetById),
            new { id = record.Id },
            response);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        UpdateAttendanceRequest request)
    {
        FluentValidation.Results.ValidationResult validation =
            await _updateValidator.ValidateAsync(request);

        if (!validation.IsValid)
        {
            return validation.ToBadRequest();
        }

        AttendanceRecord? record = await _db.AttendanceRecords
            .FirstOrDefaultAsync(x => x.Id == id);

        if (record is null)
        {
            return NotFound();
        }

        bool duplicateExists = await _db.AttendanceRecords.AnyAsync(x =>
            x.Id != id &&
            x.ServiceDate == request.ServiceDate &&
            x.ServiceType == request.ServiceType);

        if (duplicateExists)
        {
            return BadRequest("Another attendance record already exists for this service date and service type.");
        }

        AttendanceResponse before = AttendanceResponse.FromEntity(record);

        record.ServiceDate = request.ServiceDate;
        record.ServiceType = request.ServiceType;
        record.Men = request.Men;
        record.Women = request.Women;
        record.Children = request.Children;
        record.Visitors = request.Visitors;
        record.Notes = request.Notes?.Trim();

        await _db.SaveChangesAsync();

        await _auditService.LogAsync(
            "AttendanceRecord",
            record.Id,
            AuditAction.Updated,
            before: before,
            after: AttendanceResponse.FromEntity(record));

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        AttendanceRecord? record = await _db.AttendanceRecords
            .FirstOrDefaultAsync(x => x.Id == id);

        if (record is null)
        {
            return NotFound();
        }

        AttendanceResponse before = AttendanceResponse.FromEntity(record);

        record.IsDeleted = true;
        record.DeletedAt = DateTimeOffset.UtcNow;
        record.DeletedBy = User.Identity?.Name ?? "system";

        await _db.SaveChangesAsync();

        await _auditService.LogAsync(
            "AttendanceRecord",
            record.Id,
            AuditAction.Deleted,
            before: before,
            reason: "Soft deleted attendance record");

        return NoContent();
    }
}