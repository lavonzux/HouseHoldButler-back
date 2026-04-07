namespace BackendApi.Requests.Inventory;

public record CreateProductTagRequest(Guid ProductId, Guid TagId);
