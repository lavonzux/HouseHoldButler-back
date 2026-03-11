using BackendApi.Constants;

namespace BackendApi.Entities
{
    public class Expenditure
    {
        public Guid Id { get; set; }

        public Guid CategoryId { get; set; } // 支出所屬的預算類別（食品、日用品等），用來累計各類別支出
        public Category Category { get; set; } = null!;

        public decimal Amount { get; set; } // 支出金額

        public DateOnly ExpenditureDate { get; set; } // 支出發生日期（只記錄日期，不含時間），用來按月統計

        public string? Description { get; set; } // 簡短說明（例如：買菜、買衛生紙、繳水電費）

        public Guid? InventoryEventId { get; set; } // 若因購買庫存而產生支出，關聯到 InventoryEvents 的 ADD 事件
        public InventoryEvent? InventoryEvent { get; set; } 

        public Guid? ProductId { get; set; } // 若針對特定產品，可記錄（方便未來分析單品花費）
        public Product? Product { get; set; }

        public Guid? InventoryId { get; set; } // 若補充到特定庫存位置，可記錄
        public Inventory? Inventory { get; set; }

        public string Source { get; set; } = ExpenditureSource.Manual; // 支出來源，例如 "Manual"、"ReceiptScan"

        public string? Note { get; set; } // 額外備註

        public DateTimeOffset CreatedAt { get; set; } // 記錄建立時間
        public DateTimeOffset UpdatedAt { get; set; } //記錄最後修改時間
    }
}
