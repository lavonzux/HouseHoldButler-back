using BackendApi.Dtos;
using BackendApi.Entities;
using BackendApi.Requests.Inventory;

namespace BackendApi.Services.Interfaces;

public interface IProductService
{
    Task<ServiceResult<List<Product>>> GetAllAsync();
    Task<ServiceResult<Product>> GetByIdAsync(Guid id);
    Task<ServiceResult<List<ProductHistoryEntryDto>>> GetHistoryAsync(Guid id);
    Task<ServiceResult<Product>> CreateAsync(CreateProductRequest request);
    Task<ServiceResult<Product>> UpdateAsync(Guid id, UpdateProductRequest request);
    Task<ServiceResult<object?>> DeleteAsync(Guid id);
    Task<ServiceResult<object?>> ForceDeleteAsync(Guid id);
}
