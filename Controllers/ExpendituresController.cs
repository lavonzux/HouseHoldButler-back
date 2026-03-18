using BackendApi.Dtos;
using BackendApi.Entities;
using BackendApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BackendApi.Requests.Expenditure;
using System.Linq.Expressions;
using YamlDotNet.Core.Tokens;

namespace BackendApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExpendituresController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ExpendituresController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/Expenditures/getExpenditures
    // 取得支出清單（支援搜尋與分類篩選）
    [HttpGet("getExpenditures")]
    public async Task<ActionResult<IEnumerable<ExpenditureDto>>> GetExpenditures(
        [FromQuery] string? search = null,
        [FromQuery] string? categoryId = null)
    {
        var query = _context.Expenditures
            .Include(e => e.Category)
            .Include(e => e.Product)
            .AsQueryable();

        // 關鍵字搜尋（物品名稱或描述）
        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim().ToLower();
            query = query.Where(e =>
                (e.Description != null && e.Description.ToLower().Contains(search)) ||
                (e.Product != null && e.Product.Name.ToLower().Contains(search))
            );
        }

        // 分類篩選,使用 categoryId (GUID)
        if (!string.IsNullOrWhiteSpace(categoryId))
        {
            if (Guid.TryParse(categoryId, out var parsedCategoryId))
            {
                query = query.Where(e => e.CategoryId == parsedCategoryId);
            }  
        }

        var result = await query
            .OrderByDescending(e => e.ExpenditureDate)
            .Select(e => new ExpenditureDto
            {
                Id = e.Id,
                ProductName = e.Product != null ? e.Product.Name : (e.Description ?? "未命名項目"),
                Category = e.Category != null ? e.Category.Name : "未分類",
                CategoryId = e.CategoryId,
                Amount = e.Amount,
                ExpenditureDate = e.ExpenditureDate,
                Description = e.Description,
                Source = e.Source,
                CreatedAt = e.CreatedAt,
                UpdatedAt = e.UpdatedAt
            })
            .ToListAsync();

        return Ok(result);
    }

    // POST: api/Expenditures/createExpenditure
    // 新增一筆支出紀錄
    [HttpPost("createExpenditure")]
    public async Task<ActionResult<ExpenditureDto>> CreateExpenditure([FromBody] CreateExpenditureRequest request)
    {
        var expenditure = new Expenditure
        {
            Id = Guid.NewGuid(),
            Amount = request.Amount,
            ExpenditureDate = request.ExpenditureDate,
            Description = request.Description,
            Source = request.Source ?? "現金",
            ProductId = request.ProductId,          // 可選：對應庫存商品
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        if (request.CategoryId.HasValue)
        {
            expenditure.CategoryId = request.CategoryId.Value;
        }

        _context.Expenditures.Add(expenditure);
        await _context.SaveChangesAsync();

        // 回傳建立後的資料（包含關聯查詢）
        var created = await _context.Expenditures
            .Include(e => e.Category)
            .Include(e => e.Product)
            .FirstOrDefaultAsync(e => e.Id == expenditure.Id);

        if (created == null) return NotFound();

        var dto = new ExpenditureDto
        {
            Id = created.Id,
            ProductName = created.Product?.Name ?? created.Description ?? "未命名",
            Category = created.Category?.Name ?? "未分類",
            CategoryId = created.CategoryId,
            Amount = created.Amount,
            ExpenditureDate = created.ExpenditureDate,
            Description = created.Description,
            Source = created.Source,
            CreatedAt = created.CreatedAt,
            UpdatedAt = created.UpdatedAt
        };

        return CreatedAtAction(nameof(GetExpenditure), new { id = dto.Id }, dto);
    }

    // GET: api/Expenditures/getExpenditure/{id}
    // 取得單筆支出詳細資料（供編輯使用）
    [HttpGet("getExpenditure/{id}")]
    public async Task<ActionResult<ExpenditureDto>> GetExpenditure(Guid id)
    {
        var expenditure = await _context.Expenditures
            .Include(e => e.Category)
            .Include(e => e.Product)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (expenditure == null)
            return NotFound();

        var dto = new ExpenditureDto
        {
            Id = expenditure.Id,
            ProductName = expenditure.Product?.Name ?? expenditure.Description ?? "未命名",
            Category = expenditure.Category?.Name ?? "未分類",
            CategoryId = expenditure.CategoryId,
            Amount = expenditure.Amount,
            ExpenditureDate = expenditure.ExpenditureDate,
            Description = expenditure.Description,
            Source = expenditure.Source,
            CreatedAt = expenditure.CreatedAt,
            UpdatedAt = expenditure.UpdatedAt
        };

        return Ok(dto);
    }

    // PUT: api/Expenditures/updateExpenditure/{id}
    // 更新單筆支出紀錄
    [HttpPut("updateExpenditure/{id}")]
    public async Task<IActionResult> UpdateExpenditure(Guid id, [FromBody] UpdateExpenditureRequest request)
    {
        var expenditure = await _context.Expenditures
            .Include(e => e.Category)
            .Include(e => e.Product)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (expenditure == null) return NotFound();

        // 只更新有傳入的欄位
        if (request.Amount.HasValue) expenditure.Amount = request.Amount.Value;
        if (request.ExpenditureDate.HasValue) expenditure.ExpenditureDate = request.ExpenditureDate.Value;
        if (request.Description != null) expenditure.Description = request.Description;
        if (request.Source != null) expenditure.Source = request.Source;
        if (request.CategoryId.HasValue) expenditure.CategoryId = request.CategoryId.Value;
        if (request.ProductId.HasValue) expenditure.ProductId = request.ProductId.Value;

        expenditure.UpdatedAt = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync();

        // SaveChangesAsync 後必須重新加載關聯數據， 原因：EF Core 不會自動刷新已加載的導航屬性，所以 Category 仍會是舊值或 null
        await _context.Entry(expenditure).ReloadAsync();
        await _context.Entry(expenditure).Reference(e => e.Category).LoadAsync();
        await _context.Entry(expenditure).Reference(e => e.Product).LoadAsync();

        var dto = new ExpenditureDto
        {
            Id = expenditure.Id,
            ProductName = expenditure.Product?.Name ?? expenditure.Description ?? "未命名",
            Category = expenditure.Category?.Name ?? "未分類",
            CategoryId = expenditure.CategoryId,
            Amount = expenditure.Amount,
            ExpenditureDate = expenditure.ExpenditureDate,
            Description = expenditure.Description,
            Source = expenditure.Source,
            CreatedAt = expenditure.CreatedAt,
            UpdatedAt = expenditure.UpdatedAt
        };

        return Ok(dto);
    }

    // DELETE: api/Expenditures/deleteExpenditure/{id}
    // 刪除單筆支出紀錄
    [HttpDelete("deleteExpenditure/{id}")]
    public async Task<IActionResult> DeleteExpenditure(Guid id)
    {
        var expenditure = await _context.Expenditures.FindAsync(id);
        if (expenditure == null)
            return NotFound();

        _context.Expenditures.Remove(expenditure);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}