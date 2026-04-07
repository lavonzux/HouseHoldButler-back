using HouseHoldButler.Dtos;

namespace HouseHoldButler.DTOs
{
    public class MonthlyBudgetOverviewDto
    {
        public DateOnly YearMonth { get; set; }
        public decimal TotalBudget { get; set; }
        public decimal TotalSpent { get; set; }
        public decimal TotalPercentage { get; set; }
        public List<BudgetCategoryOverviewDto> Categories { get; set; } = new();
    }
}
