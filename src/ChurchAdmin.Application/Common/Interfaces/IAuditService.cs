using ChurchAdmin.Domain.Enums;

namespace ChurchAdmin.Application.Common.Interfaces;

public interface IAuditService
{
    Task LogAsync(
        string entityName,
        Guid entityId,
        AuditAction action,
        object? before = null,
        object? after = null,
        string? reason = null,
        CancellationToken cancellationToken = default);
}