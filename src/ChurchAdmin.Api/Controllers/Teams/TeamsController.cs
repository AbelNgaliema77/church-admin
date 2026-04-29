using ChurchAdmin.Api.Common;
using ChurchAdmin.Application.Common.Interfaces;
using ChurchAdmin.Domain.Entities;
using ChurchAdmin.Domain.Enums;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChurchAdmin.Api.Controllers.Teams;

[ApiController]
[Route("api/teams")]
[Authorize(Roles = "Admin")]
public sealed class TeamsController : ControllerBase
{
    private readonly IChurchAdminDbContext _db;
    private readonly IAuditService _auditService;
    private readonly IValidator<CreateTeamRequest> _createValidator;
    private readonly IValidator<UpdateTeamRequest> _updateValidator;

    public TeamsController(
        IChurchAdminDbContext db,
        IAuditService auditService,
        IValidator<CreateTeamRequest> createValidator,
        IValidator<UpdateTeamRequest> updateValidator)
    {
        _db = db;
        _auditService = auditService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpGet]
    public async Task<ActionResult<List<TeamResponse>>> GetAll()
    {
        List<Team> teams = await _db.Teams
            .OrderBy(x => x.Name)
            .ToListAsync();

        return teams.Select(TeamResponse.FromEntity).ToList();
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TeamResponse>> GetById(Guid id)
    {
        Team? team = await _db.Teams.FirstOrDefaultAsync(x => x.Id == id);

        if (team is null)
        {
            return NotFound();
        }

        return TeamResponse.FromEntity(team);
    }

    [HttpPost]
    public async Task<ActionResult<TeamResponse>> Create(CreateTeamRequest request)
    {
        FluentValidation.Results.ValidationResult validation =
            await _createValidator.ValidateAsync(request);

        if (!validation.IsValid)
        {
            return validation.ToBadRequest();
        }

        string name = request.Name.Trim();

        bool exists = await _db.Teams.AnyAsync(x => x.Name == name);

        if (exists)
        {
            return BadRequest("Team with this name already exists.");
        }

        Team team = new Team
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = request.Description?.Trim(),
            IsActive = true
        };

        _db.Teams.Add(team);

        await _db.SaveChangesAsync();

        TeamResponse response = TeamResponse.FromEntity(team);

        await _auditService.LogAsync(
            "Team",
            team.Id,
            AuditAction.Created,
            after: response);

        return CreatedAtAction(
            nameof(GetById),
            new { id = team.Id },
            response);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateTeamRequest request)
    {
        FluentValidation.Results.ValidationResult validation =
            await _updateValidator.ValidateAsync(request);

        if (!validation.IsValid)
        {
            return validation.ToBadRequest();
        }

        Team? team = await _db.Teams.FirstOrDefaultAsync(x => x.Id == id);

        if (team is null)
        {
            return NotFound();
        }

        string name = request.Name.Trim();

        bool nameExists = await _db.Teams
            .AnyAsync(x => x.Name == name && x.Id != id);

        if (nameExists)
        {
            return BadRequest("Another team with this name already exists.");
        }

        TeamResponse before = TeamResponse.FromEntity(team);

        team.Name = name;
        team.Description = request.Description?.Trim();
        team.IsActive = request.IsActive;

        await _db.SaveChangesAsync();

        await _auditService.LogAsync(
            "Team",
            team.Id,
            AuditAction.Updated,
            before: before,
            after: TeamResponse.FromEntity(team));

        return NoContent();
    }

    [HttpPatch("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        Team? team = await _db.Teams.FirstOrDefaultAsync(x => x.Id == id);

        if (team is null)
        {
            return NotFound();
        }

        bool hasActiveWorkers = await _db.WorkerTeams
            .AnyAsync(x => x.TeamId == id && x.EndDate == null);

        if (hasActiveWorkers)
        {
            return BadRequest("Team has active workers. Remove or transfer workers before deactivation.");
        }

        TeamResponse before = TeamResponse.FromEntity(team);

        team.IsActive = false;

        await _db.SaveChangesAsync();

        await _auditService.LogAsync(
            "Team",
            team.Id,
            AuditAction.Deactivated,
            before: before,
            after: TeamResponse.FromEntity(team));

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        Team? team = await _db.Teams.FirstOrDefaultAsync(x => x.Id == id);

        if (team is null)
        {
            return NotFound();
        }

        bool hasWorkerHistory = await _db.WorkerTeams
            .AnyAsync(x => x.TeamId == id);

        if (hasWorkerHistory)
        {
            return BadRequest("Team has worker history. Deactivate it instead.");
        }

        TeamResponse before = TeamResponse.FromEntity(team);

        team.IsDeleted = true;
        team.DeletedAt = DateTimeOffset.UtcNow;
        team.DeletedBy = User.Identity?.Name ?? "system";

        await _db.SaveChangesAsync();

        await _auditService.LogAsync(
            "Team",
            team.Id,
            AuditAction.Deleted,
            before: before,
            reason: "Soft deleted team");

        return NoContent();
    }
}