using HouseHoldButler.Constants;
using HouseHoldButler.Entities;
using HouseHoldButler.Services;
using HouseHoldButler.Tests.Helpers;
using Microsoft.Extensions.Logging.Abstractions;

namespace HouseHoldButler.Tests.Services;

public class InventoryRecalculationServiceTests
{
    // ── helpers ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Seeds Product + Inventory, then returns a FRESH service context sharing the same
    /// SQLite in-memory connection. Keeping contexts separate mirrors production behavior
    /// where each request gets a scoped DbContext.
    /// Java analogy: persist with one EntityManager, then test with a new one over the same H2 DB.
    /// </summary>
    private static async Task<(InventoryRecalculationService svc, Guid inventoryId, TestDatabase db)> SetupAsync(
        decimal avgConsumptionRate = 0m,
        decimal initialQuantity = 1m,
        DateTimeOffset? createdAt = null,
        DateTime? nearestExpiryDate = null,
        string status = InventoryStatus.Active,
        List<InventoryEvent>? events = null)
    {
        var testDb = new TestDatabase();
        var now = createdAt ?? DateTimeOffset.UtcNow;

        // ── Seed context ──
        using var seedCtx = testDb.CreateContext();

        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Test Product",
            AvgConsumptionRate = avgConsumptionRate,
            LowStockThreshold = 0.2m,
            CreatedAt = now,
            UpdatedAt = now
        };

        var inventory = new Inventory
        {
            Id = Guid.NewGuid(),
            ProductId = product.Id,
            InitialQuantity = initialQuantity,
            CurrentQuantity = initialQuantity,
            Status = status,
            NearestExpiryDate = nearestExpiryDate,
            CreatedAt = now,
            UpdatedAt = now
        };

        seedCtx.Products.Add(product);
        seedCtx.Inventories.Add(inventory);

        if (events != null)
        {
            foreach (var evt in events)
            {
                evt.InventoryId = inventory.Id;
                seedCtx.InventoryEvents.Add(evt);
            }
        }

        await seedCtx.SaveChangesAsync();

        // ── Service context (fresh change tracker) ──
        var svcCtx = testDb.CreateContext();
        var svc = new InventoryRecalculationService(svcCtx, NullLogger<InventoryRecalculationService>.Instance);

