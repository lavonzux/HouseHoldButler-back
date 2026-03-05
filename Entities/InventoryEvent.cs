using BackendApi.Constants;

namespace BackendApi.Entities;

// Immutable event record — the ONLY source of truth for inventory state changes.
// Never update or delete rows in this table; append only.
//
// Event types (see InventoryEventType constants):
//   ADD     — user purchased / added stock. QuantityDelta > 0.
//   DEPLETE — user marks item as fully used up. QuantityDelta sets quantity to 0.
//   ADJUST  — user corrects the estimated quantity. QuantityDelta = new absolute value.
//   EXPIRE  — system marks stock as expired. QuantityDelta = 0.
//
// Note: There is intentionally no CONSUME event. Consumption between ADD/DEPLETE
// anchors is *estimated* by the background job using AvgConsumptionRate, not recorded.
public class InventoryEvent
{
    public Guid Id { get; set; }

    public Guid InventoryId { get; set; }

    // See InventoryEventType constants
    public string EventType { get; set; } = string.Empty;

    // Semantics vary by EventType:
    //   ADD     → quantity added (positive)
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