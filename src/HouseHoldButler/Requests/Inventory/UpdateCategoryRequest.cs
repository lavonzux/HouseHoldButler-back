namespace HouseHoldButler.Requests.Inventory;

public record UpdateCategoryRequest(string Name, Guid? ParentId, string? Icon);
