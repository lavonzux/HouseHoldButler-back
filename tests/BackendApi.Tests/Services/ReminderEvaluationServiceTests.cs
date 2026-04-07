using BackendApi.Constants;
using BackendApi.Entities;
using BackendApi.Services;
using BackendApi.Tests.Helpers;
using Microsoft.Extensions.Logging.Abstractions;

namespace BackendApi.Tests.Services;

public class ReminderEvaluationServiceTests
{
    // ── helpers ──────────────────────────────────────────────────────────────

    private static async Task<(ReminderEvaluationService svc, Guid inventoryId, TestDatabase db)> SetupAsync(
        decimal currentQuantity = 0.5m,
        decimal lowStockThreshold = 0.2m,
        DateTimeOffset? estimatedDepletionDate = null,
        DateTime? nearestExpiryDate = null,
        string status = InventoryStatus.Active,
        List<Reminder>? reminders = null)
    {
        var testDb = new TestDatabase();
        var now = DateTimeOffset.UtcNow;

        using var seedCtx = testDb.CreateContext();

        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Test Product",
            AvgConsumptionRate = 0.1m,
            LowStockThreshold = lowStockThreshold,
            CreatedAt = now,
            UpdatedAt = now
        };

        var inventory = new Inventory
        {
            Id = Guid.NewGuid(),
            ProductId = product.Id,
            InitialQuantity = 1m,
            CurrentQuantity = currentQuantity,
            EstimatedDepletionDate = estimatedDepletionDate,
            NearestExpiryDate = nearestExpiryDate,
            Status = status,
            CreatedAt = now,
            UpdatedAt = now
        };

        seedCtx.Products.Add(product);
        seedCtx.Inventories.Add(inventory);

        if (reminders != null)
        {
            foreach (var r in reminders)
            {
                r.InventoryId = inventory.Id;
                seedCtx.Reminders.Add(r);
            }
        }

        await seedCtx.SaveChangesAsync();

        var svcCtx = testDb.CreateContext();
        var svc = new ReminderEvaluationService(svcCtx, NullLogger<ReminderEvaluationService>.Instance);

