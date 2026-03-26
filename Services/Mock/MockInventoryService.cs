using BackendApi.Constants;
using BackendApi.Entities;
using BackendApi.Requests.Inventory;
using BackendApi.Services.Interfaces;

namespace BackendApi.Services.Mock;

/// <summary>
/// In-memory mock implementation of IInventoryService.
/// </summary>
public class MockInventoryService : IInventoryService
{
    internal static readonly List<Inventory> Store =
    [
        new Inventory
        {
            Id = new Guid("d0000000-0000-0000-0000-000000000001"),
            ProductId = new Guid("c0000000-0000-0000-0000-000000000001"), // 牛奶
            Location = "冰箱",
            InitialQuantity = 2,
            CurrentQuantity = 0.6m,  // 剩60%
            Status = InventoryStatus.Active,
            NearestExpiryDate = new DateTime(2026, 3, 25),
            EstimatedDepletionDate = new DateTimeOffset(2026, 3, 21, 0, 0, 0, TimeSpan.Zero),
            CreatedAt = new DateTimeOffset(2026, 3, 1, 0, 0, 0, TimeSpan.Zero),
            UpdatedAt = new DateTimeOffset(2026, 3, 17, 0, 0, 0, TimeSpan.Zero)
        },
        new Inventory
        {
            Id = new Guid("d0000000-0000-0000-0000-000000000002"),
            ProductId = new Guid("c0000000-0000-0000-0000-000000000002"), // 洗衣精
            Location = "洗衣間",
            InitialQuantity = 1,
            CurrentQuantity = 0.15m, // 剩15%，剛好在警戒線
            Status = InventoryStatus.Active,
            NearestExpiryDate = null,
            EstimatedDepletionDate = new DateTimeOffset(2026, 3, 25, 0, 0, 0, TimeSpan.Zero),
            CreatedAt = new DateTimeOffset(2026, 1, 15, 0, 0, 0, TimeSpan.Zero),
            UpdatedAt = new DateTimeOffset(2026, 3, 17, 0, 0, 0, TimeSpan.Zero)
        },
        new Inventory
        {
            Id = new Guid("d0000000-0000-0000-0000-000000000003"),
            ProductId = new Guid("c0000000-0000-0000-0000-000000000003"), // 雞蛋
            Location = "冰箱",
            InitialQuantity = 10,
            CurrentQuantity = 0.0m, // 已耗盡
            Status = InventoryStatus.Depleted,
            NearestExpiryDate = null,
            EstimatedDepletionDate = null,
            CreatedAt = new DateTimeOffset(2026, 3, 1, 0, 0, 0, TimeSpan.Zero),
            UpdatedAt = new DateTimeOffset(2026, 3, 15, 0, 0, 0, TimeSpan.Zero)
        }
    ];

    public Task<ServiceResult<List<Inventory>>> GetAllAsync(Guid? productId)
    {
        var query = Store.AsEnumerable();
        if (productId.HasValue)
            query = query.Where(i => i.ProductId == productId.Value);

        // Attach navigation properties from sibling stores
        var result = query.Select(i =>
        {
            i.Product = MockProductService.Store.FirstOrDefault(p => p.Id == i.ProductId)!;
            if (i.Product is not null)
                i.Product.Category = MockCategoryService.Store.FirstOrDefault(c => c.Id == i.Product.CategoryId);
            return i;
        }).ToList();

        return Task.FromResult(ServiceResult<List<Inventory>>.Success(result));
    }

    public Task<ServiceResult<Inventory>> GetByIdAsync(Guid id)
    {
        var inventory = Store.FirstOrDefault(i => i.Id == id);
        if (inventory is null)
            return Task.FromResult(ServiceResult<Inventory>.NotFound());

        inventory.Product = MockProductService.Store.FirstOrDefault(p => p.Id == inventory.ProductId)!;
        if (inventory.Product is not null)
            inventory.Product.Category = MockCategoryService.Store.FirstOrDefault(c => c.Id == inventory.Product.CategoryId);

        return Task.FromResult(ServiceResult<Inventory>.Success(inventory));
    }

    public Task<ServiceResult<Inventory>> CreateAsync(CreateInventoryRequest request)
    {
        if (MockProductService.Store.All(p => p.Id != request.ProductId))
            return Task.FromResult(ServiceResult<Inventory>.ValidationError("ProductId does not refer to an existing product."));

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
        Store.Add(inventory);
        return Task.FromResult(ServiceResult<Inventory>.Success(inventory));
    }

    public Task<ServiceResult<Inventory>> UpdateAsync(Guid id, UpdateInventoryRequest request)
    {
        var inventory = Store.FirstOrDefault(i => i.Id == id);
        if (inventory is null)
            return Task.FromResult(ServiceResult<Inventory>.NotFound());

        inventory.Location = request.Location;
        if (request.InitialQuantity.HasValue)
            inventory.InitialQuantity = request.InitialQuantity.Value;
        inventory.NearestExpiryDate = request.NearestExpiryDate;
        inventory.UpdatedAt = DateTimeOffset.UtcNow;
        return Task.FromResult(ServiceResult<Inventory>.Success(inventory));
    }

    public Task<ServiceResult<Inventory>> PatchNoteAsync(Guid id, PatchInventoryNoteRequest request)
    {
        var inventory = Store.FirstOrDefault(i => i.Id == id);
        if (inventory is null)
            return Task.FromResult(ServiceResult<Inventory>.NotFound());

        inventory.Note = request.Note;
        inventory.UpdatedAt = DateTimeOffset.UtcNow;
        return Task.FromResult(ServiceResult<Inventory>.Success(inventory));
    }

    public Task<ServiceResult<object?>> DeleteAsync(Guid id)
    {
        var inventory = Store.FirstOrDefault(i => i.Id == id);
        if (inventory is null)
            return Task.FromResult(ServiceResult<object?>.NotFound());

        if (MockInventoryEventService.Store.Any(e => e.InventoryId == id))
            return Task.FromResult(ServiceResult<object?>.Conflict("Cannot delete an inventory that has events."));

        Store.Remove(inventory);
        return Task.FromResult(ServiceResult<object?>.Success(null));
    }
}
