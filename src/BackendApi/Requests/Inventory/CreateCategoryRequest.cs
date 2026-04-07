namespace BackendApi.Requests.Inventory;

public record CreateCategoryRequest(string Name, Guid? ParentId, string? Icon);
