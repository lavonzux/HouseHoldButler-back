using BackendApi.Dtos;
using BackendApi.Entities;
using BackendApi.Models;
using BackendApi.Requests;
using BackendApi.Requests.Budget;
using BackendApi.Requests.Inventory;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SendGrid.Helpers.Mail;

namespace BackendApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<CategoriesController> _logger;

    public CategoriesController(ApplicationDbContext db, ILogger<CategoriesController> logger)
    {
        _db = db;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var categories = await _db.Categories
            .OrderBy(c => c.Name)
            .ToListAsync();
        return Ok(categories);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var category = await _db.Categories.FindAsync(id);
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

        _db.Categories.Add(category);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Created category {Id} ({Name})", category.Id, category.Name);
        return Ok(category);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateCategoryRequest request)
    {
        var category = await _db.Categories.FindAsync(id);
        if (category is null)
            return NotFound();

        category.Name = request.Name;
        category.ParentId = request.ParentId;
        category.Icon = request.Icon;

        await _db.SaveChangesAsync();

        _logger.LogInformation("Updated category {Id}", id);
        return Ok(category);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var category = await _db.Categories
            .Include(c => c.SubCategories)
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category is null)
            return NotFound();

        if (category.SubCategories.Any())
            return Conflict("Cannot delete a category that has sub-categories.");

        if (category.Products.Any())
            return Conflict("Cannot delete a category that has products.");

        _db.Categories.Remove(category);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Deleted category {Id}", id);
        return Ok();
    }

    // 取得所有分類 (供前端下拉選單使用)
    [HttpGet("getCategories")]
    public async Task<ActionResult<List<CategoryDto>>> GetCategories()
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

        return Ok(categories);
    }    
}
