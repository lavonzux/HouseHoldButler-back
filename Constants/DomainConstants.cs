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

public static class BudgetAlertLevel
{
    public const string Normal = "Normal"; // 支出占預算 60% 以下
    public const string Warning60 = "Warning60"; // 支出占預算 60%
    public const string Warning80 = "Warning80"; // 支出占預算 80%
    public const string Danger90 = "Danger90"; // 支出占預算 90%
    public const string Critical100 = "Critical100"; // 支出占預算 100% 或以上
}

public static class ExpenditureSource
{
    public const string Manual = "MANUAL"; // 手動輸入
    public const string ReceiptScan = "RECEIPT_SCAN"; // 收據掃描
}
