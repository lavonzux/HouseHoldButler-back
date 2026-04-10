using HouseHoldButler.Entities;
using HouseHoldButler.Requests.Inventory;

namespace HouseHoldButler.Services.Interfaces;

public interface IInventoryService
{
    Task<ServiceResult<List<Inventory>>> GetAllAsync(Guid? productId);
    Task<ServiceResult<Inventory>> GetByIdAsync(Guid id);
    Task<ServiceResult<Inventory>> CreateAsync(CreateInventoryRequest request);
    Task<ServiceResult<Inventory>> UpdateAsync(Guid id, UpdateInventoryRequest request);
    Task<ServiceResult<Inventory>> PatchNoteAsync(Guid id, PatchInventoryNoteRequest request);
    Task<ServiceResult<object?>> DeleteAsync(Guid id);
}
