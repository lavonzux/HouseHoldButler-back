namespace HouseHoldButler.Requests.Inventory;

public record CreateProductRequest(
    string Name,
    Guid? CategoryId,
    string? Barcode,
    string? Unit,
    decimal AvgConsumptionRate,
    decimal LowStockThreshold);
