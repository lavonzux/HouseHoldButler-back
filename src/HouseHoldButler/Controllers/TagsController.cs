using HouseHoldButler.Entities;
using HouseHoldButler.Models;
using HouseHoldButler.Requests.Inventory;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HouseHoldButler.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TagsController(ApplicationDbContext db, ILogger<TagsController> logger) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var tags = await db.Tags.OrderBy(t => t.Name).ToListAsync();
        return Ok(tags);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var tag = await db.Tags.FindAsync(id);
        if (tag is null)
            return NotFound();
        return Ok(tag);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateTagRequest request)
    {
        var tag = new Tag
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            CreatedAt = DateTimeOffset.UtcNow
        };

        db.Tags.Add(tag);
        await db.SaveChangesAsync();

        logger.LogInformation("Created tag {Id} ({Name})", tag.Id, tag.Name);
        return Ok(tag);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateTagRequest request)
    {
        var tag = await db.Tags.FindAsync(id);
        if (tag is null)
            return NotFound();

        tag.Name = request.Name;

        await db.SaveChangesAsync();

        logger.LogInformation("Updated tag {Id}", id);
        return Ok(tag);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var tag = await db.Tags
            .Include(t => t.ProductTags)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (tag is null)
            return NotFound();

        if (tag.ProductTags.Any())
            return Conflict("Cannot delete a tag that is assigned to products.");

        db.Tags.Remove(tag);
        await db.SaveChangesAsync();

        logger.LogInformation("Deleted tag {Id}", id);
        return Ok();
    }
}
