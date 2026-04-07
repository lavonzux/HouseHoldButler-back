namespace HouseHoldButler.Requests.Inventory;

public record CreateInventoryRequest(
    Guid ProductId,
    string? Location,
    decimal InitialQuantity = 1,
    DateTime? NearestExpiryDate = null,
    string? Status = null);
