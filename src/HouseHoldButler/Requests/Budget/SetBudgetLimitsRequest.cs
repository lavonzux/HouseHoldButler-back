namespace HouseHoldButler.Requests.Budget
{
    public class SetBudgetLimitsRequest
    {
        public DateOnly YearMonth { get; set; }
        public List<BudgetLimitItem> Items { get; set; } = new();
    }

    public class BudgetLimitItem
    {
        public Guid CategoryId { get; set; }
        public decimal BudgetAmount { get; set; }
    }
}
