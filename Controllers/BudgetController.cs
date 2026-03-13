using BackendApi.Constants;
using BackendApi.Dtos;
using BackendApi.DTOs;
using BackendApi.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BudgetController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public BudgetController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 取得某個月的所有類別預算 + 實際支出 + 計算百分比
        [HttpGet("getMonthlyBudget")]
        public async Task<ActionResult<MonthlyBudgetOverviewDto>> GetMonthlyBudget([FromQuery] DateOnly? yearMonth = null)
        {

            var targetMonth = yearMonth ?? DateOnly.FromDateTime(DateTime.Today);

            var limits = await _context.BudgetCategoryLimits
                .Where(b => b.YearMonth == targetMonth)
                .Include(b => b.Category)
                .ToListAsync();

            var categoryIds = limits.Select(l => l.CategoryId).ToList();

            var expenditures = await _context.Expenditures
                .Where(e => e.ExpenditureDate.Year == targetMonth.Year &&
                            e.ExpenditureDate.Month == targetMonth.Month &&
                            categoryIds.Contains(e.CategoryId))
                .GroupBy(e => e.CategoryId)
                .Select(g => new
                {
                    CategoryId = g.Key,
                    ActualSpent = g.Sum(e => e.Amount)
                })
                .ToDictionaryAsync(x => x.CategoryId, x => x.ActualSpent);

            var totalBudget = limits.Sum(l => l.BudgetAmount);
            var totalSpent = expenditures.Values.Sum();

            var items = limits.Select(l =>
            {
                var spent = expenditures.GetValueOrDefault(l.CategoryId, 0m);
                var percentage = l.BudgetAmount == 0 ? 0m : Math.Round(spent / l.BudgetAmount * 100, 1);

                return new BudgetCategoryOverviewDto
                {
                    CategoryId = l.CategoryId,
                    CategoryName = l.Category?.Name ?? "未知類別",
                    Icon = l.Category?.Icon,
                    BudgetAmount = l.BudgetAmount,
                    ActualSpent = spent,
                    Percentage = percentage
                };
            }).ToList();

            var result = new MonthlyBudgetOverviewDto
            {
                YearMonth = targetMonth,
                TotalBudget = totalBudget,
                TotalSpent = totalSpent,
                TotalPercentage = totalBudget == 0 ? 0 : Math.Round(totalSpent / totalBudget * 100, 1),
                Categories = items
            };

            return Ok(result);
        }

        // 取得未讀的預算警示通知列表（不含 Normal 等級，只回傳有意義的警示）
        [HttpGet("getBudgetAlerts")]
        public async Task<ActionResult<List<BudgetAlertDto>>> GetBudgetAlerts([FromQuery] DateOnly? yearMonth = null)
        {
            var targetMonth = yearMonth ?? DateOnly.FromDateTime(DateTime.Today);

            var alerts = await _context.BudgetAlerts
                .Where(a => a.YearMonth == targetMonth 
                        && a.AlertLevel != BudgetAlertLevel.Normal
                        && !a.IsRead)
                .Include(a => a.Category)
                .OrderByDescending(a => a.CreatedAt)
                .Select(a => new BudgetAlertDto
                {
                    Id = a.Id,
                    CategoryId = a.CategoryId,
                    CategoryName = a.Category != null ? a.Category.Name : "總預算",
                    CategoryIcon = a.Category != null ? a.Category.Icon : null,
                    YearMonth = a.YearMonth,
                    AlertLevel = a.AlertLevel,
                    Percentage = a.Percentage,
                    IsRead = a.IsRead,
                    ReadAt = a.ReadAt,
                    LastNotifiedAt = a.LastNotifiedAt,
                    CreatedAt = a.CreatedAt
                })
                .ToListAsync();

            return Ok(alerts);
        }

        // 取得未讀預算警示數量（供通知小鈴鐺使用）
        [HttpGet("getUnreadAlertCount")]
        public async Task<ActionResult<int>> GetUnreadAlertCount()
        {
            var currentMonth = DateOnly.FromDateTime(DateTime.Today);

            var count = await _context.BudgetAlerts
                .Where(a => a.YearMonth == currentMonth
                        && a.AlertLevel != BudgetAlertLevel.Normal
                        && !a.IsRead)
                .CountAsync();

            return Ok(count);
        }

        // 標記單一預算警示為已讀
        [HttpPatch("markAlertAsRead/{id}")]
        public async Task<IActionResult> MarkAlertAsRead(Guid id)
        {
            var alert = await _context.BudgetAlerts.FindAsync(id);
            if (alert == null)
            {
                return NotFound();
            }

            alert.IsRead = true;
            alert.ReadAt = DateTimeOffset.UtcNow;
            alert.UpdatedAt = DateTimeOffset.UtcNow;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        //標記當月所有預算警示為已讀
        [HttpPatch("markAllAlertsAsRead")]
        public async Task<IActionResult> MarkAllAlertsAsRead([FromQuery] DateOnly? yearMonth = null)
        {
            var targetMonth = yearMonth ?? DateOnly.FromDateTime(DateTime.Today);

            var alerts = await _context.BudgetAlerts
                .Where(a => a.YearMonth == targetMonth && !a.IsRead)
                .ToListAsync();

            var now = DateTimeOffset.UtcNow;
            foreach (var alert in alerts)
            {
                alert.IsRead = true;
                alert.ReadAt = now;
                alert.UpdatedAt = now;
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
