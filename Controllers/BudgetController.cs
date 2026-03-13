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
    }
}
