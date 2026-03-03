namespace BackendApi.Models;

public record CreateInventoryEventRequest(
    Guid InventoryId,
    string EventType,
    decimal QuantityDelta,
    string? Note,
    string? Source);