        return (svc, inventory.Id, testDb);
    }

    // ── tests ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// 情境：rate=0.1/day，庫存建立於 10 天前，沒有任何 event。
    /// 預期：CurrentQuantity 從 1.0 下降為 0，狀態變 DEPLETED，新增 DEPLETE system event。
    /// </summary>
    [Fact]
    public async Task Quantity_Decays_Over_Time_When_No_Events()
    {
        var (svc, inventoryId, testDb) = await SetupAsync(
            avgConsumptionRate: 0.1m,
            initialQuantity: 1m,
            createdAt: DateTimeOffset.UtcNow.AddDays(-10));
        await using (testDb)
        {
            await svc.RecalculateAsync(inventoryId);

            using var readCtx = testDb.CreateContext();
            var result = readCtx.Inventories.Find(inventoryId)!;
            Assert.Equal(0m, result.CurrentQuantity);
            Assert.Equal(InventoryStatus.Depleted, result.Status);
        }
    }

    /// <summary>
    /// 情境：有一個 ADJUST event 將 quantity 設為 0.5，rate=0（不消耗）。
    /// 預期：CurrentQuantity 以 ADJUST event 的值為準，狀態為 ACTIVE。
    /// </summary>
    [Fact]
    public async Task Adjust_Event_Resets_Quantity()
    {
        var createdAt = DateTimeOffset.UtcNow.AddDays(-5);
        var (svc, inventoryId, testDb) = await SetupAsync(
            avgConsumptionRate: 0m,
            initialQuantity: 1m,
            createdAt: createdAt,
            events:
            [
                new InventoryEvent
                {
                    Id = Guid.NewGuid(),
                    EventType = InventoryEventType.Adjust,
                    QuantityDelta = 0.5m,
                    Source = EventSource.Manual,
                    CreatedAt = createdAt.AddDays(1)
                }
            ]);
        await using (testDb)
        {
            await svc.RecalculateAsync(inventoryId);

            using var readCtx = testDb.CreateContext();
            var result = readCtx.Inventories.Find(inventoryId)!;
            Assert.Equal(0.5m, result.CurrentQuantity);
            Assert.Equal(InventoryStatus.Active, result.Status);
        }
    }

    /// <summary>
    /// 情境：NearestExpiryDate 已過期，無既有 EXPIRE event。
    /// 預期：Status=EXPIRED、CurrentQuantity=0、新增一個 EXPIRE system event。
    /// </summary>
    [Fact]
    public async Task Past_Expiry_Date_Sets_Expired_Status_And_Adds_Event()
    {
        var (svc, inventoryId, testDb) = await SetupAsync(
            avgConsumptionRate: 0m,
            initialQuantity: 1m,
            nearestExpiryDate: DateTime.UtcNow.AddDays(-1));
        await using (testDb)
        {
            await svc.RecalculateAsync(inventoryId);

            using var readCtx = testDb.CreateContext();
            var result = readCtx.Inventories.Find(inventoryId)!;
            Assert.Equal(InventoryStatus.Expired, result.Status);
            Assert.Equal(0m, result.CurrentQuantity);

            var expireEvent = readCtx.InventoryEvents
                .FirstOrDefault(e => e.InventoryId == inventoryId && e.EventType == InventoryEventType.Expire);
            Assert.NotNull(expireEvent);
            Assert.Equal(EventSource.System, expireEvent.Source);
        }
    }

    /// <summary>
    /// 情境：庫存已過期，且已有 system EXPIRE event（幂等測試）。
    /// 預期：不重複新增 EXPIRE event。
    /// </summary>
    [Fact]
    public async Task Expired_Inventory_Does_Not_Add_Duplicate_Expire_Event()
    {
        var (svc, inventoryId, testDb) = await SetupAsync(
            avgConsumptionRate: 0m,
            initialQuantity: 1m,
            nearestExpiryDate: DateTime.UtcNow.AddDays(-2),
            status: InventoryStatus.Expired,
            events:
            [
                new InventoryEvent
                {
                    Id = Guid.NewGuid(),
                    EventType = InventoryEventType.Expire,
                    QuantityDelta = 0,
                    Source = EventSource.System,
                    CreatedAt = DateTimeOffset.UtcNow.AddDays(-1)
                }
            ]);
        await using (testDb)
        {
            await svc.RecalculateAsync(inventoryId);

            using var readCtx = testDb.CreateContext();
            var count = readCtx.InventoryEvents
                .Count(e => e.InventoryId == inventoryId && e.EventType == InventoryEventType.Expire);
            Assert.Equal(1, count);
        }
    }

    /// <summary>
    /// 情境：rate=0.2/day，10 天前建立，quantity 自然歸零。
    /// 預期：狀態變 DEPLETED，新增 DEPLETE system event，EstimatedDepletionDate 為 null。
    /// </summary>
    [Fact]
    public async Task Quantity_Reaching_Zero_Sets_Depleted_And_Adds_Event()
    {
        var (svc, inventoryId, testDb) = await SetupAsync(
            avgConsumptionRate: 0.2m,
            initialQuantity: 1m,
            createdAt: DateTimeOffset.UtcNow.AddDays(-10));
        await using (testDb)
        {
            await svc.RecalculateAsync(inventoryId);

            using var readCtx = testDb.CreateContext();
            var result = readCtx.Inventories.Find(inventoryId)!;
            Assert.Equal(InventoryStatus.Depleted, result.Status);
            Assert.Equal(0m, result.CurrentQuantity);
            Assert.Null(result.EstimatedDepletionDate);

            var depleteEvent = readCtx.InventoryEvents
                .FirstOrDefault(e => e.InventoryId == inventoryId && e.EventType == InventoryEventType.Deplete);
            Assert.NotNull(depleteEvent);
            Assert.Equal(EventSource.System, depleteEvent.Source);
        }
    }

    /// <summary>
    /// 情境：DEPLETED 庫存被 ADJUST event 補貨（QuantityDelta=0.8），rate=0。
    /// 預期：Status 重新變回 ACTIVE，CurrentQuantity=0.8。
    /// </summary>
    [Fact]
    public async Task Restocked_Depleted_Inventory_Becomes_Active()
    {
        var createdAt = DateTimeOffset.UtcNow.AddDays(-5);
        var (svc, inventoryId, testDb) = await SetupAsync(
            avgConsumptionRate: 0m,
            initialQuantity: 0m,
            createdAt: createdAt,
            status: InventoryStatus.Depleted,
            events:
            [
                new InventoryEvent
                {
                    Id = Guid.NewGuid(),
                    EventType = InventoryEventType.Adjust,
                    QuantityDelta = 0.8m,
                    Source = EventSource.Manual,
                    CreatedAt = DateTimeOffset.UtcNow.AddDays(-1)
                }
            ]);
        await using (testDb)
        {
            await svc.RecalculateAsync(inventoryId);

            using var readCtx = testDb.CreateContext();
            var result = readCtx.Inventories.Find(inventoryId)!;
            Assert.Equal(InventoryStatus.Active, result.Status);
            Assert.Equal(0.8m, result.CurrentQuantity);
        }
    }
}
