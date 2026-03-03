namespace BackendApi.Models;

public record CreateReminderFeedbackRequest(
    Guid ReminderId,
    string FeedbackType,
    decimal? ActualQuantity,
    string? Note);
