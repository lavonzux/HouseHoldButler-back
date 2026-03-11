namespace BackendApi.Entities
{
    public class BudgetCategoryLimit
    {
        public Guid Id { get; set; }

        public Guid CategoryId { get; set; } // 對應 Categories.Id
        public Category Category { get; set; } = null!;

        public DateOnly YearMonth { get; set; } // 月份基準日

        public decimal BudgetAmount { get; set; } // 該類別該月預算額度

        public DateTimeOffset CreatedAt { get; set; } // 建立時間
        public DateTimeOffset UpdatedAt { get; set; } // 更新時間
    }
}
