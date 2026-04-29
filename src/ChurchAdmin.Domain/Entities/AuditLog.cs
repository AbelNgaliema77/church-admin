using ChurchAdmin.Domain.Common;
using ChurchAdmin.Domain.Enums;

namespace ChurchAdmin.Domain.Entities;

public sealed class AuditLog : AuditableEntity
{
    public string EntityName { get; set; } = string.Empty;

    public Guid EntityId { get; set; }

    public AuditAction Action { get; set; }

    public string? BeforeJson { get; set; }

    public string? AfterJson { get; set; }

    public string? Reason { get; set; }
}
