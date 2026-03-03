using BackendApi.Constants;
using System.Text.Json;

namespace BackendApi.Entities;

public class Reminder
{
    public Guid Id { get; set; }

    public Guid InventoryId { get; set; }

    // See ReminderType constants
    public string ReminderType { get; set; } = string.Empty;

    // See ReminderStatus constants
    public string Status { get; set; } = ReminderStatus.Pending;

    // When the background job scheduled this reminder to fire
    public DateTimeOffset ScheduledAt { get; set; }

    // Populated once the notification is actually delivered
    public DateTimeOffset? SentAt { get; set; }

    // Populated when user selects "remind me later"
    public DateTimeOffset? SnoozedUntil { get; set; }

    // Snapshot of system state at the moment this reminder was created.
    // Used to explain *why* the reminder fired and to train reminder_feedback.
    // Example: { "estimated_remaining": 0.12, "estimated_depletion_date": "2025-03-10" }
    public JsonDocument? Metadata { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    // Navigation properties
    public Inventory Inventory { get; set; } = null!;
    public ICollection<ReminderFeedback> Feedbacks { get; set; } = [];
}