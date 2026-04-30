using ChurchAdmin.Domain.Common;
using ChurchAdmin.Domain.Enums;

namespace ChurchAdmin.Domain.Entities;

public sealed class AttendanceRecord : SoftDeletableEntity
{
    public DateOnly ServiceDate { get; set; }

    public ServiceType ServiceType { get; set; }

    public int Men { get; set; }

    public int Women { get; set; }

    public int Children { get; set; }

    public int Visitors { get; set; }

    public string? Notes { get; set; }

    public int Total => Men + Women + Children + Visitors;
    public Guid ChurchId { get; set; }
    public Church Church { get; set; } = null!;
}
