namespace BackendApi.Requests.Inventory;

public record CreateInventoryRequest(
    Guid ProductId,
    string? Location,
    decimal InitialQuantity = 1,
    DateOnly? NearestExpiryDate = null,
    string? Status = null);
