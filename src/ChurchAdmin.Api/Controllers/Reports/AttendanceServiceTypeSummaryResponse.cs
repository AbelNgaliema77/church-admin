using ChurchAdmin.Domain.Enums;

namespace ChurchAdmin.Api.Controllers.Reports;

public sealed class AttendanceServiceTypeSummaryResponse
{
    public ServiceType ServiceType { get; set; }

    public int ServicesCount { get; set; }

    public int TotalAttendance { get; set; }

    public decimal AverageAttendance { get; set; }
}