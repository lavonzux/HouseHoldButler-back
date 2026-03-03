using BackendApi.Constants;
using BackendApi.Entities;
using BackendApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RemindersController(ApplicationDbContext db, ILogger<RemindersController> logger) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? inventoryId, [FromQuery] string? status)
    {
        var query = db.Reminders.AsQueryable();

        if (inventoryId.HasValue)
            query = query.Where(r => r.InventoryId == inventoryId.Value);

        if (!string.IsNullOrEmpty(status))
            query = query.Where(r => r.Status == status);

        return Ok(await query.OrderBy(r => r.ScheduledAt).ToListAsync());
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var reminder = await db.Reminders.FindAsync(id);
        if (reminder is null)
            return NotFound();
        return Ok(reminder);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateReminderRequest request)
    {
        var reminder = new Reminder
        {
            Id = Guid.NewGuid(),
            InventoryId = request.InventoryId,
            ReminderType = request.ReminderType,
            Status = request.Status ?? ReminderStatus.Pending,
            ScheduledAt = request.ScheduledAt,
            SentAt = request.SentAt,
            SnoozedUntil = request.SnoozedUntil,
            CreatedAt = DateTimeOffset.UtcNow
        };

        db.Reminders.Add(reminder);
        await db.SaveChangesAsync();

        logger.LogInformation("Created reminder {Id} ({ReminderType}) for inventory {InventoryId}",
            reminder.Id, reminder.ReminderType, reminder.InventoryId);
        return Ok(reminder);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateReminderRequest request)
    {
        var reminder = await db.Reminders.FindAsync(id);
        if (reminder is null)
            return NotFound();

        reminder.Status = request.Status;
        reminder.SentAt = request.SentAt;
        reminder.SnoozedUntil = request.SnoozedUntil;

        await db.SaveChangesAsync();

        logger.LogInformation("Updated reminder {Id}", id);
        return Ok(reminder);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var reminder = await db.Reminders
            .Include(r => r.Feedbacks)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (reminder is null)
            return NotFound();

        if (reminder.Feedbacks.Any())
            return Conflict("Cannot delete a reminder that has feedback.");

        db.Reminders.Remove(reminder);
        await db.SaveChangesAsync();

        logger.LogInformation("Deleted reminder {Id}", id);
        return Ok();
    }
}
