namespace HouseHoldButler.Dtos
{
    public class BudgetAlertDto
    {
        public Guid Id { get; set; }
        public Guid? CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string? CategoryIcon { get; set; }
        public DateOnly YearMonth { get; set; }
        public string AlertLevel { get; set; } = string.Empty;
        public decimal Percentage { get; set; }
        public bool IsRead { get; set; }
        public DateTimeOffset? ReadAt { get; set; }
        public DateTimeOffset? LastNotifiedAt { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
