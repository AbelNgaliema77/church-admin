using ChurchAdmin.Application.Common.Interfaces;
using ChurchAdmin.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace ChurchAdmin.Api.Controllers.AuditLogs;

[ApiController]
[Route("api/audit-logs")]
[Authorize(Roles = "Admin")]
public sealed class AuditLogsController : ControllerBase
{
    private readonly IChurchAdminDbContext _db;

    public AuditLogsController(IChurchAdminDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<List<AuditLogResponse>>> GetAll(
        [FromQuery] string? entityName,
        [FromQuery] Guid? entityId)
    {
        IQueryable<AuditLog> query = _db.AuditLogs;

        if (!string.IsNullOrWhiteSpace(entityName))
        {
            query = query.Where(x => x.EntityName == entityName);
        }

        if (entityId.HasValue)
        {
            query = query.Where(x => x.EntityId == entityId.Value);
        }

        List<AuditLog> auditLogs = await query
            .OrderByDescending(x => x.CreatedAt)
            .Take(200)
            .ToListAsync();

        return auditLogs.Select(AuditLogResponse.FromEntity).ToList();
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AuditLogResponse>> GetById(Guid id)
    {
        AuditLog? auditLog = await _db.AuditLogs.FirstOrDefaultAsync(x => x.Id == id);

        if (auditLog is null)
        {
            return NotFound();
        }

        return AuditLogResponse.FromEntity(auditLog);
    }
}