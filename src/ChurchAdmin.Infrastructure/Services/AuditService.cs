using System.Text.Json;
using ChurchAdmin.Application.Common.Interfaces;
using ChurchAdmin.Domain.Entities;
using ChurchAdmin.Domain.Enums;
using ChurchAdmin.Infrastructure.Persistence;

namespace ChurchAdmin.Infrastructure.Services;

public sealed class AuditService : IAuditService
{
    private readonly ChurchAdminDbContext _dbContext;

    public AuditService(ChurchAdminDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task LogAsync(
        string entityName,
        Guid entityId,
        AuditAction action,
        object? before = null,
        object? after = null,
        string? reason = null,
        CancellationToken cancellationToken = default)
    {
        AuditLog auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            EntityName = entityName,
            EntityId = entityId,
            Action = action,
            BeforeJson = before is null ? null : JsonSerializer.Serialize(before),
            AfterJson = after is null ? null : JsonSerializer.Serialize(after),
            Reason = reason
        };

        _dbContext.AuditLogs.Add(auditLog);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}