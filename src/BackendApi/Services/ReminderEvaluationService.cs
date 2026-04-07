using BackendApi.Constants;
using BackendApi.Entities;
using BackendApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BackendApi.Services;

public class ReminderEvaluationService
{
    private const int BatchSize = 50;
    private const int ExpiryLookaheadDays = 7;
    private const int DepletionLookaheadDays = 7;

    private readonly ApplicationDbContext _db;
    private readonly ILogger<ReminderEvaluationService> _logger;

    public ReminderEvaluationService(
        ApplicationDbContext db,
        ILogger<ReminderEvaluationService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task EvaluateAllAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var today = DateOnly.FromDateTime(now.UtcDateTime);

        var inventoryIds = await _db.Inventories
            .Where(i => i.Status == InventoryStatus.Active || i.Status == InventoryStatus.Depleted)
            .Select(i => i.Id)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Evaluating reminders for {Count} inventories", inventoryIds.Count);

        var created = 0;
        var errors = 0;

        foreach (var batch in inventoryIds.Chunk(BatchSize))
        {
            var inventories = await _db.Inventories
                .Include(i => i.Product)
                .Include(i => i.Reminders)
                .Where(i => batch.Contains(i.Id))
                .ToListAsync(cancellationToken);

            foreach (var inventory in inventories)
            {
                try
                {
                    created += EvaluateSingle(inventory, now, today);
                }
                catch (Exception ex)
                {
                    errors++;
                    _logger.LogError(ex, "Failed to evaluate reminders for inventory {InventoryId}", inventory.Id);
                    _db.Entry(inventory).State = EntityState.Unchanged;
                }
            }

            await _db.SaveChangesAsync(cancellationToken);
        }

        _logger.LogInformation(
            "Reminder evaluation complete: {Created} created, {Errors} errors",
            created, errors);
    }

    private int EvaluateSingle(Inventory inventory, DateTimeOffset now, DateOnly today)
    {
        var created = 0;

        // LOW_STOCK: fires for both ACTIVE and DEPLETED
        if (inventory.CurrentQuantity <= inventory.Product.LowStockThreshold)
        {
            if (TryCreateReminder(inventory, ReminderType.LowStock, now))
                created++;
        }

        // EXPIRING: fires for both ACTIVE and DEPLETED
        if (inventory.NearestExpiryDate.HasValue
            && inventory.NearestExpiryDate.Value <= now.UtcDateTime.AddDays(ExpiryLookaheadDays))
        {
            if (TryCreateReminder(inventory, ReminderType.Expiring, now))
                created++;
        }

        // DEPLETION_ESTIMATED: only for ACTIVE (DEPLETED is already gone)
        if (inventory.Status == InventoryStatus.Active
            && inventory.EstimatedDepletionDate.HasValue
            && inventory.EstimatedDepletionDate.Value <= now.AddDays(DepletionLookaheadDays))
        {
            if (TryCreateReminder(inventory, ReminderType.DepletionEstimated, now))
                created++;
        }

        return created;
    }

    /// <summary>
    /// Creates a reminder if no active one of the same type already exists.
    /// Returns true if a reminder was created.
    /// </summary>
    private bool TryCreateReminder(Inventory inventory, string reminderType, DateTimeOffset now)
    {
        var hasActive = inventory.Reminders.Any(r =>
            r.ReminderType == reminderType &&
            (r.Status == ReminderStatus.Pending ||
             (r.Status == ReminderStatus.Snoozed && r.SnoozedUntil > now)));

        if (hasActive) return false;

        _db.Reminders.Add(new Reminder
        {
            Id = Guid.NewGuid(),
            InventoryId = inventory.Id,
            ReminderType = reminderType,
            Status = ReminderStatus.Pending,
            ScheduledAt = now,
            CreatedAt = now
        });

        return true;
    }
}
