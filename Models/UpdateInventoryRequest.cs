namespace BackendApi.Models;

public record UpdateInventoryRequest(
    string? Location,
    decimal CurrentQuantity,
    DateTimeOffset? EstimatedDepletionDate,
    DateOnly? NearestExpiryDate,
    string Status);
