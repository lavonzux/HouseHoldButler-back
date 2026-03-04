namespace BackendApi.Models;

public record UpdateCategoryRequest(string Name, Guid? ParentId, string? Icon);
