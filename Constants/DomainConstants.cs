namespace BackendApi.Constants;

public static class InventoryEventType
{
    public const string Add = "ADD";
    public const string Deplete = "DEPLETE";
    public const string Adjust = "ADJUST";
    public const string Expire = "EXPIRE";
}

public static class InventoryStatus
{
    public const string Active = "ACTIVE";
    public const string Depleted = "DEPLETED";
    public const string Expired = "EXPIRED";
}

public static class EventSource
{
    public const string Manual = "MANUAL";
    public const string System = "SYSTEM";
}

public static class ReminderType
{
    public const string LowStock = "LOW_STOCK";
    public const string Expiring = "EXPIRING";
    public const string DepletionEstimated = "DEPLETION_ESTIMATED";
}

public static class ReminderStatus
{
    public const string Pending = "PENDING";
    public const string Sent = "SENT";
    public const string Dismissed = "DISMISSED";
    public const string Snoozed = "SNOOZED";
}

public static class FeedbackType
{
    public const string TooEarly = "TOO_EARLY";
    public const string TooLate = "TOO_LATE";
    public const string Accurate = "ACCURATE";
}