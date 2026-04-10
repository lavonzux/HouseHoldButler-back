using HouseHoldButler.Entities;
using HouseHoldButler.Requests.Inventory;

namespace HouseHoldButler.Services.Interfaces;

public interface IInventoryEventService
{
    Task<ServiceResult<List<InventoryEvent>>> GetAllAsync(Guid? inventoryId, Guid? productId = null);
    Task<ServiceResult<InventoryEvent>> GetByIdAsync(Guid id);
    Task<ServiceResult<InventoryEvent>> CreateAsync(CreateInventoryEventRequest request);
}
