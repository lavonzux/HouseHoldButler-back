using HouseHoldButler.Constants;
using HouseHoldButler.Models;

namespace HouseHoldButler.Entities
{
    public class BudgetAlert
    {
        public Guid Id { get; set; }
        public Guid? CategoryId { get; set; } // 對應 Categories.Id；若為總預算警示則為 null
        public Category? Category { get; set; }  // navigation (optional)

        public DateOnly YearMonth { get; set; }   // 使用 DateOnly 更明確表示「月份」

        public string AlertLevel { get; set; } = BudgetAlertLevel.Normal; // 目前警示等級，例如：Normal, Warning60, Warning80, Danger90, Critical100

        public decimal Percentage { get; set; } // 觸發時的實際百分比

        public bool IsRead { get; set; } = false; // 是否已讀，讓前端可以顯示未讀標記

        public DateTimeOffset? ReadAt { get; set; } // 標記為已讀的時間

        public DateTimeOffset? LastNotifiedAt { get; set; } // 上次發送通知的時間，用來避免短時間內重複推播

        public DateTimeOffset CreatedAt { get; set; } // 記錄建立時間
        public DateTimeOffset UpdatedAt { get; set; } // 最後更新時間
    }
}
