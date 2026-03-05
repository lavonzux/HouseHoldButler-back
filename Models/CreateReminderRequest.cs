namespace BackendApi.Models;

public record CreateReminderRequest(
    Guid InventoryId,
    string ReminderType,
    DateTimeOffset ScheduledAt,
    string? Status,
    DateTimeOffset? SentAt,
    DateTimeOffset? SnoozedUntil);
