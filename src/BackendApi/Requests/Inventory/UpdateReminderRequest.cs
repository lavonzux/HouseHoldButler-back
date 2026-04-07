namespace BackendApi.Requests.Inventory;

public record UpdateReminderRequest(
    string Status,
    DateTimeOffset? SentAt,
    DateTimeOffset? SnoozedUntil);
