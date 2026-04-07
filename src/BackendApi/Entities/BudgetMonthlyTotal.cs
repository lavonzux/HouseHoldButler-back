namespace BackendApi.Entities
{
    public class BudgetMonthlyTotal
    {
        public Guid Id { get; set; }

        public DateOnly YearMonth { get; set; } // 月份基準日

        public decimal TotalBudget { get; set; } // 該月全域總預算額度

        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
    }
}
