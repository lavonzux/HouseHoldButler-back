using BackendApi.Entities;
using BackendApi.Models;
using BackendApi.Requests.Inventory;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendApi.Controllers;

// ReminderFeedback is immutable once submitted — no update or delete endpoints.
[ApiController]
[Route("api/[controller]")]
public class ReminderFeedbacksController(ApplicationDbContext db, ILogger<ReminderFeedbacksController> logger) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? reminderId)
    {
        var query = db.ReminderFeedbacks.AsQueryable();

        if (reminderId.HasValue)
            query = query.Where(f => f.ReminderId == reminderId.Value);

        return Ok(await query.OrderBy(f => f.CreatedAt).ToListAsync());
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var feedback = await db.ReminderFeedbacks.FindAsync(id);
        if (feedback is null)
            return NotFound();
        return Ok(feedback);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateReminderFeedbackRequest request)
    {
        var feedback = new ReminderFeedback
        {
            Id = Guid.NewGuid(),
            ReminderId = request.ReminderId,
            FeedbackType = request.FeedbackType,
            ActualQuantity = request.ActualQuantity,
            Note = request.Note,
            CreatedAt = DateTimeOffset.UtcNow
        };

        db.ReminderFeedbacks.Add(feedback);
        await db.SaveChangesAsync();

        logger.LogInformation("Created feedback {Id} ({FeedbackType}) for reminder {ReminderId}",
            feedback.Id, feedback.FeedbackType, feedback.ReminderId);
        return Ok(feedback);
    }
}
