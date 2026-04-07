using HouseHoldButler.Constants;
using HouseHoldButler.Dtos;
using HouseHoldButler.Entities;
using HouseHoldButler.Requests.Inventory;
using HouseHoldButler.Services.Interfaces;

namespace HouseHoldButler.Services.Mock;

/// <summary>
/// In-memory mock implementation of IProductService.
/// </summary>
public class MockProductService : IProductService
{
    internal static readonly List<Product> Store =
    [
        new Product
        {
            Id = new Guid("c0000000-0000-0000-0000-000000000001"),
            Name = "牛奶",
            CategoryId = new Guid("b0000000-0000-0000-0000-000000000002"), // 乳製品
            Unit = "瓶",
            AvgConsumptionRate = 0.1m,  // ~10天喝完一瓶
            LowStockThreshold = 0.2m,
            CreatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
            UpdatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero)
        },
        new Product
        {
            Id = new Guid("c0000000-0000-0000-0000-000000000002"),
            Name = "洗衣精",
            CategoryId = new Guid("b0000000-0000-0000-0000-000000000021"), // 洗衣用品
            Unit = "瓶",
            AvgConsumptionRate = 0.02m, // ~50天用完一瓶
            LowStockThreshold = 0.15m,
            CreatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
            UpdatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero)
        },
        new Product
        {
            Id = new Guid("c0000000-0000-0000-0000-000000000003"),
            Name = "雞蛋",
            CategoryId = new Guid("b0000000-0000-0000-0000-000000000001"), // 生鮮蔬果
            Unit = "顆",
            AvgConsumptionRate = 0.071m, // ~14天吃完一盒
            LowStockThreshold = 0.3m,
            CreatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
            UpdatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero)
        }
    ];

    public Task<ServiceResult<List<Product>>> GetAllAsync()
    {
        var result = Store.OrderBy(p => p.Name).ToList();
        return Task.FromResult(ServiceResult<List<Product>>.Success(result));
    }

    public Task<ServiceResult<Product>> GetByIdAsync(Guid id)
    {
        var product = Store.FirstOrDefault(p => p.Id == id);
        return Task.FromResult(product is null
            ? ServiceResult<Product>.NotFound()
            : ServiceResult<Product>.Success(product));
    }

    public Task<ServiceResult<List<ProductHistoryEntryDto>>> GetHistoryAsync(Guid id)
    {
        if (Store.All(p => p.Id != id))
            return Task.FromResult(ServiceResult<List<ProductHistoryEntryDto>>.NotFound());

        var inventories = MockInventoryService.Store.Where(i => i.ProductId == id).ToList();

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

            var events = MockInventoryEventService.Store.Where(e => e.InventoryId == inv.Id);
            entries.AddRange(events.Select(e => new ProductHistoryEntryDto(
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
        return Task.FromResult(ServiceResult<List<ProductHistoryEntryDto>>.Success(sorted));
    }

    public Task<ServiceResult<Product>> CreateAsync(CreateProductRequest request)
    {
        if (request.CategoryId.HasValue && MockCategoryService.Store.All(c => c.Id != request.CategoryId.Value))
            return Task.FromResult(ServiceResult<Product>.ValidationError("CategoryId does not refer to an existing category."));

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
        Store.Add(product);
        return Task.FromResult(ServiceResult<Product>.Success(product));
    }

    public Task<ServiceResult<Product>> UpdateAsync(Guid id, UpdateProductRequest request)
    {
        var product = Store.FirstOrDefault(p => p.Id == id);
        if (product is null)
            return Task.FromResult(ServiceResult<Product>.NotFound());

        if (request.CategoryId.HasValue && MockCategoryService.Store.All(c => c.Id != request.CategoryId.Value))
            return Task.FromResult(ServiceResult<Product>.ValidationError("CategoryId does not refer to an existing category."));

        product.Name = request.Name;
        product.CategoryId = request.CategoryId;
        product.Barcode = request.Barcode;
        product.Unit = request.Unit;
        product.AvgConsumptionRate = request.AvgConsumptionRate;
        product.LowStockThreshold = request.LowStockThreshold;
        product.UpdatedAt = DateTimeOffset.UtcNow;
        return Task.FromResult(ServiceResult<Product>.Success(product));
    }

    public Task<ServiceResult<object?>> DeleteAsync(Guid id)
    {
        var product = Store.FirstOrDefault(p => p.Id == id);
        if (product is null)
            return Task.FromResult(ServiceResult<object?>.NotFound());

        if (MockInventoryService.Store.Any(i => i.ProductId == id))
            return Task.FromResult(ServiceResult<object?>.Conflict("Cannot delete a product that has inventory records."));

        Store.Remove(product);
        return Task.FromResult(ServiceResult<object?>.Success(null));
    }

    public Task<ServiceResult<object?>> ForceDeleteAsync(Guid id)
    {
        var product = Store.FirstOrDefault(p => p.Id == id);
        if (product is null)
            return Task.FromResult(ServiceResult<object?>.NotFound());

        var inventories = MockInventoryService.Store.Where(i => i.ProductId == id).ToList();
        foreach (var inv in inventories)
            MockInventoryService.Store.Remove(inv);

        Store.Remove(product);
        return Task.FromResult(ServiceResult<object?>.Success(null));
    }
}
