using HouseHoldButler.Dtos;
using HouseHoldButler.Entities;
using HouseHoldButler.Requests.Inventory;
using HouseHoldButler.Services.Interfaces;

namespace HouseHoldButler.Services.Mock;

/// <summary>
/// In-memory mock implementation of ICategoryService.
/// Use during development to skip the database.
/// Swap in Program.cs: replace AddScoped&lt;ICategoryService, CategoryService&gt;()
///                      with    AddScoped&lt;ICategoryService, MockCategoryService&gt;()
/// </summary>
public class MockCategoryService : ICategoryService
{
    // internal static so sibling mocks can do cross-entity FK checks
    internal static readonly List<Category> Store =
    [
        new Category
        {
            Id = new Guid("a0000000-0000-0000-0000-000000000001"),
            Name = "食品",
            Icon = "food",
            CreatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero)
        },
        new Category
        {
            Id = new Guid("a0000000-0000-0000-0000-000000000002"),
            Name = "飲品",
            Icon = "beverage",
            CreatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero)
        },
        new Category
        {
            Id = new Guid("a0000000-0000-0000-0000-000000000003"),
            Name = "清潔用品",
            Icon = "cleaning",
            CreatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero)
        },
        new Category
        {
            Id = new Guid("b0000000-0000-0000-0000-000000000001"),
            ParentId = new Guid("a0000000-0000-0000-0000-000000000001"),
            Name = "生鮮蔬果",
            Icon = "vegetable",
            CreatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero)
        },
        new Category
        {
            Id = new Guid("b0000000-0000-0000-0000-000000000002"),
            ParentId = new Guid("a0000000-0000-0000-0000-000000000001"),
            Name = "乳製品",
            Icon = "dairy",
            CreatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero)
        },
        new Category
        {
            Id = new Guid("b0000000-0000-0000-0000-000000000021"),
            ParentId = new Guid("a0000000-0000-0000-0000-000000000003"),
            Name = "洗衣用品",
            Icon = "laundry",
            CreatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero)
        }
    ];

    public Task<ServiceResult<List<Category>>> GetAllAsync()
    {
        var result = Store.OrderBy(c => c.Name).ToList();
        return Task.FromResult(ServiceResult<List<Category>>.Success(result));
    }

    public Task<ServiceResult<Category>> GetByIdAsync(Guid id)
    {
        var category = Store.FirstOrDefault(c => c.Id == id);
        return Task.FromResult(category is null
            ? ServiceResult<Category>.NotFound()
            : ServiceResult<Category>.Success(category));
    }

    public Task<ServiceResult<Category>> CreateAsync(CreateCategoryRequest request)
    {
        if (request.ParentId.HasValue && Store.All(c => c.Id != request.ParentId.Value))
            return Task.FromResult(ServiceResult<Category>.ValidationError("ParentId does not refer to an existing category."));

        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            ParentId = request.ParentId,
            Icon = request.Icon,
            CreatedAt = DateTimeOffset.UtcNow
        };
        Store.Add(category);
        return Task.FromResult(ServiceResult<Category>.Success(category));
    }

    public Task<ServiceResult<Category>> UpdateAsync(Guid id, UpdateCategoryRequest request)
    {
        var category = Store.FirstOrDefault(c => c.Id == id);
        if (category is null)
            return Task.FromResult(ServiceResult<Category>.NotFound());

        if (request.ParentId.HasValue && Store.All(c => c.Id != request.ParentId.Value))
            return Task.FromResult(ServiceResult<Category>.ValidationError("ParentId does not refer to an existing category."));

        category.Name = request.Name;
        category.ParentId = request.ParentId;
        category.Icon = request.Icon;
        return Task.FromResult(ServiceResult<Category>.Success(category));
    }

    public Task<ServiceResult<object?>> DeleteAsync(Guid id)
    {
        var category = Store.FirstOrDefault(c => c.Id == id);
        if (category is null)
            return Task.FromResult(ServiceResult<object?>.NotFound());

        if (Store.Any(c => c.ParentId == id))
            return Task.FromResult(ServiceResult<object?>.Conflict("Cannot delete a category that has sub-categories."));

        if (MockProductService.Store.Any(p => p.CategoryId == id))
            return Task.FromResult(ServiceResult<object?>.Conflict("Cannot delete a category that has products."));

        Store.Remove(category);
        return Task.FromResult(ServiceResult<object?>.Success(null));
    }

    public Task<ServiceResult<List<CategoryDto>>> GetCategoryDropdownAsync()
    {
        var dtos = Store
            .OrderBy(c => c.Name)
            .Select(c => new CategoryDto { Id = c.Id, Name = c.Name, Icon = c.Icon })
            .ToList();
        return Task.FromResult(ServiceResult<List<CategoryDto>>.Success(dtos));
    }
}
