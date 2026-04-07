using System;
using System.ComponentModel.DataAnnotations;

namespace BackendApi.Requests.Expenditure;

/// 用於更新既有支出紀錄的請求物件，所有欄位皆為可選，僅更新有傳值的欄位
public class UpdateExpenditureRequest
{
    /// 支出金額
    [Range(0.01, double.MaxValue, ErrorMessage = "金額必須大於 0")]
    public decimal? Amount { get; set; }

    /// 支出日期
    public DateOnly? ExpenditureDate { get; set; }

    /// 支出說明 / 備註
    [StringLength(500, ErrorMessage = "說明不得超過 500 個字元")]
    public string? Description { get; set; }

    /// 付款方式 / 來源
    [StringLength(50)]
    public string? Source { get; set; }

    /// 支出所屬分類的 ID
    public Guid? CategoryId { get; set; }

    /// 若此筆支出與某庫存商品相關，可指定商品 ID
    public Guid? ProductId { get; set; }
}