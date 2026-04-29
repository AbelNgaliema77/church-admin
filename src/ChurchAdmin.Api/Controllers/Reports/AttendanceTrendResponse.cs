namespace ChurchAdmin.Api.Controllers.Reports;

public sealed class AttendanceTrendResponse
{
    public int Year { get; set; }

    public int Month { get; set; }

    public string MonthName { get; set; } = string.Empty;

    public int Men { get; set; }

    public int Women { get; set; }

    public int Children { get; set; }

    public int Visitors { get; set; }

    public int Total { get; set; }
}