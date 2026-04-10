using HouseHoldButler.Constants;

namespace HouseHoldButler.Entities;

public class Inventory
{
    public Guid Id { get; set; }

    public Guid ProductId { get; set; }

    // Physical location hint for the user (e.g. "冰箱", "儲藏室").
    // Not enforced as a FK — kept as free text to avoid over-engineering.
    public string? Location { get; set; }

    // The quantity purchased in this buy (e.g. 12 bottles, 1 bag).
    // Defaults to 1. UI multiplies CurrentQuantity × InitialQuantity
    // to show a human-readable remaining amount.
    public decimal InitialQuantity { get; set; } = 1;

    // Estimated remaining quantity as a percentage (0.0 ~ 1.0).
    // This is a DERIVED/ESTIMATED value, not the source of truth.
    // Source of truth is InventoryEvents. Recalculated by events and nightly job.
    public decimal CurrentQuantity { get; set; }

    // Projected date when CurrentQuantity reaches zero, based on AvgConsumptionRate.
    // Null if product is non-consumable (AvgConsumptionRate == 0).
    public DateTimeOffset? EstimatedDepletionDate { get; set; }

    // Earliest expiry date among all active stock for this inventory.
    // Simplified alternative to full batch tracking — sufficient for Phase 1.
    public DateTime? NearestExpiryDate { get; set; }

    // See InventoryStatus constants
    public string Status { get; set; } = InventoryStatus.Active;

    // Optional free-text note attached to this inventory entry by the user.
    public string? Note { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    // Navigation properties
    public Product Product { get; set; } = null!;
    public ICollection<InventoryEvent> Events { get; set; } = [];
    public ICollection<Reminder> Reminders { get; set; } = [];
}