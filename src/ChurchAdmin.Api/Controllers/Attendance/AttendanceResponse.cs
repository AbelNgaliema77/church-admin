using ChurchAdmin.Domain.Entities;
using ChurchAdmin.Domain.Enums;

namespace ChurchAdmin.Api.Controllers.Attendance;

public sealed class AttendanceResponse
{
    public Guid Id { get; set; }

    public DateOnly ServiceDate { get; set; }

    public ServiceType ServiceType { get; set; }

    public int Men { get; set; }

    public int Women { get; set; }

    public int Children { get; set; }

    public int Visitors { get; set; }

    public int Total { get; set; }

    public string? Notes { get; set; }

    public static AttendanceResponse FromEntity(AttendanceRecord record)
    {
        return new AttendanceResponse
        {
            Id = record.Id,
            ServiceDate = record.ServiceDate,
            ServiceType = record.ServiceType,
            Men = record.Men,
            Women = record.Women,
            Children = record.Children,
            Visitors = record.Visitors,
            Total = record.Total,
            Notes = record.Notes
        };
    }
}