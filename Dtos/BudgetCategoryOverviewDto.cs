namespace BackendApi.Dtos
{
    public class BudgetCategoryOverviewDto
    {
        public Guid CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public string? Icon { get; set; }
        public decimal BudgetAmount { get; set; }
        public decimal ActualSpent { get; set; }
        public decimal Percentage { get; set; }
    }
}
