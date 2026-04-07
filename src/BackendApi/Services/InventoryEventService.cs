using BackendApi.Constants;
using BackendApi.Entities;
using BackendApi.Models;
using BackendApi.Requests.Inventory;
using BackendApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BackendApi.Services;

public class InventoryEventService : IInventoryEventService
{
    private readonly ApplicationDbContext _db;
    private readonly InventoryRecalculationService _recalculationService;
    private readonly ILogger<InventoryEventService> _logger;

    public InventoryEventService(
        ApplicationDbContext db,
        InventoryRecalculationService recalculationService,
        ILogger<InventoryEventService> logger)
    {
        _db = db;
        _recalculationService = recalculationService;
        _logger = logger;
    }

    public async Task<ServiceResult<List<InventoryEvent>>> GetAllAsync(Guid? inventoryId, Guid? productId = null)
    {
        var query = _db.InventoryEvents
            .Include(e => e.Inventory)
            .AsQueryable();

        if (inventoryId.HasValue)
            query = query.Where(e => e.InventoryId == inventoryId.Value);

        if (productId.HasValue)
            query = query.Where(e => e.Inventory.ProductId == productId.Value);

        var events = await query.OrderByDescending(e => e.CreatedAt).ToListAsync();
        return ServiceResult<List<InventoryEvent>>.Success(events);
    }

    public async Task<ServiceResult<InventoryEvent>> GetByIdAsync(Guid id)
    {
        var ev = await _db.InventoryEvents.FindAsync(id);
        if (ev is null)
            return ServiceResult<InventoryEvent>.NotFound();
        return ServiceResult<InventoryEvent>.Success(ev);
    }

    public async Task<ServiceResult<InventoryEvent>> CreateAsync(CreateInventoryEventRequest request)
    {
        var inventory = await _db.Inventories.FindAsync(request.InventoryId);
        if (inventory is null)
            return ServiceResult<InventoryEvent>.ValidationError("InventoryId does not refer to an existing inventory.");

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

        _db.InventoryEvents.Add(ev);
        await _db.SaveChangesAsync();

        await _recalculationService.RecalculateAsync(request.InventoryId);

        _logger.LogInformation("Created inventory event {Id} ({EventType}) for inventory {InventoryId}",
            ev.Id, ev.EventType, ev.InventoryId);
        return ServiceResult<InventoryEvent>.Success(ev);
    }
}
