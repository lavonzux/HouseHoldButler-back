namespace BackendApi.Requests.Inventory;

public record UpdateProductRequest(
    string Name,
    Guid? CategoryId,
    string? Barcode,
    string? Unit,
    decimal AvgConsumptionRate,
    decimal LowStockThreshold);
