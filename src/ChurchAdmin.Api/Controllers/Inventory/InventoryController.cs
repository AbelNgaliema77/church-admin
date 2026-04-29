using ChurchAdmin.Api.Common;
using ChurchAdmin.Application.Common.Interfaces;
using ChurchAdmin.Domain.Entities;
using ChurchAdmin.Domain.Enums;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChurchAdmin.Api.Controllers.Inventory;
[ApiController]
[Route("api/inventory")]
[Authorize(Roles = "Admin,TeamLead")]
public sealed class InventoryController : ControllerBase
{
    private readonly IChurchAdminDbContext _db;
    private readonly IAuditService _auditService;
    private readonly IValidator<CreateInventoryItemRequest> _createValidator;
    private readonly IValidator<UpdateInventoryItemRequest> _updateValidator;

    public InventoryController(
        IChurchAdminDbContext db,
        IAuditService auditService,
        IValidator<CreateInventoryItemRequest> createValidator,
        IValidator<UpdateInventoryItemRequest> updateValidator)
    {
        _db = db;
        _auditService = auditService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpGet]
    public async Task<ActionResult<List<InventoryItemResponse>>> GetAll(
        [FromQuery] Guid? teamId,
        [FromQuery] InventoryStatus? status,
        [FromQuery] InventoryCondition? condition)
    {
        IQueryable<InventoryItem> query = _db.InventoryItems
            .Include(x => x.Team);

        if (teamId.HasValue)
        {
            query = query.Where(x => x.TeamId == teamId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(x => x.Status == status.Value);
        }

        if (condition.HasValue)
        {
            query = query.Where(x => x.Condition == condition.Value);
        }

        List<InventoryItem> items = await query
            .OrderBy(x => x.Name)
            .ToListAsync();

        return items.Select(InventoryItemResponse.FromEntity).ToList();
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<InventoryItemResponse>> GetById(Guid id)
    {
        InventoryItem? item = await _db.InventoryItems
            .Include(x => x.Team)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (item is null)
        {
            return NotFound();
        }

        return InventoryItemResponse.FromEntity(item);
    }

    [HttpPost]
    public async Task<ActionResult<InventoryItemResponse>> Create(
        CreateInventoryItemRequest request)
    {
        FluentValidation.Results.ValidationResult validation =
            await _createValidator.ValidateAsync(request);

        if (!validation.IsValid)
        {
            return validation.ToBadRequest();
        }

        bool teamExists = await _db.Teams
            .AnyAsync(x => x.Id == request.TeamId && x.IsActive);

        if (!teamExists)
        {
            return BadRequest("Selected team does not exist or is inactive.");
        }

        InventoryItem item = new InventoryItem
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            TeamId = request.TeamId,
            Description = request.Description.Trim(),
            Quantity = request.Quantity,
            Condition = request.Condition,
            Status = request.Status,
            ImageUrl = string.IsNullOrWhiteSpace(request.ImageUrl)
                ? null
                : request.ImageUrl.Trim()
        };

        _db.InventoryItems.Add(item);

        await _db.SaveChangesAsync();

        InventoryItem createdItem = await _db.InventoryItems
            .Include(x => x.Team)
            .FirstAsync(x => x.Id == item.Id);

        InventoryItemResponse response = InventoryItemResponse.FromEntity(createdItem);

        await _auditService.LogAsync(
            "InventoryItem",
            item.Id,
            AuditAction.Created,
            after: response);

        return CreatedAtAction(
            nameof(GetById),
            new { id = item.Id },
            response);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        UpdateInventoryItemRequest request)
    {
        FluentValidation.Results.ValidationResult validation =
            await _updateValidator.ValidateAsync(request);

        if (!validation.IsValid)
        {
            return validation.ToBadRequest();
        }

        InventoryItem? item = await _db.InventoryItems
            .Include(x => x.Team)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (item is null)
        {
            return NotFound();
        }

        bool teamExists = await _db.Teams
            .AnyAsync(x => x.Id == request.TeamId && x.IsActive);

        if (!teamExists)
        {
            return BadRequest("Selected team does not exist or is inactive.");
        }

        InventoryItemResponse before = InventoryItemResponse.FromEntity(item);

        item.Name = request.Name.Trim();
        item.TeamId = request.TeamId;
        item.Description = request.Description.Trim();
        item.Quantity = request.Quantity;
        item.Condition = request.Condition;
        item.Status = request.Status;
        item.ImageUrl = string.IsNullOrWhiteSpace(request.ImageUrl)
            ? null
            : request.ImageUrl.Trim();

        await _db.SaveChangesAsync();

        InventoryItem updatedItem = await _db.InventoryItems
            .Include(x => x.Team)
            .FirstAsync(x => x.Id == id);

        await _auditService.LogAsync(
            "InventoryItem",
            item.Id,
            AuditAction.Updated,
            before: before,
            after: InventoryItemResponse.FromEntity(updatedItem));

        return NoContent();
    }

    [HttpPatch("{id:guid}/approve")]
    public async Task<IActionResult> Approve(Guid id)
    {
        InventoryItem? item = await _db.InventoryItems
            .Include(x => x.Team)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (item is null)
        {
            return NotFound();
        }

        if (item.Status == InventoryStatus.Retired)
        {
            return BadRequest("Retired items cannot be approved.");
        }

        InventoryItemResponse before = InventoryItemResponse.FromEntity(item);

        item.Status = InventoryStatus.Approved;

        await _db.SaveChangesAsync();

        await _auditService.LogAsync(
            "InventoryItem",
            item.Id,
            AuditAction.Approved,
            before: before,
            after: InventoryItemResponse.FromEntity(item));

        return NoContent();
    }

    [HttpPatch("{id:guid}/retire")]
    public async Task<IActionResult> Retire(Guid id)
    {
        InventoryItem? item = await _db.InventoryItems
            .Include(x => x.Team)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (item is null)
        {
            return NotFound();
        }

        InventoryItemResponse before = InventoryItemResponse.FromEntity(item);

        item.Status = InventoryStatus.Retired;

        await _db.SaveChangesAsync();

        await _auditService.LogAsync(
            "InventoryItem",
            item.Id,
            AuditAction.Retired,
            before: before,
            after: InventoryItemResponse.FromEntity(item));

        return NoContent();
    }

    [HttpPatch("{id:guid}/mark-lost")]
    public async Task<IActionResult> MarkLost(Guid id)
    {
        InventoryItem? item = await _db.InventoryItems
            .Include(x => x.Team)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (item is null)
        {
            return NotFound();
        }

        if (item.Status == InventoryStatus.Retired)
        {
            return BadRequest("Retired items cannot be marked as lost.");
        }

        InventoryItemResponse before = InventoryItemResponse.FromEntity(item);

        item.Condition = InventoryCondition.Lost;

        await _db.SaveChangesAsync();

        await _auditService.LogAsync(
            "InventoryItem",
            item.Id,
            AuditAction.Updated,
            before: before,
            after: InventoryItemResponse.FromEntity(item),
            reason: "Marked item as lost");

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        InventoryItem? item = await _db.InventoryItems
            .Include(x => x.Team)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (item is null)
        {
            return NotFound();
        }

        if (item.Status == InventoryStatus.Approved)
        {
            return BadRequest("Approved inventory should not be deleted. Retire it instead.");
        }

        InventoryItemResponse before = InventoryItemResponse.FromEntity(item);

        item.IsDeleted = true;
        item.DeletedAt = DateTimeOffset.UtcNow;
        item.DeletedBy = User.Identity?.Name ?? "system";

        await _db.SaveChangesAsync();

        await _auditService.LogAsync(
            "InventoryItem",
            item.Id,
            AuditAction.Deleted,
            before: before,
            reason: "Soft deleted inventory item");

        return NoContent();
    }
}