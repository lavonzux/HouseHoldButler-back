using BackendApi.Dtos;
using BackendApi.Entities;
using BackendApi.Models;
using BackendApi.Requests.Inventory;
using BackendApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BackendApi.Services;

public class CategoryService : ICategoryService
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<CategoryService> _logger;

    public CategoryService(ApplicationDbContext db, ILogger<CategoryService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<ServiceResult<List<Category>>> GetAllAsync()
    {
        var categories = await _db.Categories
            .OrderBy(c => c.Name)
            .ToListAsync();
        return ServiceResult<List<Category>>.Success(categories);
    }

    public async Task<ServiceResult<Category>> GetByIdAsync(Guid id)
    {
        var category = await _db.Categories.FindAsync(id);
        if (category is null)
            return ServiceResult<Category>.NotFound();
        return ServiceResult<Category>.Success(category);
    }

    public async Task<ServiceResult<Category>> CreateAsync(CreateCategoryRequest request)
    {
        if (request.ParentId.HasValue)
        {
            var parent = await _db.Categories.FindAsync(request.ParentId.Value);
            if (parent is null)
                return ServiceResult<Category>.ValidationError("ParentId does not refer to an existing category.");
        }

        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            ParentId = request.ParentId,
            Icon = request.Icon,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _db.Categories.Add(category);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Created category {Id} ({Name})", category.Id, category.Name);
        return ServiceResult<Category>.Success(category);
    }

    public async Task<ServiceResult<Category>> UpdateAsync(Guid id, UpdateCategoryRequest request)
    {
        var category = await _db.Categories.FindAsync(id);
        if (category is null)
            return ServiceResult<Category>.NotFound();

        if (request.ParentId.HasValue)
        {
            var parent = await _db.Categories.FindAsync(request.ParentId.Value);
            if (parent is null)
                return ServiceResult<Category>.ValidationError("ParentId does not refer to an existing category.");
        }

        category.Name = request.Name;
        category.ParentId = request.ParentId;
        category.Icon = request.Icon;

        await _db.SaveChangesAsync();

        _logger.LogInformation("Updated category {Id}", id);
        return ServiceResult<Category>.Success(category);
    }

    public async Task<ServiceResult<object?>> DeleteAsync(Guid id)
    {
        var category = await _db.Categories
            .Include(c => c.SubCategories)
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category is null)
            return ServiceResult<object?>.NotFound();

        if (category.SubCategories.Any())
            return ServiceResult<object?>.Conflict("Cannot delete a category that has sub-categories.");

        if (category.Products.Any())
            return ServiceResult<object?>.Conflict("Cannot delete a category that has products.");

        _db.Categories.Remove(category);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Deleted category {Id}", id);
        return ServiceResult<object?>.Success(null);
    }

    public async Task<ServiceResult<List<CategoryDto>>> GetCategoryDropdownAsync()
    {
        var categories = await _db.Categories
            .OrderBy(c => c.Name)
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Icon = c.Icon
            })
            .ToListAsync();

        return ServiceResult<List<CategoryDto>>.Success(categories);
    }
}
