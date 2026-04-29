using ChurchAdmin.Application.Common.Interfaces;
using ChurchAdmin.Domain.Entities;
using ChurchAdmin.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace ChurchAdmin.Api.Controllers.Finance;

[ApiController]
[Route("api/finance")]
[Authorize(Roles = "Admin")]
public sealed class FinanceController : ControllerBase
{
    private readonly IChurchAdminDbContext _db;
    private readonly IAuditService _auditService;

    public FinanceController(
        IChurchAdminDbContext db,
        IAuditService auditService)
    {
        _db = db;
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
        IQueryable<FinanceEntry> query = _db.FinanceEntries;

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

        if (category.HasValue)
        {
            query = query.Where(x => x.Category == category.Value);
        }

        if (verified.HasValue)
        {
            query = query.Where(x => x.IsVerified == verified.Value);
        }

        List<FinanceEntry> entries = await query
            .OrderByDescending(x => x.ServiceDate)
            .ThenBy(x => x.Category)
            .ToListAsync();

        return entries.Select(FinanceEntryResponse.FromEntity).ToList();
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<FinanceEntryResponse>> GetById(Guid id)
    {
        FinanceEntry? entry = await _db.FinanceEntries
            .FirstOrDefaultAsync(x => x.Id == id);

        if (entry is null)
        {
            return NotFound();
        }

        return FinanceEntryResponse.FromEntity(entry);
    }

    [HttpGet("summary")]
    public async Task<ActionResult<FinanceSummaryResponse>> GetSummary(
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
    public async Task<ActionResult<FinanceEntryResponse>> Create(
        CreateFinanceEntryRequest request)
    {
        string? validationError = ValidateRequest(
            request.ServiceDate,
            request.Amount,
            request.Notes);

        if (validationError is not null)
        {
            return BadRequest(validationError);
        }

        FinanceEntry entry = new FinanceEntry
        {
            Id = Guid.NewGuid(),
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

        await _auditService.LogAsync(
            "FinanceEntry",
            entry.Id,
            AuditAction.Created,
            after: response);

        return CreatedAtAction(
            nameof(GetById),
            new { id = entry.Id },
            response);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        UpdateFinanceEntryRequest request)
    {
        FinanceEntry? entry = await _db.FinanceEntries
            .FirstOrDefaultAsync(x => x.Id == id);

        if (entry is null)
        {
            return NotFound();
        }

        if (entry.IsVerified)
        {
            return BadRequest("Verified finance entries cannot be edited. Create a correction entry instead.");
        }

        string? validationError = ValidateRequest(
            request.ServiceDate,
            request.Amount,
            request.Notes);

        if (validationError is not null)
        {
            return BadRequest(validationError);
        }

        FinanceEntryResponse before = FinanceEntryResponse.FromEntity(entry);

        entry.ServiceDate = request.ServiceDate;
        entry.ServiceType = request.ServiceType;
        entry.Category = request.Category;
        entry.Amount = request.Amount;
        entry.PaymentMethod = request.PaymentMethod;
        entry.Notes = request.Notes?.Trim();

        await _db.SaveChangesAsync();

        await _auditService.LogAsync(
            "FinanceEntry",
            entry.Id,
            AuditAction.Updated,
            before: before,
            after: FinanceEntryResponse.FromEntity(entry));

        return NoContent();
    }

    [HttpPatch("{id:guid}/verify")]
    public async Task<IActionResult> Verify(Guid id)
    {
        FinanceEntry? entry = await _db.FinanceEntries
            .FirstOrDefaultAsync(x => x.Id == id);

        if (entry is null)
        {
            return NotFound();
        }

        if (entry.IsVerified)
        {
            return BadRequest("Finance entry is already verified.");
        }

        FinanceEntryResponse before = FinanceEntryResponse.FromEntity(entry);

        entry.IsVerified = true;
        entry.VerifiedAt = DateTimeOffset.UtcNow;
        entry.VerifiedBy = "system";

        await _db.SaveChangesAsync();

        await _auditService.LogAsync(
            "FinanceEntry",
            entry.Id,
            AuditAction.Verified,
            before: before,
            after: FinanceEntryResponse.FromEntity(entry));

        return NoContent();
    }

    [HttpPost("{id:guid}/corrections")]
    public async Task<ActionResult<FinanceEntryResponse>> CreateCorrection(
        Guid id,
        CreateFinanceCorrectionRequest request)
    {
        FinanceEntry? originalEntry = await _db.FinanceEntries
            .FirstOrDefaultAsync(x => x.Id == id);

        if (originalEntry is null)
        {
            return NotFound("Original finance entry was not found.");
        }

        if (!originalEntry.IsVerified)
        {
            return BadRequest("Corrections are only required for verified finance entries. Edit the unverified entry instead.");
        }

        if (request.Amount == 0)
        {
            return BadRequest("Correction amount cannot be zero.");
        }

        if (string.IsNullOrWhiteSpace(request.Reason))
        {
            return BadRequest("Correction reason is required.");
        }

        FinanceEntry correctionEntry = new FinanceEntry
        {
            Id = Guid.NewGuid(),
            ServiceDate = originalEntry.ServiceDate,
            ServiceType = originalEntry.ServiceType,
            Category = request.Category,
            Amount = request.Amount,
            PaymentMethod = request.PaymentMethod,
            IsVerified = false,
            CorrectionOfFinanceEntryId = originalEntry.Id,
            Notes = $"Correction: {request.Reason.Trim()}"
        };

        _db.FinanceEntries.Add(correctionEntry);

        await _db.SaveChangesAsync();

        FinanceEntryResponse response = FinanceEntryResponse.FromEntity(correctionEntry);

        await _auditService.LogAsync(
            "FinanceEntry",
            correctionEntry.Id,
            AuditAction.Corrected,
            after: response,
            reason: request.Reason.Trim());

        return CreatedAtAction(
            nameof(GetById),
            new { id = correctionEntry.Id },
            response);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        FinanceEntry? entry = await _db.FinanceEntries
            .FirstOrDefaultAsync(x => x.Id == id);

        if (entry is null)
        {
            return NotFound();
        }

        if (entry.IsVerified)
        {
            return BadRequest("Verified finance entries cannot be deleted. Create a correction entry instead.");
        }

        FinanceEntryResponse before = FinanceEntryResponse.FromEntity(entry);

        entry.IsDeleted = true;
        entry.DeletedAt = DateTimeOffset.UtcNow;
        entry.DeletedBy = User.Identity?.Name ?? "system";

        await _db.SaveChangesAsync();

        await _auditService.LogAsync(
            "FinanceEntry",
            entry.Id,
            AuditAction.Deleted,
            before: before,
            reason: "Soft deleted finance entry");

        return NoContent();
    }

    private static string? ValidateRequest(
        DateOnly serviceDate,
        decimal amount,
        string? notes)
    {
        if (serviceDate == default)
        {
            return "Service date is required.";
        }

        if (serviceDate > DateOnly.FromDateTime(DateTime.UtcNow.Date))
        {
            return "Service date cannot be in the future.";
        }

        if (amount <= 0)
        {
            return "Amount must be greater than zero.";
        }

        if (!string.IsNullOrWhiteSpace(notes) && notes.Length > 1000)
        {
            return "Notes cannot exceed 1000 characters.";
        }

        return null;
    }
}