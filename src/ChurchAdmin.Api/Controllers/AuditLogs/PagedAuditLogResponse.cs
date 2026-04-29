namespace ChurchAdmin.Api.Controllers.AuditLogs;

public sealed class PagedAuditLogResponse
{
    public int Page { get; set; }

    public int PageSize { get; set; }

    public int TotalCount { get; set; }

    public int TotalPages { get; set; }

    public List<AuditLogResponse> Items { get; set; } = [];
}