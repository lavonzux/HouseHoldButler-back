using BackendApi.Constants;
using BackendApi.Entities;
using BackendApi.Requests.Inventory;
using BackendApi.Services.Interfaces;

namespace BackendApi.Services.Mock;

/// <summary>
/// In-memory mock implementation of IInventoryEventService.
/// RecalculateAsync is intentionally skipped — CurrentQuantity is set directly in mock data.
/// </summary>
public class MockInventoryEventService : IInventoryEventService
{
    internal static readonly List<InventoryEvent> Store =
    [
        new InventoryEvent
        {
            Id = new Guid("e0000000-0000-0000-0000-000000000001"),
            InventoryId = new Guid("d0000000-0000-0000-0000-000000000001"), // 牛奶
            EventType = InventoryEventType.Adjust,
            QuantityDelta = 1.0m,
            Note = "補貨",
            Source = EventSource.Manual,
            CreatedAt = new DateTimeOffset(2026, 3, 1, 0, 0, 0, TimeSpan.Zero)
        },
        new InventoryEvent
        {
            Id = new Guid("e0000000-0000-0000-0000-000000000002"),
            InventoryId = new Guid("d0000000-0000-0000-0000-000000000001"), // 牛奶（第二次 adjust）
            EventType = InventoryEventType.Adjust,
            QuantityDelta = 0.6m,
            Note = "手動校正",
            Source = EventSource.Manual,
            CreatedAt = new DateTimeOffset(2026, 3, 17, 0, 0, 0, TimeSpan.Zero)
        },
        new InventoryEvent
        {
            Id = new Guid("e0000000-0000-0000-0000-000000000003"),
            InventoryId = new Guid("d0000000-0000-0000-0000-000000000003"), // 雞蛋（已耗盡）
            EventType = InventoryEventType.Deplete,
            QuantityDelta = 0,
            Note = "Auto-depleted by system: estimated quantity reached zero",
            Source = EventSource.System,
            CreatedAt = new DateTimeOffset(2026, 3, 15, 0, 0, 0, TimeSpan.Zero)
        }
    ];

    public Task<ServiceResult<List<InventoryEvent>>> GetAllAsync(Guid? inventoryId, Guid? productId = null)
    {
        var query = Store.AsEnumerable();

        if (inventoryId.HasValue)
            query = query.Where(e => e.InventoryId == inventoryId.Value);

        if (productId.HasValue)
        {
            var inventoryIds = MockInventoryService.Store
                .Where(i => i.ProductId == productId.Value)
                .Select(i => i.Id)
                .ToHashSet();
            query = query.Where(e => inventoryIds.Contains(e.InventoryId));
        }

        var result = query.OrderByDescending(e => e.CreatedAt).ToList();
        return Task.FromResult(ServiceResult<List<InventoryEvent>>.Success(result));
    }

    public Task<ServiceResult<InventoryEvent>> GetByIdAsync(Guid id)
    {
        var ev = Store.FirstOrDefault(e => e.Id == id);
        return Task.FromResult(ev is null
            ? ServiceResult<InventoryEvent>.NotFound()
            : ServiceResult<InventoryEvent>.Success(ev));
    }

    public Task<ServiceResult<InventoryEvent>> CreateAsync(CreateInventoryEventRequest request)
    {
        if (MockInventoryService.Store.All(i => i.Id != request.InventoryId))
            return Task.FromResult(ServiceResult<InventoryEvent>.ValidationError("InventoryId does not refer to an existing inventory."));

        var ev = new InventoryEvent
        {
            Id = Guid.NewGuid(),
            InventoryId = request.InventoryId,
            EventType = request.EventType,
            QuantityDelta = request.QuantityDelta,
            Note = request.Note,
            Source = request.Source ?? EventSource.Manual,
            CreatedAt = DateTimeOffset.UtcNow
        };
        Store.Add(ev);

        // Recalculation is skipped in mock — update CurrentQuantity directly for ADJUST events
        if (request.EventType == InventoryEventType.Adjust)
        {
            var inventory = MockInventoryService.Store.FirstOrDefault(i => i.Id == request.InventoryId);
            if (inventory is not null)
            {
                inventory.CurrentQuantity = request.QuantityDelta;
                inventory.UpdatedAt = DateTimeOffset.UtcNow;
                inventory.Status = request.QuantityDelta == 0 ? InventoryStatus.Depleted : InventoryStatus.Active;
            }
        }
        else if (request.EventType is InventoryEventType.Deplete or InventoryEventType.Expire)
        {
            var inventory = MockInventoryService.Store.FirstOrDefault(i => i.Id == request.InventoryId);
            if (inventory is not null)
            {
                inventory.CurrentQuantity = 0;
                inventory.UpdatedAt = DateTimeOffset.UtcNow;
                inventory.Status = request.EventType == InventoryEventType.Expire
                    ? InventoryStatus.Expired
                    : InventoryStatus.Depleted;
            }
        }

        return Task.FromResult(ServiceResult<InventoryEvent>.Success(ev));
    }
}
