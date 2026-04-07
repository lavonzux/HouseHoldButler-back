namespace HouseHoldButler.Dtos;

public class ExpenditureDto
{
    public Guid Id { get; set; } // 支出 ID
    public string ProductName { get; set; } = null!; // 物品名稱（來自 Product.Name）
    public string Category { get; set; } = null!; // 分類名稱（來自 Category.Name）
    public Guid? CategoryId { get; set; } // 分類 ID
    public decimal Amount { get; set; } // 支出金額
    public DateOnly ExpenditureDate { get; set; } // 支出日期（yyyy-MM-dd）
    public string? Description { get; set; } // 說明（選填）
    public string Source { get; set; } = null!; // 來源
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}