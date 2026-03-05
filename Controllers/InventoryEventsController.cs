using BackendApi.Constants;
using BackendApi.Entities;
using BackendApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendApi.Controllers;

// InventoryEvent is an append-only audit log — no update or delete endpoints.
[ApiController]
[Route("api/[controller]")]
public class InventoryEventsController(ApplicationDbContext db, ILogger<InventoryEventsController> logger) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? inventoryId)
    {
        var query = db.InventoryEvents.AsQueryable();

        if (inventoryId.HasValue)
            query = query.Where(e => e.InventoryId == inventoryId.Value);

        return Ok(await query.OrderBy(e => e.CreatedAt).ToListAsync());
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var ev = await db.InventoryEvents.FindAsync(id);
        if (ev is null)
            return NotFound();
        return Ok(ev);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateInventoryEventRequest request)
    {
        var ev = new InventoryEvent
        {
            Id = Guid.NewGuid(),
            InventoryId = request.InventoryId,
            EventType = request.EventType,
            QuantityDelta = request.QuantityDelta,
            Note = request.Note,
            Source = request.Source ?? EventSource.Manual,
            CreatedAt = DateTimeOffset.UtcNow
        };

        db.InventoryEvents.Add(ev);
        await db.SaveChangesAsync();

        logger.LogInformation("Created inventory event {Id} ({EventType}) for inventory {InventoryId}",
            ev.Id, ev.EventType, ev.InventoryId);
        return Ok(ev);
    }
}
