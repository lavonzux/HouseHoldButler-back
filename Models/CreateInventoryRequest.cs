namespace BackendApi.Models;

public record CreateInventoryRequest(
    Guid ProductId,
    string? Location,
    decimal CurrentQuantity,
    DateTimeOffset? EstimatedDepletionDate,
    DateOnly? NearestExpiryDate,
    string? Status);
