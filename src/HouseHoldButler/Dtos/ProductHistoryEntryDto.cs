namespace HouseHoldButler.Dtos;

public record ProductHistoryEntryDto(
    string EntryType,         // "PURCHASE" | "DEPLETE" | "ADJUST" | "EXPIRE"
    DateTimeOffset OccurredAt,
    Guid InventoryId,
    string InventoryStatus,
    decimal? InitialQuantity, // PURCHASE only
    string? Location,         // PURCHASE only
    decimal? QuantityDelta,   // ADJUST only
    string? Source,           // null for PURCHASE
    string? Note
);
