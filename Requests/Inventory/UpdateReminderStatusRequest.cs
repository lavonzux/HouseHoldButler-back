namespace BackendApi.Requests.Inventory;

public record UpdateReminderStatusRequest(
    string Status,
    DateTimeOffset? SnoozedUntil);
