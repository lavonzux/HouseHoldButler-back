using BackendApi.Constants;
using BackendApi.Entities;
using BackendApi.Models;
using BackendApi.Requests.Inventory;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InventoriesController(ApplicationDbContext db, ILogger<InventoriesController> logger) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? productId)
    {
        var query = db.Inventories.AsQueryable();

        if (productId.HasValue)
            query = query.Where(i => i.ProductId == productId.Value);

        return Ok(await query.ToListAsync());
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var inventory = await db.Inventories.FindAsync(id);
        if (inventory is null)
            return NotFound();
        return Ok(inventory);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateInventoryRequest request)
    {
        var inventory = new Inventory
        {
            Id = Guid.NewGuid(),
            ProductId = request.ProductId,
            Location = request.Location,
            InitialQuantity = request.InitialQuantity,
            CurrentQuantity = 1.0m,
            NearestExpiryDate = request.NearestExpiryDate,
            Status = request.Status ?? InventoryStatus.Active,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        db.Inventories.Add(inventory);
        await db.SaveChangesAsync();

        logger.LogInformation("Created inventory {Id} for product {ProductId}", inventory.Id, inventory.ProductId);
        return Ok(inventory);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateInventoryRequest request)
    {
        var inventory = await db.Inventories.FindAsync(id);
        if (inventory is null)
            return NotFound();

        inventory.Location = request.Location;
        if (request.InitialQuantity.HasValue)
            inventory.InitialQuantity = request.InitialQuantity.Value;
        inventory.NearestExpiryDate = request.NearestExpiryDate;
        inventory.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync();

        logger.LogInformation("Updated inventory {Id}", id);
        return Ok(inventory);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var inventory = await db.Inventories
            .Include(i => i.Events)
            .Include(i => i.Reminders)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (inventory is null)
            return NotFound();

        if (inventory.Events.Any())
            return Conflict("Cannot delete an inventory that has events.");

        if (inventory.Reminders.Any())
            return Conflict("Cannot delete an inventory that has reminders.");

        db.Inventories.Remove(inventory);
        await db.SaveChangesAsync();

        logger.LogInformation("Deleted inventory {Id}", id);
        return Ok();
    }
}