        return (svc, inventory.Id, testDb);
    }

    // ── tests ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// 情境：CurrentQuantity 低於 LowStockThreshold。
    /// 預期：建立一個 LOW_STOCK PENDING reminder。
    /// </summary>
    [Fact]
    public async Task Low_Stock_Creates_LowStock_Reminder()
    {
        var (svc, inventoryId, testDb) = await SetupAsync(
            currentQuantity: 0.1m,
            lowStockThreshold: 0.2m);
        await using (testDb)
        {
            await svc.EvaluateAllAsync();

            using var readCtx = testDb.CreateContext();
            var reminder = readCtx.Reminders.FirstOrDefault(r =>
                r.InventoryId == inventoryId && r.ReminderType == ReminderType.LowStock);
            Assert.NotNull(reminder);
            Assert.Equal(ReminderStatus.Pending, reminder.Status);
        }
    }

    /// <summary>
    /// 情境：NearestExpiryDate 在 7 天內。
    /// 預期：建立一個 EXPIRING PENDING reminder。
    /// </summary>
    [Fact]
    public async Task Expiring_Within_Week_Creates_Expiring_Reminder()
    {
        var (svc, inventoryId, testDb) = await SetupAsync(
            nearestExpiryDate: DateTime.UtcNow.AddDays(3));
        await using (testDb)
        {
            await svc.EvaluateAllAsync();

            using var readCtx = testDb.CreateContext();
            var reminder = readCtx.Reminders.FirstOrDefault(r =>
                r.InventoryId == inventoryId && r.ReminderType == ReminderType.Expiring);
            Assert.NotNull(reminder);
            Assert.Equal(ReminderStatus.Pending, reminder.Status);
        }
    }

    /// <summary>
    /// 情境：EstimatedDepletionDate 在 7 天內，狀態為 ACTIVE。
    /// 預期：建立一個 DEPLETION_ESTIMATED PENDING reminder。
    /// </summary>
    [Fact]
    public async Task Depletion_Within_Week_Creates_DepletionEstimated_Reminder()
    {
        var (svc, inventoryId, testDb) = await SetupAsync(
            estimatedDepletionDate: DateTimeOffset.UtcNow.AddDays(5),
            status: InventoryStatus.Active);
        await using (testDb)
        {
            await svc.EvaluateAllAsync();

            using var readCtx = testDb.CreateContext();
            var reminder = readCtx.Reminders.FirstOrDefault(r =>
                r.InventoryId == inventoryId && r.ReminderType == ReminderType.DepletionEstimated);
            Assert.NotNull(reminder);
            Assert.Equal(ReminderStatus.Pending, reminder.Status);
        }
    }

    /// <summary>
    /// 情境：LOW_STOCK reminder 已存在且狀態為 PENDING（幂等測試）。
    /// 預期：不重複建立。
    /// </summary>
    [Fact]
    public async Task Active_Pending_Reminder_Prevents_Duplicate()
    {
        var (svc, inventoryId, testDb) = await SetupAsync(
            currentQuantity: 0.1m,
            lowStockThreshold: 0.2m,
            reminders:
            [
                new Reminder
                {
                    Id = Guid.NewGuid(),
                    ReminderType = ReminderType.LowStock,
                    Status = ReminderStatus.Pending,
                    ScheduledAt = DateTimeOffset.UtcNow,
                    CreatedAt = DateTimeOffset.UtcNow
                }
            ]);
        await using (testDb)
        {
            await svc.EvaluateAllAsync();

            using var readCtx = testDb.CreateContext();
            var count = readCtx.Reminders.Count(r =>
                r.InventoryId == inventoryId && r.ReminderType == ReminderType.LowStock);
            Assert.Equal(1, count);
        }
    }

    /// <summary>
    /// 情境：LOW_STOCK reminder 存在且狀態為 SNOOZED，SnoozedUntil 在未來。
    /// 預期：不重複建立（視為仍有效的 reminder）。
    /// </summary>
    [Fact]
    public async Task Snoozed_Not_Yet_Expired_Prevents_Duplicate()
    {
        var (svc, inventoryId, testDb) = await SetupAsync(
            currentQuantity: 0.1m,
            lowStockThreshold: 0.2m,
            reminders:
            [
                new Reminder
                {
                    Id = Guid.NewGuid(),
                    ReminderType = ReminderType.LowStock,
                    Status = ReminderStatus.Snoozed,
                    SnoozedUntil = DateTimeOffset.UtcNow.AddDays(2),
                    ScheduledAt = DateTimeOffset.UtcNow,
                    CreatedAt = DateTimeOffset.UtcNow
                }
            ]);
        await using (testDb)
        {
            await svc.EvaluateAllAsync();

            using var readCtx = testDb.CreateContext();
            var count = readCtx.Reminders.Count(r =>
                r.InventoryId == inventoryId && r.ReminderType == ReminderType.LowStock);
            Assert.Equal(1, count);
        }
    }

    /// <summary>
    /// 情境：LOW_STOCK reminder 存在且狀態為 SNOOZED，但 SnoozedUntil 已過期。
    /// 預期：建立新的 PENDING reminder（snooze 到期後應重新提醒）。
    /// </summary>
    [Fact]
    public async Task Snoozed_Expired_Allows_New_Reminder()
    {
        var (svc, inventoryId, testDb) = await SetupAsync(
            currentQuantity: 0.1m,
            lowStockThreshold: 0.2m,
            reminders:
            [
                new Reminder
                {
                    Id = Guid.NewGuid(),
                    ReminderType = ReminderType.LowStock,
                    Status = ReminderStatus.Snoozed,
                    SnoozedUntil = DateTimeOffset.UtcNow.AddDays(-1), // 已過期
                    ScheduledAt = DateTimeOffset.UtcNow.AddDays(-3),
                    CreatedAt = DateTimeOffset.UtcNow.AddDays(-3)
                }
            ]);
        await using (testDb)
        {
            await svc.EvaluateAllAsync();

            using var readCtx = testDb.CreateContext();
            var count = readCtx.Reminders.Count(r =>
                r.InventoryId == inventoryId && r.ReminderType == ReminderType.LowStock);
            Assert.Equal(2, count); // 舊的 snoozed + 新的 pending
        }
    }
}
