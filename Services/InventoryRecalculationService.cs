using BackendApi.Constants;
using BackendApi.Entities;
using BackendApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BackendApi.Services;

public class InventoryRecalculationService
{
    private const int BatchSize = 50;

    private readonly ApplicationDbContext _db;
    private readonly ILogger<InventoryRecalculationService> _logger;

    public InventoryRecalculationService(
        ApplicationDbContext db,
        ILogger<InventoryRecalculationService> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <summary>
    /// Recalculates a single inventory by ID. Called immediately when an event is created.
    /// </summary>
    public async Task RecalculateAsync(Guid inventoryId, CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var today = DateOnly.FromDateTime(now.UtcDateTime);

        var inventory = await _db.Inventories
            .Include(i => i.Product)
            .Include(i => i.Events)
            .FirstOrDefaultAsync(i => i.Id == inventoryId, cancellationToken);

        if (inventory is null)
        {
            _logger.LogWarning("Inventory {InventoryId} not found for recalculation", inventoryId);
            return;
        }

        RecalculateSingle(inventory, now, today);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Recalculated inventory {InventoryId}, quantity={Quantity}, status={Status}",
            inventoryId, inventory.CurrentQuantity, inventory.Status);
    }

    /// <summary>
    /// Batch recalculates all active/depleted inventories. Called by nightly background job.
    /// </summary>
    public async Task RecalculateAllAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var today = DateOnly.FromDateTime(now.UtcDateTime);

        var inventoryIds = await _db.Inventories
            .Where(i => i.Status == InventoryStatus.Active || i.Status == InventoryStatus.Depleted)
            .Select(i => i.Id)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Recalculating {Count} inventories", inventoryIds.Count);

        var processed = 0;
        var errors = 0;

        foreach (var batch in inventoryIds.Chunk(BatchSize))
        {
            var inventories = await _db.Inventories
                .Include(i => i.Product)
                .Include(i => i.Events)
                .Where(i => batch.Contains(i.Id))
                .ToListAsync(cancellationToken);

            foreach (var inventory in inventories)
            {
                try
                {
                    RecalculateSingle(inventory, now, today);
                    processed++;
                }
                catch (Exception ex)
                {
                    errors++;
                    _logger.LogError(ex, "Failed to recalculate inventory {InventoryId}", inventory.Id);
                    _db.Entry(inventory).State = EntityState.Unchanged;
                }
            }

            await _db.SaveChangesAsync(cancellationToken);
        }

        _logger.LogInformation(
            "Recalculation complete: {Processed} processed, {Errors} errors",
            processed, errors);
    }

    private void RecalculateSingle(Inventory inventory, DateTimeOffset now, DateOnly today)
    {
        var rate = inventory.Product.AvgConsumptionRate;
        var events = inventory.Events
            .OrderBy(e => e.CreatedAt)
            .ThenBy(e => e.Id)
            .ToList();

        // Step 1: Check expiry
        if (inventory.NearestExpiryDate.HasValue && inventory.NearestExpiryDate.Value <= now.UtcDateTime)
        {
            inventory.Status = InventoryStatus.Expired;
            inventory.CurrentQuantity = 0;
            inventory.EstimatedDepletionDate = null;
            inventory.UpdatedAt = now;

            // Append EXPIRE event if not already the last system event
            if (!IsLastSystemEvent(events, InventoryEventType.Expire))
            {
                _db.InventoryEvents.Add(new InventoryEvent
                {
                    Id = Guid.NewGuid(),
                    InventoryId = inventory.Id,
                    EventType = InventoryEventType.Expire,
                    QuantityDelta = 0,
                    Note = "Auto-expired by system: nearest expiry date reached",
                    Source = EventSource.System,
                    CreatedAt = now
                });
            }

            return;
        }

        // Step 2: Find last anchor event (ADJUST, DEPLETE, EXPIRE)
        decimal anchorQuantity = 0;
        DateTimeOffset anchorTime = inventory.CreatedAt;
        int anchorIndex = -1;

        for (var i = events.Count - 1; i >= 0; i--)
        {
            var evt = events[i];
            if (evt.EventType is InventoryEventType.Adjust
                or InventoryEventType.Deplete
                or InventoryEventType.Expire)
            {
                anchorIndex = i;
                anchorTime = evt.CreatedAt;
                anchorQuantity = evt.EventType == InventoryEventType.Adjust
                    ? evt.QuantityDelta
                    : 0;
                break;
            }
        }

        // Step 3: Replay events after anchor
        var quantity = anchorQuantity;
        var lastEventTime = anchorTime;

        var replayEvents = anchorIndex >= 0
            ? events.Skip(anchorIndex + 1)
            : events;

        foreach (var evt in replayEvents)
        {
            // Subtract consumption since last event
            if (rate > 0)
            {
                var elapsedDays = (decimal)(evt.CreatedAt - lastEventTime).TotalDays;
                quantity -= rate * elapsedDays;
                quantity = Math.Max(0, quantity);
            }

            // Apply event
            switch (evt.EventType)
            {
                case InventoryEventType.Adjust:
                    quantity = evt.QuantityDelta;
                    break;
                case InventoryEventType.Deplete:
                case InventoryEventType.Expire:
                    quantity = 0;
                    break;
            }

            lastEventTime = evt.CreatedAt;
        }

        // Step 4: Apply consumption to now
        if (rate > 0)
        {
            var daysSinceLastEvent = (decimal)(now - lastEventTime).TotalDays;
            quantity -= rate * daysSinceLastEvent;
            quantity = Math.Max(0, quantity);
        }

        // Step 5: Set derived fields
        inventory.CurrentQuantity = quantity;
        inventory.UpdatedAt = now;

        if (rate > 0 && quantity > 0)
        {
            var daysUntilDepletion = (double)(quantity / rate);
            inventory.EstimatedDepletionDate = now.AddDays(daysUntilDepletion);
        }
        else
        {
            inventory.EstimatedDepletionDate = null;
        }

        if (quantity == 0)
        {
            inventory.Status = InventoryStatus.Depleted;

            if (!IsLastSystemEvent(events, InventoryEventType.Deplete))
            {
                _db.InventoryEvents.Add(new InventoryEvent
                {
                    Id = Guid.NewGuid(),
                    InventoryId = inventory.Id,
                    EventType = InventoryEventType.Deplete,
                    QuantityDelta = 0,
                    Note = "Auto-depleted by system: estimated quantity reached zero",
                    Source = EventSource.System,
                    CreatedAt = now
                });
            }
        }
        else
        {
            // Handles restocked DEPLETED items becoming ACTIVE again
            inventory.Status = InventoryStatus.Active;
        }
    }

    private static bool IsLastSystemEvent(List<InventoryEvent> events, string eventType)
    {
        if (events.Count == 0) return false;
        var last = events[^1];
        return last.EventType == eventType && last.Source == EventSource.System;
    }
}
