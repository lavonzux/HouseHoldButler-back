namespace HouseHoldButler.Requests.Inventory;

public record UpdateReminderStatusRequest(
    string Status,
    DateTimeOffset? SnoozedUntil);
