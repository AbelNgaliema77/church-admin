using ChurchAdmin.Domain.Entities;
using ChurchAdmin.Domain.Enums;

namespace ChurchAdmin.Api.Controllers.AuditLogs;

public sealed class AuditLogResponse
{
    public Guid Id { get; set; }

    public string EntityName { get; set; } = string.Empty;

    public Guid EntityId { get; set; }

    public AuditAction Action { get; set; }

    public string? BeforeJson { get; set; }

    public string? AfterJson { get; set; }

    public string? Reason { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public string CreatedBy { get; set; } = string.Empty;

    public static AuditLogResponse FromEntity(AuditLog auditLog)
    {
        return new AuditLogResponse
        {
            Id = auditLog.Id,
            EntityName = auditLog.EntityName,
            EntityId = auditLog.EntityId,
            Action = auditLog.Action,
            BeforeJson = auditLog.BeforeJson,
            AfterJson = auditLog.AfterJson,
            Reason = auditLog.Reason,
            CreatedAt = auditLog.CreatedAt,
            CreatedBy = auditLog.CreatedBy
        };
    }
}