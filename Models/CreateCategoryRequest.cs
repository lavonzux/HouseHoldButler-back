namespace BackendApi.Models;

public record CreateCategoryRequest(string Name, Guid? ParentId, string? Icon);
