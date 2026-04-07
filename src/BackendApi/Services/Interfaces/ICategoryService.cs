using BackendApi.Dtos;
using BackendApi.Entities;
using BackendApi.Requests.Inventory;

namespace BackendApi.Services.Interfaces;

public interface ICategoryService
{
    Task<ServiceResult<List<Category>>> GetAllAsync();
    Task<ServiceResult<Category>> GetByIdAsync(Guid id);
    Task<ServiceResult<Category>> CreateAsync(CreateCategoryRequest request);
    Task<ServiceResult<Category>> UpdateAsync(Guid id, UpdateCategoryRequest request);
    Task<ServiceResult<object?>> DeleteAsync(Guid id);
    Task<ServiceResult<List<CategoryDto>>> GetCategoryDropdownAsync();
}
