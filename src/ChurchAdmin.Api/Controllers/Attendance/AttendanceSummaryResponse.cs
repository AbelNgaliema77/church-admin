using ChurchAdmin.Domain.Enums;

namespace ChurchAdmin.Api.Controllers.Attendance;

public sealed class AttendanceSummaryResponse
{
    public DateOnly From { get; set; }

    public DateOnly To { get; set; }

    public ServiceType? ServiceType { get; set; }

    public int ServicesCount { get; set; }

    public int TotalMen { get; set; }

    public int TotalWomen { get; set; }

    public int TotalChildren { get; set; }

    public int TotalVisitors { get; set; }

    public int TotalAttendance { get; set; }

    public decimal AverageAttendance { get; set; }

    public DateOnly? HighestAttendanceDate { get; set; }

    public int HighestAttendanceTotal { get; set; }

    public DateOnly? LowestAttendanceDate { get; set; }

    public int LowestAttendanceTotal { get; set; }
}