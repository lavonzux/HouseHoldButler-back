using HouseHoldButler.Constants;

namespace HouseHoldButler.Entities;

// Immutable event record — the ONLY source of truth for inventory state changes.
// Never update or delete rows in this table; append only.
//
// Event types (see InventoryEventType constants):
//   DEPLETE — user marks item as fully used up. QuantityDelta is ignored (implies 0).
//   ADJUST  — user corrects the estimated quantity. QuantityDelta = new absolute value (0.0 ~ 1.0).
//   EXPIRE  — system marks stock as expired. QuantityDelta is ignored (implies 0).
//
// Note: There is intentionally no CONSUME event. Consumption between anchors
// is *estimated* by the recalculation service using AvgConsumptionRate, not recorded.
public class InventoryEvent
{
    public Guid Id { get; set; }

    public Guid InventoryId { get; set; }

    // See InventoryEventType constants
    public string EventType { get; set; } = string.Empty;

    // Semantics vary by EventType:
    //   DEPLETE → ignored (implies full depletion)
    //   ADJUST  → new absolute quantity (0.0 ~ 1.0)
    //   EXPIRE  → ignored (implies full expiry)
    public decimal QuantityDelta { get; set; }

    public string? Note { get; set; }

    // See EventSource constants (MANUAL = user action, SYSTEM = background job)
    public string Source { get; set; } = EventSource.Manual;

    public DateTimeOffset CreatedAt { get; set; }

    // Navigation properties
    public Inventory Inventory { get; set; } = null!;
}