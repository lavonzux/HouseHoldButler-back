using BackendApi.Constants;
using BackendApi.Dtos;
using BackendApi.Entities;
using BackendApi.Models;
using BackendApi.Requests.Inventory;
using BackendApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BackendApi.Services;

public class ProductService : IProductService
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<ProductService> _logger;

    public ProductService(ApplicationDbContext db, ILogger<ProductService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<ServiceResult<List<Product>>> GetAllAsync()
    {
        var products = await _db.Products.OrderBy(p => p.Name).ToListAsync();
        return ServiceResult<List<Product>>.Success(products);
    }

    public async Task<ServiceResult<Product>> GetByIdAsync(Guid id)
    {
        var product = await _db.Products.FindAsync(id);
        if (product is null)
            return ServiceResult<Product>.NotFound();
        return ServiceResult<Product>.Success(product);
    }

    public async Task<ServiceResult<List<ProductHistoryEntryDto>>> GetHistoryAsync(Guid id)
    {
        var exists = await _db.Products.AnyAsync(p => p.Id == id);
        if (!exists)
            return ServiceResult<List<ProductHistoryEntryDto>>.NotFound();

        var inventories = await _db.Inventories
            .Include(i => i.Events)
            .Where(i => i.ProductId == id)
            .ToListAsync();

        var entries = new List<ProductHistoryEntryDto>();

        foreach (var inv in inventories)
        {
            entries.Add(new ProductHistoryEntryDto(
                EntryType: "PURCHASE",
                OccurredAt: inv.CreatedAt,
                InventoryId: inv.Id,
                InventoryStatus: inv.Status,
                InitialQuantity: inv.InitialQuantity,
                Location: inv.Location,
                QuantityDelta: null,
                Source: null,
                Note: inv.Note
            ));

            entries.AddRange(inv.Events.Select(e => new ProductHistoryEntryDto(
                EntryType: e.EventType,
                OccurredAt: e.CreatedAt,
                InventoryId: inv.Id,
                InventoryStatus: inv.Status,
                InitialQuantity: null,
                Location: null,
                QuantityDelta: e.EventType == InventoryEventType.Adjust ? e.QuantityDelta : null,
                Source: e.Source,
                Note: e.Note
            )));
        }

        var sorted = entries.OrderByDescending(e => e.OccurredAt).ToList();
        return ServiceResult<List<ProductHistoryEntryDto>>.Success(sorted);
    }

    public async Task<ServiceResult<Product>> CreateAsync(CreateProductRequest request)
    {
        if (request.CategoryId.HasValue)
        {
            var category = await _db.Categories.FindAsync(request.CategoryId.Value);
            if (category is null)
                return ServiceResult<Product>.ValidationError("CategoryId does not refer to an existing category.");
        }

        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            CategoryId = request.CategoryId,
            Barcode = request.Barcode,
            Unit = request.Unit,
            AvgConsumptionRate = request.AvgConsumptionRate,
            LowStockThreshold = request.LowStockThreshold,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        _db.Products.Add(product);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Created product {Id} ({Name})", product.Id, product.Name);
        return ServiceResult<Product>.Success(product);
    }

    public async Task<ServiceResult<Product>> UpdateAsync(Guid id, UpdateProductRequest request)
    {
        var product = await _db.Products.FindAsync(id);
        if (product is null)
            return ServiceResult<Product>.NotFound();

        if (request.CategoryId.HasValue)
        {
            var category = await _db.Categories.FindAsync(request.CategoryId.Value);
            if (category is null)
                return ServiceResult<Product>.ValidationError("CategoryId does not refer to an existing category.");
        }

        product.Name = request.Name;
        product.CategoryId = request.CategoryId;
        product.Barcode = request.Barcode;
        product.Unit = request.Unit;
        product.AvgConsumptionRate = request.AvgConsumptionRate;
        product.LowStockThreshold = request.LowStockThreshold;
        product.UpdatedAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync();

        _logger.LogInformation("Updated product {Id}", id);
        return ServiceResult<Product>.Success(product);
    }

    public async Task<ServiceResult<object?>> DeleteAsync(Guid id)
    {
        var product = await _db.Products
            .Include(p => p.Inventories)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product is null)
            return ServiceResult<object?>.NotFound();

        if (product.Inventories.Any())
            return ServiceResult<object?>.Conflict("Cannot delete a product that has inventory records.");

        _db.Products.Remove(product);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Deleted product {Id}", id);
        return ServiceResult<object?>.Success(null);
    }

    public async Task<ServiceResult<object?>> ForceDeleteAsync(Guid id)
    {
        var product = await _db.Products
            .Include(p => p.Inventories)
                .ThenInclude(i => i.Events)
            .Include(p => p.Inventories)
                .ThenInclude(i => i.Reminders)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product is null)
            return ServiceResult<object?>.NotFound();

        var inventoryCount = product.Inventories.Count;

        foreach (var inventory in product.Inventories)
        {
            _db.InventoryEvents.RemoveRange(inventory.Events);
            _db.Reminders.RemoveRange(inventory.Reminders);
        }
        _db.Inventories.RemoveRange(product.Inventories);
        _db.Products.Remove(product);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Force deleted product {Id} along with {Count} inventory record(s)", id, inventoryCount);
        return ServiceResult<object?>.Success(null);
    }
}
