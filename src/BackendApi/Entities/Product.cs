namespace BackendApi.Entities;

public class Product
{
    public Guid Id { get; set; }

    // nullable: allows products to be uncategorized
    public Guid? CategoryId { get; set; }

    public string Name { get; set; } = string.Empty;

    // For barcode scanner integration (Phase 7)
    public string? Barcode { get; set; }

    // Display unit for the product (e.g. "瓶", "包", "kg").
    // Used by UI to convert percentage back to a human-readable quantity.
    public string? Unit { get; set; }

    // Estimated percentage consumed per day (0.0 ~ 1.0).
    // 0 = non-consumable item (e.g. appliances) — no depletion reminder will fire.
    // NOTE: This is a single-household assumption. If multi-household support is
    // ever added, this field should move to Inventory.
    public decimal AvgConsumptionRate { get; set; }

    // Percentage threshold (0.0 ~ 1.0) below which a LOW_STOCK reminder is created.
    public decimal LowStockThreshold { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    // Navigation properties
    public Category? Category { get; set; }
    public ICollection<ProductTag> ProductTags { get; set; } = [];
    public ICollection<Inventory> Inventories { get; set; } = [];
}