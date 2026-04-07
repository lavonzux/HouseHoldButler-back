namespace BackendApi.Requests.Inventory;

public record UpdateInventoryRequest(
    string? Location,
    decimal? InitialQuantity,
    DateTime? NearestExpiryDate);
