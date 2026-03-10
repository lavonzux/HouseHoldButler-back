using BackendApi.Constants;
using BackendApi.Entities;
using BackendApi.Models;
using BackendApi.Requests.Inventory;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RemindersController(ApplicationDbContext db, ILogger<RemindersController> logger) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? inventoryId,
        [FromQuery] string? status,
        [FromQuery] string? type)
    {
        var query = db.Reminders.AsQueryable();

        if (inventoryId.HasValue)
            query = query.Where(r => r.InventoryId == inventoryId.Value);

        if (!string.IsNullOrEmpty(status))
            query = query.Where(r => r.Status == status);

        if (!string.IsNullOrEmpty(type))
            query = query.Where(r => r.ReminderType == type);

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

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, UpdateReminderStatusRequest request)
    {
        var validStatuses = new[] { ReminderStatus.Dismissed, ReminderStatus.Snoozed };
        if (!validStatuses.Contains(request.Status))
            return BadRequest($"Status must be one of: {string.Join(", ", validStatuses)}");

        if (request.Status == ReminderStatus.Snoozed)
        {
            if (request.SnoozedUntil is null)
                return BadRequest("SnoozedUntil is required when status is SNOOZED.");
            if (request.SnoozedUntil <= DateTimeOffset.UtcNow)
                return BadRequest("SnoozedUntil must be in the future.");
        }

        var reminder = await db.Reminders.FindAsync(id);
        if (reminder is null)
            return NotFound();

        var allowedCurrentStatuses = new[] { ReminderStatus.Pending, ReminderStatus.Snoozed };
        if (!allowedCurrentStatuses.Contains(reminder.Status))
            return Conflict($"Cannot update status of a reminder that is already {reminder.Status}.");

        reminder.Status = request.Status;
        reminder.SnoozedUntil = request.Status == ReminderStatus.Snoozed ? request.SnoozedUntil : null;

        await db.SaveChangesAsync();

        logger.LogInformation("Updated reminder {Id} status to {Status}", id, request.Status);
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
