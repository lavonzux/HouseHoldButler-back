using BackendApi.Entities;
using BackendApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController(ApplicationDbContext db, ILogger<CategoriesController> logger) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var categories = await db.Categories
            .OrderBy(c => c.Name)
            .ToListAsync();
        return Ok(categories);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var category = await db.Categories.FindAsync(id);
        if (category is null)
            return NotFound();
        return Ok(category);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateCategoryRequest request)
    {
        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            ParentId = request.ParentId,
            Icon = request.Icon,
            CreatedAt = DateTimeOffset.UtcNow
        };

        db.Categories.Add(category);
        await db.SaveChangesAsync();

        logger.LogInformation("Created category {Id} ({Name})", category.Id, category.Name);
        return Ok(category);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateCategoryRequest request)
    {
        var category = await db.Categories.FindAsync(id);
        if (category is null)
            return NotFound();

        category.Name = request.Name;
        category.ParentId = request.ParentId;
        category.Icon = request.Icon;

        await db.SaveChangesAsync();

        logger.LogInformation("Updated category {Id}", id);
        return Ok(category);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var category = await db.Categories
            .Include(c => c.SubCategories)
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category is null)
            return NotFound();

        if (category.SubCategories.Any())
            return Conflict("Cannot delete a category that has sub-categories.");

        if (category.Products.Any())
            return Conflict("Cannot delete a category that has products.");

        db.Categories.Remove(category);
        await db.SaveChangesAsync();

        logger.LogInformation("Deleted category {Id}", id);
        return Ok();
    }
}
