using BackendApi.Constants;
using BackendApi.Entities;
using BackendApi.Models;
using BackendApi.Requests.Inventory;
using BackendApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BackendApi.Services;

public class InventoryService : IInventoryService
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<InventoryService> _logger;

    public InventoryService(ApplicationDbContext db, ILogger<InventoryService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<ServiceResult<List<Inventory>>> GetAllAsync(Guid? productId)
    {
        var query = _db.Inventories
            .Include(i => i.Product)
            .ThenInclude(p => p.Category)
            .AsQueryable();

        if (productId.HasValue)
            query = query.Where(i => i.ProductId == productId.Value);

        var inventories = await query.ToListAsync();
        return ServiceResult<List<Inventory>>.Success(inventories);
    }

    public async Task<ServiceResult<Inventory>> GetByIdAsync(Guid id)
    {
        var inventory = await _db.Inventories
            .Include(i => i.Product)
            .ThenInclude(p => p.Category)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (inventory is null)
            return ServiceResult<Inventory>.NotFound();
        return ServiceResult<Inventory>.Success(inventory);
    }

    public async Task<ServiceResult<Inventory>> CreateAsync(CreateInventoryRequest request)
    {
        var product = await _db.Products.FindAsync(request.ProductId);
        if (product is null)
            return ServiceResult<Inventory>.ValidationError("ProductId does not refer to an existing product.");

        var inventory = new Inventory
        {
            Id = Guid.NewGuid(),
            ProductId = request.ProductId,
            Location = request.Location,
            InitialQuantity = request.InitialQuantity,
            CurrentQuantity = 1.0m,
            NearestExpiryDate = request.NearestExpiryDate,
            Status = request.Status ?? InventoryStatus.Active,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        _db.Inventories.Add(inventory);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Created inventory {Id} for product {ProductId}", inventory.Id, inventory.ProductId);
        return ServiceResult<Inventory>.Success(inventory);
    }

    public async Task<ServiceResult<Inventory>> UpdateAsync(Guid id, UpdateInventoryRequest request)
    {
        var inventory = await _db.Inventories.FindAsync(id);
        if (inventory is null)
            return ServiceResult<Inventory>.NotFound();

        inventory.Location = request.Location;
        if (request.InitialQuantity.HasValue)
            inventory.InitialQuantity = request.InitialQuantity.Value;
        inventory.NearestExpiryDate = request.NearestExpiryDate;
        inventory.UpdatedAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync();

        _logger.LogInformation("Updated inventory {Id}", id);
        return ServiceResult<Inventory>.Success(inventory);
    }

    public async Task<ServiceResult<Inventory>> PatchNoteAsync(Guid id, PatchInventoryNoteRequest request)
    {
        var inventory = await _db.Inventories.FindAsync(id);
        if (inventory is null)
            return ServiceResult<Inventory>.NotFound();

        inventory.Note = request.Note;
        inventory.UpdatedAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync();

        _logger.LogInformation("Patched note on inventory {Id}", id);
        return ServiceResult<Inventory>.Success(inventory);
    }

    public async Task<ServiceResult<object?>> DeleteAsync(Guid id)
    {
        var inventory = await _db.Inventories
            .Include(i => i.Events)
            .Include(i => i.Reminders)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (inventory is null)
            return ServiceResult<object?>.NotFound();

        if (inventory.Events.Any())
            return ServiceResult<object?>.Conflict("Cannot delete an inventory that has events.");

        if (inventory.Reminders.Any())
            return ServiceResult<object?>.Conflict("Cannot delete an inventory that has reminders.");

        _db.Inventories.Remove(inventory);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Deleted inventory {Id}", id);
        return ServiceResult<object?>.Success(null);
    }
}
