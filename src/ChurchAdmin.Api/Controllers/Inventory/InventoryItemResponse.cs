using ChurchAdmin.Domain.Entities;
using ChurchAdmin.Domain.Enums;

namespace ChurchAdmin.Api.Controllers.Inventory;

public sealed class InventoryItemResponse
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public Guid TeamId { get; set; }

    public string TeamName { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public InventoryCondition Condition { get; set; }

    public InventoryStatus Status { get; set; }

    public string? ImageUrl { get; set; }

    public static InventoryItemResponse FromEntity(InventoryItem item)
    {
        return new InventoryItemResponse
        {
            Id = item.Id,
            Name = item.Name,
            TeamId = item.TeamId,
            TeamName = item.Team.Name,
            Description = item.Description,
            Quantity = item.Quantity,
            Condition = item.Condition,
            Status = item.Status,
            ImageUrl = item.ImageUrl
        };
    }
}