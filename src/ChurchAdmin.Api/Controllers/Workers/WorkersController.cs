using ChurchAdmin.Api.Common;
using ChurchAdmin.Application.Common.Interfaces;
using ChurchAdmin.Domain.Entities;
using ChurchAdmin.Domain.Enums;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChurchAdmin.Api.Controllers.Workers;

[ApiController]
[Route("api/workers")]
[Authorize(Roles = "Admin,TeamLead")]
public sealed class WorkersController : ControllerBase
{
    private readonly IChurchAdminDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditService _auditService;
    private readonly IValidator<CreateWorkerRequest> _createValidator;
    private readonly IValidator<UpdateWorkerRequest> _updateValidator;
    private readonly IValidator<AssignWorkerTeamRequest> _assignValidator;

    public WorkersController(
        IChurchAdminDbContext db,
        ICurrentUserService currentUser,
        IAuditService auditService,
        IValidator<CreateWorkerRequest> createValidator,
        IValidator<UpdateWorkerRequest> updateValidator,
        IValidator<AssignWorkerTeamRequest> assignValidator)
    {
        _db = db;
        _currentUser = currentUser;
        _auditService = auditService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _assignValidator = assignValidator;
    }

    [HttpGet]
    public async Task<ActionResult<List<WorkerResponse>>> GetAll()
    {
        Guid churchId = GetChurchId();

        List<Worker> workers = await _db.Workers
            .Where(x => x.ChurchId == churchId)
            .Include(x => x.WorkerTeams)
            .ThenInclude(x => x.Team)
            .OrderBy(x => x.FullName)
            .ToListAsync();

        return workers.Select(WorkerResponse.FromEntity).ToList();
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<WorkerResponse>> GetById(Guid id)
    {
        Guid churchId = GetChurchId();

        Worker? worker = await _db.Workers
            .Where(x => x.ChurchId == churchId)
            .Include(x => x.WorkerTeams)
            .ThenInclude(x => x.Team)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (worker is null)
        {
            return NotFound();
        }

        return WorkerResponse.FromEntity(worker);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<WorkerResponse>> Create(CreateWorkerRequest request)
    {
        FluentValidation.Results.ValidationResult validation =
            await _createValidator.ValidateAsync(request);

        if (!validation.IsValid)
        {
            return validation.ToBadRequest();
        }

        Guid churchId = GetChurchId();

        bool teamExists = await _db.Teams
            .AnyAsync(x => x.Id == request.TeamId && x.ChurchId == churchId && x.IsActive);

        if (!teamExists)
        {
            return BadRequest("Selected team does not exist or is inactive.");
        }

        string email = request.Email.Trim().ToLowerInvariant();

        bool emailExists = await _db.Workers
            .AnyAsync(x => x.Email == email && x.ChurchId == churchId);

        if (emailExists)
        {
            return BadRequest("A worker with this email already exists.");
        }

        Worker worker = new Worker
        {
            Id = Guid.NewGuid(),
            ChurchId = churchId,
            FullName = request.FullName.Trim(),
            Email = email,
            Phone = request.Phone.Trim(),
            DateOfBirth = request.DateOfBirth,
            StartedServing = request.StartedServing,
            Baptized = request.Baptized,
            Address = request.Address.Trim(),
            Status = WorkerStatus.Active
        };

        WorkerTeam workerTeam = new WorkerTeam
        {
            Id = Guid.NewGuid(),
            WorkerId = worker.Id,
            TeamId = request.TeamId,
            RoleInTeam = request.RoleInTeam,
            StartDate = request.StartedServing
        };

        worker.WorkerTeams.Add(workerTeam);
        _db.Workers.Add(worker);

        await _db.SaveChangesAsync();

        Worker created = await _db.Workers
            .Include(x => x.WorkerTeams)
            .ThenInclude(x => x.Team)
            .FirstAsync(x => x.Id == worker.Id);

        WorkerResponse response = WorkerResponse.FromEntity(created);

        await _auditService.LogAsync("Worker", worker.Id, AuditAction.Created, after: response);

        return CreatedAtAction(nameof(GetById), new { id = worker.Id }, response);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id, UpdateWorkerRequest request)
    {
        FluentValidation.Results.ValidationResult validation =
            await _updateValidator.ValidateAsync(request);

        if (!validation.IsValid)
        {
            return validation.ToBadRequest();
        }

        Guid churchId = GetChurchId();

        Worker? worker = await _db.Workers
            .Where(x => x.ChurchId == churchId)
            .Include(x => x.WorkerTeams)
            .ThenInclude(x => x.Team)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (worker is null)
        {
            return NotFound();
        }

        string email = request.Email.Trim().ToLowerInvariant();

        bool emailTaken = await _db.Workers
            .AnyAsync(x => x.Email == email && x.ChurchId == churchId && x.Id != id);

        if (emailTaken)
        {
            return BadRequest("Another worker with this email already exists.");
        }

        WorkerResponse before = WorkerResponse.FromEntity(worker);

        worker.FullName = request.FullName.Trim();
        worker.Email = email;
        worker.Phone = request.Phone.Trim();
        worker.DateOfBirth = request.DateOfBirth;
        worker.StartedServing = request.StartedServing;
        worker.Baptized = request.Baptized;
        worker.Address = request.Address.Trim();
        worker.Status = request.Status;

        await _db.SaveChangesAsync();

        await _auditService.LogAsync("Worker", worker.Id, AuditAction.Updated,
            before: before, after: WorkerResponse.FromEntity(worker));

        return NoContent();
    }

    [HttpPost("{id:guid}/teams")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AssignToTeam(Guid id, AssignWorkerTeamRequest request)
    {
        FluentValidation.Results.ValidationResult validation =
            await _assignValidator.ValidateAsync(request);

        if (!validation.IsValid)
        {
            return validation.ToBadRequest();
        }

        Guid churchId = GetChurchId();

        Worker? worker = await _db.Workers
            .Where(x => x.ChurchId == churchId)
            .Include(x => x.WorkerTeams)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (worker is null)
        {
            return NotFound("Worker not found.");
        }

        bool teamExists = await _db.Teams
            .AnyAsync(x => x.Id == request.TeamId && x.ChurchId == churchId && x.IsActive);

        if (!teamExists)
        {
            return BadRequest("Selected team does not exist or is inactive.");
        }

        bool alreadyMember = worker.WorkerTeams.Any(x => x.TeamId == request.TeamId && x.EndDate is null);

        if (alreadyMember)
        {
            return BadRequest("Worker is already a member of this team.");
        }

        WorkerTeam workerTeam = new WorkerTeam
        {
            Id = Guid.NewGuid(),
            WorkerId = worker.Id,
            TeamId = request.TeamId,
            RoleInTeam = request.RoleInTeam,
            StartDate = request.StartDate
        };

        _db.WorkerTeams.Add(workerTeam);
        await _db.SaveChangesAsync();

        await _auditService.LogAsync("Worker", worker.Id, AuditAction.Assigned,
            after: request, reason: "Assigned worker to team");

        return NoContent();
    }

    [HttpDelete("{id:guid}/teams/{teamId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> RemoveFromTeam(Guid id, Guid teamId)
    {
        Guid churchId = GetChurchId();

        // Verify worker belongs to this church
        bool workerExists = await _db.Workers
            .AnyAsync(x => x.Id == id && x.ChurchId == churchId);

        if (!workerExists)
        {
            return NotFound();
        }

        WorkerTeam? workerTeam = await _db.WorkerTeams
            .FirstOrDefaultAsync(x => x.WorkerId == id && x.TeamId == teamId && x.EndDate == null);

        if (workerTeam is null)
        {
            return NotFound();
        }

        workerTeam.EndDate = DateOnly.FromDateTime(DateTime.UtcNow);
        await _db.SaveChangesAsync();

        await _auditService.LogAsync("Worker", id, AuditAction.Removed,
            reason: "Removed worker from team");

        return NoContent();
    }

    [HttpPatch("{id:guid}/deactivate")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        Guid churchId = GetChurchId();

        Worker? worker = await _db.Workers
            .Where(x => x.ChurchId == churchId)
            .Include(x => x.WorkerTeams)
            .ThenInclude(x => x.Team)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (worker is null)
        {
            return NotFound();
        }

        WorkerResponse before = WorkerResponse.FromEntity(worker);
        worker.Status = WorkerStatus.Inactive;
        await _db.SaveChangesAsync();

        await _auditService.LogAsync("Worker", worker.Id, AuditAction.Deactivated,
            before: before, after: WorkerResponse.FromEntity(worker));

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        Guid churchId = GetChurchId();

        Worker? worker = await _db.Workers
            .Where(x => x.ChurchId == churchId)
            .Include(x => x.WorkerTeams)
            .ThenInclude(x => x.Team)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (worker is null)
        {
            return NotFound();
        }

        WorkerResponse before = WorkerResponse.FromEntity(worker);

        worker.IsDeleted = true;
        worker.DeletedAt = DateTimeOffset.UtcNow;
        worker.DeletedBy = _currentUser.Email;

        await _db.SaveChangesAsync();

        await _auditService.LogAsync("Worker", worker.Id, AuditAction.Deleted,
            before: before, reason: "Soft deleted");

        return NoContent();
    }

    private Guid GetChurchId()
    {
        Guid? churchId = _currentUser.ChurchId;

        if (churchId is null || churchId == Guid.Empty)
        {
            throw new InvalidOperationException("User is not associated with a church.");
        }

        return churchId.Value;
    }
}
