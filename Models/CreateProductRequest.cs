namespace BackendApi.Models;

public record CreateProductRequest(
    string Name,
    Guid? CategoryId,
    string? Barcode,
    decimal AvgConsumptionRate,
    decimal LowStockThreshold);
