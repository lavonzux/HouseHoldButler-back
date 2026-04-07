namespace BackendApi.Requests.Inventory;

public record CreateReminderFeedbackRequest(
    Guid ReminderId,
    string FeedbackType,
    decimal? ActualQuantity,
    string? Note);
