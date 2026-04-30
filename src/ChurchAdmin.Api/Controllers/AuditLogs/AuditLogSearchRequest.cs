using ChurchAdmin.Domain.Enums;

namespace ChurchAdmin.Api.Controllers.AuditLogs;

public sealed class AuditLogSearchRequest
{
    public string? EntityName { get; set; }

    public Guid? EntityId { get; set; }

    public AuditAction? Action { get; set; }

    public string? CreatedBy { get; set; }

    public DateTimeOffset? From { get; set; }

    public DateTimeOffset? To { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 50;
}