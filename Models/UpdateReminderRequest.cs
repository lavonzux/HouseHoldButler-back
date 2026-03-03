namespace BackendApi.Models;

public record UpdateReminderRequest(
    string Status,
    DateTimeOffset? SentAt,
    DateTimeOffset? SnoozedUntil);
