using ChurchAdmin.Domain.Common;
using ChurchAdmin.Domain.Enums;

namespace ChurchAdmin.Domain.Entities;

public sealed class InventoryItem : SoftDeletableEntity
{
    public string Name { get; set; } = string.Empty;

    public Guid TeamId { get; set; }

    public Team Team { get; set; } = null!;

    public string Description { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public InventoryCondition Condition { get; set; }

    public InventoryStatus Status { get; set; } = InventoryStatus.PendingApproval;

    public string? ImageUrl { get; set; }
}
