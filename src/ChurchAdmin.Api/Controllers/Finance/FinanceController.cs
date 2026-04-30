using ChurchAdmin.Application.Common.Interfaces;
using ChurchAdmin.Domain.Entities;
using ChurchAdmin.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChurchAdmin.Api.Controllers.Finance;

[ApiController]
[Route("api/finance")]
[Authorize(Roles = "Admin,Finance")]
public sealed class FinanceController : ControllerBase
{
    private readonly IChurchAdminDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditService _auditService;

    public FinanceController(
        IChurchAdminDbContext db,
        ICurrentUserService currentUser,
        IAuditService auditService)
    {
        _db = db;
        _currentUser = currentUser;
        _auditService = auditService;
    }

    [HttpGet]
    public async Task<ActionResult<List<FinanceEntryResponse>>> GetAll(
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        [FromQuery] ServiceType? serviceType,
        [FromQuery] FinanceCategory? category,
        [FromQuery] bool? verified)
    {
        Guid churchId = GetChurchId();

        IQueryable<FinanceEntry> query = _db.FinanceEntries
            .Where(x => x.ChurchId == churchId);

        if (from.HasValue) query = query.Where(x => x.ServiceDate >= from.Value);
        if (to.HasValue) query = query.Where(x => x.ServiceDate <= to.Value);
        if (serviceType.HasValue) query = query.Where(x => x.ServiceType == serviceType.Value);
        if (category.HasValue) query = query.Where(x => x.Category == category.Value);
        if (verified.HasValue) query = query.Where(x => x.IsVerified == verified.Value);

        List<FinanceEntry> entries = await query
            .OrderByDescending(x => x.ServiceDate)
            .ThenBy(x => x.Category)
            .ToListAsync();

        return entries.Select(FinanceEntryResponse.FromEntity).ToList();
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<FinanceEntryResponse>> GetById(Guid id)
    {
        Guid churchId = GetChurchId();

        FinanceEntry? entry = await _db.FinanceEntries
            .FirstOrDefaultAsync(x => x.Id == id && x.ChurchId == churchId);

        if (entry is null) return NotFound();

        return FinanceEntryResponse.FromEntity(entry);
    }

    [HttpGet("summary")]
    public async Task<ActionResult<FinanceSummaryResponse>> GetSummary(
        [FromQuery] DateOnly from,
        [FromQuery] DateOnly to,
        [FromQuery] ServiceType? serviceType)
    {
        if (from == default || to == default)
            return BadRequest("From and to dates are required.");

        if (from > to)
            return BadRequest("From date cannot be after to date.");

        Guid churchId = GetChurchId();

        IQueryable<FinanceEntry> query = _db.FinanceEntries
            .Where(x => x.ChurchId == churchId && x.ServiceDate >= from && x.ServiceDate <= to);

        if (serviceType.HasValue)
            query = query.Where(x => x.ServiceType == serviceType.Value);

        List<FinanceEntry> entries = await query.ToListAsync();

        return new FinanceSummaryResponse
        {
            From = from,
            To = to,
            ServiceType = serviceType,
            EntriesCount = entries.Count,
            TotalAmount = entries.Sum(x => x.Amount),
            TotalsByCategory = entries
                .GroupBy(x => x.Category)
                .ToDictionary(x => x.Key, x => x.Sum(y => y.Amount)),
            TotalsByPaymentMethod = entries
                .GroupBy(x => x.PaymentMethod)
                .ToDictionary(x => x.Key, x => x.Sum(y => y.Amount))
        };
    }

    [HttpPost]
    public async Task<ActionResult<FinanceEntryResponse>> Create(CreateFinanceEntryRequest request)
    {
        string? error = ValidateRequest(request.ServiceDate, request.Amount, request.Notes);
        if (error is not null) return BadRequest(error);

        Guid churchId = GetChurchId();

        FinanceEntry entry = new FinanceEntry
        {
            Id = Guid.NewGuid(),
            ChurchId = churchId,
            ServiceDate = request.ServiceDate,
            ServiceType = request.ServiceType,
            Category = request.Category,
            Amount = request.Amount,
            PaymentMethod = request.PaymentMethod,
            IsVerified = false,
            Notes = request.Notes?.Trim()
        };

        _db.FinanceEntries.Add(entry);
        await _db.SaveChangesAsync();

        FinanceEntryResponse response = FinanceEntryResponse.FromEntity(entry);
        await _auditService.LogAsync("FinanceEntry", entry.Id, AuditAction.Created, after: response);

        return CreatedAtAction(nameof(GetById), new { id = entry.Id }, response);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateFinanceEntryRequest request)
    {
        Guid churchId = GetChurchId();

        FinanceEntry? entry = await _db.FinanceEntries
            .FirstOrDefaultAsync(x => x.Id == id && x.ChurchId == churchId);

        if (entry is null) return NotFound();

        if (entry.IsVerified)
            return BadRequest("Verified entries cannot be edited. Create a correction entry instead.");

        string? error = ValidateRequest(request.ServiceDate, request.Amount, request.Notes);
        if (error is not null) return BadRequest(error);

        FinanceEntryResponse before = FinanceEntryResponse.FromEntity(entry);

        entry.ServiceDate = request.ServiceDate;
        entry.ServiceType = request.ServiceType;
        entry.Category = request.Category;
        entry.Amount = request.Amount;
        entry.PaymentMethod = request.PaymentMethod;
        entry.Notes = request.Notes?.Trim();

        await _db.SaveChangesAsync();

        await _auditService.LogAsync("FinanceEntry", entry.Id, AuditAction.Updated,
            before: before, after: FinanceEntryResponse.FromEntity(entry));

        return NoContent();
    }

    [HttpPatch("{id:guid}/verify")]
    public async Task<IActionResult> Verify(Guid id)
    {
        Guid churchId = GetChurchId();

        FinanceEntry? entry = await _db.FinanceEntries
            .FirstOrDefaultAsync(x => x.Id == id && x.ChurchId == churchId);

        if (entry is null) return NotFound();
        if (entry.IsVerified) return BadRequest("Finance entry is already verified.");

        FinanceEntryResponse before = FinanceEntryResponse.FromEntity(entry);

        entry.IsVerified = true;
        entry.VerifiedAt = DateTimeOffset.UtcNow;
        entry.VerifiedBy = _currentUser.Email;

        await _db.SaveChangesAsync();

        await _auditService.LogAsync("FinanceEntry", entry.Id, AuditAction.Verified,
            before: before, after: FinanceEntryResponse.FromEntity(entry));

        return NoContent();
    }

    [HttpPost("{id:guid}/corrections")]
    public async Task<ActionResult<FinanceEntryResponse>> CreateCorrection(
        Guid id,
        CreateFinanceCorrectionRequest request)
    {
        Guid churchId = GetChurchId();

        FinanceEntry? original = await _db.FinanceEntries
            .FirstOrDefaultAsync(x => x.Id == id && x.ChurchId == churchId);

        if (original is null) return NotFound("Original finance entry not found.");
        if (!original.IsVerified) return BadRequest("Corrections are only for verified entries.");
        if (request.Amount == 0) return BadRequest("Correction amount cannot be zero.");
        if (string.IsNullOrWhiteSpace(request.Reason)) return BadRequest("Correction reason is required.");

        FinanceEntry correction = new FinanceEntry
        {
            Id = Guid.NewGuid(),
            ChurchId = churchId,
            ServiceDate = original.ServiceDate,
            ServiceType = original.ServiceType,
            Category = request.Category,
            Amount = request.Amount,
            PaymentMethod = request.PaymentMethod,
            IsVerified = false,
            CorrectionOfFinanceEntryId = original.Id,
            Notes = $"Correction: {request.Reason.Trim()}"
        };

        _db.FinanceEntries.Add(correction);
        await _db.SaveChangesAsync();

        FinanceEntryResponse response = FinanceEntryResponse.FromEntity(correction);
        await _auditService.LogAsync("FinanceEntry", correction.Id, AuditAction.Corrected,
            after: response, reason: request.Reason.Trim());

        return CreatedAtAction(nameof(GetById), new { id = correction.Id }, response);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        Guid churchId = GetChurchId();

        FinanceEntry? entry = await _db.FinanceEntries
            .FirstOrDefaultAsync(x => x.Id == id && x.ChurchId == churchId);

        if (entry is null) return NotFound();
        if (entry.IsVerified) return BadRequest("Verified entries cannot be deleted. Create a correction entry instead.");

        FinanceEntryResponse before = FinanceEntryResponse.FromEntity(entry);

        entry.IsDeleted = true;
        entry.DeletedAt = DateTimeOffset.UtcNow;
        entry.DeletedBy = _currentUser.Email;

        await _db.SaveChangesAsync();

        await _auditService.LogAsync("FinanceEntry", entry.Id, AuditAction.Deleted,
            before: before, reason: "Soft deleted");

        return NoContent();
    }

    private Guid GetChurchId()
    {
        Guid? churchId = _currentUser.ChurchId;
        if (churchId is null || churchId == Guid.Empty)
            throw new InvalidOperationException("User is not associated with a church.");
        return churchId.Value;
    }

    private static string? ValidateRequest(DateOnly serviceDate, decimal amount, string? notes)
    {
        if (serviceDate == default) return "Service date is required.";
        if (serviceDate > DateOnly.FromDateTime(DateTime.UtcNow.Date)) return "Service date cannot be in the future.";
        if (amount <= 0) return "Amount must be greater than zero.";
        if (!string.IsNullOrWhiteSpace(notes) && notes.Length > 1000) return "Notes cannot exceed 1000 characters.";
        return null;
    }
}
