using HouseHoldButler.Dtos;
using HouseHoldButler.Entities;
using HouseHoldButler.Requests.Inventory;

namespace HouseHoldButler.Services.Interfaces;

public interface ICategoryService
{
    Task<ServiceResult<List<Category>>> GetAllAsync();
    Task<ServiceResult<Category>> GetByIdAsync(Guid id);
    Task<ServiceResult<Category>> CreateAsync(CreateCategoryRequest request);
    Task<ServiceResult<Category>> UpdateAsync(Guid id, UpdateCategoryRequest request);
    Task<ServiceResult<object?>> DeleteAsync(Guid id);
    Task<ServiceResult<List<CategoryDto>>> GetCategoryDropdownAsync();
}
