namespace HouseHoldButler.Entities;

// Records user feedback on a reminder, used to calibrate AvgConsumptionRate over time.
// The feedback loop: reminder fires → user says "too early" → system adjusts rate downward.
public class ReminderFeedback
{
    public Guid Id { get; set; }

    // FK to the specific reminder that prompted this feedback
    public Guid ReminderId { get; set; }

    // See FeedbackType constants
    public string FeedbackType { get; set; } = string.Empty;

    // Optional: user reports the actual remaining quantity at feedback time.
    // This serves as a calibration anchor for the estimation model.
    public decimal? ActualQuantity { get; set; }

    public string? Note { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    // Navigation properties
    public Reminder Reminder { get; set; } = null!;
}